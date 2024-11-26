using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VTC.Kernel.Video;
using VTC.Messages;
using Akka;
using Akka.Actor;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Timer = System.Threading.Timer;
using NLog;
using VTC.CaptureSource;
using VTC.Common;
using VTC.UI;
using VTC.UserConfiguration;

namespace VTC.Actors
{
    class FrameGrabActor : ReceiveActor
    {
        private ICaptureSource CaptureSource;
        private IActorRef ProcessingActor;
        private IActorRef LoggingActor;
        private IActorRef SequencingActor;

        private int FramesProcessed = 0;
        private double TotalFramesInVideo = 0;
        private DateTime ProcessingStartTime;
        private bool LiveCapture = false;
        private int LowFpsCount = 0;
        private const double LowFpsThreshold = 0.5;
        private const int LowFpsCountThreshold = 2;
        private const int NullFrameThreshold = 10;
        private int NullFrameCount = 0;
        private DateTime LastFrameTimestamp = DateTime.MinValue;

        public delegate void UpdateUIDelegate(TrafficCounterUIAccessoryInfo accessoryInfo);
        private UpdateUIDelegate _updateUiDelegate;

        private const int FRAME_DELAY_MS = 1;
        private const int FRAME_TIMEOUT_MS = 60000;

        private bool _loggingActorNeedsBackgroundUpdate = false;

        private VTC.Common.UserConfig _userConfig = new UserConfig();

        public FrameGrabActor()
        {
            Receive<NewVideoSourceMessage>(message =>
                HandleNewVideoSource(message.CaptureSource)
            );

            Receive<NewProcessingActorMessage>(message =>
                HandleNewProcessingActor(message.ActorRef)
            );

            Receive<GetNextFrameMessage>(message =>
                GetNewFrame()
            );

            Receive<LoggingActorMessage>(message =>
                UpdateLoggingActor(message.ActorRef)
            );

            Receive<SequencingActorMessage>(message =>
                UpdateSequencingActor(message.ActorRef)
            );

            Receive<CheckConnectivityMessage>(message =>
                CheckConnectivity()
            );

            Receive<UpdateFramerateMessage>(message =>
                UpdateFramerate()
            );

            Receive<UpdateUiAccessoryHandlerMessage>(message =>
                UpdateUiHandler(message.UiDelegate)
            );

            Receive<UpdateVideoPropertiesMessage>(message =>
                UpdateVideoProperties(message.Fps, message.TotalFrames)
            );

            Receive<LoadUserConfigMessage>(message => 
                LoadUserConfig()
            );

            Context.System.Scheduler.ScheduleTellRepeatedly(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 5), Context.Parent, new ActorHeartbeatMessage(Self), Self);
            Context.System.Scheduler.ScheduleTellRepeatedly(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 5), Self, new UpdateFramerateMessage(), Self);

            Self.Tell(new LoadUserConfigMessage());

