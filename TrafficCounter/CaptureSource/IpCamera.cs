using System;
using Emgu.CV;

namespace VTC.CaptureSource
{
    public class IpCamera : CaptureSource
    {
        public readonly string ConnectionString;
        private DateTime _startDateTime;

        public IpCamera(string name, string connectionString) : base(name)
        {
            ConnectionString = connectionString;
            _startDateTime = DateTime.Now;
        }

        protected override VideoCapture GetCapture()
        {
            return new VideoCapture(ConnectionString);
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
