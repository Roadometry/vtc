using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;
using VTC.Common.RegionConfig;

namespace VTC.Kernel
{
    public static class PolygonTrajectorySynthesizer
    {
        private const int NumberOfInterpolatedStepsSyntheticTrajectory = 20;

        public static TrackedObject SyntheticTrajectory(System.Drawing.Point approachVertex, System.Drawing.Point exitVertex, RoadLine approachRoadLine, RoadLine exitRoadLine)
        {
            var initialState = new StateEstimate();
            initialState.X = approachVertex.X;
            initialState.Y = approachVertex.Y;
            var p0 = new Point(Convert.ToInt32(initialState.X),Convert.ToInt32(initialState.Y));

            var finalState = new StateEstimate();
            finalState.X = exitVertex.X;
            finalState.Y = exitVertex.Y;
            var p2 = new Point(Convert.ToInt32(finalState.X), Convert.ToInt32(finalState.Y));

            var pApproachRoadlineMidpoint = new Point();
            pApproachRoadlineMidpoint.X = Convert.ToInt32((approachRoadLine.ApproachCentroidX + approachRoadLine.ExitCentroidX)/2);
            pApproachRoadlineMidpoint.Y = Convert.ToInt32((approachRoadLine.ApproachCentroidY + approachRoadLine.ExitCentroidY)/2);

            var pExitRoadlineMidpoint = new Point();
            pExitRoadlineMidpoint.X = Convert.ToInt32((exitRoadLine.ApproachCentroidX + exitRoadLine.ExitCentroidX)/2);
            pExitRoadlineMidpoint.Y = Convert.ToInt32((exitRoadLine.ApproachCentroidY + exitRoadLine.ExitCentroidY)/2);

            var p1 = new Point();
            p1.X = Convert.ToInt32((pApproachRoadlineMidpoint.X + pExitRoadlineMidpoint.X)/2);
            p1.Y = Convert.ToInt32((pApproachRoadlineMidpoint.Y + pExitRoadlineMidpoint.Y)/2);
            //p1 = Intersection(approachRoadLine, exitRoadLine);            

            TrackedObject syntheticTrajectory = new TrackedObject(initialState,0);

            for (int i = 1; i < NumberOfInterpolatedStepsSyntheticTrajectory -1; i++)
            {
                var lastPoint = syntheticTrajectory.StateHistory.Last();
                var u = Convert.ToDouble(i) / NumberOfInterpolatedStepsSyntheticTrajectory;
                var p = QuadInterpolated(p0, p1, p2, u);
                var derivative = QuadInterpolatedDerivative(p0,p1,p2,u);
                var se = new StateEstimate();
                se.X = p.X;
                se.Y = p.Y;
                se.Vx = derivative.X;
                se.Vy = derivative.Y;
                syntheticTrajectory.StateHistory.Add(se); 
            }

            syntheticTrajectory.StateHistory.Add(finalState);

            syntheticTrajectory.StateHistory[0].Vx = syntheticTrajectory.StateHistory[1].Vx;
            syntheticTrajectory.StateHistory[0].Vy = syntheticTrajectory.StateHistory[1].Vy;

            syntheticTrajectory.StateHistory.Last().Vx = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vx;
            syntheticTrajectory.StateHistory.Last().Vy = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vy;

            return syntheticTrajectory;
        }

