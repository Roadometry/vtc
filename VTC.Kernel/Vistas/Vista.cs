using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using VTC.Common;
using Point = System.Drawing.Point;
using NLog;
using VTC.Classifier;
using VTC.Common.RegionConfig;

namespace VTC.Kernel.Vistas
{
    /// <summary>
    /// A Vista takes in a stream of frames and produces tracked object measurements.
    /// Vistas perform background subtraction, MHT and overlay rendering.
    /// </summary>
    public class Vista
    {
        private static readonly Logger Logger = LogManager.GetLogger("vista");

        #region Static colors

        private static readonly Bgr WhiteColor = new Bgr(Color.White);
        private static readonly Bgr BlueColor = new Bgr(Color.Blue);
        private static readonly Bgr StateColorGreen = new Bgr(0.0, 255.0, 0.0);
        private static readonly Bgr StateColorRed = new Bgr(0.0, 0.0, 255.0);

        #endregion

        //************* Configuration constants ****************
        private static readonly int MeasurementArrayQueueMaxLength = 300;

        private static readonly int MHTMaxUpdateTimeMs = 1000;

        //************* Strings ****************
        private static readonly string ApproachText = "Approach";
        private static readonly string ExitText = "Exit";

        //************* Main image variables ***************
        public Image<Bgr, float> RoiImage; //Area occupied by traffic
        public int _width;
        public int _height;

        public Queue<Measurement[]> MeasurementArrayQueue;              // For displaying detection history (colored dots)
        public Queue<Measurement[]> DiscardedMeasurementArrayQueue;     // For displaying discarded detections
        public Image<Bgr, float> ColorBackground { get; private set; }  //Average Background being formed

        //************* Multiple Hypothesis Tracker ***************  
        private MultipleHypothesisTracker _mht;

        //************* Multiple object detector ******************  
        public YoloClassifier _yoloClassifier = new YoloClassifier();
        public YoloIntegerNameMapping _yoloNameMapping = new YoloIntegerNameMapping();

        //************* Debug statistics ******************  
        private bool _debugMode = false;   //Prevent cancellation token from killing MHT update

        private RegionConfig _regionConfiguration;
        private UserConfig _userConfiguration;
        public RegionConfig RegionConfiguration
        {
            get
            {
                return _regionConfiguration;
            }
            private set
            {
                if (_regionConfiguration == value) return;

                _regionConfiguration = value;

                RoiImage = RegionConfiguration.RoiMask.GetMask(_width, _height, WhiteColor);
            }
        }

        public List<TrackedObject> CurrentVehicles => _mht.CurrentVehicles;
        public List<TrackedObject> DeletedVehicles => _mht.DeletedVehicles;

        public bool CpuMode => _yoloClassifier.cpuMode;

        private string ApproachName(int number)
        {
            return ApproachText + " " + number;
        }

        private string ExitName(int number)
        {
            return ExitText + " " + number;
        }

        public Vista(int width, int height, RegionConfig regionConfiguration, UserConfig userConfiguration)
        {
            Console.WriteLine("New vista created");
            var tempLogger = LogManager.GetLogger("userlog");
            tempLogger.Log(LogLevel.Info, "Vista: Initializer.");

            _width = width;
            _height = height;

            tempLogger.Log(LogLevel.Info, "Vista: Initializer: Creating measurement queue.");
            MeasurementArrayQueue = new Queue<Measurement[]>(MeasurementArrayQueueMaxLength);
            DiscardedMeasurementArrayQueue = new Queue<Measurement[]>(MeasurementArrayQueueMaxLength);

            tempLogger.Log(LogLevel.Info, "Vista: Initializer: Initializing configuration.");
            RegionConfiguration = regionConfiguration ?? new RegionConfig();

            _userConfiguration = userConfiguration;

            tempLogger.Log(LogLevel.Info, "Vista: Initializer: Updating configuration.");
            UpdateRegionConfiguration(RegionConfiguration);

            tempLogger.Log(LogLevel.Info, "Vista: Initializer finished.");
        }

