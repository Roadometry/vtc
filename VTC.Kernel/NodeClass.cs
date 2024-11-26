using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using VTC.Common;

namespace VTC.Kernel
{

    public class Node<T>
    {
        public Node<T> Parent;
        public List<Node<T>> Children;

        public T NodeData; 

        public Node(T value) : this()
        {
            NodeData = value;
        }

        public Node()
        {
            Children = new List<Node<T>>();
        }

        public Node<T> GetRoot()
        {            
            
            Func<Node<T>, Node<T>> getParent = null;
            getParent = x =>
            {
                if (x.Parent != null) { return getParent(x.Parent); } else { return this; }
            };

            return getParent(this);
        }

        public virtual void AddChild(T value)
        {
            Node<T> newchild = new Node<T>(value) {Parent = this};
            Children?.Add(newchild);
        }

        //Recursive function used in GetChain to iterate upwards through tree pushing each new node to the front of a list 
        private List<Node<T>> AddBack(List<Node<T>> x)
        {
        if (x[0].Parent != null)
                {
                    x.Add(x[0].Parent); //If parent node exists, add to start of the SortedList 
                    AddBack(x); //Continue process with parent node
                }
        return x;
        }

        /// <summary>
        /// Return list of nodes starting from root to this node
        /// </summary>
        /// <returns></returns>
        public List<Node<T>> PathFromRoot() 
        {
            List<Node<T>> pathList = new List<Node<T>> {this};
            return AddBack(pathList);
        }

        /// <summary>
        /// Return list containing all nodes in this tree
        /// </summary>
        public List<Node<T>> ToList()
        {
            List<Node<T>> list = new List<Node<T>>();
            Node<T> root = GetRoot();
            Action<Node<T>> traverse = null;
            traverse = x => { list.Add(x); x.Children.ForEach(traverse); };
            traverse(root);
            return list;
        }

        /// <summary>
        /// Return list containing all leaf nodes in this tree
        /// </summary>
        public List<Node<T>> GetLeafNodes()
        {
            var list = new List<Node<T>>();
            var root = GetRoot();
            Action<Node<T>> traverseLeafnodes = null;
            traverseLeafnodes = x => {
               if (x.Children == null || x.Children.Count == 0) //Not sure why checking against null doesn't work here, maybe IsNull is not implemented. Why isn't this problem encountered in other places?
                   list.Add(x); 
               else
                x.Children.ForEach(traverseLeafnodes); 
            };
            traverseLeafnodes(root);
            return list;
        }

        //Get number of nodes from root to leaf nodes, following "first child" path. This assumes that all branches are of equal depth. 
        public int TreeDepth()
        {
            var depth = TraverseDepth(this);
            return depth;
        }

        private static int TraverseDepth(Node<T> inputNode)
        { 
            var depth = 0;
            if (inputNode.Children.Count > 0)
                depth = inputNode.Children.Select(TraverseDepth).Concat(new[] {depth}).Max();

            return depth+1;
        }

        //Get this node's depth
        public int NodeDepth()
        {
            var nodeDepth = 1;
            var currentNode = this;
                    while (currentNode.Parent != null)
                {
                    currentNode = currentNode.Parent;
                    nodeDepth++;
                }
            return nodeDepth;
        }

        //Return array populated with # of nodes at each depth 
        public int[] NodeCountByDepth()
        {
            var treeDepth = TreeDepth();
            var depthArray = new int[treeDepth];
            var nodeList = ToList();
            foreach (var thisNode in nodeList)
                depthArray[thisNode.NodeDepth() - 1]++;

            return depthArray;
        }

        public int NumChildren()
        {
            var numChildren = 0;
            Action<Node<T>> traverse = null;
            traverse = x =>
            {
                numChildren++;
                if (x.Children == null || x.Children.Count == 0) //Not sure why checking against null doesn't work here, maybe IsNull is not implemented. Why isn't this problem encountered in other places?
                    return;
                x.Children.ForEach(traverse);
            };
            traverse(this);
            return numChildren-1;
        }

    }


    /// <summary>
    /// Object containing data-measurement association, associate state estimates and all other associated probablities and esimates
    /// necessary for multiple hypothesis tracking. 
    /// </summary>
    public class StateHypothesis
    {
        public double Probability;

        public bool[,] AssignmentMatrix;

        public List<TrackedObject> DeletedVehicles;

        public List<TrackedObject> Vehicles;

        public List<TrackedObject> NewVehicles;

        public int MissDetectionThreshold;

        public StateHypothesis(int missThreshold)
        {
            Probability = 1;
            AssignmentMatrix = new bool[0,0];
            Vehicles = new List<TrackedObject>();
            DeletedVehicles = new List<TrackedObject>();
            NewVehicles = new List<TrackedObject>();
            MissDetectionThreshold = missThreshold;
        }

        public StateHypothesis(double initialProbablity, int numVehicles, int numMeasurements,int missThreshold)
        {
            Probability = initialProbablity;
            AssignmentMatrix = new bool[numMeasurements, numVehicles + 2];
            DeletedVehicles = new List<TrackedObject>();
            Vehicles = new List<TrackedObject>(numVehicles);
            MissDetectionThreshold = missThreshold;
        }

