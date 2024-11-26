using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Drawing;
using System.Threading;
using System.Web.UI;
using VTC.Common;
using VTC.Common.RegionConfig;

namespace VTC.Kernel
{
    /// <summary>
    /// Track hypotheses for multiple targets based on Reid's Multiple Hypothesis
    /// Tracking Algorithm
    /// </summary>
    public class MultipleHypothesisTracker
    {
        private RegionConfig _regionConfig;
        private HypothesisTree _hypothesisTree;
        public VelocityField VelocityField { get; }
        public List<TrajectoryFade> Trajectories;
        private int _numProcessedFrames = 0;

        public int ValidationRegionDeviation => _regionConfig.ValRegDeviation;

        public List<TrackedObject> CurrentVehicles => _hypothesisTree?.NodeData?.Vehicles;

        public List<TrackedObject> DeletedVehicles => _hypothesisTree?.NodeData?.DeletedVehicles;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MultipleHypothesisTracker(RegionConfig regionConfig, VelocityField velocityField)
        {
            _regionConfig = regionConfig;

            StateHypothesis initialHypothesis = new StateHypothesis(_regionConfig.MissThreshold);
            _hypothesisTree = new HypothesisTree(initialHypothesis);
            _hypothesisTree.PopulateSystemDynamicsMatrices(_regionConfig.Q_position, _regionConfig.Q_color, _regionConfig.Q_size, _regionConfig.R_position, _regionConfig.R_color, _regionConfig.R_size, _regionConfig.CompensationGain);

            VelocityField = velocityField;
            Trajectories = new List<TrajectoryFade>();   
        }

        /// <summary>
        /// Update targets from all child hypotheses with a new set of coordinates
        /// </summary>
        /// <param name="detections">Information about each detected item present in the latest readings.  This 
        /// list is assumed to be complete.</param>
        /// <param name="ct">Token for breaking out of execution</param>
        /// <param name="timestep">Time (in seconds) since previous set of measurements/detections.</param>
        public void Update(Measurement[] detections, double timestep)
        {
            int numDetections = detections.Length;

            //Maintain hypothesis tree
            if (_hypothesisTree.Children.Count > 0)
            {
                if (_hypothesisTree.TreeDepth() > _regionConfig.MaxHypTreeDepth)
                {
                    _hypothesisTree.Prune(1);
                    _hypothesisTree = _hypothesisTree.GetChild(0);
                }
            }

            List<Node<StateHypothesis>> childNodeList = _hypothesisTree.GetLeafNodes();
            foreach (Node<StateHypothesis> childNode in childNodeList) //For each lowest-level hypothesis node
            {
                // Update child node
                if (numDetections > 0)
                {
                    GenerateChildNodes(detections, childNode, timestep);
                }
                else
                {
                    int numExistingTargets = childNode.NodeData.Vehicles.Count;
                    StateHypothesis childHypothesis = new StateHypothesis(_regionConfig.MissThreshold);
                    childNode.AddChild(childHypothesis);
                    HypothesisTree childHypothesisTree = new HypothesisTree(childNode.Children[0].NodeData)
                    {
                        Parent = childNode
                    };
                    childHypothesisTree.PopulateSystemDynamicsMatrices(_regionConfig.Q_position, _regionConfig.Q_color, _regionConfig.Q_size, _regionConfig.R_position, _regionConfig.R_color, _regionConfig.R_size, _regionConfig.CompensationGain);

                    childHypothesis.Probability = Math.Pow((1 - _regionConfig.Pd), numExistingTargets);
                    //Update states for vehicles without Measurement
                    for (int j = 0; j < numExistingTargets; j++)
                    {
                        //Updating state for missed measurement
                        StateEstimate lastState = childNode.NodeData.Vehicles[j].StateHistory.Last();
                        StateEstimate noMeasurementUpdate = lastState.PropagateStateNoMeasurement(timestep, _hypothesisTree.H, _hypothesisTree.R(lastState.Size), _hypothesisTree.F(timestep), _hypothesisTree.Q(timestep, lastState.Size), _hypothesisTree.CompensationGain);
                        childHypothesisTree.UpdateVehicleFromPrevious(j, noMeasurementUpdate, false);
                    }
                }

            }

            // Insert velocities of current vehicles into the velocity field for later use when adding new vehicles
            var pointVelocityDic = new Dictionary<Point, VelocityField.Velocity>();
            foreach (var v in CurrentVehicles)
            {
                var lastState = v.StateHistory.Last();
                var coords = new Point((int)lastState.X, (int)lastState.Y);
                var velocity = new VelocityField.Velocity(lastState.Vx, lastState.Vy);

                pointVelocityDic[coords] = velocity;
            }

            VelocityField.TryInsertVelocitiesAsync(pointVelocityDic);

            UpdateTrajectoriesList();

            _numProcessedFrames++;
        }

