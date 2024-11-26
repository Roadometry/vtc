using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using VTC.Common;

namespace VTC.Kernel
{
    public class TrajectoryVector
    {
        public double x;
        public double y;

        public double AngleRad()
        {
            return Math.Atan2(-y, x);
        }
    }

    public class TrajectoryValidity
    {
        public bool valid = true;
        public string description = "";
    }

    public class TrajectorySimilarity
    {
        private const double POSITION_MULTIPLIER = 0.011;
        private const double ANGLE_MULTIPLIER = 0.01;
        private const double CURVATURE_MULTIPLIER = 500.0;
        private const int STOP_COUNT_THRESHOLD = 5;
        private const double STOPPED_VELOCITY_MAGNITUDE_THRESHOLD = 1.5;

        public static Movement MatchNearestTrajectory(TrackedObject d, string classType, int minPathLength, List<Movement> trajectoryPrototypes)
        {
            //If no trajectories have been configured, we want this function to behave nicely for new users, so we count everything (instead of counting nothing).
            if (trajectoryPrototypes.Count == 0)
            {
                var movement = new Movement("Unknown", "Unknown", Turn.Unknown, ObjectType.Unknown, d.StateHistory, DateTime.Now, false);
                return movement;
            }

            var matchedTrajectoryName = BestMatchTrajectory(d.StateHistory, trajectoryPrototypes, classType);
            return matchedTrajectoryName;
        }

        public static TrajectoryValidity ValidateTrajectory(TrackedObject d,  int minPathLength, double missRatioThreshold, double covarianceThreshold, double smoothnessThreshold, double movementLengthRatioThreshold)
        {
            var tv = new TrajectoryValidity();
            tv.valid = true;

            var distance = d.NetMovement();
            if (distance < minPathLength)
            {
                tv.description = "Trajectory rejected: path too short (" + Math.Round(distance) + ")";
                tv.valid = false;
            }

            var missRatio = d.MissRatio();
            if (missRatio > missRatioThreshold)
            {
                tv.description = "Trajectory rejected: miss ratio too high (" + Math.Round(missRatio,1) + ")";
                tv.valid = false;
            }

            var fpc = d.FinalPositionCovariance();
            if (fpc > covarianceThreshold)
            { 
                tv.description = "Trajectory rejected: final position covariance too high (" + Math.Round(fpc) + ")"; 
                tv.valid = false;
            }

            var smoothness = d.StateHistory.Smoothness();
            if (smoothness < smoothnessThreshold)
            {
                tv.description = "Trajectory rejected: smoothness too low (" + Math.Round(smoothness,2) + ")";
                tv.valid = false;
            }

            var movementLengthRatio = distance/d.PathLengthIntegral();
            if (movementLengthRatio < movementLengthRatioThreshold)
            {
                tv.description = "Trajectory rejected: movement-length ratio too low (" + Math.Round(movementLengthRatio, 2) + ")";
                tv.valid = false;
            }

            return tv;
        }