        public StateEstimate[] GetStateEstimates()
        {
            StateEstimate[] vehicleStateEstimates = new StateEstimate[Vehicles.Count];
            for (int i = 0; i < Vehicles.Count; i++)
                vehicleStateEstimates[i] = Vehicles[i].StateHistory[Vehicles[i].StateHistory.Count - 1];
            
            return vehicleStateEstimates;
        }

        public StateEstimate[,] GetDeletedStateEstimates()
        {
            StateEstimate[,] deletedVehicleStateEstimates = new StateEstimate[DeletedVehicles.Count,2];
            for (int i = 0; i < DeletedVehicles.Count; i++)
            {
                try
                {
                    deletedVehicleStateEstimates[i, 0] = DeletedVehicles[i].StateHistory[0];
                }
                catch (Exception)
                {
                    throw new ArgumentException("Initial state history missing");
                }

                try
                {
                    deletedVehicleStateEstimates[i, 1] = DeletedVehicles[i].StateHistory[DeletedVehicles[i].StateHistory.Count - 1];
                }
                catch (Exception)
                {
                    var errorString = "Final state history missing. History length: " + DeletedVehicles[i].StateHistory.Count;
                    throw new ArgumentException(errorString);
                }
            }

            return deletedVehicleStateEstimates;
        }

        public StateEstimate[] GetNewStateEstimates()
        {
            var newVehicleStateEstimates = new StateEstimate[NewVehicles.Count];
            for (var i = 0; i < NewVehicles.Count; i++)
                newVehicleStateEstimates[i] = NewVehicles[i].StateHistory[NewVehicles[i].StateHistory.Count-1];

            return newVehicleStateEstimates;
        }

        public void AddVehicle(int x, int y, double vx, double vy, int red, int green, int blue, int size, double covx, double covy, double covVx, double covVy, double covR, double covG, double covB, double covSize, int classId, int frame)
        {
            Dictionary<int, int> classDetectionCounts = new Dictionary<int, int>();
            classDetectionCounts.Add(classId, 1);

            var initialState = new StateEstimate
            {
                X = x,
                Y = y,
                Red = red,
                Green = green,
                Blue = blue,
                Vx = vx,
                Vy = vy,
                Size = size,
                CovX = covx,
                CovY = covy,
                CovVx = covVx,
                CovVy = covVy,
                CovRed = covR,
                CovGreen = covG,
                CovBlue = covB,
                CovSize = covSize,
                CovVSize = covSize,
                ClassDetectionCounts = classDetectionCounts
            };

            var newVehicle = new TrackedObject(initialState,frame);
            Vehicles.Add(newVehicle);
            NewVehicles.Add(newVehicle);
        }
    }

    /// <summary>
    /// Main structure containing hypothesis tree for Multiple Hypothesis Tracking algorithm
    /// </summary>
    public class HypothesisTree : Node<StateHypothesis> 
    {
        public DenseMatrix H; //Measurement equation
        public DenseMatrix P; //

        private double _qPosition;
        private double _qColor;
        private double _qSize;

        private double _rPosition;
        private double _rColor;
        private double _rSize;

        public double CompensationGain; //Gain applied to process noise when a measurement is missed

        public HypothesisTree(StateHypothesis value) : base(value)
        {
        }

        // ************************************************ //
        // *************** System Dynamics: *************** //
        // ************************************************ //
        //  x_new  = x_old + dt*vx;
        //  vy_new = vy_old
        //  y_new  = y_old + dt*vy
        //  vx_new = vx_old
        //  R_new = R_old
        //  G_new = G_old
        //  B_new = B_old
        // ************************************************ //
        public void PopulateSystemDynamicsMatrices(double qPosition, double qColor, double qSize, double rPosition, double rColor, double rSize, double compensationGain)
        {
            _qPosition = qPosition;
            _qColor = qColor;
            _qSize = qSize;

            H = new DenseMatrix(6, 9)
            {
                [0, 0] = 1,
                [1, 2] = 1,
                [2, 4] = 1,
                [3, 5] = 1,
                [4, 6] = 1,
                [5, 7] = 1
            }; // Measurement equation: x,y,R,G,B,Size are observed (not velocities)

            _rPosition = rPosition;
            _rColor = rColor;
            _rSize = rSize;

            CompensationGain = compensationGain;
        }

        public DenseMatrix F(double dt)
        {
            DenseMatrix m = new DenseMatrix(9, 9)
            {
                [0, 0] = 1,
                [0, 1] = dt,
                [1, 1] = 1,
                [2, 2] = 1,
                [2, 3] = dt,
                [3, 3] = 1,
                [4, 4] = 1,
                [5, 5] = 1,
                [6, 6] = 1,
                [7, 7] = 1,
                [7, 8] = dt,
                [8, 8] = 1
            }; //Motion equation

            return m;
        }

