using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Kernel.Extensions;
using VTC.Common.RegionConfig;
using NLog;

namespace VTC.RegionConfiguration
{
    public partial class PolygonBuilderControl : UserControl
    {
        public event EventHandler OnDoneClicked;
        public event EventHandler OnCancelClicked;

        private readonly Image<Bgr, float> _startImage;
        private Point? _mouseDownLocation ;

        public Polygon Coordinates = new Polygon();

        public int CircleRadius = 5;
        public int LineDrawWidth = 2;

        public Bgr IntersectionColor = new Bgr(Color.Red);
        public Bgr IncompleteColor = new Bgr(Color.Blue);
        public Bgr CompleteColor = new Bgr(Color.Green);
        public Bgr CircleColor = new Bgr(Color.Green);

        private static readonly Logger Logger = LogManager.GetLogger("main.form");

        public PolygonBuilderControl(Image<Bgr, float> bgImage, Polygon startCoords)
        {
            InitializeComponent();

            _startImage = bgImage.Clone();

            pictureBox1.Image = _startImage.ToBitmap();

            pictureBox1.Size = _startImage.Size;

            if (null != startCoords)
            {
                foreach (var coord in startCoords)
                {
                    Coordinates.Add(coord);
                }
            }

            Draw(_startImage);
        }

        private void pbBgImage_MouseUp(object sender, MouseEventArgs e)
        {
            // Discard errant mouseups
            if (null == _mouseDownLocation) return;

            var mouseUpLocation = new Point(e.Location.X, e.Location.Y);

            if (mouseUpLocation.X >= _startImage.Width) mouseUpLocation.X = _startImage.Width - 1;
            if (mouseUpLocation.Y >= _startImage.Height) mouseUpLocation.Y = _startImage.Height - 1;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        // If clicked on first existing point, close polygon
                        if (TryClosePolygon((Point)_mouseDownLocation, mouseUpLocation))
                            break;

                        // If start and end points are far apart, drag the appropriate coord
                        if (TryDragCoord((Point)_mouseDownLocation, mouseUpLocation))
                            break;

                        // If nothing else, insert a new coordinate

                        // If the polygon is already closed, start over 
                        if (PolygonClosed)
                        {
                            Coordinates.Clear();
                        }
                        Coordinates.Add(mouseUpLocation);
                    }
                    break;
                case MouseButtons.Right:
                    {
                        // Delete coord
                        try
                        {
                            var existing = Coordinates.First(c => mouseUpLocation.DistanceTo(c) <= CircleRadius);
                            var existingIndex = Coordinates.IndexOf(existing);
                            Coordinates.RemoveAt(existingIndex);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, ex.Message);
                        }
                    }
                    break;
            }

            // Make sure the mouse down location isn't re-used later accidentally
            _mouseDownLocation = null;

            Draw(_startImage);
        }

        private void pbBgImage_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownLocation = new Point(e.Location.X, e.Location.Y);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Coordinates.Clear();

            Draw(_startImage);
        }

        public void Draw(Image<Bgr, float> source)
        {
            var newImage = source.Clone();

            var segments = GetSegments();
            var intersectingSegments = segments.Where(s => LineSegment2D_IntersectsAny(s, segments)).ToList();
            segments.RemoveAll(s => intersectingSegments.Contains(s));

            // If closed, draw green.  Otherwise, blue.  Intersections are red
            var segmentColor = PolygonClosed ? CompleteColor : IncompleteColor;

            // Draw good line segments
            foreach (var segment in segments)
            {
                newImage.Draw(segment, segmentColor, LineDrawWidth);
            }

            // Draw intersecting segments
            foreach (var intersection in intersectingSegments)
            {
                newImage.Draw(intersection, IntersectionColor, LineDrawWidth);
            }

            // Mark verticies with circle
            foreach (var point in Coordinates)
            {
                newImage.Draw(new CircleF(point, CircleRadius), CircleColor, LineDrawWidth);
            }

            pictureBox1.Image = newImage.ToBitmap();

            if (!Coordinates.Any())
            {
                tbMessages.Text = "Click OK to accept changes.";
                btnDone.Enabled = true;
            } 
            else if (intersectingSegments.Any())
            {
                tbMessages.Text = "Polygon can not contain intersecting segments";
                btnDone.Enabled = false;
            }
            else if (!PolygonClosed)
            {
                tbMessages.Text = "Polygon is not closed";
                btnDone.Enabled = false;
            }
            else
            {
                tbMessages.Text = "Click OK to accept changes.";
                btnDone.Enabled = true;
            }
        }

        private bool PolygonClosed
        {
            get
            {
                if (Coordinates.Count < 3) return false;

                return (Coordinates.First().Equals(Coordinates.Last()));
            }
        }

        private List<LineSegment2D> GetSegments()
        {
            var lines = new List<LineSegment2D>();

            if (Coordinates.Count < 2) return lines;

            for (int i = 1; i < Coordinates.Count; i++)
            {
                lines.Add(new LineSegment2D(Coordinates.ElementAt(i - 1), Coordinates.ElementAt(i)));
            }

            return lines;
        }

        private bool LineSegment2D_IntersectsAny(LineSegment2D line, IEnumerable<LineSegment2D> collection)
        {
            foreach (var l in collection)
            {
                if (line.Equals(l)) continue;

                if (line.Crosses(l)) return true;
            }

            return false;
        }

        private bool TryClosePolygon(Point down, Point up)
        {
            // Close the polygon if the down and up points are close (meaning nothing was dragged), 
            // and they are close to the first coordinate in the list

            if (PolygonClosed) return false;

            if (null == Coordinates || Coordinates.Count < 3) return false;

            if (down.DistanceTo(up) > CircleRadius) return false;

            if (up.DistanceTo(Coordinates.First()) > CircleRadius) return false;

            // Close the polygon
            Coordinates.Add(Coordinates.First());

            //Calculate centroid
            var points = Coordinates.Select(x => new NetTopologySuite.Geometries.Coordinate(x.X, x.Y)).ToArray();
            NetTopologySuite.Geometries.LinearRing ring = new NetTopologySuite.Geometries.LinearRing(points);
            NetTopologySuite.Geometries.Polygon ntsPoly = new NetTopologySuite.Geometries.Polygon(ring);
            NetTopologySuite.Algorithm.Centroid ntsCentroid = new NetTopologySuite.Algorithm.Centroid(ntsPoly);
            Coordinates.Centroid.X = (int) ntsCentroid.GetCentroid().X;
            Coordinates.Centroid.Y = (int) ntsCentroid.GetCentroid().Y;

            return true;
        }

        private bool TryDragCoord(Point start, Point end)
        {
            int indexOfExisting;
            Point coord;

            if (Coordinates.Count < 1)
            {
                return false;
            }

            try
            {
                var coordinatesInRange = Coordinates.Where(c => start.DistanceTo(c) <= CircleRadius);
                if (coordinatesInRange.Any())
                {
                    coord = coordinatesInRange.First();
                    indexOfExisting = Coordinates.IndexOf(coord);

                    if (end.X < 0) end.X = 0;
                    if (end.Y < 0) end.Y = 0;

                    coord.X = end.X;
                    coord.Y = end.Y;
                    Coordinates.RemoveAt(indexOfExisting);
                    Coordinates.Insert(indexOfExisting, coord);
                    return true;
                }
            }
            catch(InvalidOperationException ex)
            {
                return false;
            }

            return false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            OnCancelClicked?.Invoke(sender, e);
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            OnDoneClicked?.Invoke(sender, e);
        }
    }
}
