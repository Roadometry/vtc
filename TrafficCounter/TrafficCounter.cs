using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using NLog;
using VTC.CaptureSource;
using VTC.Kernel.Video;
using VTC.Common;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Sentry;
using VTC.Actors;
using VTC.Messages;
using VTC.UI;
using VTC.Classifier;
using VTC.UserConfiguration;

namespace VTC
{
   public partial class TrafficCounter : Form
   {
        #region VideoDisplays
        private VideoMux _videoMux;
        #endregion

        private readonly ActorSystem _actorSystem;

        private readonly DateTime _applicationStartTime;

        private readonly List<ICaptureSource> _cameras = new List<ICaptureSource>(); //List of all video input devices. Index, file location, name
        private ICaptureSource _selectedCaptureSource;

        // unit tests has own settings, so need to store "pairs" (capture, settings)
        private CaptureContext[] _testCaptureContexts;

        private Logger _userLogger = LogManager.GetLogger("userlog"); // special logger for user messages

        private const string _dsn = "https://5cdde3c580914972844fda3e965812ae@sentry.io/1248715";

        private UserConfig _userConfig = new UserConfig();

        public delegate void UpdateInfoUIDelegate(string infoString);
        public delegate void UpdateStatsUIDelegate(string statString);
        public delegate void UpdateDebugDelegate(string debugString);

