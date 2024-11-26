using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Messaging;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Akka.Actor;
using Emgu.CV;
using NLog;
using VTC.CaptureSource;
using VTC.Common;
using VTC.Common.RegionConfig;
using VTC.Kernel.Video;
using VTC.Messages;
using VTC.UserConfiguration;

namespace VTC.Actors
{
    class SequencingActor : ReceiveActor
    {
        private readonly List<ICaptureSource> _cameras = new List<ICaptureSource>(); //List of all video input devices. Index, file location, name
        private List<BatchVideoJob> _videoJobs;
        private BatchVideoJob _currentJob;

        private IActorRef _loggingActor;

        private IActorRef _frameGrabActor;

        private IActorRef _processingActor;
        private bool _processingActorInitialized = false;

        private bool _splitVideoMode = false;
        private bool _startedFirstSplitVideo = false;

        private IActorRef _configActor;
        private Dictionary<VideoProcessingRequestMessage, BatchVideoJob> _automationRequestJobs;
        private string _currentVideoName = "";

        private TrafficCounter.UpdateInfoUIDelegate _updateInfoUiDelegate;

        private VTC.Common.UserConfig _userConfig = new UserConfig();

        public SequencingActor()
        {
            _videoJobs = new List<BatchVideoJob>();
            _automationRequestJobs = new Dictionary<VideoProcessingRequestMessage, BatchVideoJob>();

            Receive<LoggingActorMessage>(message =>
                UpdateLoggingActor(message.ActorRef)
            );

            Receive<NewProcessingActorMessage>(message =>
                HandleNewProcessingActor(message.ActorRef)
            );

            Receive<FrameGrabActorMessage>(message =>
                HandleNewFrameGrabActor(message.ActorRef)
            );

            Receive<ConfigurationActorMessage>(message =>
                HandleNewConfigurationActor(message.ActorRef)
            );

            Receive<CaptureSourceCompleteMessage>(message =>
                SendNotificationForLastAndDequeue()
            );

            Receive<VideoJobsMessage>(message =>
                ResetAndEnqueueVideos(message.VideoJobsList)
            );

            Receive<ActorHeartbeatMessage>(message =>
                Heartbeat()
            );

            Receive<CheckVideoProcessingRequestsIPCMessage>(message =>
                CheckForNewJobs()
            );

            Receive<RegionConfigLookupResponseMessage>(message =>
                AssociateJobWithConfiguration(message.Configuration, message.JobId)
            );

            Receive<LoadUserConfigMessage>(message => 
                LoadUserConfig()
            );

            Receive<UpdateInfoUiHandlerMessage>(message =>
                UpdateInfoUiHandler(message.InfoUiDelegate)
            );

            Receive<InitializationCompleteMessage>(message =>
                UpdateInitializationStatus(message.Actor)
            );

            Receive<BeginSplitVideoMessage>(message =>
                BeginSplitVideoMode()
            );

            Self.Tell(new ActorHeartbeatMessage(Self));

            Self.Tell(new LoadUserConfigMessage());
        }

        private void SendNotificationForLastAndDequeue()
        {
            if (_currentJob == null)
            {
                _loggingActor.Tell(new LogMessage("SequencingActor thinks a non-existant job has completed. This can happen with unreliable IP-camera streams.", LogLevel.Error, "SequencingActor"));
                return;
            }
            
            DequeueVideo();
        }

        private void CheckForNewJobs()
        {
        }

