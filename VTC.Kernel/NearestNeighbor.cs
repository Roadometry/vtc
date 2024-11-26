using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VTC.Kernel
{
    public class NearestNeighbor
    {
        private static readonly double _sqrt2 = 1.41421356237;

        public static Point FindNearestNeighbor(Point origin, IEnumerable<Point> neighbors, int imageWidth, int imageHeight)
        {
            bool match = false;
            int i = 0;
            while (!match)
            {
                var value = neighbors.FirstOrDefault()

                i++;
            }

            var minnn = neighbors.Min(n => Math.Min(Math.Abs(n.X - origin.X),Math.Abs(n.Y - origin.Y)));
            var delta = _sqrt2 * minnn;

            var leftInnerX = origin.X - minnn;
            var rightInnerX = origin.X + minnn;
            var leftOuterX = origin.X - delta;
            var rightOuterX = origin.X + delta;
            
            var bottomInnerY = origin.Y - minnn;
            var topInnerY = origin.Y + minnn;
            var bottomOuterY = origin.Y - delta;
            var topOuterY = origin.Y + delta;


            var toTest = neighbors.Where(n =>
            {
                if (n.X < leftOuterX) return false;
                if (n.X > rightOuterX) return false;
                if (n.Y < bottomOuterY) return false;
                if (n.Y > topOuterY) return false;

                if (n.X <= leftInnerX) return true;
                if (n.X >= rightInnerX) return true;
                if (n.Y <= bottomInnerY) return true;
                if (n.Y >= topInnerY) return true;

                return true;
            });

                //int rectMinX = origin.X;
                //int rectMaxX = origin.X;
                //int rectMinY = origin.Y;
                //int rectMaxY = origin.Y;

                //var overlap = neighbors.FirstOrDefault(n => n.X == origin.X && n.Y == origin.Y);
                //if (null != overlap && (overlap.X != 0 || overlap.Y != 0))
                //    return overlap;

                //// We will cut down the amount of work to do by using the knowledge that once we find one matching
                //// point on the perimiter of a rectangle centered on the origin point, the closest neighbor must be
                //// either that point on the perimiter, or within (sqrt(2) * (1 dimensional distance to perimeter of the rectangle))


                //var distToRect = 0;
                //int distToRectMin, distToRectMax;
                //while (true)
                //{
                //    // Increment rectangle size
                //    IncrementRectangleSize(ref distToRect, ref rectMinX, ref rectMaxX, ref rectMinY, ref rectMaxY);

                //    if (neighbors.Any(n => PointOnHorizLine(n, rectMinX, rectMaxX, rectMinY))) 
                //        break;

                //    if (neighbors.Any(n => PointOnHorizLine(n, rectMinX, rectMaxX, rectMaxY)))
                //        break;

                //    if (neighbors.Any(n => PointOnVertLine(n, rectMinY, rectMaxY, rectMinX)))
                //        break;

                //    if (neighbors.Any(n => PointOnVertLine(n, rectMinY, rectMaxY, rectMaxX)))
                //        break;
                //}

                //distToRectMin = distToRect;
                //distToRectMax = (int)Math.Ceiling(_sqrt2 * distToRectMin);

                //int minX = origin.X - distToRectMax;
                //int maxX = origin.X + distToRectMax;
                //int minY = origin.Y - distToRectMax;
                //int maxY = origin.Y + distToRectMax;
                //var toTest = neighbors.ToList();

                //// Remove anything outside the big rectangle
                //toTest.RemoveAll(n =>
                //{
                //    if (n.X < minX) return true;
                //    if (n.X > maxX) return true;
                //    if (n.Y < minY) return true;
                //    if (n.Y > maxY) return true;

                //    return false;
                //});

                //// Remove anything inside the small rectangle
                //minX = origin.X - distToRectMin;
                //maxX = origin.X + distToRectMin;
                //minY = origin.Y - distToRectMin;
                //maxY = origin.Y + distToRectMin;

                //toTest.RemoveAll(n =>
                //{
                //    if (n.X <= minX) return false;
                //    if (n.X >= maxX) return false;
                //    if (n.Y <= minY) return false;
                //    if (n.Y >= maxY) return false;

                //    return true;
                //});

            Point min = toTest.First();
            int minDist = int.MaxValue;
            foreach (var n in toTest)
            {
                var x = Math.Abs(n.X - origin.X);
                var y = Math.Abs(n.Y - origin.Y);

                var dist = x * x + y * y;

                if (dist < minDist)
                {
                    minDist = dist;
                    min = n;
                }
            }

                return min;
        }

        private static bool PointOnHorizLine(Point p, int lineBeginX, int lineEndX, int lineY)
        {
            if (p.Y != lineY) return false;

            if (p.X < lineBeginX) return false;

            if (p.X > lineEndX) return false;

            return true;
        }

        private static bool PointOnVertLine(Point p, int lineBeginY, int lineEndY, int lineX)
        {
            if (p.X != lineX) return false;

            if (p.Y < lineBeginY) return false;

            if (p.Y > lineEndY) return false;

            return true;
        }

        private static void IncrementRectangleSize(ref int distance, ref int x1, ref int x2, ref int y1, ref int y2)
        {
            distance++;
            x1--;
            y1--;
            x2++;
            y2++;
        }
    }
}
