using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NetTopologySuite.Algorithm;


namespace VTC.Common.RegionConfig
{
    public class ExamplePath
    {
        public bool PedestrianOnly { get; set; } = false;

        public bool Ignored { get; set; } = false;

        public Turn TurnType { get; set; } = Turn.Unknown;

        public string Approach { get; set; } = "";

        public string Exit { get; set; } = "";

        public List<Point> Points { get; set; } = new List<Point>();

        public ExamplePath()
        {

        }

        public ExamplePath(bool pedestrianOnly)
        {
            PedestrianOnly = pedestrianOnly;
        }

        public string Description()
        {
            var description = Approach + " to " + Exit;
            if (Ignored)
            {
                description += " (ignored)";
            }

            if (PedestrianOnly)
            {
                description += " (pedestrian)";
            }

            return description;
        }

        public Image<Bgr, float> GetMask(int width, int height, Bgr fgColor)
        {
            var coords = new List<Point>();

            // Can't simply use fillConvexPoly here as we expect that the poly is not
            // usually convex.  Instead, draw the poly and use the flood tool to fill in the rest of the image with black

            // In order for flood tool not to mess up if roi extends from one end to another, move any points on the border inwards
            // by one pixel.  This allows a buffer for the flood tool to travel around.
            foreach (var coord in Points)
            {
                var x = coord.X;
                var y = coord.Y;

                if (x <= 0) x = 1;
                if (y <= 0) y = 1;
                if (x >= width) x = width - 1;
                if (y >= height) y = height - 1;

                coords.Add(new Point(x, y));
            }

            var image = new Image<Bgr, float>(width, height);
            image.SetValue(fgColor);

            if (!coords.Any())
            {
                return image;
            }

            image.DrawPolyline(coords.ToArray(), false, new Bgr(Color.Black), 3);

            var lastPoint = coords.Last();
            image.Draw(new CircleF(new PointF(lastPoint.X, lastPoint.Y), 10), new Bgr(0,0,0), 2, LineType.EightConnected);
            
            return image;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var pObj = (ExamplePath)obj;
            foreach (var point in pObj.Points)
            {
                if (Points.Contains(point))
                {
                    continue; //Make sure that this list contains all points contained in compared list
                }
                return false;
            }

            foreach (var point in Points)
            {
                if (pObj.Points.Contains(point))
                {
                    continue; //Make sure that compared list contains all points contained in this list
                }
                return false;
            }

            return true;
        }
    }
}
