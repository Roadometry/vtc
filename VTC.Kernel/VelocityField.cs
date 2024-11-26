using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Kernel.Extensions;

namespace VTC.Kernel
{
    public class VelocityField
    {
        public struct Velocity
        {
            public double Vx;
            public double Vy;

            public Velocity(double x, double y)
            {
                Vx = x;
                Vy = y;
            }
        }

        private readonly Velocity[,] _velocityField;
        private readonly int _fieldHeight;
        private readonly int _fieldWidth;
        private readonly int _sourceWidth;
        private readonly int _sourceHeight;
        private const double Alpha = 0.001;
        private const int DistanceThreshold = 6;
        private readonly Mutex _updateMutex = new Mutex();

        public Image<Gray, Byte> ProjectedPointsImage;

        public VelocityField(int sourceWidth, int sourceHeight)
        {
            _fieldWidth = 50;
            _fieldHeight = 50;
            _sourceHeight = sourceHeight;
            _sourceWidth = sourceWidth;

            _velocityField = new Velocity[_fieldWidth, _fieldHeight];
            ProjectedPointsImage = new Image<Gray, byte>(_fieldWidth, _fieldHeight, new Gray(0));
        }

        public Velocity GetAvgVelocity(int x, int y)
        {
            GetNormalizedCoordinate(x, y, out x, out y);
            return _velocityField[x, y];
        }

        public Velocity GetAvgVelocity(Point p)
        {
            return GetAvgVelocity(p.X, p.Y);
        }

        private void GetNormalizedCoordinate(int x, int y, out int xNormal, out int yNormal)
        {
            // Cache the normalized values to reduce multiplication and division
            //if (!_horizontalCoordLookup.ContainsKey(x))
            //{
            //    var value = (x * _fieldWidth) / _sourceWidth;
            //    value = Math.Min(value, _fieldWidth-1);
            //    _horizontalCoordLookup[x] = value;
            //}

            //xNormal = _horizontalCoordLookup[x];
            xNormal = (x * _fieldWidth) / _sourceWidth;
            xNormal = Math.Min(xNormal, _fieldWidth - 1);
            xNormal = Math.Max(xNormal, 0);

            //if (!_verticalCoordLookup.ContainsKey(y))
            //{
            //    var value = (y * _fieldHeight) / _sourceHeight;
            //    value = Math.Min(value, _fieldHeight-1);
            //    _horizontalCoordLookup[y] = value;
            //}

            //yNormal = _horizontalCoordLookup[y];
            yNormal = (y * _fieldHeight) / _sourceHeight;
            yNormal = Math.Min(yNormal, _fieldHeight - 1);
            yNormal = Math.Max(yNormal, 0);
        }

        private void InsertVelocities(Dictionary<Point, Velocity> measurements)
        {
            // If we're already busy inserting velocities, just abort
            if (!_updateMutex.WaitOne(0))
            {
                return;
            }

            try
            {
                var neighbors = new List<Point>();
                var velocities = new List<Velocity>();
                ProjectedPointsImage = new Image<Gray, byte>(_fieldWidth, _fieldHeight, new Gray(0));

                // Since the velocity field grid is smaller that the source image, we need to
                // Normalize the measurement coordinates
                foreach (var kvp in measurements)
                {
                    var pt = kvp.Key;
                    var vel = kvp.Value;

                    int x, y;
                    GetNormalizedCoordinate(pt.X, pt.Y, out x, out y);

                    neighbors.Add(new Point(x,y));
                    velocities.Add(vel);
                }

                if (!neighbors.Any())
                    return;

                var point = new Point();
                for (int x = 0; x < _fieldWidth; x++)
                {
                    point.X = x;
                    for (int y = 0; y < _fieldHeight; y++)
                    {
                        point.Y = y;

                        var nearest = point.NearestNeighborIndex(neighbors);

                        if (point.DistanceTo(neighbors[nearest]) < DistanceThreshold)
                        {
                            var distanceDivisor = point.DistanceTo(neighbors[nearest]) > 1
                                ? point.DistanceTo(neighbors[nearest])
                                : 1;

                            var vx = _velocityField[x, y].Vx * (1 - Alpha) / distanceDivisor;
                            vx += (velocities[nearest].Vx * Alpha);

                            var vy = _velocityField[x, y].Vy * (1 - Alpha) / distanceDivisor;
                            vy += (velocities[nearest].Vy * Alpha);

                            _velocityField[x, y].Vx = vx;
                            _velocityField[x, y].Vy = vy;    
                        }
                        else
                        {
                            _velocityField[x, y].Vx = _velocityField[x, y].Vx*(1-Alpha);
                            _velocityField[x, y].Vy = _velocityField[x, y].Vy*(1-Alpha);    
                        }
                    }
                }

                foreach (var v in neighbors)
                    ProjectedPointsImage.Draw(new CircleF(new PointF(v.X, v.Y), 1), new Gray(255));
            }
            finally
            {
                _updateMutex.ReleaseMutex();
            }
        }

