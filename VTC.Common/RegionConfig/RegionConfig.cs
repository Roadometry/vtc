using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Schema;

namespace VTC.Common.RegionConfig
{ 
    [DataContract]
    public class RegionConfig
    {
        [DataMember]
        public Polygon RoiMask;

        [DataMember]
        public readonly Guid ConfigGuid;

        [DataMember]
        public Dictionary<string, Polygon> Regions;

        [DataMember] public List<ExamplePath> ExamplePaths;

        [Description("The name of this configuration")]
        [DataMember]
        public string Title { get; set; }

        [Description("Q_position: position movement covariance. Decrease for objects with smooth movement; increase for objects with complex movement.")]
        [DataMember]
        public double Q_position { get; set; } = 100;

        [Description("Q_color: color-change covariance. Decrease for scenes with consistent lighting; increase for scenes with variable lighting.")]
        [DataMember]
        public double Q_color { get; set; } = 100;

        [Description("R_position: position measurement covariance. Decrease when detection-quality is high. Increase when detection-quality is low.")]
        [DataMember]
        public double R_position { get; set; } = 3;

        [Description("R_color: color measurement covariance. Decrease for scenes with consistent lighting; increase for scenes with variable lighting.")]
        [DataMember]
        public double R_color { get; set; } = 100;

        [Description("Initial X covariance: increase for scenes with nearby vehicles. Decrease for scenes with far-away vehicles.")]
        [DataMember]
        public double VehicleInitialCovX { get; set; } = 4;

        [Description("Initial Vx covariance: increase for scenes with rapidly-moving vehicles. Decrease for scenes with slowly-moving vehicles.")]
        [DataMember]
        public double VehicleInitialCovVX { get; set; } = 3000;

        [Description("Initial Y covariance: increase for scenes with nearby vehicles. Decrease for scenes with far-away vehicles.")]
        [DataMember]
        public double VehicleInitialCovY { get; set; } = 4;

        [Description("Initial Vy covariance: increase for scenes with rapidly-moving vehicles. Decrease for scenes with slowly-moving vehicles.")]
        [DataMember]
        public double VehicleInitialCovVY { get; set; } = 3000;

        [Description("Initial R covariance: initial assumption on R-channel color certainty.")]
        [DataMember]
        public double VehicleInitialCovR { get; set; } = 100;

        [Description("Initial G covariance: initial assumption on G-channel color certainty.")]
        [DataMember]
        public double VehicleInitialCovG { get; set; } = 100;

        [Description("Initial B covariance: initial assumption on B-channel color certainty.")]
        [DataMember]
        public double VehicleInitialCovB { get; set; } = 100;

        [Description("Compensation gain: how aggressively the tracker seeks a lost object.")]
        [DataMember]
        public double CompensationGain { get; set; } = 5;

        [Description("MinObjectSize: objects with less than this many pixels will be ignored.")]
        [DataMember]
        public int MinObjectSize { get; set; } = 1;

        [Description("MaxObjectSize: objects with more than this many pixels will be ignored.")]
        [DataMember]
        public int MaxObjectSize { get; set; } = 1000000;

        [Description("MissThreshold: if an object is not detected for this many frames, it will be considered to be gone.")]
        [DataMember]
        public int MissThreshold { get; set; } = 5;

        [Description("Lambda-F: The odds of a false-positive object detection per frame.")]
        [DataMember]
        public double LambdaF { get; set; } = 1E-11;

        [Description("Lambda-N: The odds of a new object detection per frame.")]
        [DataMember]
        public double LambdaN { get; set; } = 1E-08;

        [Description("Pd: probability of detecting a particular vehicle.")]
        [DataMember]
        public double Pd { get; set; } = 0.95;

        [Description("Px: probability of a particular vehicle exiting the scene in any frame.")]
        [DataMember]
        public double Px { get; set; } = 0.05;

        [Description("VehicleInitialCovSize: the uncertainty in initial object size. Increase if large object tracking-quality is poor.")]
        [DataMember]
        public double VehicleInitialCovSize { get; set; } = 1000;

        [Description("Q_size: object size change. Increase for scenes where objects appear to change in size significantly, i.e. for fish-eye or wide-angle lenses.")]
        [DataMember]
        public double Q_size { get; set; } = 9500;

        [Description("R_size: size measurement uncertainty. Increase if objects are being 'lost' by trackers. Decrease if trackers are swapping.")]
        [DataMember]
        public double R_size { get; set; } = 250;

        [Description("MinPathLength: Object trajectories shorter than this length (in pixels) are discarded without being counted.")]
        [DataMember]
        public int MinPathLength { get; set; } = 100;

        [Description("MHT maximum hypothesis tree-depth: how many frames of tracker history are retained. Increase for better quality; decrease for faster processing.")]
        [DataMember]
        public int MaxHypTreeDepth { get; set; } = 1;

        [Description("Maximum number of tracked objects.")]
        [DataMember]
        public int MaxTargets { get; set; } = 6;

