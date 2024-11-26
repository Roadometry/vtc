using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using NLog;
using VTC.Common;
using VTC.Kernel.Video;
using VTC.Messages;
using VTC.UserConfiguration;

namespace VTC.Actors
{
    class SupervisorActor : ReceiveActor
    {
        private FrameGrabActor.UpdateUIDelegate _frameGrabUpdateUiDelegate; //We store this redundantly in SupervisorActor in case FrameGrabActor needs to be restarted.
        private ICaptureSource _captureSource;

        private IActorRef _frameGrabActor;
        private IActorRef _loggingActor;
        private IActorRef _configurationActor;
        private IActorRef _processingActor;
        private IActorRef _sequencingActor;

        public delegate void UpdateActorStatusDelegate(Dictionary<string, DateTime> Statuses);
        private UpdateActorStatusDelegate _updateActorStatusDelegate;

        private Dictionary<string, DateTime> _actorStatuses = new Dictionary<string, DateTime>();

        private static readonly Logger Logger = LogManager.GetLogger("main.form");

        private VTC.Common.UserConfig _userConfig = new UserConfig();

        public SupervisorActor()
        {
            Receive<FrameGrabActorMessage>(message =>
                UpdateFrameGrabActor(message.ActorRef)
            );

            Receive<LoggingActorMessage>(message =>
                UpdateLoggingActor(message.ActorRef)
            );

            Receive<NewProcessingActorMessage>(message =>
                HandleNewProcessingActor(message.ActorRef)
            );

            Receive<SequencingActorMessage>(message =>
                UpdateSequencingActor(message.ActorRef)
            );

            Receive<ConfigurationActorMessage>(message =>
                UpdateConfigurationActor(message.ActorRef)
            );

            Receive<RequestVideoSourceMessage>(message =>
                HandleVideoSourceRequest(message.ActorRef)
            );

            Receive<CreateAllActorsMessage>(message =>
                CreateAllActors(message.UpdateUiDelegate, message.UpdateStatsUiDelegate, message.UpdateInfoUiDelegate, message.UpdateFrameGrabUiDelegate, message.UpdateDebugDelegate)
            );

            Receive<UpdateActorStatusHandlerMessage>(message =>
                HandleNewUpdateActorStatusDelegate(message.UpdateStatusDelegate)
            );

            Receive<ActorHeartbeatMessage>(message =>
                HandleActorHeartbeatMessage(message.FromActor)
            );

            Receive<RestartFrameGrabActorMessage>(message =>
                RestartFrameGrabActor()
            );

            Receive<LoadUserConfigMessage>(message => 
                LoadUserConfig()
            );

            Receive<NewVideoSourceMessage>(message => 
                StoreVideoSourceInfo(message.CaptureSource)
            );

            Receive<CheckActorLiveness>(message => 
                CheckLiveness()
            );

            Receive<Failure>(message =>
                HandleFailure(message.Exception)
            );

            Self.Tell(new LoadUserConfigMessage());

            Context.System.Scheduler.ScheduleTellRepeatedly(60000, 60000, Self, new CheckActorLiveness(), Self);
        }

        void UpdateFrameGrabActor(IActorRef actor)
        {
            _frameGrabActor = actor;
        }

        void UpdateLoggingActor(IActorRef actor)
        {
            _loggingActor = actor;
        }

        void HandleNewProcessingActor(IActorRef actor)
        {
            _processingActor = actor;
        }

        void UpdateSequencingActor(IActorRef actor)
        {
            _sequencingActor = actor;
        }

        void UpdateConfigurationActor(IActorRef actor)
        {
            _configurationActor = actor;
        }

        void HandleNewUpdateActorStatusDelegate(UpdateActorStatusDelegate updateDelegate)
        {
            _updateActorStatusDelegate = updateDelegate;
        }

        void HandleActorHeartbeatMessage(IActorRef sender)
        {
            if (sender == null)
            {
                return;
            }

            if (!_actorStatuses.ContainsKey(Sender.Path.Name))
            {
                _actorStatuses.Add(Sender.Path.Name, DateTime.Now);
                UpdateActorStatusIndicators();
            }
            else
            {
                _actorStatuses[Sender.Path.Name] = DateTime.Now;
                UpdateActorStatusIndicators();
            }
        }

        void IntroduceAllActors()
        {
            //Introduce actors to each other
            _processingActor.Tell(new LoggingActorMessage(_loggingActor));
            _processingActor.Tell(new ConfigurationActorMessage(_configurationActor));
            _sequencingActor.Tell(new LoggingActorMessage(_loggingActor));
            _sequencingActor.Tell(new NewProcessingActorMessage(_processingActor));
            _sequencingActor.Tell(new ConfigurationActorMessage(_configurationActor));
            _configurationActor.Tell(new NewProcessingActorMessage(_processingActor));
            _configurationActor.Tell(new SequencingActorMessage(_sequencingActor));
            _configurationActor.Tell(new LoggingActorMessage(_loggingActor));
            _configurationActor.Tell(new SupervisorActorMessage(Self));
            _loggingActor.Tell(new SequencingActorMessage(_sequencingActor));
            _loggingActor.Tell(new ConfigurationActorMessage(_configurationActor));
            IntroduceNewFrameGrabActor();
        }