        public void UpdateRegionConfig(RegionConfig newRegionConfig)
        {
            _regionConfig = newRegionConfig;
            _hypothesisTree.PopulateSystemDynamicsMatrices(_regionConfig.Q_position, _regionConfig.Q_color, _regionConfig.Q_size, _regionConfig.R_position, _regionConfig.R_color, _regionConfig.R_size, _regionConfig.CompensationGain);
        }

        public StateHypothesis MostLikelyStateHypothesis()
        {
            //_hypothesisTree.Children.Sort(HypothesisTree.ProbCompare);
            //var bestHypothesis = _hypothesisTree.Children.First();

            //var leafNodes = _hypothesisTree.GetLeafNodes();
            //leafNodes.Sort(HypothesisTree.ProbCompare);
            //var bestHypothesis = leafNodes.First();
            //return bestHypothesis.NodeData;
            return _hypothesisTree.NodeData;
        }

        /// <summary>
        /// Generate child nodes and fill hypotheses for a given node, using the provided measurements
        /// </summary>
        /// <param name="detections">Coordinates of all detections present in the latest measurements.  This 
        /// list is assumed to be complete.</param>
        /// <param name="hypothesisNode">The node to build the new hypotheses from</param>
        /// <param name="ct">Token for breaking out of execution</param>
        /// <param name="timestep">Time-difference in seconds between this and previous frame.</param>
        private void GenerateChildNodes(Measurement[] detections, Node<StateHypothesis> hypothesisNode, double timestep)
        {
            //Allocate matrix one column for each existing vehicle plus one column for new vehicles and one for false positives, one row for each object detection event
            int numExistingTargets = hypothesisNode.NodeData.Vehicles.Count;
            int numDetections = detections.Length;

            //Got detections
            DenseMatrix falseAssignmentMatrix = DenseMatrix.Create(numDetections, numDetections, (x, y) => double.MinValue);
            double falseAssignmentCost = Math.Log10(_regionConfig.LambdaF);
            double[] falseAssignmentDiagonal = Enumerable.Repeat(falseAssignmentCost, numDetections).ToArray();
            falseAssignmentMatrix.SetDiagonal(falseAssignmentDiagonal); //Represents a false positive

            DenseMatrix newTargetMatrix = DenseMatrix.Create(numDetections, numDetections, (x, y) => double.MinValue);
            double newTargetCost = Math.Log10(_regionConfig.LambdaN);
            double[] newTargetDiagonal = Enumerable.Repeat(newTargetCost, numDetections).ToArray();
            newTargetMatrix.SetDiagonal(newTargetDiagonal); //Represents a new object to track

            StateEstimate[] targetStateEstimates = hypothesisNode.NodeData.GetStateEstimates();

            //Generate a matrix where each row signifies a detection and each column signifies an existing target
            //The value in each cell is the probability of the row's measurement occuring for the column's object
            DenseMatrix ambiguityMatrix = GenerateAmbiguityMatrix(detections, numExistingTargets, targetStateEstimates, timestep);

            //Generating expanded hypothesis
            //Hypothesis matrix needs to have a unique column for each detection being treated as a false positive or new object
            DenseMatrix hypothesisExpanded = GetExpandedHypothesis(
                numDetections, 
                numExistingTargets, 
                ambiguityMatrix, 
                falseAssignmentMatrix, 
                newTargetMatrix
                );

            GenerateChildHypotheses(detections, numDetections, hypothesisNode, numExistingTargets, hypothesisExpanded, timestep);
        }

