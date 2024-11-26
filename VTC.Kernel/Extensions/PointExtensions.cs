using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Common.RegionConfig;
using System.Collections.Generic;

namespace VTC.Kernel.Extensions
{
    public static class PointExtensions
    {
        public static double DistanceTo(this Point a, Point b)
        {
            var distanceX = Math.Abs(a.X - b.X);
            var distanceY = Math.Abs(a.Y - b.Y);

            var distance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2));

            return distance;
        }

        public static int NearestNeighborIndex(this Point point, IEnumerable<Point> neighbors)
        {
            // Assumes at least one neighbor

            int nearest = 0;
            double nearestDistance = double.MaxValue;
            var neighborsList = neighbors.ToList();

            for (int i = 0; i < neighborsList.Count; i++)
            {
                var dist = point.DistanceTo(neighborsList[i]);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearest = i;
                }
            }

            return nearest;
        }
    }
}