        public Measurement[] MeasurementsArray;
        public void Update(Image<Bgr, byte> newFrame, double timestep)
        {
            try
            {
                var measurementsList = _yoloClassifier.DetectFrameYolo(newFrame);
                var measurementsFilteredByROI = measurementsList.Where(m => IsContainedInROI(m));
                var measurementsFilteredBySize = measurementsFilteredByROI.Where(m => m.Size > _regionConfiguration.MinObjectSize && m.Size < _regionConfiguration.MaxObjectSize);
                
                if(_regionConfiguration.FilterOverlap)
                {
                    var measurementsFilteredByOverlap = FilterOverlap(measurementsFilteredBySize.ToList());
                    MeasurementsArray = measurementsFilteredByOverlap.Where(m => DetectionClasses.ClassDetectionWhitelist.Contains(YoloIntegerNameMapping.GetObjectTypeFromClassInteger(m.ObjectClass, _yoloNameMapping.IntegerToObjectType))).ToArray();
                }
                else
                {
                    MeasurementsArray = measurementsFilteredBySize.Where(m => DetectionClasses.ClassDetectionWhitelist.Contains(YoloIntegerNameMapping.GetObjectTypeFromClassInteger(m.ObjectClass, _yoloNameMapping.IntegerToObjectType))).ToArray();
                }

                var filteredMeasurements = measurementsList.RemoveAll( item => MeasurementsArray.Contains(item) );
                
                MeasurementArrayQueue.Enqueue(MeasurementsArray);
                while (MeasurementArrayQueue.Count > MeasurementArrayQueueMaxLength)
                    MeasurementArrayQueue.Dequeue();

                //Keep a queue of discarded measurements for display/troubleshooting purposes.
                DiscardedMeasurementArrayQueue.Enqueue(measurementsList.ToArray());
                while (DiscardedMeasurementArrayQueue.Count > MeasurementArrayQueueMaxLength)
                    DiscardedMeasurementArrayQueue.Dequeue();

                _mht.Update(MeasurementsArray, timestep);

                GC.KeepAlive(newFrame);
            }
            catch (Exception e)
            {
#if DEBUG
                Logger.Log(LogLevel.Error, "In Vista:Update(), " + e.Message);
                throw;           
#else
                Logger.Log(LogLevel.Error, "In Vista:Update(), " + e.Message);
#endif
            }   
        }

        private List<Measurement> FilterOverlap(List<Measurement> boundingBoxes)
        { 
            //Algorithm:
            //
            //For each element in the boundingBoxes list, compare against all other elements in the non-overlapping list.
            //If another 'overlapping' element is found, skip to the next element in the boundingBoxlist. Otherwise, add this element
            // to the non-overlapping list.

            var nonOverlappingBoundingBoxes = new List<Measurement>();

            foreach(var bb in boundingBoxes)
            { 
                if(nonOverlappingBoundingBoxes.Where( m => BoxesAreOverlapping(m,bb)).Any())
                { 
                    continue;    
                }
                else
                { 
                    nonOverlappingBoundingBoxes.Add(bb);    
                }
            }

            return nonOverlappingBoundingBoxes;
        }

        private bool BoxesAreOverlapping(Measurement a, Measurement b)
        { 
            var rA = new Rectangle((int)a.X, (int)a.Y, (int)a.Width, (int)a.Height);
            var rB = new Rectangle((int)b.X, (int)b.Y, (int)b.Width, (int)b.Height);

            if(rA.Contains(rB) || rB.Contains(rA))
            { 
                return true;    
            }

            //Detect partial overlap?
            var rA_original = rA;
            var rB_original = rB;
            rA.Intersect(rB);
            rB.Intersect(rA_original);
            
            var intersectionAreaA = (double) rA.Height * rA.Width;
            var originalAreaA = (double) rA_original.Height * rA_original.Width;
            var intersectionAreaRatioA = intersectionAreaA/originalAreaA;
            var intersectionAIsOverlapping = (intersectionAreaRatioA >= _regionConfiguration.OverlapRatio) && intersectionAreaRatioA <= (2.0 -_regionConfiguration.OverlapRatio);

            var intersectionAreaB = (double) rB.Height * rB.Width;
            var originalAreaB = (double) rB_original.Height * rB_original.Width;
            var intersectionAreaRatioB = intersectionAreaB / originalAreaB;
            var intersectionBIsOverlapping = (intersectionAreaRatioB >= _regionConfiguration.OverlapRatio) && intersectionAreaRatioB <= (2.0 - _regionConfiguration.OverlapRatio);

            if(intersectionAIsOverlapping || intersectionBIsOverlapping)
            {
                return true;
            }

            return false;
        }

