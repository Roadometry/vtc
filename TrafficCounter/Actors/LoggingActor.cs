using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceProcess;
using System.Text;
using System.Messaging;
using System.Windows.Forms;
using Akka.Actor;
using Emgu.CV;
using Emgu.CV.Structure;
using NLog;
using Npgsql;
using Sentry;
//using Sentry.Protocol;
using VTC.Classifier;
using VTC.Common;
using VTC.Common.RegionConfig;
using VTC.Kernel;
using VTC.Messages;
using VTC.Remote;
using VTC.db;
using VTC.Reporting;
using VTC.UserConfiguration;

namespace VTC.Actors
{
    public class LoggingActor : ReceiveActor
    {
        private UInt64 _nFramesProcessed = 0;
        private double _fps = 24.0;

        private bool _liveMode = true;

        private readonly MovementCount _turnStats = new MovementCount();

        private static readonly Logger Logger = LogManager.GetLogger("main.form");
        private static readonly Logger UserLogger = LogManager.GetLogger("userlog"); // special logger for user messages

        private RegionConfig _regionConfig = new RegionConfig();
        private EventConfig _eventConfig;
        private string _currentVideoName = "Unknown video";
        private DateTime _videoStartTime = DateTime.Now;
        private string _currentOutputFolder;
        private BatchVideoJob _currentJob;
        private DbConnection _dbConnection;
        private bool _splitVideoMode = false;
        private bool _receivedFirstSplitVideo = false;

        private MultipleTrajectorySynthesizer mts;

        private IActorRef _sequencingActor;
        private IActorRef _configurationActor;
        private Image<Bgr, byte> _background;

        //public delegate void UpdateStatsUIDelegate(string statString);
        private TrafficCounter.UpdateStatsUIDelegate _updateStatsUiDelegate;

        //public delegate void UpdateInfoUIDelegate(string infoString);
        private TrafficCounter.UpdateInfoUIDelegate _updateInfoUiDelegate;

        //public delegate void UpdateDebugDelegate(string debugString);
        private TrafficCounter.UpdateDebugDelegate _updateDebugDelegate;

        private YoloIntegerNameMapping _yoloNameMapping = new YoloIntegerNameMapping();

        private VTC.Common.UserConfig _userConfig = new UserConfig();

        private List<Movement> MovementTransmitBuffer = new List<Movement>();

