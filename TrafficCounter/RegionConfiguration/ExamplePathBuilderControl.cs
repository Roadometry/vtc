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
    public partial class ExamplePathBuilderControl : UserControl
    {
        public event EventHandler OnDoneClicked;
        public event EventHandler OnCancelClicked;

        private readonly Image<Bgr, float> _startImage;
        private Point? _mouseDownLocation ;

        public ExamplePath Coordinates = new ExamplePath();

        public int CircleRadius = 5;
        public int LineDrawWidth = 2;

        public Bgr IntersectionColor = new Bgr(Color.Red);
        public Bgr IncompleteColor = new Bgr(Color.Blue);
        public Bgr CompleteColor = new Bgr(Color.Green);
        public Bgr CircleColor = new Bgr(Color.Green);

        private static readonly Logger Logger = LogManager.GetLogger("main.form");

        public ExamplePathBuilderControl(Image<Bgr, float> bgImage, ExamplePath startPath)
        {
            InitializeComponent();

            _startImage = bgImage.Clone();

            pictureBox1.Image =  _startImage.ToBitmap();

            pictureBox1.Size = _startImage.Size;

            if (null != startPath)
            {
                foreach (var coord in startPath.Points)
                {
                    Coordinates.Points.Add(coord);
                }

                
            }

            Coordinates.Approach = startPath.Approach;
            Coordinates.Exit = startPath.Exit;
            Coordinates.Ignored = startPath.Ignored;
            Coordinates.PedestrianOnly = startPath.PedestrianOnly;

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
                        // If start and end points are far apart, drag the appropriate coord
                        if (TryDragCoord((Point)_mouseDownLocation, mouseUpLocation))
                            break;

                        // If nothing else, insert a new coordinate
                        Coordinates.Points.Add(mouseUpLocation);
                    }
                    break;
                case MouseButtons.Right:
                    {
                        // Delete coord
                        try
                        {
                            var existing = Coordinates.Points.First(c => mouseUpLocation.DistanceTo(c) <= CircleRadius);
                            var existingIndex = Coordinates.Points.IndexOf(existing);
                            Coordinates.Points.RemoveAt(existingIndex);
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
            Coordinates.Points.Clear();

            Draw(_startImage);
        }

        public void Draw(Image<Bgr, float> source)
        {
            var newImage = source.Clone();
            var segments = GetSegments();

            // If closed, draw green.  Otherwise, blue.  Intersections are red
            var segmentColor = CompleteColor;

            // Draw good line segments
            foreach (var segment in segments)
            {
                newImage.Draw(segment, segmentColor, LineDrawWidth);
            }

            // Mark verticies with circle
            foreach (var point in Coordinates.Points)
            {
                newImage.Draw(new CircleF(point, CircleRadius), CircleColor, LineDrawWidth);
            }

            pictureBox1.Image = newImage.ToBitmap();

            if (!Coordinates.Points.Any())
            {
                tbMessages.Text = "Click OK to accept changes.";
                btnDone.Enabled = true;
            } 
            else
            {
                tbMessages.Text = "Click OK to accept changes.";
                btnDone.Enabled = true;
            }
        }

        private List<LineSegment2D> GetSegments()
        {
            var lines = new List<LineSegment2D>();

            if (Coordinates.Points.Count < 2) return lines;

            for (int i = 1; i < Coordinates.Points.Count; i++)
            {
                lines.Add(new LineSegment2D(Coordinates.Points.ElementAt(i - 1), Coordinates.Points.ElementAt(i)));
            }

            return lines;
        }

        private bool TryDragCoord(Point start, Point end)
        {
            if (Coordinates.Points.Count() < 1)
            {
                return false;
            }

            try
            {
                var coordinatesInRange = Coordinates.Points.Where(c => start.DistanceTo(c) <= CircleRadius);
                if (coordinatesInRange.Any())
                {
                    var coord = coordinatesInRange.First();
                    var indexOfExisting = Coordinates.Points.IndexOf(coord);

                    if (end.X < 0) end.X = 0;
                    if (end.Y < 0) end.Y = 0;

                    coord.X = end.X;
                    coord.Y = end.Y;
                    Coordinates.Points.RemoveAt(indexOfExisting);
                    Coordinates.Points.Insert(indexOfExisting, coord);

                    return true;
                }
            }
            catch
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
