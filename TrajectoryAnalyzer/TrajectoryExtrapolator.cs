using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;

namespace TrajectoryAnalyzer
{
    static class TrajectoryExtrapolator
    {
        public static List<StateEstimate> ExtrapolatedTrajectory(List<StateEstimate> trajectory, int xmax, int ymax)
        {
            var extrapolated_trajectory = new List<StateEstimate>();

            //Extrapolate arrival
            var m1 = trajectory.First();
            var firstSampleIndex = 5;
            if (trajectory.Count < 10)
            {
                firstSampleIndex = 2;
            }
            var m2 = trajectory[firstSampleIndex];
            var m0 = ExtrapolatePreviousMeasurement(m1, m2, xmax, ymax);

            //Extrapolate exit
            var m4 = trajectory.Last();
            var previousSampleIndex = trajectory.Count - 5;
            if (trajectory.Count < 10)
            {
                previousSampleIndex = trajectory.Count - 2;
            }
            var m3 = trajectory[previousSampleIndex];
            var m5 = ExtrapolateNextMeasurement(m3, m4, xmax, ymax);

            extrapolated_trajectory.Add(m0);
            foreach (var m in trajectory)
            {
                extrapolated_trajectory.Add(m);
            }
            extrapolated_trajectory.Add(m5);

            return extrapolated_trajectory;
        }


        static StateEstimate ExtrapolatePreviousMeasurement(StateEstimate m1, StateEstimate m2, int xmax, int ymax)
        {
            var m0 = new StateEstimate();
            var vxInitial = m2.X - m1.X;
            var vyInitial = m2.Y - m1.Y;

            if (vxInitial == 0.0 && vyInitial == 0.0)
            {
                return m1;
            }

            m0.X = m1.X;
            m0.Y = m1.Y;

            while (m0.X > 0 && m0.Y > 0 && m0.X < xmax && m0.Y < ymax)
            {
                m0.X -= vxInitial;
                m0.Y -= vyInitial;
            }

            return m0;
        }

        static StateEstimate ExtrapolateNextMeasurement(StateEstimate m1, StateEstimate m2, int xmax, int ymax)
        {
            var m3 = new StateEstimate();
            var vxInitial = m2.X - m1.X;
            var vyInitial = m2.Y - m1.Y;

            if (vxInitial == 0.0 && vyInitial == 0.0)
            {
                return m2;
            }

            m3.X = m2.X;
            m3.Y = m2.Y;

            while (m3.X > 0 && m3.Y > 0 && m3.X < xmax && m3.Y < ymax)
            {
                m3.X += vxInitial;
                m3.Y += vyInitial;
            }

            return m3;
        }

    }
}