        public static double PathIntegralCost(List<StateEstimate> trajectory1, List<StateEstimate> trajectory2)
        {
            var cost = 0.0;
            //Adjust the speed estimate for the first element of trajectory1.
            //This is necessary because MHT does not assign the speed of the object initially.
            trajectory1.First().Vx = trajectory1[1].Vx;
            trajectory1.First().Vy = trajectory1[1].Vy;

            //This index is used to force the comparison to proceed in the 'forwards' direction; we do not
            //want to accidentally iterate backwards on t2.
            var indexT2 = 0;

            //These variables are used to detect and clip (discard) match-costs for long periods where the vehicle is stopped. 
            //If this operation is not performed, the periods where the vehicle is stopped are over-weighted and tend to dominate
            //the classifier's decision.
            bool objectStopped = false;
            int stopCount = 0;

            for (int i=0;i<trajectory1.Count;i++)
            {
                var trajectory1StateEstimate = trajectory1[i];
                var trajectory2NearestStateEstimate = NearestPointOnTrajectory(trajectory1StateEstimate, trajectory2.GetRange(indexT2,trajectory2.Count()-indexT2));
                indexT2 = trajectory2.IndexOf(trajectory2NearestStateEstimate);

                var positionCost = Math.Sqrt(Math.Pow(trajectory1StateEstimate.X - trajectory2NearestStateEstimate.X,2) + Math.Pow(trajectory1StateEstimate.Y - trajectory2NearestStateEstimate.Y,2));

                TrajectoryVector iv1 = new TrajectoryVector();
                TrajectoryVector iv2 = new TrajectoryVector();

                iv1.x = trajectory1StateEstimate.Vx;
                iv1.y = trajectory1StateEstimate.Vy;

                iv2.x = trajectory2NearestStateEstimate.Vx;
                iv2.y = trajectory2NearestStateEstimate.Vy;

                var angleDiff = CompareAngles(iv1, iv2);
                var velocityMagnitude = Math.Sqrt(Math.Pow(iv1.x,2) + Math.Pow(iv1.y,2));

                if (velocityMagnitude < STOPPED_VELOCITY_MAGNITUDE_THRESHOLD)
                {
                    stopCount++;
                }
                else
                {
                    stopCount = 0;
                }

                if (stopCount >= STOP_COUNT_THRESHOLD)
                {
                    objectStopped = true;
                }
                else
                {
                    objectStopped = false;
                }

                var thisPointCost = POSITION_MULTIPLIER*positionCost + ANGLE_MULTIPLIER*angleDiff*velocityMagnitude;
                if (!objectStopped)
                {
                    cost += thisPointCost;
                }
            }

            var curvature1 = Curvature(trajectory1);
            var curvature2 = Curvature(trajectory2);
            var curvatureDifference = Math.Abs(curvature1 - curvature2);
            cost += curvatureDifference*CURVATURE_MULTIPLIER;
            
            return cost;
        }