        void IntroduceNewFrameGrabActor()
        {
            _sequencingActor.Tell(new FrameGrabActorMessage(_frameGrabActor));
            _configurationActor.Tell(new FrameGrabActorMessage(_frameGrabActor));
            _frameGrabActor.Tell(new NewProcessingActorMessage(_processingActor));
            _frameGrabActor.Tell(new LoggingActorMessage(_loggingActor));
            _frameGrabActor.Tell(new SequencingActorMessage(_sequencingActor));
        }

        void CreateAllActors(ProcessingActor.UpdateUIDelegate updateUiDelegate, TrafficCounter.UpdateStatsUIDelegate statsUiDelegate, TrafficCounter.UpdateInfoUIDelegate infoUiDelegate, FrameGrabActor.UpdateUIDelegate frameGrabUiDelegate, TrafficCounter.UpdateDebugDelegate debugDelegate)
        {
            _processingActor = Context.ActorOf(Props.Create(typeof(ProcessingActor)).WithMailbox("processing-bounded-mailbox"), "ProcessingActor");
            _processingActor.Tell(new UpdateUiHandlerMessage(updateUiDelegate));
            _frameGrabActor = Context.ActorOf(Props.Create(typeof(FrameGrabActor)).WithMailbox("processing-bounded-mailbox"),"FrameGrabActor");
            _frameGrabActor.Tell(new UpdateUiAccessoryHandlerMessage(frameGrabUiDelegate));
            _frameGrabUpdateUiDelegate = frameGrabUiDelegate;
            _loggingActor = Context.ActorOf(Props.Create(typeof(LoggingActor)).WithMailbox("processing-bounded-mailbox"),"LoggingActor");
            _loggingActor.Tell(new UpdateStatsUiHandlerMessage(statsUiDelegate));
            _loggingActor.Tell(new UpdateInfoUiHandlerMessage(infoUiDelegate));
            _loggingActor.Tell(new UpdateDebugHandlerMessage(debugDelegate));
            _sequencingActor = Context.ActorOf(Props.Create(typeof(SequencingActor)).WithMailbox("processing-bounded-mailbox"),"SequencingActor");
            _sequencingActor.Tell(new UpdateInfoUiHandlerMessage(infoUiDelegate));
            _configurationActor = Context.System.ActorOf(Props.Create(() => new
                ConfigurationActor()).WithDispatcher("synchronized-dispatcher"), "ConfigurationActor");

            IntroduceAllActors();
        }

        void UpdateActorStatusIndicators()
        {
            _updateActorStatusDelegate?.Invoke(_actorStatuses);
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);

            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }

        private void HandleVideoSourceRequest(IActorRef actor)
        {
            if (_captureSource != null)
            {
                actor.Tell(new NewVideoSourceMessage(_captureSource));
            }
        }

        private void RestartFrameGrabActor()
        {
            //Stop old frame-grab actor and pause.
            Context.Stop(_frameGrabActor);

            //Create new frame-grab actor.
            _frameGrabActor = Context.ActorOf(Props.Create(typeof(FrameGrabActor)).WithMailbox("processing-bounded-mailbox"),"FrameGrabActor");
            if (_frameGrabUpdateUiDelegate != null)
            {
                _frameGrabActor.Tell(new UpdateUiAccessoryHandlerMessage(_frameGrabUpdateUiDelegate));
            }

            //Restart and reintroduce.
            IntroduceNewFrameGrabActor();

            if (_captureSource.IsLiveCapture())
            {
                _frameGrabActor.Tell(new NewVideoSourceMessage(_captureSource));
                _frameGrabActor.Tell(new GetNextFrameMessage());

                _loggingActor?.Tell(
                    new LogMessage("Supervisor Actor: Frame-grab actor is restarted using live source " + _captureSource.Name, LogLevel.Debug, "SupervisorActor"));
            }
            else
            {
                _loggingActor?.Tell(
                    new LogMessage("Supervisor Actor: Frame-grab actor is restarted. No live source.", LogLevel.Debug, "SupervisorActor"));
            }
        }

        private void StoreVideoSourceInfo(ICaptureSource source)
        {
            _captureSource = source;
        }

        private void CheckLiveness()
        {
            UpdateActorStatusIndicators();

            var timeSinceFrameGrabHeartbeat = DateTime.Now - _actorStatuses["FrameGrabActor"];
            if (timeSinceFrameGrabHeartbeat.Seconds > 60)
            {
                _loggingActor?.Tell(
                    new LogMessage("Supervisor Actor: FrameGrab Actor heartbeat is stale, restarting.", LogLevel.Debug, "SupervisorActor"));
                RestartFrameGrabActor();
            }
        }

        private void HandleFailure(Exception ex)
        {
            Log(ex.Message + " from " + ex.Source + " via " + ex.StackTrace + "(InnerException: " + ex.InnerException + ")", LogLevel.Error, "SupervisorActor");
        }

        private void Log(string text, LogLevel level, string actorName)
        {
            Logger.Log(level, actorName + ":" + text);
        }

    }
}