        [Description("K-hypotheses (MHT branch factor): how many association-hypotheses are generated per timestep. Increase for better quality; decrease for faster processing.")]
        [DataMember]
        public int KHypotheses { get; set; } = 1;

        [Description("Validation-region deviation: The number of Mahalanobis-distance deviations allowable from the predicted detection location, used in association-gating. Reduce if trackers are swapping. Increase if trackers are falling behind objects.")]
        [DataMember]
        public int ValRegDeviation { get; set; } = 4;

        [Description("Position-covariance threshold: if the tracked object's final position-covariance is above this value, it is not counted. Increase this value for scenes with fast-moving vehicles.")]
        [DataMember]
        public double PositionCovarianceThreshold { get; set; } = 4000.0;

        [Description("Missed-ratio threshold: the miss-ratio is the number of non-detections divided by the total number of time-steps that an object is tracked for. Increase this value for fast-moving vehicles or scenes with heavy occlusion.")]
        [DataMember]
        public double MissRatioThreshold { get; set; } = 3.5;

        [Description("Smoothness threshold: this parameter allows VTC to reject jagged, discontinuous trajectories.")]
        [DataMember]
        public double SmoothnessThreshold { get; set; } = 0.1;

        [Description("Movement-length ratio: this parameter allows VTC to reject trajectories with low net-movement-to-integrated-path-length ratio.")]
        [DataMember]
        public double MovementLengthRatio { get; set; } = 0.5;

        [Description("Overlap ratio: when overlap filtering is enabled, this area-ratio threshold is applied to detect overlap and eliminate double detections.")]
        [DataMember]
        public double OverlapRatio { get; set; } = 0.8;

        [Description("Site token: copy this from the web dashboard for your site.")]
        [DataMember]
        public string SiteToken { get; set; } = "";

        [Description("SendToServer: set this to transmit movements and images to a remote server.")]
        [DataMember]
        public bool SendToServer { get; set; } = false;

        [Description("DisableUTurns: Do not classify any movements as U-turns.")]
        [DataMember]
        public bool DisableUTurns { get; set; } = false;

        [Description("FilterOverlap: Filter overlapping bounding-boxes.")]
        [DataMember]
        public bool FilterOverlap { get; set; } = true;

        [Description("StrictContainment: Set to 'true' to evaluate polygon containment for a bird's-eye perspective.")]
        [DataMember]
        public bool StrictContainment { get; set; } = false;

        [Description("Approach 1 name: Use this field to customize the name of this approach in output reports.")]
        [DataMember]
        public string Approach1Name { get; set; } = "";

        [Description("Approach 2 name: Use this field to customize the name of this approach in output reports.")]
        [DataMember]
        public string Approach2Name { get; set; } = "";

        [Description("Approach 3 name: Use this field to customize the name of this approach in output reports.")]
        [DataMember]
        public string Approach3Name { get; set; } = "";

        [Description("Approach 4 name: Use this field to customize the name of this approach in output reports.")]
        [DataMember]
        public string Approach4Name { get; set; } = "";

        [Description("Exit 1 name: Use this field to customize the name of this exit in output reports.")]
        [DataMember]
        public string Exit1Name { get; set; } = "";

        [Description("Exit 2 name: Use this field to customize the name of this exit in output reports.")]
        [DataMember]
        public string Exit2Name { get; set; } = "";

        [Description("Exit 3 name: Use this field to customize the name of this exit in output reports.")]
        [DataMember]
        public string Exit3Name { get; set; } = "";

        [Description("Exit 4 name: Use this field to customize the name of this exit in output reports.")]
        [DataMember]
        public string Exit4Name { get; set; } = "";

        public RegionConfig()
        {
            RoiMask = new Polygon();

            RoiMask.Add(new Point(0,0));
            RoiMask.Add(new Point(639,0));
            RoiMask.Add(new Point(639,479));
            RoiMask.Add(new Point(0,479));
            RoiMask.Add(new Point(0, 0));

            Regions = new Dictionary<string, Polygon>
            {
                {"Approach 1", new Polygon()},
                {"Approach 2", new Polygon()},
                {"Approach 3", new Polygon()},
                {"Approach 4", new Polygon()},
                {"Exit 1", new Polygon()},
                {"Exit 2", new Polygon()},
                {"Exit 3", new Polygon()},
                {"Exit 4", new Polygon()},
                {"Sidewalk 1", new Polygon(true)},
                {"Sidewalk 2", new Polygon(true)},
                {"Sidewalk 3", new Polygon(true)},
                {"Sidewalk 4", new Polygon(true)}
            };
            ExamplePaths = new List<ExamplePath>();

            ConfigGuid = Guid.NewGuid();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var rObj = (RegionConfig) obj;

            if (rObj.ConfigGuid != ConfigGuid)
            {
                return false;
            }

            return true;
        }

        public void SanitizeBadValues()
        { 
            if(MaxObjectSize == 0)
            { 
                MaxObjectSize = 10000;
            }
                
        }

    }
}
