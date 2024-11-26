using System;
using System.Reflection;
using System.Windows.Forms;
using System.Management;
using NLog;
using VTC.Common;

namespace VTC
{
    static class Program
    {
        private static readonly Logger Logger = LogManager.GetLogger("app.global");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (_, e) => Logger.Error(e.Exception, "Thread exception");
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Logger.Error((Exception)e.ExceptionObject, "Unhandled exception");

            int delayMs = 2000;
            var ss = new SplashScreen(delayMs);
            ss.Show();

            Logger.Info("***** Start. v." + Assembly.GetExecutingAssembly().GetName().Version);
            if (args.Length > 0)
            {
                Logger.Info("Arguments: " + string.Join(";", args));
            }

            string appArgument = null;
            if (args.Length == 1) appArgument = args[0];

            try
            {
                var form = CreateTrafficCounterForm(false, appArgument);
                Application.Run(form);
            }
            catch(AccessViolationException ex)
            { 
                Logger.Log(LogLevel.Error, ex.Message);       
            }
        }

        private static Form CreateTrafficCounterForm(bool isLicensed, string args)
        {
            return new TrafficCounter(isLicensed, args);
        }
    }
}