        public static TrackedObject SyntheticTrajectoryCircleSegment(System.Drawing.Point approachVertex, System.Drawing.Point exitVertex, RoadLine approachRoadLine, RoadLine exitRoadLine)
        {
            Console.WriteLine("--SyntheticTrajectory--");
            Console.WriteLine("ApproachVertex:("+approachVertex.X+","+approachVertex.Y+")");
            Console.WriteLine("ExitVertex:("+exitVertex.X+","+exitVertex.Y+")");
            Console.WriteLine("ApproachRoadline:("+approachRoadLine.ApproachCentroidX+","+approachRoadLine.ApproachCentroidY+") to ("+approachRoadLine.ExitCentroidX+","+approachRoadLine.ExitCentroidY+")");
            Console.WriteLine("ExitRoadLine:("+exitRoadLine.ApproachCentroidX+","+exitRoadLine.ApproachCentroidY+") to ("+exitRoadLine.ExitCentroidX+","+exitRoadLine.ExitCentroidY+")");


            var initialState = new StateEstimate();
            initialState.X = approachVertex.X;
            initialState.Y = approachVertex.Y;
            var p0 = new Point(Convert.ToInt32(initialState.X),Convert.ToInt32(initialState.Y));

            var finalState = new StateEstimate();
            finalState.X = exitVertex.X;
            finalState.Y = exitVertex.Y;
            var p2 = new Point(Convert.ToInt32(finalState.X), Convert.ToInt32(finalState.Y));

            var dx = (approachRoadLine.ExitCentroidX - approachRoadLine.ApproachCentroidX)*0.1;
            var dy = (approachRoadLine.ExitCentroidY - approachRoadLine.ApproachCentroidY)*0.1;
            var p1 = new Point();
            p1.X = Convert.ToInt32(approachVertex.X + dx);
            p1.Y = Convert.ToInt32(approachVertex.Y + dy);

            TrackedObject syntheticTrajectory = new TrackedObject(initialState,0);

            var c = new Circle(p0,p1,p2);
            var a1 = c.AngleFromPoint(p1);
            var a2 = c.AngleFromPoint(p2);
            var d = c.TurnDirection(p0,p1);

            var angleDelta = 0.0;

            if(d == Circle.Direction.Clockwise)
            {
                
            }
            else if (d == Circle.Direction.Counterclockwise)
            {

            }

            syntheticTrajectory.StateHistory.Add(finalState);

            syntheticTrajectory.StateHistory[0].Vx = syntheticTrajectory.StateHistory[1].Vx;
            syntheticTrajectory.StateHistory[0].Vy = syntheticTrajectory.StateHistory[1].Vy;

            syntheticTrajectory.StateHistory.Last().Vx = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vx;
            syntheticTrajectory.StateHistory.Last().Vy = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vy;

            return syntheticTrajectory;
        }

        public static TrackedObject SyntheticTrajectory(System.Drawing.Point approachVertex, System.Drawing.Point exitVertex, RoadLine approachRoadLine)
        {
            var initialState = new StateEstimate();
            initialState.X = approachVertex.X;
            initialState.Y = approachVertex.Y;
            var p0 = new Point(Convert.ToInt32(initialState.X),Convert.ToInt32(initialState.Y));

            var finalState = new StateEstimate();
            finalState.X = exitVertex.X;
            finalState.Y = exitVertex.Y;
            var p2 = new Point(Convert.ToInt32(finalState.X), Convert.ToInt32(finalState.Y));

            var p1 = new Point();

            //Use a straight-line approximation
            p1.X = Convert.ToInt32((approachRoadLine.ApproachCentroidX + approachRoadLine.ExitCentroidX) / 2);
            p1.Y = Convert.ToInt32((approachRoadLine.ApproachCentroidY + approachRoadLine.ExitCentroidY) / 2);

            TrackedObject syntheticTrajectory = new TrackedObject(initialState,0);

            for (int i = 1; i < NumberOfInterpolatedStepsSyntheticTrajectory - 1; i++)
            {
                var se = new StateEstimate();
                se.X = initialState.X + ((double)i/(NumberOfInterpolatedStepsSyntheticTrajectory-1)) * (finalState.X - initialState.X);
                se.Y = initialState.Y + ((double)i/(NumberOfInterpolatedStepsSyntheticTrajectory-1)) * (finalState.Y - initialState.Y);
                se.Vx = se.X - syntheticTrajectory.StateHistory.Last().X;
                se.Vy = se.Y - syntheticTrajectory.StateHistory.Last().Y;
                syntheticTrajectory.StateHistory.Add(se);
            }

            syntheticTrajectory.StateHistory.Add(finalState);

            syntheticTrajectory.StateHistory[0].Vx = syntheticTrajectory.StateHistory[1].Vx;
            syntheticTrajectory.StateHistory[0].Vy = syntheticTrajectory.StateHistory[1].Vy;
            syntheticTrajectory.StateHistory.Last().Vx = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vx;
            syntheticTrajectory.StateHistory.Last().Vy = syntheticTrajectory.StateHistory[syntheticTrajectory.StateHistory.Count - 2].Vy;

            return syntheticTrajectory;
        }