        private TelemetryClient _telemetry;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isLicensed">If false, software will shut down after a few minutes</param>
        /// <param name="appArgument">Can mean different things (Local file with video, Unit tests, etc).</param>
        public TrafficCounter(bool isLicensed, string appArgument = null)
       {           
            InitializeComponent();

           LoadUserConfig();

           _userLogger.Log(LogLevel.Info, "VTC: Launched");

            var gpuDetector = new GPUDetector();

            //Disable eventhandler for the changing combobox index.
            CameraComboBox.SelectedIndexChanged -= CameraComboBox_SelectedIndexChanged;

            //Enable eventhandler for the changing combobox index.
            CameraComboBox.SelectedIndexChanged += CameraComboBox_SelectedIndexChanged;

            //Create video windows
            CreateVideoWindows();

            _applicationStartTime = DateTime.Now;            

            var config = ConfigurationFactory.ParseString(@"
                processing-bounded-mailbox {
                    mailbox-type = ""Akka.Dispatch.BoundedMailbox""
                    mailbox-capacity = 10
                    mailbox-push-timeout-time = 10s
                }
                synchronized-dispatcher {
                  type = ""SynchronizedDispatcher""
                   throughput = 10
                }
                ");

            _actorSystem = ActorSystem.Create("VTCActorSystem", config);
            var supervisorActor = _actorSystem.ActorOf(Props.Create(typeof(SupervisorActor)).WithDispatcher("synchronized-dispatcher"), "SupervisorActor");
            supervisorActor.Tell(new CreateAllActorsMessage(UpdateUI, UpdateStatsUI, UpdateInfoBox, UpdateUIAccessoryInfo, UpdateDebugInfo));
            supervisorActor.Tell(new UpdateActorStatusHandlerMessage(UpdateActorStatusIndicators));

           // otherwise - run in standard mode
           InitializeCameraSelection(appArgument);
       }

        private void UpdateUI(TrafficCounterUIUpdateInfo updateInfo)
        {
            // Update image boxes
            UpdateImageBoxes(updateInfo.StateImage);

            if (!trackedObjectsTextbox.IsHandleCreated)
            {
                return;
            }
            trackedObjectsTextbox.Invoke((MethodInvoker) delegate
            {
                trackedObjectsTextbox.Text = $"{updateInfo.Measurements.Length} objects currently detected";
                trackedObjectsTextbox.Text += Environment.NewLine + $"{updateInfo.StateEstimates.Length} objects currently tracked";
            });

            var activeTime = DateTime.Now - _applicationStartTime;
            if (!timeActiveTextBox.IsHandleCreated)
            {
                return;
            }
            timeActiveTextBox.Invoke((MethodInvoker) delegate
            {
                timeActiveTextBox.Text = activeTime.ToString(@"dd\.hh\:mm\:ss");
            });

            if (!fpsTextbox.IsHandleCreated)
            {
                return;
            }
            fpsTextbox.Invoke((MethodInvoker) delegate
                {
                    fpsTextbox.Text = $"{Math.Round(updateInfo.Fps,1)} FPS";
                }
            );
        }

        private void UpdateUIAccessoryInfo(TrafficCounterUIAccessoryInfo accessoryInfo)
        {
            // Update image boxes
            remainingTimeBox.Invoke((MethodInvoker) delegate
            {
                remainingTimeBox.Text = accessoryInfo.EstimatedTimeRemaining.ToString(@"dd\.hh\:mm\:ss");
            });

            frameTextbox.Invoke((MethodInvoker) delegate
            {
                frameTextbox.Text = accessoryInfo.ProcessedFrames.ToString();
            });
        }

        private void UpdateStatsUI(string statString)
        {
            // Update statistics 
            if (!tbVistaStats.IsHandleCreated)
            {
                return;
            }
            tbVistaStats.Invoke((MethodInvoker)delegate
            {
                tbVistaStats.Text = statString;
            });
        }

        private void UpdateDebugInfo(string debugString)
        {
            // Update statistics
            if (!debugTextbox.IsHandleCreated)
            {
                return;
            }
            debugTextbox.Invoke((MethodInvoker)delegate
            {
                debugTextbox.Text = debugString;
            });
        }

        /// <summary>
        /// Try to detect unit tests. Play unit tests (if detected).
        /// </summary>
        /// <param name="assemblyName">Assembly name with test scenarios.</param>
        /// <returns><c>true</c> if unit tests detected.</returns>
        private bool DetectTestScenarios(string assemblyName)
        {
            var result = false;

            try
            {
                while (! string.IsNullOrWhiteSpace(assemblyName))
                {
                    //TODO: Pass test capture contexts to FrameGrabber actor
                    // ensure absolute path
                    if (! Path.IsPathRooted(assemblyName))
                    {
                        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        if(currentDir != null)
                            assemblyName = Path.Combine(currentDir, assemblyName);
                    }

                    if (! File.Exists(assemblyName)) break;

                    var assembly = Assembly.LoadFile(assemblyName);
                    if (assembly == null) break;

                    _testCaptureContexts = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof (ICaptureContextProvider))
                                    && (t.GetConstructor(Type.EmptyTypes) != null)) // expected default constructor
                        .Select(t => Activator.CreateInstance(t) as ICaptureContextProvider)
                        .SelectMany(instance => instance.GetCaptures())
                        .ToArray();

                    foreach (var captureContext in _testCaptureContexts)
                    {
                        AddCamera(captureContext.Capture);
                    }

                    Log(LogLevel.Trace, "Test mode detected.");
                    result = true;
                    break;
                }
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Use this function to terminate the application if a trial license timeout occurs
        /// </summary>
        private void NotifyLicenseAndExit()
        {
            MessageBox.Show(
                "This is only a trial version! Please purchase a license from roadometry.com to use software longer than 30 minutes.");
            Application.Exit();
        }

        private void heartbeatTimer_Tick(object sender, EventArgs e)
        {
            var heartbeatPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VTC\\heartbeat";
            var heartbeatFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VTC";

            if (!Directory.Exists(heartbeatFolderPath))
                Directory.CreateDirectory(heartbeatFolderPath);

            if (!File.Exists(heartbeatPath))
                File.Create(heartbeatPath).Close();

            File.SetLastWriteTime(heartbeatPath, DateTime.Now);
        }

        #region Camera

        private void AddCamera(ICaptureSource camera)
        {
            _cameras.Add(camera);
            CameraComboBox.Items.Add(camera.Name);
        }

        private async Task LaunchIPCamera(string ipCameraName, int cameraIndex)
        {
            if (_cameras.Count < cameraIndex + 1)
            {
                return;
            }

            await Task.Delay(5000);
            var configurationActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
            UpdateCaptureSource(_cameras[cameraIndex], false);
            configurationActor.Tell(new LoadConfigurationByNameMessage(ipCameraName));
        }

        private void InitializeCameraSelection(string cameraSelectionString)
        {
            _cameras.Clear();

            AddIPCameraIfValid(_userConfig.Camera1Name,_userConfig.Camera1Url);
            AddIPCameraIfValid(_userConfig.Camera2Name,_userConfig.Camera2Url);
            AddIPCameraIfValid(_userConfig.Camera3Name,_userConfig.Camera3Url);

            switch (cameraSelectionString)
            {
                case "IP1":
                    Task.Run(async () => { await LaunchIPCamera("IP1", 0); });
                    break;
                case "IP2":
                    Task.Run(async () => { await LaunchIPCamera("IP2", 1); });
                    break;
                case "IP3":
                    Task.Run(async () => { await LaunchIPCamera("IP3", 2); });
                    break;
            }

            //List all video input devices.
            var systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            //Variable to indicate the device´s index.
            int deviceIndex = 0;

            //Add every device to the global variable and to the camera combobox
            foreach (DsDevice camera in systemCameras)
            {
                //Add Device with an index and a name to the List.
                AddCamera(new SystemCamera(camera.Name, deviceIndex));

                //Increment the index.
                deviceIndex++;
            }
        }

        private void AddIPCameraIfValid(string name, string url)
        {
            if (name == null || url == null)
            {
                return;
            }

            if(name.Length > 0 && url.Length > 0)
            {
                AddCamera(new IpCamera(name, url));
            }
        }

        /// <summary>
        /// Check if video file exists.
        /// </summary>
        /// <param name="filename">Pathname to check.</param>
        /// <returns><c>true</c> for existing file.</returns>
        private bool UseLocalVideo(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;
            if (File.Exists(filename)) return true;

            Log(LogLevel.Error, $"Video file is not found ('{filename}').");

            return false;
        }

        #endregion

        #region Logging, Measurement and Export

        private void Log(LogLevel logLevel, string text)
        {
            var loggingActor = _actorSystem?.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
            loggingActor?.Tell(new LogMessage(text, logLevel, "TrafficCounter"));
        }

        private void UserLog(string text)
        {
            var loggingActor = _actorSystem?.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
            loggingActor?.Tell(new LogUserMessage(text, LogLevel.Info, "TrafficCounter"));
        }

        #endregion

        #region Batch Processing

        private VideoFileCapture LoadCameraFromFilename(string filename)
        {
            var vfc = new VideoFileCapture(filename);
            AddCamera(vfc);
            return vfc;
        }

        #endregion

        #region Rendering
        private void CreateVideoWindows()
        {
            _videoMux = new VideoMux();
            _videoMux.Show();
        }

        private void UpdateImageBoxes(Image<Bgr, byte> stateImage)
        {
            try
            {
                _videoMux.UpdateImage(stateImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
        #endregion

        #region Click Handlers

        private void TrafficCounter_FormClosed(object sender, FormClosedEventArgs e)
        {
            _userLogger.Log(LogLevel.Info, "VTC: Application closed");
        }

        /// <summary>
        /// Method which reacts to the change of the camera selection combobox.
        /// </summary>
        private void CameraComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCaptureSource(_cameras[CameraComboBox.SelectedIndex],true);
        }

        private void UpdateCaptureSource(ICaptureSource source, bool interactive)
        {
            var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
            var frameGrabActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/FrameGrabActor");
            var configurationActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
            var supervisorActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor");
            _selectedCaptureSource = source;

            //Create new output folder
            loggingActor.Tell(new NewVideoSourceMessage(_selectedCaptureSource));
            loggingActor.Tell(new FileCreationTimeMessage(_selectedCaptureSource.StartDateTime()));

            //Change the capture device.
            frameGrabActor.Tell(new NewVideoSourceMessage(_selectedCaptureSource));
            supervisorActor.Tell(new NewVideoSourceMessage(_selectedCaptureSource));
            frameGrabActor.Tell(new GetNextFrameMessage());

            //Tell the configuration actor about this camera
            var selected_camera_list = new List<ICaptureSource>();
            selected_camera_list.Add(_selectedCaptureSource);
            configurationActor.Tell(new CamerasMessage(selected_camera_list));

            if (interactive)
            {
                configurationActor.Tell(new OpenRegionConfigurationScreenMessage());
            }
        }

        private void btnConfigureRegions_Click(object sender, EventArgs e)
        {
            var configurationActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
            var selectedCameraList = new List<ICaptureSource>();
            if (_selectedCaptureSource != null)
            {
                selectedCameraList.Add(_selectedCaptureSource);
            }
            
            configurationActor.Tell(new CamerasMessage(selectedCameraList));
            configurationActor.Tell(new OpenRegionConfigurationScreenMessage());
        }

        private void SelectVideosButton_Click(object sender, EventArgs e)
        {
            var dr = selectVideoFilesDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var videoJobs = new List<BatchVideoJob>();
                var videoPathsList = selectVideoFilesDialog.FileNames.ToList();
                foreach (var job in videoPathsList.Select(p => new BatchVideoJob {VideoPath = p}))
                {
                    var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
                    loggingActor?.Tell(new VideoJobsMessage(videoJobs));
                    videoJobs.Add(job);
                }

                var configurationActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
                configurationActor.Tell(new VideoJobsMessage(videoJobs));
                configurationActor.Tell(new CamerasMessage(_cameras));
                configurationActor.Tell(new CurrentJobMessage(videoJobs.First()));
                btnConfigureRegions_Click(null, null);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generateReportButton_Click(object sender, EventArgs e)
        {
            var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
            loggingActor?.Tell(new GenerateReportMessage());
        }

        private void UpdateInfoBox(string text)
        {
            if (infoBox.InvokeRequired)
            {
                infoBox.Invoke((MethodInvoker) (() => infoBox.AppendText(text + Environment.NewLine)));
                return;
            }
            infoBox.AppendText(text + Environment.NewLine);
        }

        private void UpdateActorStatusIndicators(Dictionary<string, DateTime> actorStatuses)
        {
            loggingIndicator.Invoke((MethodInvoker)delegate
            {
                if (actorStatuses.ContainsKey("LoggingActor"))
                {
                    var span = DateTime.Now - actorStatuses["LoggingActor"];
                    loggingIndicator.On = span < TimeSpan.FromSeconds(30);
                }
                else
                    loggingIndicator.On = false;
            });

            processingIndicator.Invoke((MethodInvoker)delegate
            {
                if (actorStatuses.ContainsKey("ProcessingActor"))
                {
                    var span = DateTime.Now - actorStatuses["ProcessingActor"];
                    processingIndicator.On = span < TimeSpan.FromSeconds(30);
                }
                else
                    processingIndicator.On = false;
            });

            framegrabbingIndicator.Invoke((MethodInvoker)delegate
            {
                if (actorStatuses.ContainsKey("FrameGrabActor"))
                {
                    var span = DateTime.Now - actorStatuses["FrameGrabActor"];
                    framegrabbingIndicator.On = span < TimeSpan.FromSeconds(30);
                }
                else
                    framegrabbingIndicator.On = false;
            });

            sequencingIndicator.Invoke((MethodInvoker)delegate
            {
                if (actorStatuses.ContainsKey("SequencingActor"))
                {
                    var span = DateTime.Now - actorStatuses["SequencingActor"];
                    sequencingIndicator.On = span < TimeSpan.FromSeconds(30);
                }
                else
                    sequencingIndicator.On = false;
            });

            configurationIndicator.Invoke((MethodInvoker)delegate
            {
                if (actorStatuses.ContainsKey("ConfigurationActor"))
                {
                    var span = DateTime.Now - actorStatuses["ConfigurationActor"];
                    configurationIndicator.On = span < TimeSpan.FromSeconds(30);
                }
                else
                    configurationIndicator.On = false;
            });
        }

        private void userSettingsButton_Click(object sender, EventArgs e)
        {
            var se = new SettingsEditor();
            se.Show();
        }

        private void LoadUserConfig()
        {
            string UserConfigSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\VTC\\userConfig.xml";
            IUserConfigDataAccessLayer _userConfigDataAccessLayer = new FileUserConfigDal(UserConfigSavePath);

            _userConfig = _userConfigDataAccessLayer.LoadUserConfig();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm(Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                false);
            aboutForm.Show();
        }

        private void ResetCountsButton_Click(object sender, EventArgs e)
        {
            var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
            loggingActor?.Tell(new ClearStatsMessage());
        }

        private void resetDatabaseButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the database? This will clear all previous video-counts!", "Confirm database-reset", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
                loggingActor?.Tell(new ResetDatabaseMessage());

                MessageBox.Show("Ok, the database has been reset.");
            }
        }

        public class FilenameNumericalComparator : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                var filenameA = Path.GetFileName(a);
                var filenameB = Path.GetFileName(b);
                bool successA = Int32.TryParse(filenameA, out var intA);
                bool successB = Int32.TryParse(filenameB, out var intB);

                if (!successA || !successB)
                {
                    return 0;
                }

                if (intA == intB)
                {
                    return 0;
                }

                if (intA < intB)
                {
                    return -1;
                }

                return 1;
            }
        }
        
        private void loadSplitVideoButton_Click(object sender, EventArgs e)
        {
            var dr = selectVideoFilesDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var loggingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/LoggingActor");
                var configurationActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/ConfigurationActor");
                var sequencingActor = _actorSystem.ActorSelection("akka://VTCActorSystem/user/SupervisorActor/SequencingActor");

                var videoJobs = new List<BatchVideoJob>();
                var videoPathsList = selectVideoFilesDialog.FileNames.ToList();

                videoPathsList.Sort(new FilenameNumericalComparator());

                sequencingActor?.Tell(new BeginSplitVideoMessage());
                loggingActor?.Tell(new BeginSplitVideoMessage());

                foreach (var job in videoPathsList.Select(p => new BatchVideoJob { VideoPath = p }))
                {
                    loggingActor?.Tell(new SplitVideoJobsMessage(videoJobs));
                    videoJobs.Add(job);
                }

                configurationActor.Tell(new SplitVideoJobsMessage(videoJobs));
                configurationActor.Tell(new CamerasMessage(_cameras));
                configurationActor.Tell(new CurrentJobMessage(videoJobs.First()));
                btnConfigureRegions_Click(null, null);
            }
        }
    }
}