        /// <summary>
        /// Fill hypotheses for children of a given node, using the provided measurements
        /// </summary>
        /// <param name="coords">Coordinates of all detections present in the latest readings.</param>
        /// <param name="numDetections">Nomber of targets present in the new measurements</param>
        /// <param name="hypothesisParent">Hypothesis node to add child hypotheses to</param>
        /// <param name="numExistingTargets">Number of currently detected targets</param>
        /// <param name="hypothesisExpanded">Hypothesis matrix</param>
        /// <param name="timestep">Time difference in seconds between this and previous frame.</param>
        private void GenerateChildHypotheses(Measurement[] coords, int numDetections, Node<StateHypothesis> hypothesisParent, int numExistingTargets, DenseMatrix hypothesisExpanded, Double timestep)
        {
            //Calculate K-best assignment using Murty's algorithm
            double[,] costs = hypothesisExpanded.ToArray();
            for (int i = 0; i < costs.GetLength(0); i++)
                for (int j = 0; j < costs.GetLength(1); j++)
                    costs[i, j] = -costs[i, j];

            List<int[]> kBest = OptAssign.FindKBestAssignments(costs, _regionConfig.KHypotheses);
            int numTargetsCreated = 0;

            //Generate child hypotheses from k-best assignments
            for (int i = 0; i < kBest.Count; i++)
            {
                int[] assignment = kBest[i];
                StateHypothesis childHypothesis = new StateHypothesis(_regionConfig.MissThreshold);
                hypothesisParent.AddChild(childHypothesis);
                HypothesisTree childHypothesisTree = new HypothesisTree(hypothesisParent.Children[i].NodeData)
                {
                    Parent = hypothesisParent
                };
                childHypothesisTree.PopulateSystemDynamicsMatrices(_regionConfig.Q_position, _regionConfig.Q_color, _regionConfig.Q_size, _regionConfig.R_position, _regionConfig.R_color, _regionConfig.R_size, _regionConfig.CompensationGain);

                childHypothesis.Probability = OptAssign.AssignmentCost(costs, assignment);
                //Update states for vehicles without measurements
                for (int j = 0; j < numExistingTargets; j++)
                {
                    //If this target is not detected
                    if (!(assignment.Contains(j + numDetections)))
                    {
                        //Updating state for missed measurement
                        StateEstimate lastState = hypothesisParent.NodeData.Vehicles[j].StateHistory.Last();
                        StateEstimate noMeasurementUpdate = lastState.PropagateStateNoMeasurement(timestep, _hypothesisTree.H, _hypothesisTree.R(lastState.Size), _hypothesisTree.F(timestep), _hypothesisTree.Q(timestep,lastState.Size), _hypothesisTree.CompensationGain);
                        childHypothesisTree.UpdateVehicleFromPrevious(j, noMeasurementUpdate, false);
                    }
                }

                for (int j = 0; j < numDetections; j++)
                {

                    //Account for new vehicles
                    if (assignment[j] >= numExistingTargets + numDetections && numExistingTargets + numTargetsCreated < _regionConfig.MaxTargets) //Add new vehicle
                    {
                        // Find predicted velocity
                        var velocity = VelocityField.GetAvgVelocity((int)coords[j].X, (int)coords[j].Y);

                        var size = coords[j].Size;
                        var approximateRadius = Math.Sqrt(size);

                        //Creating new vehicle
                        numTargetsCreated++;
                        childHypothesis.AddVehicle(
                            Convert.ToInt16(coords[j].X), 
                            Convert.ToInt16(coords[j].Y), 
                            velocity.Vx, 
                            velocity.Vy,
                            Convert.ToInt16(coords[j].Red),
                            Convert.ToInt16(coords[j].Green),
                            Convert.ToInt16(coords[j].Blue), 
                            Convert.ToInt32(coords[j].Size),
                            _regionConfig.VehicleInitialCovX * approximateRadius,
                            _regionConfig.VehicleInitialCovY * approximateRadius,
                            _regionConfig.VehicleInitialCovVX * approximateRadius,
                            _regionConfig.VehicleInitialCovVY * approximateRadius,
                            _regionConfig.VehicleInitialCovR,
                            _regionConfig.VehicleInitialCovG,
                            _regionConfig.VehicleInitialCovB,
                            _regionConfig.VehicleInitialCovSize * approximateRadius,
                            coords[j].ObjectClass,
                            _numProcessedFrames
                            );

                    }
                    else if (assignment[j] >= numDetections && assignment[j] < numDetections + numExistingTargets) //Update states for vehicles with measurements
                    {
                        //Updating vehicle with measurement
                        StateEstimate lastState = hypothesisParent.NodeData.Vehicles[assignment[j] - numDetections].StateHistory.Last();
                        StateEstimate estimatedState = lastState.PropagateState(timestep, _hypothesisTree.H, _hypothesisTree.R(lastState.Size), _hypothesisTree.F(timestep), _hypothesisTree.Q(timestep,lastState.Size), coords[j]);
                        childHypothesisTree.UpdateVehicleFromPrevious(assignment[j] - numDetections, estimatedState, true);
                    }

                }
            }
        }