        internal void TryInsertVelocitiesAsync(Dictionary<Point, Velocity> measurements)
        {
            Task.Factory.StartNew(() => InsertVelocities(measurements));
        }

        public void Draw<TColor, TDepth>(Image<TColor, TDepth> image, TColor color, int thickness) 
            where TColor : struct, IColor 
            where TDepth : new()
        {
            _updateMutex.WaitOne();
            try
            {
                var segmentWidth = image.Width/_fieldWidth;
                var segmentHeight = image.Height/_fieldHeight;

                var cursorStart = new Point(segmentWidth/2, segmentHeight/2);

                for (int x = 0; x < _fieldWidth; x++)
                {
                
                    for (int y = 0; y < _fieldHeight; y++)
                    {
                        var cursorEnd = new Point(
                            (int) (cursorStart.X + _velocityField[x, y].Vx),
                            (int) (cursorStart.Y + _velocityField[x, y].Vy)
                            );

                        var line = new LineSegment2D(cursorStart, cursorEnd);

                        if (line.Length > 1)
                        {
                            image.Draw(line, color, thickness);
                            image.Draw(new CircleF(cursorStart, 1), color, thickness);    
                        }

                        cursorStart.Y += segmentHeight;
                    }

                    cursorStart.Y = segmentHeight / 2;
                    cursorStart.X += segmentWidth;
                }
            }
            finally
            {
                _updateMutex.ReleaseMutex();
            }
        }

        public void Draw<TColor, TDepth>(Image<TColor, TDepth> image, TColor color, int thickness, int[][][] field, int localSegmentWidth, int localSegmentHeight)
            where TColor : struct, IColor
            where TDepth : new()
        {
            int multiplier = 10;

            _updateMutex.WaitOne();
            try
            {
                var localfieldWidth = field.GetLength(0);
                var localfieldHeight = field[0].GetLength(0);
                //var segmentWidth = image.Width / _localfieldWidth;
                //var segmentHeight = image.Height / _localfieldHeight;
                //var segmentWidth = 10;  //TODO: Set this to Optical Flow downsample limit
                //var segmentHeight = 10;

                var cursorStart = new Point(localSegmentWidth / 2, localSegmentHeight / 2);

                for (int x = 0; x < localfieldWidth; x += localSegmentWidth)
                {

                    for (int y = 0; y < localfieldHeight; y += localSegmentHeight)
                    {
                        var cursorEnd = new Point(
                            cursorStart.X + multiplier * field[x][y][0],
                            cursorStart.Y + multiplier * field[x][y][1]
                            );

                        var line = new LineSegment2D(cursorStart, cursorEnd);

                        if (line.Length > 1)
                        {
                            image.Draw(line, color, thickness);
                            image.Draw(new CircleF(cursorStart, 1), color, thickness);
                        }

                        cursorStart.Y += localSegmentHeight;
                    }

                    cursorStart.Y = localSegmentHeight / 2;
                    cursorStart.X += localSegmentWidth;
                }
            }
            finally
            {
                _updateMutex.ReleaseMutex();
            }
        }
    } 
}