        public static string CostExplanation(List<StateEstimate> trajectory1, List<StateEstimate> trajectory2)
        {
            var explanation = "";
            var cost = 0.0;
            var totalAngleCost = 0.0;
            var totalPositionCost = 0.0;
            //Adjust the speed estimate for the first element of trajectory1.
            //This is necessary because MHT does not assign the speed of the object initially.
            trajectory1.First().Vx = trajectory1[1].Vx;
            trajectory1.First().Vy = trajectory1[1].Vy;

            //This index is used to force the comparison to proceed in the 'forwards' direction; we do not
            //want to accidentally iterate backwards on t2.
            var indexT2 = 0;

            //These variables are used to detect and clip (discard) match-costs for long periods where the vehicle is stopped. 
            //If this operation is not performed, the periods where the vehicle is stopped are over-weighted and tend to dominate
            //the classifier's decision.
            bool objectStopped = false;
            int stopCount = 0;

            for(int i=0;i<trajectory1.Count;i++)
            {
                var trajectory1StateEstimate = trajectory1[i];
                var trajectory2NearestStateEstimate = NearestPointOnTrajectory(trajectory1StateEstimate, trajectory2.GetRange(indexT2,trajectory2.Count()-indexT2));
                indexT2 = trajectory2.IndexOf(trajectory2NearestStateEstimate);

                var positionCost = Math.Sqrt(Math.Pow(trajectory1StateEstimate.X - trajectory2NearestStateEstimate.X,2) + Math.Pow(trajectory1StateEstimate.Y - trajectory2NearestStateEstimate.Y,2));
                totalPositionCost += positionCost;

                TrajectoryVector iv1 = new TrajectoryVector();
                TrajectoryVector iv2 = new TrajectoryVector();

                iv1.x = trajectory1StateEstimate.Vx;
                iv1.y = trajectory1StateEstimate.Vy;

                iv2.x = trajectory2NearestStateEstimate.Vx;
                iv2.y = trajectory2NearestStateEstimate.Vy;

                var angleDiff = CompareAngles(iv1, iv2);
                var velocityMagnitude = Math.Sqrt(Math.Pow(iv1.x,2) + Math.Pow(iv1.y,2));

                if (velocityMagnitude < STOPPED_VELOCITY_MAGNITUDE_THRESHOLD)
                {
                    stopCount++;
                }
                else
                {
                    stopCount = 0;
                }

                if (stopCount >= STOP_COUNT_THRESHOLD)
                {
                    objectStopped = true;
                }
                else
                {
                    objectStopped = false;
                }

                totalAngleCost += angleDiff * velocityMagnitude;
                var thisPointCost = POSITION_MULTIPLIER*positionCost + ANGLE_MULTIPLIER*angleDiff*velocityMagnitude;

                if (!objectStopped)
                {
                    cost += thisPointCost;
                }

                //System.Diagnostics.Debug.WriteLine("TotalCost," + Math.Round(thisPointCost,2) + ",PositionCost," + Math.Round(positionCost,2) + ",AngleCost," + Math.Round(angleDiff,2) + ",Angle1," + Math.Round(iv1.AngleRad(),2) + ",Angle2," + Math.Round(iv2.AngleRad(),2) + ",Vx1," + Math.Round(iv1.x,2) + ",Vy1," + Math.Round(iv1.y,2) + ",Vx2," + Math.Round(iv2.x,2) + ",Vy2," + Math.Round(iv2.y,2),"indexT2",indexT2);
            }
            
            var curvature1 = Curvature(trajectory1);
            var curvature2 = Curvature(trajectory2);
            var curvatureDifference = Math.Abs(curvature1 - curvature2);
            explanation += "Curvature: " + Math.Round(curvature1,3) + ", compared-curvature: " + Math.Round(curvature2,3);
            explanation += ", Angle-cost: " + Math.Round(totalAngleCost,1) + ", Position-cost: " + Math.Round(totalPositionCost) + ", Curvature-cost: " + Math.Round(curvatureDifference,3);
            var weightedAngleCost = totalAngleCost*ANGLE_MULTIPLIER;
            var weightedPositionCost = totalPositionCost*POSITION_MULTIPLIER;
            var weightedCurvatureCost = curvatureDifference * CURVATURE_MULTIPLIER;
            explanation += ", Weighted-angle-cost: " + Math.Round(weightedAngleCost,1) + ", Weighted-position-cost: " + Math.Round(weightedPositionCost,1) + ", Weighted-curvature-cost: " + Math.Round(weightedCurvatureCost,1);
            return explanation;
        }

        public static double Curvature(List<StateEstimate> trajectory)
        { 
            var curvature = 0.0;
            var firstStateEstimate = trajectory.First();
            TrajectoryVector ivfirst = new TrajectoryVector();

            ivfirst.x = firstStateEstimate.Vx;
            ivfirst.y = firstStateEstimate.Vy;

            var lastAngle = Math.Atan2(-ivfirst.y, ivfirst.x);
            var maxVelocityMagnitude = 1.0;

            for(int i=0;i<trajectory.Count;i++)
            {
                var trajectoryStateEstimate = trajectory[i];
                //var velocityUncertainty = Math.Sqrt(Math.Pow(trajectoryStateEstimate.CovVx,2) + Math.Pow(trajectoryStateEstimate.CovVy,2))/100.0;
                var velocityMagnitude = Math.Sqrt(Math.Pow(trajectoryStateEstimate.Vx,2) + Math.Pow(trajectoryStateEstimate.Vy,2));
                if(velocityMagnitude > maxVelocityMagnitude)
                {
                    maxVelocityMagnitude = velocityMagnitude;
                }
                TrajectoryVector iv = new TrajectoryVector();
                iv.x = trajectoryStateEstimate.Vx;
                iv.y = trajectoryStateEstimate.Vy;
                var angle = Math.Atan2(-iv.y, iv.x);
                var angle_diff = MathHelper.WrapAngle(angle - lastAngle);
                curvature += angle_diff*velocityMagnitude;
                lastAngle = angle;
            }

            curvature /= Math.Pow(maxVelocityMagnitude,2); //Normalize by maximum velocity
            return curvature;
        }

