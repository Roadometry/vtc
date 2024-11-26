using System;
using System.Collections.Generic;
using Emgu.CV.Structure;

namespace VTC.Kernel.Extensions
{
    public static class LineSegment2DExtensions
    {
        public static void EquationOfLine(this LineSegment2D line, out double slope, out double yIntercept)
        {
            double dy = line.P2.Y - line.P1.Y;
            double dx = line.P2.X - line.P1.X;

            slope = dy / dx;

            // y=mx+b, b = y-mx
            yIntercept = line.P2.Y - (slope * line.P2.X);
        }

        public static bool Crosses(this LineSegment2D line1, LineSegment2D line2)
        {
            if (line1.Equals(line2)) return true;

            double m1, m2, b1, b2;
            EquationOfLine(line1, out m1, out b1);
            EquationOfLine(line2, out m2, out b2);

            var minX1 = Math.Min(line1.P1.X, line1.P2.X);
            var maxX1 = Math.Max(line1.P1.X, line1.P2.X);
            var minX2 = Math.Min(line2.P1.X, line2.P2.X);
            var maxX2 = Math.Max(line2.P1.X, line2.P2.X);

            var minY1 = Math.Min(line1.P1.Y, line1.P2.Y);
            var maxY1 = Math.Max(line1.P1.Y, line1.P2.Y);
            var minY2 = Math.Min(line2.P1.Y, line2.P2.Y);
            var maxY2 = Math.Max(line2.P1.Y, line2.P2.Y);

            // Handle special cases:


            // Both lines vertical
            if (double.IsInfinity(m1) && double.IsInfinity(m2))
            {
                // Parallel but not overlapping
                if (line1.P1.X != line2.P1.X) return false;

                // Segments on same x, but don't overlap in y
                if (maxY1 <= minY2) return false;
                if (maxY2 <= minY1) return false;
                
                return true;
            }

            // Only line1 is vertical 
            if (double.IsInfinity(m1))
            {
                var line1X = line1.P1.X;

                // If line2 does not occur in the x range of line1
                if (line1X >= maxX2) return false;
                if (line1X <= minX2) return false;

                // Solve line2 for x of line1.  If the y value is within line1, they intersect
                var y = m2 * line1X + b2;
                y = Math.Round(y, 0);

                if (y > minY1 && y < maxY1) return true;

                return false;
            }
            
            // Only line2 vertical
            if (double.IsInfinity(m2))
            {
                var line2X = line2.P1.X;

                // If line1 does not occur in the x range of line2
                if (line2X >= maxX1) return false;
                if (line2X <= minX1) return false;

                // Solve line1 for x of line2.  If the y value is within line2, they intersect
                var y = m1 * line2.P1.X + b1;
                y = Math.Round(y, 0);

                if (y > minY2 && y < maxY2) return true;

                return false;
            }

            if (m1 == m2)
            {
                // Lines are parallel and possibly horizontal
                
                // If they don't exist in the same x regions, they don't overlap:
                if (maxX1 <= minX2 || maxX2 <= minX1) return false;
                
                // Exist in same x regions.  Check if the y intercept values line up to see if they overlap
                if (b1 != b2) return false;

                return true;
            }


            // Equate both lines and solve for x
            // m1 * x + b1 = m2 * x + b2
            // x = (b2-b1)/(m1-m2)
            var x = (b2 - b1) / (m1 - m2);
            x = Math.Round(x, 0);

            // Ignore start point if it is on the other line
            if (x <= minX1) return false;
            if (x <= minX2) return false;
            if (x >= maxX1) return false;
            if (x >= maxX2) return false;
            

            return true;
        }

        public static bool CrossesAny(this LineSegment2D line, IEnumerable<LineSegment2D> collection)
        {
            foreach (var l in collection)
            {
                if (line.Equals(l)) continue;

                if (line.Crosses(l)) return true;
            }

            return false;
        }
    }
}
