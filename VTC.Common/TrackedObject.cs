using System;
using System.Collections.Generic;
using System.Linq;

namespace VTC.Common
{
    //TODO: Reconsider whether this should be a struct or class. Not sure why it was originally declared as a struct.
    public struct TrackedObject
    {
        public string ObjectType;
        public StateEstimateList StateHistory;
        public int FirstDetectionFrame;

        public TrackedObject(StateEstimate initialState, int frame)
        {
            StateHistory = new StateEstimateList { initialState};
            ObjectType = "unknown";
            FirstDetectionFrame = frame;
        }

        public TrackedObject(IEnumerable<StateEstimate> stateHistoryOld, StateEstimate currentState, int frame)
        {
            StateHistory = new StateEstimateList();
            StateHistory.AddRange(stateHistoryOld);
            StateHistory.Add(currentState);
            ObjectType = "unknown";
            FirstDetectionFrame = frame;
        }

        public double PathLengthIntegral()
        {
            double distance_integral = 0;
            StateEstimate current_position = StateHistory.First();
            for (int i = 0; i < StateHistory.Count(); i++)
            {
                var next_position = StateHistory.ElementAt(i);
                var x_increment = next_position.X - current_position.X;
                var y_increment = next_position.Y - current_position.Y;
                var total_increment = Math.Sqrt(Math.Pow(x_increment, 2) + Math.Pow(y_increment, 2));
                distance_integral += total_increment;
                current_position = next_position;
            }

            return distance_integral;
        }

        public double NetMovement()
        {
            StateEstimate initialPosition = StateHistory.First();
            StateEstimate finalPosition = StateHistory.Last();
            var distance = Math.Sqrt(Math.Pow(finalPosition.X - initialPosition.X,  2) + Math.Pow(finalPosition.Y - initialPosition.Y,2));
            return distance;
        }

        public double MissRatio()
        {
            var missRatio = (double) StateHistory.Sum(se => se.TotalMissedDetections) / StateHistory.Count();
            return missRatio;
        }

        public double FinalPositionCovariance()
        {
            var finalStateEstimate = StateHistory.Last();
            var covariance = Math.Sqrt( Math.Pow(finalStateEstimate.CovX,2) + Math.Pow(finalStateEstimate.CovX,2));
            return covariance;
        }
    }
}