        public LoggingActor()
        {
            InitializeEventConfig();

            mts = new MultipleTrajectorySynthesizer();

            Receive<FileCreationTimeMessage>(message =>
                UpdateFileCreationTime(message.FileCreationTime)
            );

            Receive<LogMessage>(message =>
                Log(message.Text, message.Level, message.ActorName)
            );

            Receive<LogUserMessage>(message =>
                LogUser(message.Text, message.Level, message.ActorName)
            );

            Receive<GenerateReportMessage>(message =>
                GenerateReport()
            );

            Receive<NewVideoSourceMessage>(message =>
                UpdateVideoSourceInfo(message)
            );

            ReceiveAsync<TrackingEventMessage>(async message =>
                TrajectoryListHandler(message.EventArgs)
            );

            Receive<UpdateRegionConfigurationMessage>(message =>
                UpdateConfig(message.Config)
            );

            ReceiveAsync<HandleUpdatedBackgroundFrameMessage>(async message =>
                UpdateBackgroundFrame(message.Frame)
            );

            Receive<CaptureSourceCompleteMessage>(message =>
                OnCaptureSourceComplete()
            );

            Receive<GenerateDailyReportMessage>(message =>
                GenerateDailyReport()
            );

            Receive<UpdateStatsUiHandlerMessage>(message =>
                UpdateStatsUiHandler(message.StatsUiDelegate)
            );

            Receive<UpdateInfoUiHandlerMessage>(message =>
                UpdateInfoUiHandler(message.InfoUiDelegate)
            );

            Receive<UpdateDebugHandlerMessage>(message =>
                UpdateDebugHandler(message.DebugDelegate)
            );

            ReceiveAsync<DashboardHeartbeatMessage>(async message =>
                DashboardHeartbeat()
            );

            Receive<SequencingActorMessage>(message =>
                UpdateSequencingActor(message.ActorRef)
            );

            Receive<ConfigurationActorMessage>(message =>
                UpdateConfigurationActor(message.ActorRef)
            );

            Receive<CopyGroundtruthMessage>(message =>
                CopyGroundTruth(message.GroundTruthPath)
            );

            Receive<FrameCountMessage>(message => 
                UpdateFrameCount(message.Count)
            );

            Receive<ResetDatabaseMessage>(message =>
                ResetDatabase()
            );

            Receive<LoadUserConfigMessage>(message => 
                LoadUserConfig()
            );

            Receive<ValidateConfigurationMessage>(message =>
                CheckConfiguration()
            );

            ReceiveAsync<FlushBuffersMessage>(async message =>
                FlushTransmitBuffer()
            );

            Receive<ClearStatsMessage>(message =>
                ClearTurnStats()
            );

            Receive<NewBatchJobMessage>(message =>
                UpdateBatchJob(message.Job)
            );

            Receive<BeginSplitVideoMessage>(message =>
                BeginSplitVideoMode()
            );

            InitializeDatabase();

            Self.Tell(new LoadUserConfigMessage());

            Context.Parent.Tell(new RequestVideoSourceMessage(Self));

            Context.System.Scheduler.ScheduleTellRepeatedly(new TimeSpan(0, 1, 0),new TimeSpan(0,5,0),Self, new DashboardHeartbeatMessage(), Self);
            Context.System.Scheduler.ScheduleTellRepeatedly(new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0), Self, new FlushBuffersMessage(), Self);
            Context.System.Scheduler.ScheduleTellRepeatedly(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 5), Context.Parent, new ActorHeartbeatMessage(Self), Self);