        /// <summary>
        /// Generate the hypothesis matrix
        /// </summary>
        /// <param name="numDetections">Number of targets detected in current measurements</param>
        /// <param name="numExistingTargets">Number of currently detected targets</param>
        /// <param name="ambiguityMatrix">Ambiguity matrix containing probability that a given measurement belongs to a given target</param>
        /// <param name="falseAssignmentMatrix">Probability matrix if false assignment</param>
        /// <param name="newTargetMatrix">Probability matrix indicating likelihood that a measurement is from a new target</param>
        /// <returns></returns>
        private static DenseMatrix GetExpandedHypothesis(int numDetections, int numExistingTargets, DenseMatrix ambiguityMatrix, DenseMatrix falseAssignmentMatrix, DenseMatrix newTargetMatrix)
        {
            DenseMatrix hypothesisExpanded;
            if (numExistingTargets > 0)
            {
                //Expanded hypothesis: targets exist
                DenseMatrix targetAssignmentMatrix = (DenseMatrix)ambiguityMatrix.SubMatrix(0, numDetections, 1, numExistingTargets);
                hypothesisExpanded = new DenseMatrix(numDetections, 2 * numDetections + numExistingTargets);
                hypothesisExpanded.SetSubMatrix(0, numDetections, 0, numDetections, falseAssignmentMatrix);
                hypothesisExpanded.SetSubMatrix(0, numDetections, numDetections, numExistingTargets, targetAssignmentMatrix);
                hypothesisExpanded.SetSubMatrix(0, numDetections, numDetections + numExistingTargets, numDetections, newTargetMatrix);
            }
            else
            {
                //Expanded hypothesis: no targets
                hypothesisExpanded = new DenseMatrix(numDetections, 2 * numDetections);
                hypothesisExpanded.SetSubMatrix(0, numDetections, 0, numDetections, falseAssignmentMatrix);
                hypothesisExpanded.SetSubMatrix(0, numDetections, numDetections, numDetections, newTargetMatrix);
            }
            return hypothesisExpanded;
        }

