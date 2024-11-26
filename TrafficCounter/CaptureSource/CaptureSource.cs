using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using VTC.Common;
using NLog;

namespace VTC.CaptureSource
{
    public abstract class CaptureSource : Kernel.Video.ICaptureSource
    {
        private VideoCapture _cameraCapture;
        private static readonly Logger Logger = LogManager.GetLogger("main.form");
        private Image<Bgr, byte> _frame = new Image<Bgr, byte>(640,480);

        public int Width
        {
            get
            {
                if (_cameraCapture == null) throw new ApplicationException("Camera is not initialized.");
                return _cameraCapture.Width;
            }
        }

        public int Height
        {
            get
            {
                if (_cameraCapture == null) throw new ApplicationException("Camera is not initialized.");
                return _cameraCapture.Height;
            }
        }

        public string Name { get; }

        private bool _captureComplete;
        public bool CaptureComplete()
        {
            return _captureComplete;
        }

        protected abstract VideoCapture GetCapture();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Capture source name.</param>
        protected CaptureSource(string name)
        {
            Name = name;
        }

        public delegate void OnCaptureComplete();
        public OnCaptureComplete CaptureCompleteEvent;
        public OnCaptureComplete CaptureTerminatedEvent;

        /// <summary>
        /// Get next frame from camera or terminate capture
        /// </summary>
        /// <returns></returns>
        public Image<Bgr, Byte> QueryFrame()
        {
            try
            {
                if (_cameraCapture != null)
                {
                    {
                        using(var frame = _cameraCapture.QueryFrame())
                        {
                            if (frame == null)
                            {
                                if (_cameraCapture.CaptureSource == VideoCapture.CaptureModuleType.Camera)
                                {
                                    //_captureComplete = true;
                                    //CaptureTerminatedEvent?.Invoke();
                                }
                                else
                                {
                                    _captureComplete = true;
                                    CaptureCompleteEvent?.Invoke();
                                }

                                return null;
                            }

                            var tnow = DateTime.Now;
                            var tdelta = tnow - _lastFrameTime;
                            var tdeltaSafe = (tdelta.Ticks == 0) ? new TimeSpan(1) : tdelta;
                            if (IsLiveCapture())
                            {
                                _fps = 1.0 / ((tdeltaSafe).Milliseconds / 1000.0);
                            }
                            _lastFrameTime = tnow;

                            _frame = frame.ToImage<Bgr, byte>().Resize(640, 480, Inter.Cubic);
                        }
                        return _frame;
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Logger.Log(LogLevel.Error, e);
            }
            catch (AccessViolationException e)
            {
                Logger.Log(LogLevel.Error, e);
            }
            
            return null;
        }

        /// <summary>
        /// Initialize camera.
        /// </summary>
        /// <param name="settings"></param>
        public void Init()
        {
            _cameraCapture = GetCapture();
        }

        /// <summary>
        /// Destroy underlying camera.
        /// </summary>
        public void Destroy()
        {
            if (null != _cameraCapture)
            {
                _cameraCapture.Dispose();
                _cameraCapture = null;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public abstract bool IsLiveCapture();

        private DateTime _lastFrameTime = DateTime.Now; //Only used by live and IP camera, not video-files.
        public double _fps = 0.0;
        public double _rotation = 0.0;
        public DateTime _startDate; //Only used by video files. Taken from MediaInfo element Encoded_Date

        public abstract double FPS();

        public abstract DateTime StartDateTime();

        public abstract double Rotation();
    }
}