        public DenseMatrix Q(double dt, double size)
        {
            var radius = Math.Sqrt(size);
            var m = new DenseMatrix(9, 9)
            {
                [0, 0] = (dt*dt*dt*dt/4)*_qPosition * radius,
                [0, 1] = (dt*dt*dt/3)* _qPosition * radius,
                [1, 0] = (dt*dt*dt/3)* _qPosition * radius,
                [1, 1] = (dt*dt/2)* _qPosition * radius,
                [2, 2] = (dt*dt*dt*dt/4)* _qPosition * radius,
                [2, 3] = (dt*dt*dt/3)* _qPosition * radius,
                [3, 2] = (dt*dt*dt/3)* _qPosition * radius,
                [3, 3] = (dt*dt/2)* _qPosition * radius,
                [4, 4] = _qColor,
                [5, 5] = _qColor,
                [6, 6] = _qColor,
                [7, 7] = _qSize * radius * radius,
                [8, 8] = _qSize * radius * radius * (dt * dt / 2)
            }; //Process covariance

            return m;
        }

        public DenseMatrix R(double size)
        {
            var measurementRadius = Math.Sqrt(Math.Abs(size)); // "Size" can occasionally become negative in a predicted value due to a negative VSize propagation.
            DenseMatrix matrixR = new DenseMatrix(6, 6)
            {
                [0, 0] = _rPosition * measurementRadius,
                [1, 1] = _rPosition * measurementRadius,
                [2, 2] = _rColor,
                [3, 3] = _rColor,
                [4, 4] = _rColor,
                [5, 5] = _rSize * measurementRadius * measurementRadius
            }; //Measurement covariance

            return matrixR;
        }

        /// <summary>
        /// Create new child hypothesis
        /// </summary>
        /// <param name="value">New child's StateHypothesis</param>
        public override void AddChild(StateHypothesis value)
        {
            var newchild = new HypothesisTree(value) {Parent = this};
            Children?.Add(newchild);
        }

        /// <summary>
        /// Copies & updates state history of a vehicle from parent node referenced by integer
        /// </summary>
        /// <param name="address">Index of vehicle to be updated in parent StateHypothesis</param>
        /// <param name="currentState">New state of vehicle to be updated</param>
        /// <param name="withMeasurement">True if the new state comes from a measurement update; false if the new state comes from a pure prediction. </param>
        public void UpdateVehicleFromPrevious(int address, StateEstimate currentState, bool withMeasurement)
        {
            var parentHypothesis = Parent.NodeData;
            var lastFrameVehicle = parentHypothesis.Vehicles[address];
            var updatedVehicle = new TrackedObject(lastFrameVehicle.StateHistory, currentState, lastFrameVehicle.FirstDetectionFrame);
            if (withMeasurement)
                currentState.SuccessiveMissedDetections = 0;

            var isWithinBounds = currentState.X >= 0.0 && currentState.Y >= 0.0 && currentState.X <= 640.0 && currentState.Y <= 480;
            

            if ((currentState.SuccessiveMissedDetections < NodeData.MissDetectionThreshold) && isWithinBounds)
                NodeData.Vehicles.Add(updatedVehicle);
            else
                NodeData.DeletedVehicles.Add(updatedVehicle);
        }


        /// <summary>
        /// Compare probability of two hypotheses
        /// </summary>
        /// <param name="x">Reference to first node</param>
        /// <param name="y">Refernce to second node</param>
        /// <returns>1 if p(a)>p(b), 0 otherwise</returns>
        public static int ProbCompare(Node<StateHypothesis> x, Node<StateHypothesis> y)
        {
            var a = (HypothesisTree)x;
            var b = (HypothesisTree)y;

            if (a.ChildProbability() > b.ChildProbability())
                return 1;
            return 0;
        }
        
        /// <summary>
        /// Remove lowest probability child nodes until k nodes remain
        /// </summary>
        /// <param name="numRemaining">Final number of nodes after pruning</param>
        public void Prune(int numRemaining)
        {
            if (Children.Count <= 0) return;
            Children.Sort(ProbCompare);

            while (Children.Count > numRemaining)
                Children.RemoveAt(Children.Count - 1);
        }

        public double ChildProbability()
        {
            var prob = TraverseChildProbabilities(this);
            return prob;
        }

        private double TraverseChildProbabilities(Node<StateHypothesis> inputNode)
        {
            var prob = double.MaxValue;
            if (inputNode.Children.Count > 0)
                prob = inputNode.Children.Select(TraverseChildProbabilities).Concat(new[] {prob}).Min();
            else
                return NodeData.Probability;

            return prob + NodeData.Probability;
        }

        public HypothesisTree GetChild(int index)
        {
            var child = (HypothesisTree) Children[index];
            child.CompensationGain = CompensationGain;
            child._qColor = _qColor;
            child._qPosition = _qPosition;
            child._qSize = _qSize;
            child._rColor = _rColor;
            child._rSize = _rSize;
            child._rPosition = _rPosition;
            
            child.P = P;
            child.H = H;
            
            foreach (var thisChild in Children)
            thisChild.Parent = null;

            Children = null;
            
            return child;
        }
    }






}