        public static double CompareAngles(TrajectoryVector v1, TrajectoryVector v2)
        {
            var angle1 = Math.Atan2(-v1.y, v1.x);
            var angle2 = Math.Atan2(-v2.y, v2.x);
            var angle_diff = Math.Abs(MathHelper.WrapAngle(angle1 - angle2));
            return angle_diff;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public static Movement BestMatchTrajectory(List<StateEstimate> matchTrajectory,
            List<Movement> trajectoryPrototypes, string classType)
        {
            var match = trajectoryPrototypes.First();
            var matchedTrajectories = new ConcurrentBag<MatchTrajectory>();
           
            Parallel.ForEach(trajectoryPrototypes, (tp) =>
            {
                var mt = new MatchTrajectory(tp.Approach, tp.Exit, tp.TrafficObjectType, tp.TurnType,tp.StateEstimates, DateTime.Now, 0, tp.Ignored);
                bool isValidPersonMatch = classType.ToLower() == "person" && tp.TrafficObjectType == ObjectType.Person;
                bool isValidVehicleMatch = classType.ToLower() != "person" && tp.TrafficObjectType != ObjectType.Person;
                if(isValidPersonMatch || isValidVehicleMatch)
                {
                    if (tp.TurnType == Turn.UTurn)
                    {
                        // 2.0 is just a rough heuristic to implement a 'u-turn likelihood prior'. 
                        // This is the simplest way of avoiding adding a new configuration parameter,
                        // while reducing the over-counting of U-turns without completely eliminating them.
                        mt.matchCost = 1.1*PathIntegralCost(matchTrajectory, mt.StateEstimates);
                    }
                    else
                    {
                        mt.matchCost = PathIntegralCost(matchTrajectory, mt.StateEstimates);
                    }
                    matchedTrajectories.Add(mt);
                }
            });

            var sortedMatchTrajectores = matchedTrajectories.ToList<MatchTrajectory>();
            sortedMatchTrajectores.Sort(new MatchTrajectoryComparer());
            match = sortedMatchTrajectores.FirstOrDefault();
            return match;
        }

        public static StateEstimate NearestPointOnTrajectory(StateEstimate point, List<StateEstimate> trajectory)
        {
            return trajectory.OrderBy(se => StateEstimatesDistance(point, se)).First();
        }

        static StateEstimate MostSimilarPointOnTrajectory(StateEstimate point, List<StateEstimate> trajectory)
        {
            return trajectory.OrderBy(se => InertialDistance(point, se)).First();
        }

        static double StateEstimatesDistance(StateEstimate point1, StateEstimate point2)
        {
            var distance = Math.Sqrt(Math.Pow(point1.X - point2.X,2) + Math.Pow(point1.Y - point2.Y,2));
            return distance;
        }

        static double InertialDistance(StateEstimate point1, StateEstimate point2)
        {
            var positionDistance = Math.Sqrt(Math.Pow(point1.X - point2.X,2) + Math.Pow(point1.Y - point2.Y,2));
            
            TrajectoryVector iv1 = new TrajectoryVector();
            TrajectoryVector iv2 = new TrajectoryVector();
            iv1.x = point1.Vx;
            iv1.y = point1.Vy;
            iv2.x = point2.Vx;
            iv2.y = point2.Vy;

            var angleDiff = CompareAngles(iv1, iv2);
            var velocityMagnitude = Math.Sqrt(Math.Pow(iv1.x,2) + Math.Pow(iv1.y,2));
            var thisPointCost = POSITION_MULTIPLIER*positionDistance + ANGLE_MULTIPLIER*angleDiff*velocityMagnitude;  
            return thisPointCost;
        }
    }

    public class MatchTrajectory : Movement
    {
        public double matchCost;

        public MatchTrajectory(string approach, string exit, ObjectType ot, Turn turn, StateEstimateList stateEstimates, DateTime timestamp, int frame, bool ignored) : base(approach,exit,turn,ot,stateEstimates, timestamp, ignored)
        { 
        }
    }

    class MatchTrajectoryComparer : IComparer<MatchTrajectory>
    {
        public int Compare(MatchTrajectory x, MatchTrajectory y)
        {
            return x.matchCost.CompareTo(y.matchCost);
        }
    }
}