        private void DequeueVideo()
        {
            if (_videoJobs.Count > 0)
            {
                if (_splitVideoMode)
                {
                    _loggingActor?.Tell(new LogUserMessage("Loading split-video from batch", LogLevel.Info, "SequencingActor"));
                }
                else
                {
                    _loggingActor?.Tell(new LogUserMessage("Loading video from batch", LogLevel.Info, "SequencingActor"));
                }

                
                _currentJob = _videoJobs.First();

                if (_currentJob.RegionConfiguration != null)
                {
                    var captureSource = LoadCameraFromFilename(_currentJob.VideoPath);

                    if (!_splitVideoMode || !_startedFirstSplitVideo)
                    {
                        _currentJob.StartDateTime = DateTime.Now;

                        if (_userConfig.AllowUserOverrideDatetime)
                        {
                            var overrideTimeForm = new VideoDateTimeForm(captureSource.StartDateTime());
                            overrideTimeForm.ShowDialog();
                            captureSource._startDate = overrideTimeForm.OverrideTime;
                        }
                    }

                    _frameGrabActor.Tell(new NewVideoSourceMessage(captureSource));
                    _currentVideoName = captureSource.Name;
                    _frameGrabActor.Tell(new UpdateVideoPropertiesMessage(captureSource.FPS(), captureSource.FrameCount));

                    if (_splitVideoMode == false || _startedFirstSplitVideo == false)
                    {
                        _loggingActor.Tell(new NewVideoSourceMessage(captureSource));
                        _loggingActor.Tell(new FileCreationTimeMessage(captureSource.StartDateTime()));
                        _processingActor.Tell(new UpdateRegionConfigurationMessage(_currentJob.RegionConfiguration));
                        _loggingActor.Tell(new UpdateRegionConfigurationMessage(_currentJob.RegionConfiguration));
                        _loggingActor.Tell(new CopyGroundtruthMessage(_currentJob.GroundTruthPath));
                        _loggingActor.Tell(new NewBatchJobMessage(_currentJob));
                    }

                    _frameGrabActor.Tell(new GetNextFrameMessage());
                }
                else
                {
                    Log(LogLevel.Warn, $"Ignoring job without configuration ({_currentJob.VideoPath})");
                    UserLog("Ignoring job without configuration");
                }

                if (_splitVideoMode && _startedFirstSplitVideo == false)
                {
                    _startedFirstSplitVideo = true;
                    UserLog("Start processing first segment.");
                }

                _videoJobs.Remove(_currentJob);
            }
            else
            {
                UserLog("Batch complete.");

                var outputPathString = _userConfig.OutputPath;
                if(string.IsNullOrEmpty(outputPathString))
                {
                    outputPathString = "desktop";
                }

                UserLog("Movement counts saved to " + outputPathString);

                _splitVideoMode = false;
                _startedFirstSplitVideo = false;
            }
        }

        private VideoFileCapture LoadCameraFromFilename(string filename)
        {
            var vfc = new VideoFileCapture(filename);
            AddCamera(vfc);
            return vfc;
        }

        private void AddCamera(ICaptureSource camera)
        {
            _cameras.Add(camera);
        }

        private void Log(LogLevel logLevel, string text)
        {
            _loggingActor?.Tell(new LogMessage(text, logLevel, "SequencingActor"));
        }

        private void UserLog(string text)
        {
            _loggingActor?.Tell(new LogUserMessage(text, LogLevel.Info, "SequencingActor"));
        }

        private void UpdateLoggingActor(IActorRef actorRef)
        {
            try
            {
                _loggingActor = actorRef;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in UpdateLoggingActor:" + ex.Message);
            }
        }

        private void HandleNewProcessingActor(IActorRef actorRef)
        {
            _processingActor = actorRef;
        }

        private void HandleNewFrameGrabActor(IActorRef actorRef)
        {
            _frameGrabActor = actorRef;
        }

        private void HandleNewConfigurationActor(IActorRef actorRef)
        {
            _configActor = actorRef;
        }

        private void Heartbeat()
        {
            Context.Parent.Tell(new ActorHeartbeatMessage(Self));
            //TODO: Call function to check for new jobs here
            Context.System.Scheduler.ScheduleTellOnce(5000, Self, new ActorHeartbeatMessage(Self), Self);
            Self.Tell(new CheckVideoProcessingRequestsIPCMessage());
        }

        private void EnqueueVideoJobs(List<BatchVideoJob> newVideoJobs)
        {
            foreach (var videoJob in newVideoJobs)
            {
                _videoJobs.Add(videoJob);
            }
        }

        private void ResetAndEnqueueVideos(List<BatchVideoJob> newVideoJobs)
        {
            _cameras.Clear();
            _videoJobs.AddRange(newVideoJobs);
            DequeueVideo();
        }

        private void AssociateJobWithConfiguration(RegionConfig config, int jobId)
        {
            bool eachHasConfiguration = true;
            Console.WriteLine("Got config:" + config.Title);
            foreach (var bvj in _automationRequestJobs.Values)
            {
                if (bvj.Id == jobId)
                {
                    bvj.RegionConfiguration = config;
                    EnqueueVideoJobs(new List<BatchVideoJob> { bvj });
                }

                if (bvj.RegionConfiguration == null)
                {
                    eachHasConfiguration = false;
                }
            }

            if (!eachHasConfiguration) return;

            //When all videos have recieved their configuration callbacks, begin processing
            _cameras.Clear();
            DequeueVideo();
        }

        private void BeginSplitVideoMode()
        {
            _splitVideoMode = true;
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);

            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }

        private void UpdateInitializationStatus(IActorRef actor)
        { 
            if(actor == _processingActor)
            { 
                _processingActorInitialized = true;    
            }            
        }

        private void UpdateInfoUiHandler(TrafficCounter.UpdateInfoUIDelegate handler)
        {
            try
            {
                _updateInfoUiDelegate = handler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("(UpdateInfoUiHandler) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite);
            }

        }
    }

    [Serializable]
    public class VideoProcessingRequestMessage
    {
        public string VideoFilePath = "";
        public string ConfigurationName = "";
        public string ManualCountsPath = "";
        public int JobId;
    }
}