        private Boolean MeasurementsOverlap(Measurement x, Measurement y)
        {
            var distance = Math.Sqrt(Math.Pow((x.X - y.X), 2) + Math.Pow((x.Y - y.Y), 2));
            Boolean within_position_tolerance = distance < 5;

            var sizeRatio = 1.0;
            if(y.Size != 0)
            {
                sizeRatio = x.Size / y.Size;
            }

            var widthRatio = 1.0;
            if(y.Width != 0)
            { 
                widthRatio = x.Width/y.Width;    
            }

            var heightRatio = 1.0;
            if (y.Height != 0)
            {
                heightRatio = x.Height / y.Height;
            }

            Boolean withinRatioTolerance = false;
            if (sizeRatio > 0.9 && sizeRatio < 1.1 && widthRatio < 1.2 && widthRatio > 0.8 && heightRatio < 1.2 && heightRatio > 0.8)
            {
                withinRatioTolerance = true;
            }

            if(within_position_tolerance && withinRatioTolerance)
            { 
                return true;    
            }

            return false;
        }

        public void UpdateRegionConfiguration(RegionConfig newRegionConfig)
        {
            RegionConfiguration = newRegionConfig;
            var vf = new VelocityField(_width, _height);
            _mht = new MultipleHypothesisTracker(newRegionConfig, vf);
            for (int i = 1; i <= 4; i++)
            {
                var approachName = ApproachName(i);
                var exitName = ExitName(i);

                if (!RegionConfiguration.Regions.ContainsKey(approachName))
                    RegionConfiguration.Regions.Add(approachName, new Polygon());

                if (!RegionConfiguration.Regions.ContainsKey(exitName))
                    RegionConfiguration.Regions.Add(exitName, new Polygon());
            }
            RoiImage = RegionConfiguration.RoiMask.GetMask(_width, _height, WhiteColor);
        }

        public void DrawVelocityField<TColor, TDepth>(Image<TColor, TDepth> image, TColor color, int thickness)
           where TColor : struct, IColor
           where TDepth : new()
        {
            _mht?.VelocityField.Draw(image, color, thickness);
        }

        public Image<Bgr, float> GetBackgroundImage()
        {
            var lightColor = new Bgr(50,50,50);
            Image<Bgr, float> overlay = new Image<Bgr, float>(_width, _height, lightColor);
            overlay = overlay.And(RoiImage);
            if (ColorBackground != null)
            {
                overlay = overlay + ColorBackground;    
            }
            return overlay;
        }

