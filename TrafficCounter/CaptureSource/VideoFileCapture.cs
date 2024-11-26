using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Accord.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MediaInfo.DotNetWrapper;
using Debug = System.Diagnostics.Debug;


namespace VTC.CaptureSource
{
    public class VideoFileCapture : CaptureSource
    {
        private readonly string _path;
        public double FrameCount;

        /// <summary>
        /// Override video API
        /// </summary>
        private VideoCapture.API? ApiOverride;

        public VideoFileCapture(string path)
            : base("File: " + Path.GetFileName(path))
        {
            _path = path;
            var capture = new VideoCapture(_path);
            FrameCount = capture.GetCaptureProperty(CapProp.FrameCount);

            var mi = new MediaInfo.DotNetWrapper.MediaInfo();
            mi.Open(_path);
            var s = mi.Inform();
            Console.WriteLine(s);
            var fpsString = mi.Get(MediaInfo.DotNetWrapper.Enumerations.StreamKind.Video,0,"FrameRate");
            var rotationString = mi.Get(MediaInfo.DotNetWrapper.Enumerations.StreamKind.Video,0,"Rotation");
            var dateModified = File.GetLastWriteTimeUtc(_path);
            var selectedFileDate = dateModified;
            try
            {
                var encodedDateString =
                    mi.Get(MediaInfo.DotNetWrapper.Enumerations.StreamKind.Video, 0, "Encoded_Date");

                if (encodedDateString.Length >= 23)
                {
                    var trimmedEncodedDateString = encodedDateString.Substring(4);
                    selectedFileDate = DateTime.ParseExact(trimmedEncodedDateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None);

                    if (selectedFileDate < dateModified)
                    {
                        _startDate = selectedFileDate;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            _startDate = selectedFileDate;

            try
            {
                _fps = fpsString.Length > 0 ? Double.Parse(fpsString,CultureInfo.InvariantCulture) : 24.0;
            }
            catch(FormatException ex)
            {
                _fps = 24.0;
            }

            try
            {
                _rotation = rotationString.Length > 0 ? Double.Parse(rotationString) : 0.0;
            }
            catch (FormatException ex)
            {
                _rotation = 0.0;
            }

        }

        protected override VideoCapture GetCapture()
        {
            //This is an ugly but necessary bit of code to work around apparent bugs in EmguCV/OpenCV.
            //It seems that no single back-end video API is compatible will all types of video files. 
            //By trial and error, I have found that switch/selector below has the highest chance of 
            //working with any given video file without user input. 
            //The ideal solution would be to investigate and fix the root-cause in the video decoder, but
            //this could be time consuming so I'm leaving it for another day.
            var extension = Path.GetExtension(_path);
            if (extension != ".mkv")
            {
                var capture = new VideoCapture(_path, VideoCapture.API.Msmf);
                var frame = capture.QueryFrame();
                if (frame != null)
                {
                    FrameCount = capture.GetCaptureProperty(CapProp.FrameCount);
                    Debug.WriteLine("Selected Msmf");
                    return capture;
                }

                capture = new VideoCapture(_path);
                frame = capture.QueryFrame();
                if (frame != null)
                {
                    Debug.WriteLine("Selected none");
                    FrameCount = capture.GetCaptureProperty(CapProp.FrameCount);
                    return capture;
                }
            }
            else
            {
                var capture = new VideoCapture(_path, VideoCapture.API.Ffmpeg);
                var frame = capture.QueryFrame();
                if (frame != null)
                {
                    Debug.WriteLine("Selected ffmpeg");
                    FrameCount = capture.GetCaptureProperty(CapProp.FrameCount);
                    return capture;
                }
            }
            
            throw new VideoFormatException();
        }

        public override bool IsLiveCapture()
        {
            return false;
        }

        public override double FPS()
        {
            return _fps;
        }

        public override DateTime StartDateTime()
        {
            return _startDate;
        }

        public override double Rotation()
        { 
            return _rotation;    
        }
    }

    public class VideoFormatException : Exception
    {
        public override string Message
        {
            get
            {
                return "Video format could not be decoded";
            }
        }
       
    }
}