            Context.System.Scheduler.ScheduleTellRepeatedly(60000, 60000, Self, new CheckConnectivityMessage(), Self);
        }

        private void HandleNewVideoSource(ICaptureSource captureSource)
        {
            if (captureSource == CaptureSource) return;

            if (captureSource == null)
            {
                CaptureSource = null;
                return;
            }

            CaptureSource?.Destroy();
            CaptureSource = captureSource;
            CaptureSource.Init();

            FramesProcessed = 0;
            ProcessingStartTime = DateTime.Now;

            //Use hard-coded VGA resolution instead of captureSource.Width / captureSource.Height
            ProcessingActor.Tell(new UpdateVideoDimensionsMessage(640, 480));
            LoggingActor.Tell(new LogMessage($"CaptureSource: {captureSource.Name}", LogLevel.Debug, "FrameGrabActor"));
        }

        private void GetNewFrame()
        {

            try
            {
                var frame = TimeoutFrameQuery();
                var fps = CaptureSource?.FPS();

                if (frame != null && CaptureSource != null && fps.HasValue)
                {
                    var rotatedFrame = frame.Rotate(CaptureSource.Rotation(), frame.GetAverage(),false).Resize(640, 480, Inter.Cubic);

                    NullFrameCount = 0;
                    var ts_measured = DateTime.Now - LastFrameTimestamp;
                    LastFrameTimestamp = DateTime.Now;
                    var timestep_measured = ts_measured.TotalSeconds; //Use this if we're running from video. In this scenario, the measured delay is relevant.
                    var timestep_calculated = 1.0 / fps; //Use the video-file's stated FPS if we're reading from disk, regardless of how quickly we're actually reading.
                    var timestep_selected = CaptureSource.IsLiveCapture() ? timestep_measured : timestep_calculated;

                    var cloned = rotatedFrame.Clone();
                    ProcessingActor?.Tell(new ProcessNextFrameMessage(cloned, timestep_selected.Value));
                    if (FramesProcessed < 10)
                    {
                        var clone2 = rotatedFrame.Clone();
                        var configurationActor =
                            Context.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
                        configurationActor.Tell(new FrameMessage(clone2));
                    }

                    FramesProcessed++;
                }
                else
                {
                    NullFrameCount++;
                }

                if (NullFrameCount > NullFrameThreshold)
                {
                    LoggingActor.Tell(
                        new LogMessage("Null-frame threshold, performing error-recovery.", LogLevel.Debug, "FrameGrabActor"));
                    LiveCameraErrorRecovery();
                    LoggingActor.Tell(
                        new LogMessage("Error-recovery complete.", LogLevel.Debug, "FrameGrabActor"));
                    NullFrameCount = 0;
                }

                if (fps < LowFpsThreshold)
                {
                    LowFpsCount++;
                }

                if (LowFpsCount > LowFpsCountThreshold)
                {
                    LoggingActor.Tell(new LogMessage("Frame-rate low, performing error-recovery.", LogLevel.Debug, "FrameGrabActor"));
                    LiveCameraErrorRecovery();
                    LoggingActor.Tell(
                        new LogMessage("Error-recovery complete.", LogLevel.Debug, "FrameGrabActor"));
                    LowFpsCount = 0;
                }

                var completed = CaptureSource?.CaptureComplete();
                var isLive = CaptureSource?.IsLiveCapture();
                if (!completed.HasValue || !completed.Value)
                {
                    Context.System.Scheduler.ScheduleTellOnce(0, Self, new GetNextFrameMessage(), Self);
                    return;
                }

                if (isLive.Value)
                {
                    //Don't terminate frame-grab process during live acquisition, even if CaptureComplete indicates that the capture is complete.
                    Context.System.Scheduler.ScheduleTellOnce(0, Self, new GetNextFrameMessage(), Self);
                    return; 
                }

                ProcessingActor?.Tell(new CaptureSourceCompleteMessage());
                LoggingActor?.Tell(new LogUserMessage("Video complete", LogLevel.Info, "FrameGrabActor"));
            }
            catch (TimeoutException ex)
            {
                LoggingActor.Tell(
                    new LogMessage("Frame-query timeout, performing error-recovery.", LogLevel.Debug, "FrameGrabActor"));
                LiveCameraErrorRecovery();
            }

        }

        private Emgu.CV.Image<Emgu.CV.Structure.Bgr,Byte> TimeoutFrameQuery()
        {
            Emgu.CV.Image<Bgr, byte> frame = new Emgu.CV.Image<Bgr, byte>(640,480);
            var task = Task.Run(() => CaptureSource?.QueryFrame());
            bool isCompletedSuccessfully = task.Wait(TimeSpan.FromMilliseconds(FRAME_TIMEOUT_MS));
            if (isCompletedSuccessfully)
            {
                return task.Result;
            }
            else
            {
                throw new TimeoutException("Frame query timeout.");
            }
        }

        private void HandleNewProcessingActor(IActorRef actorRef)
        {
            ProcessingActor = actorRef;
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new FrameGrabActor());
        }

        private void UpdateLoggingActor(IActorRef actorRef)
        {
            try
            {
                LoggingActor = actorRef;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in UpdateLoggingActor:" + ex.Message);
            }
        }

        private void UpdateSequencingActor(IActorRef actorRef)
        {
            try
            {
                SequencingActor = actorRef;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in UpdateSequencingActor:" + ex.Message);
            }
        }

        private void UpdateFramerate()
        {
            if (TotalFramesInVideo == 0)
                return;

            var totalProcessingTime = DateTime.Now - ProcessingStartTime;
            var framesProcessedPerSecond = FramesProcessed / totalProcessingTime.TotalSeconds;
            var estimatedRemainingTimeSeconds = (TotalFramesInVideo - FramesProcessed) / framesProcessedPerSecond;

            if (estimatedRemainingTimeSeconds < 0)
                return;

            if (estimatedRemainingTimeSeconds == Double.PositiveInfinity)
                return;

            //TODO: Investigate how estimatedRemainingTimeSeconds is becoming NaN
            if (estimatedRemainingTimeSeconds == Double.NaN)
            {
                estimatedRemainingTimeSeconds = 0.0;
            }

            var remainingTime = TimeSpan.FromSeconds(estimatedRemainingTimeSeconds);

            var tui = new TrafficCounterUIAccessoryInfo();
            tui.EstimatedTimeRemaining = remainingTime;
            tui.ProcessedFrames = FramesProcessed;
            _updateUiDelegate?.Invoke(tui);
        }

        private void UpdateUiHandler(UpdateUIDelegate handler)
        {
            try
            {
                _updateUiDelegate = handler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in UpdateUiHandler:" + ex.Message);
            }
        }

        private void UpdateVideoProperties(double fps, double totalFrames)
        {
            TotalFramesInVideo = totalFrames;
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);

            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }

        private void LiveCameraErrorRecovery()
        {
            if (CaptureSource is IpCamera oldCaptureSource)
            {
                LoggingActor.Tell(
                    new LogMessage("Error-recovery: deleting and re-initializing CaptureSource: " + oldCaptureSource.Name + " @ " + oldCaptureSource.ConnectionString, LogLevel.Debug, "FrameGrabActor"));
                var newCaptureSource = new IpCamera(oldCaptureSource.Name,oldCaptureSource.ConnectionString);
                CaptureSource?.Destroy();
                CaptureSource = newCaptureSource;
                CaptureSource.Init();
            }
        }

        private void CheckConnectivity()
        {
            if (CaptureSource != null)
            {
                if (!CaptureSource.IsLiveCapture())
                {
                    return;
                }
            }

            //Check time-stamp of last received frame
            if (LastFrameTimestamp == DateTime.MinValue)
            {
                LoggingActor.Tell(
                    new LogMessage("FrameGrab Actor: frame timestamp is not initialized.", LogLevel.Debug, "FrameGrabActor"));
            }
            else
            {
                var ts = DateTime.Now - LastFrameTimestamp;
                LoggingActor.Tell(
                    new LogMessage("FrameGrab Actor: ms since last frame = " + ts.Milliseconds, LogLevel.Debug, "FrameGrabActor"));

                if (ts.Milliseconds > 5000)
                {
                    LiveCameraErrorRecovery();
                }
            }

            LoggingActor.Tell(
                new LogMessage("FrameGrab Actor: null frame count = " + NullFrameCount, LogLevel.Debug, "FrameGrabActor"));
        }
    }
}