        public Image<Bgr, byte> GetCurrentStateImage(Image<Bgr, byte> frame)
        {
            var stateImage = frame.Clone();

            List<TrackedObject> vehicles = _mht.MostLikelyStateHypothesis().Vehicles;

            if (_userConfiguration.DisplayPolygons)
            {
                foreach (var r in _regionConfiguration.Regions)
                {
                    var pts = r.Value.ToArray();
                    if (pts.Length > 0)
                    {
                        stateImage.DrawPolyline(pts, true, BlueColor);
                    }
                    //stateImage.Draw(r.Value.ToArray(), BlueColor, 1);
                    //var m = r.Value.GetMask(stateImage.Width, stateImage.Height, BlueColor);
                    //stateImage = stateImage.Add(m.Convert<Bgr, byte>());

                }
            }

            vehicles.ForEach(delegate(TrackedObject vehicle)
            {
                if (vehicle.NetMovement() < 10.0)
                {
                    return;
                }

                var lastState = vehicle.StateHistory.Last();
                var x = (float) lastState.X;
                var y = (float) lastState.Y;

                var validationRegionDeviation = _mht.ValidationRegionDeviation;
#if DEBUG
                // Draw class
                var g = Graphics.FromImage(stateImage.ToBitmap());
                var className = YoloIntegerNameMapping.GetObjectNameFromClassInteger(lastState.MostFrequentClassId(), _yoloNameMapping.IntegerToObjectName);
                g.DrawString(className, new Font(FontFamily.GenericMonospace, (float) 10.0), new SolidBrush(Color.White), (float)vehicle.StateHistory.Last().X, (float)vehicle.StateHistory.Last().Y);  
                    
                // Draw uncertainty circle
                var radius = validationRegionDeviation *
                            (float) Math.Sqrt(Math.Pow(lastState.CovX, 2) + (float) Math.Pow(lastState.CovY, 2));
                if (radius < 2.0)
                    radius = (float) 2.0;

                if (radius > 50.0)
                    radius = (float) 50.0;

                stateImage.Draw(new CircleF(new PointF(x, y), radius), StateColorGreen);


                // Draw velocity vector
                stateImage.Draw(
                new LineSegment2D(new Point((int) x, (int) y),
                    new Point((int) (x + lastState.Vx), (int) (y + lastState.Vy))), StateColorRed, 1);
#else
                stateImage.Draw(new CircleF(new PointF(x, y), 10),
                    new Bgr(vehicle.StateHistory.Last().Blue, vehicle.StateHistory.Last().Green,
                        vehicle.StateHistory.Last().Red), 2);
                stateImage.Draw(new CircleF(new PointF(x, y), 2), StateColorGreen);
#endif
            });

            foreach (var t in _mht.Trajectories)
            {
                var distance = Math.Sqrt(Math.Pow(t.StateEstimates.First().X - t.StateEstimates.Last().X, 2) +
                                            Math.Pow(t.StateEstimates.First().Y - t.StateEstimates.Last().Y, 2));

                if (!(distance > _regionConfiguration.MinPathLength)) continue;

                var ageFactor = 1.0 - (double) (DateTime.Now - t.ExitTime).Ticks / TimeSpan.FromSeconds(3).Ticks;
                var trajectoryColor = new Bgr(0, 0, 200*ageFactor);
                var trajectoryRendering = t.StateEstimates.Select(s => new Point((int)s.X, (int)s.Y)).ToArray();
                stateImage.DrawPolyline(trajectoryRendering, false, trajectoryColor);
            }

            var latestMeasurements = MeasurementArrayQueue.Last();
            foreach (var m in latestMeasurements)
            {
                stateImage.Draw(new Rectangle((int) (m.X - m.Width/2), (int) (m.Y - m.Height/2), (int) m.Width, (int) m.Height), new Bgr(Color.Chartreuse), 1);
            }

            var latestDiscardedMeasurements = DiscardedMeasurementArrayQueue.Last();
            foreach (var m in latestDiscardedMeasurements)
            {
                stateImage.Draw(new Rectangle((int)(m.X - m.Width / 2), (int)(m.Y - m.Height / 2), (int)m.Width, (int)m.Height), new Bgr(Color.SlateGray), 1);
            }


            return stateImage;
        }

        private bool IsContainedInROI(Measurement m)
        {
            if (m.X >= RoiImage.Width || m.Y >= RoiImage.Height || m.X < 0 || m.Y < 0)
            {return false;}

            // Containment in this context should evaluate to 'true' when a vehicle appears to be occupying this polygon (from above)
            // after accounting for perspective. In practice, this means that we do not want to evaluate containment using
            // the exact rectangle-center, because the rectangle-center may be outside the polygon in the image-plan but the vehicle
            // may be sitting directly on top of the polygon if the entire scene was viewed from above. 
            //
            // Typically, users will draw the polygons *as if* the scene were being viewed from above. Most user-drawn polygons will *not*
            // contain the vehicles in the image-plane in locations where the user considers the vehicle to be contained within the polygon.
            // 
            // This containment 

            if(_regionConfiguration.StrictContainment)
            {
                return ((int)RoiImage[(int)m.Y, (int)m.X].Blue != 0);
            }

            var approximateLeftWheelX = m.X - m.Width/4;
            var approximateRightWheelX = m.X + m.Width/4;
            var approximateWheelY = m.Y + m.Height/4;

            var leftWheelContained = ((int)RoiImage[(int)approximateWheelY, (int)approximateLeftWheelX].Blue != 0);
            var rightWheelContained = ((int)RoiImage[(int)approximateWheelY, (int)approximateRightWheelX].Blue != 0);

            return leftWheelContained && rightWheelContained;
        }

    }
}
        