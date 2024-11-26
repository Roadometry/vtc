using System;
using Emgu.CV;

namespace VTC.CaptureSource
{
    public class SystemCamera : CaptureSource
    {
        private readonly int _systemDeviceIndex;
        private DateTime _startDateTime;

        public SystemCamera(string name, int systemDeviceIndex) : base(name)
        {
            _systemDeviceIndex = systemDeviceIndex;
            _startDateTime = DateTime.Now;
        }

        protected override VideoCapture GetCapture()
        {
            return new VideoCapture(_systemDeviceIndex);
        }

        public override bool IsLiveCapture()
        {
            return true;
        }

        public override double FPS()
        {
            return _fps;
        }

        public override DateTime StartDateTime()
        {
            return _startDateTime;
        }

        public override double Rotation()
        {
            return 0.0;
        }
    }
}
