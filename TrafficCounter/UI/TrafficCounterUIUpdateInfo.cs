using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using VTC.Common;

namespace VTC.UI
{
    class TrafficCounterUIUpdateInfo
    {
        public Image<Bgr, byte> StateImage;
        public Image<Bgr, byte> Frame;
        public Measurement[] Measurements;
        public StateEstimate[] StateEstimates;
        public double Fps;
    }

    class TrafficCounterUIAccessoryInfo
    {
        public TimeSpan EstimatedTimeRemaining;
        public int ProcessedFrames;
    }
}