            Log("LoggingActor initialized.", LogLevel.Info, "LoggingActor");
        }

        protected override void PreStart()
        {
            base.PreStart();
        }

        protected override void PreRestart(Exception cause, object msg)
        {
            Log("(PreRestart) Restarting due to " + cause.Message + " at " + cause.TargetSite + ", Trace:" + cause.StackTrace, LogLevel.Error, "LoggingActor");
            base.PreRestart(cause, msg);
        }

        protected override void PostRestart(Exception cause)
        {
            Log("(PostRestart) restarted due to " + cause.Message + " at " + cause.TargetSite + ", Trace:" + cause.StackTrace, LogLevel.Error, "LoggingActor");
            base.PostRestart(cause);
        }

        protected override void PostStop()
        {
            _dbConnection.Close();
            base.PostStop();
        }

        private void InitializeDatabase()
        {
            _dbConnection = DatabaseManager.OpenConnection(_userConfig);
        }

        private void ResetDatabase()
        {
            try
            {
                DatabaseManager.ResetDatabase(_dbConnection, _userConfig);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        private void UpdateFileCreationTime(DateTime dt)
        {
            Log("New file creation time " + dt, LogLevel.Info, "LoggingActor");
            _videoStartTime = dt;
        }

        private void BeginSplitVideoMode()
        {
            _splitVideoMode = true;
            _receivedFirstSplitVideo = false;
        }

        private void CreateOrReplaceOutputFolderIfExists()
        {
            //Create output folder
            //TODO: Figure out how to get _selectedCamera.Name value here
            var folderPath = VTC.Common.VTCPaths.FolderPath(_currentVideoName,_videoStartTime, _userConfig);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
            _currentOutputFolder = folderPath;
        }
        
        private void ClearTurnStats()
        {
            _turnStats.Clear();
        }

        private void Log(string text, LogLevel level, string actorName)
        {
            Logger.Log(level, actorName + ":" + text);
            if (level == LogLevel.Error)
            {
                var ev = new SentryEvent {Level = Sentry.SentryLevel.Error, Message = actorName + ": " + text};
                SentrySdk.CaptureEvent(ev);
            }
        }

        private void LogUser(string text, LogLevel level, string actorName)
        {
            UserLogger.Log(level, text);
            _updateInfoUiDelegate?.Invoke(text);
        }

        private void OnCaptureSourceComplete()
        {
            GenerateReport();
            _splitVideoMode = false;
            _receivedFirstSplitVideo = false;
        }

        private void GenerateReport()
        {
            try
            {

                var movements = DatabaseManager.GetMovementsByJob(_dbConnection, _currentJob.Id);
                CreateOrReplaceOutputFolderIfExists();

                var folderPath = _currentOutputFolder;
                GenerateRegionsLegendImage(folderPath);

                var tnow = VideoTime();

                ReportGenerator.CopyAssetsToExportFolder(folderPath);
                ReportGenerator.GenerateSummaryReportHtml(folderPath, _currentVideoName,  tnow, movements);
                ReportGenerator.GenerateCSVReport(folderPath, _currentVideoName, tnow, movements);

                _sequencingActor?.Tell(new CaptureSourceCompleteMessage(folderPath));

                var ev = new SentryEvent {Level = SentryLevel.Error, Message = "Report generated"};
                SentrySdk.CaptureEvent(ev);
            }
            catch (NullReferenceException e)
            {
                Logger.Log(LogLevel.Error, "(GenerateReport) " + e);
            }

        }

        private void GenerateDailyReport()
        {
            if(!_liveMode)
            {
                return;
            }

            Logger.Log(LogLevel.Info, "Generating daily report...");

            GenerateReport();

            _videoStartTime = DateTime.Now;
            CreateOrReplaceOutputFolderIfExists();

            Logger.Log(LogLevel.Info, "Daily report generated. Starting new day.");
        }

        private void GenerateRegionsLegendImage(string folderPath)
        {
            try
            {
                var maskedBackground = _background.Clone();
                foreach (var p in _regionConfig.Regions)
                {
                    var mask = p.Value.GetMask(_background.Width, _background.Height, new Bgr(60, 60, 60)).Convert<Bgr, byte>();
                    maskedBackground = maskedBackground.Add(mask);
                }

                var bmp = maskedBackground.ToBitmap();
                var g = Graphics.FromImage(bmp);
                foreach (var p in _regionConfig.Regions)
                {
                    if (p.Value.Count <= 2) continue;
                    var x = p.Value.Centroid.X;
                    var y = p.Value.Centroid.Y;
                    var font = new Font(FontFamily.GenericSansSerif, (float)14.0);
                    var size = TextRenderer.MeasureText(p.Key, font);
                    var name = RemapRegionName(p.Key);
                    g.DrawString(name, font,
                        new SolidBrush(Color.Black), x - size.Width / 2, y);
                }

                var examplePaths = mts.GenerateExamplePathTrajectories(_regionConfig);
                foreach (var st in examplePaths)
                {
                    
                    var font = new Font(FontFamily.GenericSansSerif, (float)10.0);
                    var size = TextRenderer.MeasureText(st.Approach, font);
                    g.DrawString(st.Approach, font,
                        new SolidBrush(Color.Black),(float) st.StateEstimates.First().X - (float) (size.Width / 2.0), (float) st.StateEstimates.First().Y);

                    size = TextRenderer.MeasureText(st.Exit, font);
                    g.DrawString(st.Exit, font,
                        new SolidBrush(Color.Black), (float)st.StateEstimates.Last().X - (float)(size.Width / 2.0), (float)st.StateEstimates.Last().Y);
                }

                bmp.Save(Path.Combine(folderPath,
                    "RegionsLegend.png"));
            }
            catch (Exception ex)
            {
                //TODO: Deal with exception
                Logger.Log(LogLevel.Error, "(GenerateRegionsLegendImage) " + ex);
            }
        }

        private void UpdateVideoSourceInfo(NewVideoSourceMessage message)
        {
            Logger.Log(LogLevel.Info, "New video source " + message.CaptureSource.Name);

            
            if (_splitVideoMode == false || _receivedFirstSplitVideo == false)
            {
                ClearTurnStats();

                _currentVideoName = message.CaptureSource.Name;
                var ev = new SentryEvent { Level = SentryLevel.Info, Message = "New video source" };
                SentrySdk.CaptureEvent(ev);

                _videoStartTime = message.CaptureSource.StartDateTime();
                _liveMode = message.CaptureSource.IsLiveCapture();
                _fps = message.CaptureSource.FPS();
            }

            if (_splitVideoMode)
            {
                _receivedFirstSplitVideo = true;
            }
        }

        private void UpdateConfig(RegionConfig config)
        {
            Log("(UpdateConfig) ", LogLevel.Info, "LoggingActor");

            try
            {
                _regionConfig = config;
                mts.GenerateSyntheticTrajectories(_regionConfig);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "(UpdateConfig) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite);
            }

        }

        private async void TrajectoryListHandler(TrackingEvents.TrajectoryListEventArgs args)
        {
            if (_regionConfig == null)
            {
                Log("_regionConfig is null in TrajectoryListHandler.", LogLevel.Error, "LoggingActor");
                _configurationActor?.Tell(new RequestConfigurationMessage(Self));
                return;
            }

            if (_yoloNameMapping == null)
            {
                Log("_yoloNameMapping is null in TrajectoryListHandler.", LogLevel.Error, "LoggingActor");
                return;
            }

            if (_userConfig == null)
            {
                Log("_userConfig: _currentOutputFolder is null in TrajectoryListHandler.", LogLevel.Error, "LoggingActor");
                return;
            }

            try
            {
                foreach (var d in args.TrackedObjects)
                {
                    var mostFrequentClassId = d.StateHistory.Last().MostFrequentClassId();
                    var mostLikelyClassType =
                        YoloIntegerNameMapping.GetObjectNameFromClassInteger(mostFrequentClassId,
                            _yoloNameMapping.IntegerToObjectName);

                    if (mostLikelyClassType == "person" && _userConfig.CountPedestriansAsMotorcycles)
                    {
                        mostLikelyClassType = "motorcycle";
                    }

                    if (mostLikelyClassType == "motorcycle" && _userConfig.CountMotorcyclesAsCars)
                    {
                        mostLikelyClassType = "car";
                    }

                    if (mostLikelyClassType == "bus" && _userConfig.CountBusesAsTrucks)
                    {
                        mostLikelyClassType = "truck";
                    }

                    var validity = TrajectorySimilarity.ValidateTrajectory(d,  _regionConfig.MinPathLength, _regionConfig.MissRatioThreshold, _regionConfig.PositionCovarianceThreshold, _regionConfig.SmoothnessThreshold, _regionConfig.MovementLengthRatio);
                    if(validity.valid == false)
                    {
                        _updateDebugDelegate?.Invoke(validity.description);
                        continue;
                    }
                    var movement = TrajectorySimilarity.MatchNearestTrajectory(d, mostLikelyClassType, _regionConfig.MinPathLength, mts.TrajectoryPrototypes);
                    if (movement == null) continue;
                    if (movement.Ignored) continue;
                    var uppercaseClassType = CommonFunctions.FirstCharToUpper(mostLikelyClassType);
                    var mappedApproach = RemapApproachName(movement.Approach);
                    var mappedExit = RemapExitName(movement.Exit);
                    var editedMovement = new Movement(mappedApproach, mappedExit, movement.TurnType, (ObjectType) Enum.Parse(typeof(ObjectType),uppercaseClassType), d.StateHistory, VideoTime(), false, _currentJob.Id);
                    IncrementTurnStatistics(editedMovement);
                    editedMovement.Save(_dbConnection,_userConfig);

                    if (_regionConfig.SendToServer)
                    {
                        if(string.IsNullOrEmpty(_regionConfig.SiteToken))
                        {
                            Log("Bad site token", LogLevel.Error, "LoggingActor");
                            return;
                        }

                        if (string.IsNullOrEmpty((_userConfig
                            .ServerUrl)))
                        {
                            Log("Bad server URL", LogLevel.Error, "LoggingActor");
                            return;
                        }

                        try
                        {
                            var rs = new RemoteServer();
                            var rsr = await rs.SendMovement(editedMovement, _regionConfig.SiteToken, _userConfig.ServerUrl);
                            //TODO: Do we want to log these errors? - Alex
                            //if (rsr != HttpStatusCode.OK)
                            //{
                            //   Log("Movement POST failed:" + rsr, LogLevel.Error, "LoggingActor");
                            //}
                        }
                        catch (HttpRequestException httpEx)
                        {
                            Log("Buffering movement for re-send", LogLevel.Warn, "LoggingActor");
                            MovementTransmitBuffer.Add(editedMovement);
                        }
                    }
                }

                var stats = GetStatString();
                _updateStatsUiDelegate?.Invoke(stats);
            }
            catch(Exception ex)
            { 
                Log("(TrajectoryListHandler) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite, LogLevel.Error, "LoggingActor");
            }
        }

        private string RemapApproachName(string approach)
        {
            if (approach == "Approach 1" && !string.IsNullOrEmpty(_regionConfig.Approach1Name))
            {
                return _regionConfig.Approach1Name;
            }

            if (approach == "Approach 2" && !string.IsNullOrEmpty(_regionConfig.Approach2Name))
            {
                return _regionConfig.Approach2Name;
            }

            if (approach == "Approach 3" && !string.IsNullOrEmpty(_regionConfig.Approach3Name))
            {
                return _regionConfig.Approach3Name;
            }

            if (approach == "Approach 4" && !string.IsNullOrEmpty(_regionConfig.Approach4Name))
            {
                return _regionConfig.Approach4Name;
            }

            return approach;
        }

        private string RemapExitName(string exit)
        {
            if (exit == "Exit 1" && !string.IsNullOrEmpty(_regionConfig.Exit1Name))
            {
                return _regionConfig.Exit1Name;
            }

            if (exit == "Exit 2" && !string.IsNullOrEmpty(_regionConfig.Exit2Name))
            {
                return _regionConfig.Exit2Name;
            }

            if (exit == "Exit 3" && !string.IsNullOrEmpty(_regionConfig.Exit3Name))
            {
                return _regionConfig.Exit3Name;
            }

            if (exit == "Exit 4" && !string.IsNullOrEmpty(_regionConfig.Exit4Name))
            {
                return _regionConfig.Exit4Name;
            }

            return exit;
        }

        string RemapRegionName(string region)
        {
            if (region.Contains("Approach"))
            {
                return RemapApproachName(region);
            }

            if (region.Contains("Exit"))
            {
                return RemapExitName(region);
            }

            return region;
        }

        private void IncrementTurnStatistics(Movement tp)
        {
            if (!_turnStats.ContainsKey(tp)) _turnStats[tp] = 0;
            {
                _turnStats[tp]++;
            }
        }

        private DateTime VideoTime()
        { 
            if(_liveMode)
            {
                return DateTime.Now;
            }

            DateTime videoTime = _videoStartTime.AddSeconds(_nFramesProcessed/_fps);
            return videoTime;
        }

        private void InitializeEventConfig()
        {
            //Hard-coded configuration for a standard intersection. TODO: Load configuration from eventConfig.xml 
            _eventConfig = new EventConfig();
            _eventConfig.Events.Add(new RegionTransition(1, 1, false), "straight");
            _eventConfig.Events.Add(new RegionTransition(2, 2, false), "straight");
            _eventConfig.Events.Add(new RegionTransition(3, 3, false), "straight");
            _eventConfig.Events.Add(new RegionTransition(4, 4, false), "straight");

            _eventConfig.Events.Add(new RegionTransition(1, 2, false), "left");
            _eventConfig.Events.Add(new RegionTransition(2, 3, false), "left");
            _eventConfig.Events.Add(new RegionTransition(3, 4, false), "left");
            _eventConfig.Events.Add(new RegionTransition(4, 1, false), "left");

            _eventConfig.Events.Add(new RegionTransition(1, 4, false), "right");
            _eventConfig.Events.Add(new RegionTransition(2, 1, false), "right");
            _eventConfig.Events.Add(new RegionTransition(3, 2, false), "right");
            _eventConfig.Events.Add(new RegionTransition(4, 3, false), "right");

            _eventConfig.Events.Add(new RegionTransition(1, 3, false), "uturn");
            _eventConfig.Events.Add(new RegionTransition(2, 4, false), "uturn");
            _eventConfig.Events.Add(new RegionTransition(3, 1, false), "uturn");
            _eventConfig.Events.Add(new RegionTransition(4, 2, false), "uturn");

            _eventConfig.Events.Add(new RegionTransition(1, 2, true), "crossing" );
            _eventConfig.Events.Add(new RegionTransition(2, 1, true), "crossing" );

            _eventConfig.Events.Add(new RegionTransition(2, 3, true), "crossing" );
            _eventConfig.Events.Add(new RegionTransition(3, 2, true), "crossing" );

            _eventConfig.Events.Add(new RegionTransition(3, 4, true), "crossing" );
            _eventConfig.Events.Add(new RegionTransition(4, 3, true), "crossing" );

            _eventConfig.Events.Add(new RegionTransition(4, 1, true), "crossing" );
            _eventConfig.Events.Add(new RegionTransition(1, 4, true), "crossing" );
        }

        private async void UpdateBackgroundFrame(Image<Bgr, byte> image)
        {
            try
            {
                if (image == null)
                {
                    Log("received null-image in UpdateBackgroundFrame.", LogLevel.Error, "LoggingActor");
                    return;
                }

                if (_regionConfig == null)
                {
                    Log("_regionConfig is null in UpdateBackgroundFrame.", LogLevel.Error, "LoggingActor");
                    _configurationActor?.Tell(new RequestConfigurationMessage(Self));
                    return;
                }

                if (_userConfig == null)
                {
                    Log("_userConfig is null in UpdateBackgroundFrame.", LogLevel.Error, "LoggingActor");
                    return;
                }

                _background = image.Clone();
                image.Dispose();

                if (_regionConfig.SendToServer)
                {
                    var rs = new RemoteServer();
                    var rsr = await rs.SendImage(_background.ToBitmap(), _regionConfig.SiteToken, _userConfig.ServerUrl);

                    if (rsr != HttpStatusCode.OK)
                    {
                        Log("Image-upload failed:" + rsr, LogLevel.Error, "LoggingActor");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("(UpdateBackgroundFrame) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite, LogLevel.Error, "LoggingActor");
            }

        }

        public string GetStatString()
        {
            var sb = new StringBuilder();
            var totalObjects = 0;

            if(_userConfig.SimplifiedCountDisplay)
            { 
               //Get list of approaches seen so far
                var approaches = new List<string>();
                foreach (var ts in _turnStats)
                {
                    if (!approaches.Contains(ts.Key.Approach))
                    {
                        approaches.Add(ts.Key.Approach);
                    }
                }

                foreach (var approach in approaches)
                {
                    //Get total for this approach, so that we can avoid showing approaches with 0 counts
                    long total = 0;
                    foreach (var ts in _turnStats)
                    {
                        if (ts.Key.Approach == approach)
                        {
                            total += ts.Value;
                        }
                    }

                    if (total > 0)
                    {
                        sb.AppendLine(PerApproachClassCount(_turnStats, approach));
                    }
                }
               
                foreach (var kvp in _turnStats)
                {
                    totalObjects += (int)kvp.Value;
                }
            }
            else
            {
                foreach (var kvp in _turnStats)
                {
                    sb.AppendLine(kvp.Key + ":  " + kvp.Value);
                    totalObjects += (int)kvp.Value;
                }
            }

            sb.AppendLine("");
            sb.AppendLine("Total objects counted: " + totalObjects);

            return sb.ToString();
        }

        private string PerApproachClassCount(MovementCount count, string approachName)
        { 
            var sb = new StringBuilder();

            long carCount = 0;
            long busCount = 0;
            long bicycleCount = 0;
            long motorcycleCount = 0;
            long truckCount = 0;
            long personCount = 0;
            long unknownCount = 0;

            foreach (var kvp in _turnStats)
            {
                if(kvp.Key.Approach == approachName)
                {
                    switch(kvp.Key.TrafficObjectType)
                    {
                        case ObjectType.Car:
                            carCount += kvp.Value;
                            break;
                        case ObjectType.Bus:
                            busCount += kvp.Value;
                            break;
                        case ObjectType.Bicycle:
                            bicycleCount += kvp.Value;
                            break;
                        case ObjectType.Motorcycle:
                            motorcycleCount += kvp.Value;
                            break;
                        case ObjectType.Truck:
                            truckCount += kvp.Value;
                            break;
                        case ObjectType.Person:
                            personCount += kvp.Value;
                            break;
                        case ObjectType.Unknown:
                            unknownCount += kvp.Value;
                            break;
                    }
                }
            }

            sb.AppendLine(approachName + "\tCar: " + carCount + Environment.NewLine
                 + "\t\tTruck: " + truckCount + Environment.NewLine 
                 + "\t\tBus: " + busCount + Environment.NewLine 
                 + "\t\tBicycle: " + bicycleCount + Environment.NewLine 
                 + "\t\tMotorcycle: " + motorcycleCount + Environment.NewLine 
                 + "\t\tPerson: " + personCount + Environment.NewLine
                );

            return sb.ToString();
        }

        private void UpdateStatsUiHandler(TrafficCounter.UpdateStatsUIDelegate handler)
        {
            try
            {
                _updateStatsUiDelegate = handler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("(UpdateStatsUiHandler) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite);
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

        private void UpdateDebugHandler(TrafficCounter.UpdateDebugDelegate handler)
        {
            try
            {
                _updateDebugDelegate = handler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("(UpdateDebugDelegate) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite);
            }
        }

        private async void DashboardHeartbeat()
        {
            if (_regionConfig == null || _userConfig == null)
            {
                Log("Heartbeat skipped, region-configuration or user-configuration is missing.", LogLevel.Warn, "LoggingActor");
                return;
            }

            if (_regionConfig.SendToServer)
            {
                try
                {
                    var rs = new RemoteServer();
                    var rsr = await rs.SendHeartbeat(_regionConfig.SiteToken, _userConfig.ServerUrl);
                    if (rsr != HttpStatusCode.OK)
                    {
                        Log("Heartbeat POST failed:" + rsr, LogLevel.Error, "LoggingActor");
                    }
                    else
                    {
                        Log("Heartbeat success", LogLevel.Info, "LoggingActor");
                    }

                    string heartbeatStatus = "Heartbeat: " + rsr;
                    _updateDebugDelegate?.Invoke(heartbeatStatus);
                }
                catch (Exception ex)
                {
                    Log(ex.Message + " at " + ex.StackTrace + " , exception " + ex.InnerException, LogLevel.Error,
                        "LoggingActor");
                }

            }
            else
            {
                Log("Send-to-server is disabled, no heartbeat transmitted.", LogLevel.Info, "LoggingActor");
            }
        }

        private void UpdateSequencingActor(IActorRef actorRef)
        {
            try
            {
                _sequencingActor = actorRef;
            }
            catch (Exception ex)
            {
                MessageBox.Show("(UpdateSequencingActor) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite);
            }
        }

        private void CopyGroundTruth(string groundTruthPath)
        {
            try
            {
                if (groundTruthPath == null)
                {
                    Log("Ground truth file path is null.", LogLevel.Info, "LoggingActor");
                    return;
                }
                var folderPath = _currentOutputFolder;
                var filepath = Path.Combine(folderPath, "Manual counts.json");
                    File.Copy(groundTruthPath, filepath, true);
            }
            catch(Exception ex)
            {
                Log("(CopyGroundTruth) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite, LogLevel.Error, "LoggingActor");
            }
            
        }

        private void UpdateFrameCount(UInt64 count)
        { 
            _nFramesProcessed = count;    
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);

            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }

        private void CheckConfiguration()
        {
            if (_regionConfig == null)
            {
                Log("RegionConfig is null.", LogLevel.Error, "LoggingActor");
                _configurationActor?.Tell(new RequestConfigurationMessage(Self));
            }
            else if (_regionConfig.RoiMask == null)
            {
                Log("ROI mask is null.", LogLevel.Error, "LoggingActor");
                _configurationActor?.Tell(new RequestConfigurationMessage(Self));
            }
            else if (_regionConfig.RoiMask.Count < 3)
            {
                Log("ROI mask has " + _regionConfig.RoiMask.Count + " vertices; 3 or more expected.", LogLevel.Error, "LoggingActor");
                _configurationActor?.Tell(new RequestConfigurationMessage(Self));
            }
            else if (!_regionConfig.RoiMask.PolygonClosed)
            {
                Log("ROI mask is not a closed polygon.", LogLevel.Error, "LoggingActor");
                _configurationActor?.Tell(new RequestConfigurationMessage(Self));
            }
        }

        private void UpdateConfigurationActor(IActorRef actorRef)
        {
            try
            {
                _configurationActor = actorRef;
            }
            catch (Exception ex)
            {
               Log("(UpdateConfigurationActor) " + ex.Message + ", " + ex.InnerException + " in " + ex.StackTrace + " at " + ex.TargetSite, LogLevel.Error, "LoggingActor");
            }
        }

        private async void FlushTransmitBuffer()
        {
            var transmittedMovements = new List<Movement>();

            Log("FlushTransmitBuffer", LogLevel.Info, "LoggingActor");

            foreach (var m in MovementTransmitBuffer)
            {
                try
                {
                    var rs = new RemoteServer();
                    var rsr = await rs.SendMovement(m, _regionConfig.SiteToken, _userConfig.ServerUrl);
                    if (rsr != HttpStatusCode.OK)
                    {
                        Log("Movement-buffer transmit failed:" + rsr, LogLevel.Info, "LoggingActor");
                    }
                    else
                    {
                        transmittedMovements.Add(m);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Log("Movement-buffer transmit failed:" + httpEx.Message, LogLevel.Error, "LoggingActor");
                    break;
                }
            }

            foreach (var m in transmittedMovements)
            {
                MovementTransmitBuffer.Remove(m);
            }
        }

        void UpdateBatchJob(BatchVideoJob job)
        { 
            job.Save(_dbConnection, _userConfig);
            _currentJob = job;    
        }

        [Serializable]
        public class VideoProcessingCompleteNotificationMessage
        {
            public string VideoFilePath = "";
            public string OutputFolderPath = "";
            public string ConfigurationName = "";
            public string ManualCountsPath = "";
            public int JobId;
        }
    }
}