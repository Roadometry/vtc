using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;


namespace VTC.Common.RegionConfig
{
    public class Polygon : List<Point>
    {
        public bool PolygonClosed => Count >= 3 && (this.First().Equals(this.Last()));

        public Point Centroid;      
        
        public bool PedestrianOnly = false;

        public string DisplayName = "";

        public void UpdateCentroid()
        {
            // TODO: Handle centroid of empty polygon
            if (Count == 0)
                return;

            var points = this.Select(x => new NetTopologySuite.Geometries.Coordinate(x.X, x.Y)).ToArray();
            var ring = new NetTopologySuite.Geometries.LinearRing(points);
            var ntsPoly = new NetTopologySuite.Geometries.Polygon(ring);
            var ntsCentroid = new NetTopologySuite.Algorithm.Centroid(ntsPoly);
            Centroid.X = (int)ntsCentroid.GetCentroid().X;
            Centroid.Y = (int)ntsCentroid.GetCentroid().Y;
        }

        public Polygon()
        {

        }

        public Polygon(bool pedestrianOnly)
        {
            PedestrianOnly = pedestrianOnly;
        }

        public Image<Bgr, float> GetMask(int width, int height, Bgr fgColor)
        {
            var coords = new List<Point>();

            // Can't simply use fillConvexPoly here as we expect that the poly is not
            // usually convex.  Instead, draw the poly and use the flood tool to fill in the rest of the image with black

            // In order for flood tool not to mess up if roi extends from one end to another, move any points on the border inwards
            // by one pixel.  This allows a buffer for the flood tool to travel around.
            foreach (var coord in this)
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

            image.DrawPolyline(coords.ToArray(), true, new Bgr(Color.Black));

            var lo = new MCvScalar(1, 1, 1);
            var up = new MCvScalar(1, 1, 1);

            IInputOutputArray iioArray = image;
            var mask = new Image<Gray, byte>(new Size(image.Width + 2, image.Height + 2));
            try
            {
                Rectangle rect;
                CvInvoke.FloodFill(iioArray, mask, new Point(0, 0), new MCvScalar(0, 0, 0), out rect, lo, up);
            }
            catch(Emgu.CV.Util.CvException e)
            {
                Debug.WriteLine("Exception in GetMask:CvInvoke.FloodFill:" + e.ErrorMessage);
            }

            return image;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var pObj = (Polygon)obj;
            foreach (var point in pObj)
            {
                if (this.Contains(point))
                {
                    continue; //Make sure that this list contains all points contained in compared list
                }
                return false;
            }

            foreach (var point in this)
            {
                if (pObj.Contains(point))
                {
                    continue; //Make sure that compared list contains all points contained in this list
                }
                return false;
            }

            return true;
        }
    }
}