        /// <summary>
        /// Generate a matrix where each row signifies a detection and each column signifies an existing target
        /// The value in each cell is the probability of the row's measurement occuring for the column's object
        /// </summary>
        /// <param name="coordinates">New measurements</param>
        /// <param name="numExistingTargets">Number of currently detected targets</param>
        /// <param name="targetStateEstimates">Latest state estimates for each known target</param>
        /// <returns></returns>
        private DenseMatrix GenerateAmbiguityMatrix(Measurement[] coordinates, int numExistingTargets, StateEstimate[] targetStateEstimates, double timestep)
        {
            // TODO:  Can't we get numExistingTargets from target_state_estimates Length?

            int numDetections = coordinates.Length;
            var ambiguityMatrix = new DenseMatrix(numDetections, numExistingTargets + 2);
            Normal norm = new Normal();
            
            for (int i = 0; i < numExistingTargets; i++)
            {
                //Get this car's estimated next position using Kalman predictor
                StateEstimate noMeasurementEstimate = targetStateEstimates[i].PropagateStateNoMeasurement(timestep, _hypothesisTree.H, _hypothesisTree.R(targetStateEstimates[i].Size), _hypothesisTree.F(timestep), _hypothesisTree.Q(timestep, targetStateEstimates[i].Size), _hypothesisTree.CompensationGain);

                DenseMatrix pBar = new DenseMatrix(9, 9)
                {
                    [0, 0] = noMeasurementEstimate.CovX,
                    [1, 1] = noMeasurementEstimate.CovVx,
                    [2, 2] = noMeasurementEstimate.CovY,
                    [3, 3] = noMeasurementEstimate.CovVy,
                    [4, 4] = noMeasurementEstimate.CovRed,
                    [5, 5] = noMeasurementEstimate.CovGreen,
                    [6, 6] = noMeasurementEstimate.CovBlue,
                    [7, 7] = noMeasurementEstimate.CovSize,
                    [8, 8] = noMeasurementEstimate.CovVSize
                };

                DenseMatrix hTrans = (DenseMatrix)_hypothesisTree.H.Transpose();
                DenseMatrix b = _hypothesisTree.H * pBar * hTrans + _hypothesisTree.R(targetStateEstimates[i].Size);
                DenseMatrix bInverse = (DenseMatrix)b.Inverse();

                for (int j = 0; j < numDetections; j++)
                {
                    DenseMatrix zMeas = new DenseMatrix(6, 1)
                    {
                        [0, 0] = coordinates[j].X,
                        [1, 0] = coordinates[j].Y,
                        [2, 0] = coordinates[j].Red,
                        [3, 0] = coordinates[j].Green,
                        [4, 0] = coordinates[j].Blue,
                        [5, 0] = coordinates[j].Size
                    };


                    DenseMatrix zEst = new DenseMatrix(6, 1)
                    {
                        [0, 0] = noMeasurementEstimate.X,
                        [1, 0] = noMeasurementEstimate.Y,
                        [2, 0] = noMeasurementEstimate.Red,
                        [3, 0] = noMeasurementEstimate.Green,
                        [4, 0] = noMeasurementEstimate.Blue,
                        [5, 0] = noMeasurementEstimate.Size
                    };

                    DenseMatrix residual = StateEstimate.Residual(zEst, zMeas);
                    DenseMatrix residualTranspose = (DenseMatrix)residual.Transpose();
                    DenseMatrix mahalanobis = residualTranspose * bInverse * residual;
                    double mahalanobisDistance = Math.Sqrt(mahalanobis[0, 0]);

                    if (mahalanobisDistance > ValidationRegionDeviation)
                    {
                        ambiguityMatrix[j, i + 1] = Double.MinValue;
                    }
                    else
                    {
                        ambiguityMatrix[j, i + 1] = Math.Log10(_regionConfig.Pd * norm.Density(mahalanobisDistance) / (1 - _regionConfig.Pd));
                    }
                }
            }
            return ambiguityMatrix;
        }

        private void UpdateTrajectoriesList()
        {
            //Eliminate stale trajectories
            foreach (var t in Trajectories.ToList())
                if (t.ExitTime < DateTime.Now - TimeSpan.FromSeconds(3))
                    Trajectories.Remove(t);

            //Add new trajectories
            foreach (var v in DeletedVehicles)
            {
                var t = new TrajectoryFade
                {
                    ExitTime = DateTime.Now,
                    StateEstimates = v.StateHistory
                };
                Trajectories.Add(t);
            }
        }

    }

    public struct TrajectoryFade
    {
        public DateTime ExitTime;
        public List<StateEstimate> StateEstimates;
    }
}