        public static Point Intersection(RoadLine approachLine, RoadLine exitLine)
        {
            var roadIntersection = new Point();

            var x1 = approachLine.ApproachCentroidX;
            var y1 = approachLine.ApproachCentroidY;
            var x2 = approachLine.ExitCentroidX;
            var y2 = approachLine.ExitCentroidY;

            var x3 = exitLine.ApproachCentroidX;
            var y3 = exitLine.ApproachCentroidY;
            var x4 = exitLine.ExitCentroidX;
            var y4 = exitLine.ExitCentroidY;

            roadIntersection.X = Convert.ToInt32(((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
            roadIntersection.Y = Convert.ToInt32(((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

            return roadIntersection;
        }

        public static TrackedObject PopulateTrajectoryVelocities(TrackedObject trackedObject, int iterations)
        {
            for(int i=0; i<iterations;i++)
            {
                for(int j=0;j<trackedObject.StateHistory.Count;j++)
                {
                    var estimatedDerivative = EstimateDerivate(trackedObject,j);
                    trackedObject.StateHistory[j].Vx = estimatedDerivative.x;
                    trackedObject.StateHistory[j].Vy = estimatedDerivative.y;
                }
            }
            return trackedObject;
        }

        public static TrajectoryVector EstimateDerivate(TrackedObject trackedObject, int index)
        {
            TrajectoryVector derivativeEstimate = new TrajectoryVector();
            var centerDerivative = CenterDerivative(trackedObject, index);

            derivativeEstimate.x = centerDerivative.x;
            derivativeEstimate.y = centerDerivative.y;

            return derivativeEstimate;
        }

        //Calculate derivative (velocity) using forward and backward velocities, if possible
        public static TrajectoryVector CenterDerivative(TrackedObject trackedObject, int index)
        {
            TrajectoryVector vector = new TrajectoryVector();
            vector.x = 0;
            vector.y = 0;
            int numPointsUsed = 0;
            
            //If exists, get following point
            if(index + 1 < trackedObject.StateHistory.Count)
            {
                numPointsUsed++;
                var forwards = ForwardDerivative(trackedObject,index);
                vector.x += forwards.x;
                vector.y += forwards.y;
            }

            //If exists, get previous point
            if(index - 1 >= 0)
            {
                numPointsUsed++;
                var backwards = BackwardDerivative(trackedObject,index);
                vector.x += backwards.x;
                vector.y += backwards.y;
            }

            vector.x = vector.x / numPointsUsed;
            vector.y = vector.x / numPointsUsed;

            return vector;
        }

        //Get average of forward and backward neighbor's velocities, if possible
        public static TrajectoryVector NeighborsVelocity(TrackedObject trackedObject, int index)
        {
            TrajectoryVector neighborsVelocity = new TrajectoryVector();

            int numPointsUsed = 0;
            
            //If exists, get following point
            if(index + 1 < trackedObject.StateHistory.Count)
            {
                numPointsUsed++;
                var nextPoint = trackedObject.StateHistory[index + 1];
                neighborsVelocity.x += nextPoint.Vx;
                neighborsVelocity.y += nextPoint.Vy;
            }

            //If exists, get previous point
            if(index - 1 > 0)
            {
                numPointsUsed++;
                var previousPoint = trackedObject.StateHistory[index - 1];
                neighborsVelocity.x += previousPoint.Vx;
                neighborsVelocity.y += previousPoint.Vy;
            }

            neighborsVelocity.x /= numPointsUsed;
            neighborsVelocity.y /= numPointsUsed;

            return neighborsVelocity;
        }

        public static TrajectoryVector ForwardDerivative(TrackedObject trackedObject, int index)
        {
            TrajectoryVector vector = new TrajectoryVector();
            StateEstimate thisPoint = trackedObject.StateHistory[index];
            StateEstimate nextPoint = trackedObject.StateHistory[index+1];
            var forwardVx = nextPoint.X - thisPoint.X;
            var forwardVy = nextPoint.Y - thisPoint.Y;
            vector.x = forwardVx;
            vector.y = forwardVy;
            return vector;
        }

        public static TrajectoryVector BackwardDerivative(TrackedObject trackedObject, int index)
        {
            TrajectoryVector vector = new TrajectoryVector();
            StateEstimate thisPoint = trackedObject.StateHistory[index];
            StateEstimate previousPoint = trackedObject.StateHistory[index - 1];
            var backwardVx = thisPoint.X - previousPoint.X;
            var backwardVy = thisPoint.Y - previousPoint.Y;
            vector.x = backwardVx;
            vector.y = backwardVx;
            return vector;
        }

        /// <summary>
        /// 2nd-order (quadratic) Bezier interpolation of two lines
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Point QuadInterpolated(Point p0, Point p1, Point p2, double u)
        {
            var b02 = new Point();

            double b01_X, b01_Y, b11_X, b11_Y;

            b01_X = (1 - u) * p0.X + u * p1.X;
            b01_Y = (1 - u) * p0.Y + u * p1.Y;
            b11_X = (1 - u) * p1.X + u * p2.X;
            b11_Y = (1 - u) * p1.Y + u * p2.Y;

            b02.X = Convert.ToInt32((1 - u) * b01_X + u * b11_X);
            b02.Y = Convert.ToInt32((1 - u) * b01_Y + u * b11_Y);

            return b02;
        }

        /// <summary>
        /// 2nd-order (quadratic) Bezier interpolation of two lines
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Point QuadInterpolatedDerivative(Point p0, Point p1, Point p2, double u)
        {
            var b02 = new Point();

            double b01_X, b01_Y, b11_X, b11_Y;

            b01_X = (1 - u) * p0.X + u * p1.X;
            b01_Y = (1 - u) * p0.Y + u * p0.Y;
            b11_X = (1 - u) * p1.X + u * p2.X;
            b11_Y = (1 - u) * p1.Y + u * p2.Y;

            b02.X = Convert.ToInt32(b11_X - b01_X);
            b02.Y = Convert.ToInt32(b11_Y - b01_Y);

            return b02;
        }

    }

    public class RoadLine
    {
        public double ApproachCentroidX;
        public double ApproachCentroidY;

        public double ExitCentroidX;
        public double ExitCentroidY;

        public Point Interpolate(double u)
        {
            var p = new Point();

            p.X = Convert.ToInt32((1 - u) * ApproachCentroidX + u * ExitCentroidX);
            p.Y = Convert.ToInt32((1 - u) * ApproachCentroidY + u * ExitCentroidY);

            return p;
        }
    }

    public class Circle
    {
        public double Xc;
        public double Yc;
        public double R;

        public enum Direction
        {
            Clockwise,
            Counterclockwise,
            None
        };

        // Taken from:
        // http://csharphelper.com/blog/2016/09/draw-a-circle-through-three-points-in-c/
        public Circle(Point a, Point b, Point c)
        {
            // Get the perpendicular bisector of (x1, y1) and (x2, y2).
            float x1 = (b.X + a.X) / 2;
            float y1 = (b.Y + a.Y) / 2;
            float dy1 = b.X - a.X;
            float dx1 = -(b.Y - a.Y);

            // Get the perpendicular bisector of (x2, y2) and (x3, y3).
            float x2 = (c.X + b.X) / 2;
            float y2 = (c.Y + b.Y) / 2;
            float dy2 = c.X - b.X;
            float dx2 = -(c.Y - b.Y);

            // See where the lines intersect.
            bool lines_intersect, segments_intersect;
            PointF intersection, close1, close2;
            FindIntersection(
                new PointF(x1, y1), new PointF(x1 + dx1, y1 + dy1),
                new PointF(x2, y2), new PointF(x2 + dx2, y2 + dy2),
                out lines_intersect, out segments_intersect,
                out intersection, out close1, out close2);
            if (!lines_intersect)
            {
                Xc = 0;
                Yc = 0;
                R = 0;
            }
            else
            {
                Xc = intersection.X;
                Yc = intersection.Y;
                float dx = (float) Xc - a.X;
                float dy = (float) Yc- a.Y;
                R = (float)Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public double AngleFromPoint(Point p)
        {
            double angle = Math.Atan2(p.Y - Yc, p.X - Xc);
            return angle;
        }

        public Direction TurnDirection(Point p1, Point p2)
        {
            Direction d = Direction.None;

            var a1 = AngleFromPoint(p1);
            var a2 = AngleFromPoint(p2);

            if(a2>a1)
            {
                d = Direction.Clockwise;
            }
            else if (a1 < a2)
            {
                d = Direction.Counterclockwise;
            }

            return d;
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        // Taken from:
        // http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
        private static void FindIntersection(
        PointF p1, PointF p2, PointF p3, PointF p4,
        out bool lines_intersect, out bool segments_intersect,
        out PointF intersection,
        out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }

    }
}
