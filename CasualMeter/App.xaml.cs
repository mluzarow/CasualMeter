using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CasualMeter.Core.Helpers;
using log4net;
using Lunyx.Common.UI.Wpf;
using Squirrel;

namespace CasualMeter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingleInstanceApp
    {
        private const string Unique = "283055BC-D5AC-46CC-B1A9-51C053BB9028";

        private static readonly ILog Logger = LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string ExePath = AppDomain.CurrentDomain.BaseDirectory;

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                if (!Directory.Exists(Path.Combine(ExePath, "lib")))
                {   //copy git libs if necessary, see method comments for details
                    Copy(Path.Combine(ExePath,"git"),Path.Combine(ExePath,"lib"));
                }

#if !DEBUG
                Update().Wait();
#endif
                Logger.Info("Starting up.");

                // Initialize helpers
                ProcessHelper.Instance.Initialize();

                var application = new App();
                application.InitializeComponent();
                application.ShutdownMode = ShutdownMode.OnMainWindowClose;

                // register unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                application.Run();

                Logger.Info("Closing.");
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
                Environment.Exit(0);//application doesn't fully exit without this for some reason
            }
        }

        private static async Task Update()
        {
            try
            {
                using (var mgr = new UpdateManager("http://lunyx.net/CasualMeter"))
                {
                    Logger.Info("Checking for updates.");
                    if (mgr.IsInstalledApp)
                    {
                        SettingsHelper.Instance.Version = $"v{mgr.CurrentlyInstalledVersion()}";
                        Logger.Info($"Current Version: {SettingsHelper.Instance.Version}");
                        var updates = await mgr.CheckForUpdate();
                        if (updates.ReleasesToApply.Any())
                        {
                            Logger.Info("Updates found. Applying updates.");
                            var release = await mgr.UpdateApp();

                            MessageBox.Show(CleanReleaseNotes(release.GetReleaseNotes(Path.Combine(mgr.RootAppDirectory, "packages"))),
                                $"Casual Meter Update - v{release.Version}");

                            Logger.Info("Updates applied. Restarting app.");
                            UpdateManager.RestartApp();
                        }
                    }
                }
            }
            catch (Exception e)
            {   //log exception and move on
                HandleException(e);
            }
        }

        private static void HandleException(Exception e)
        {
            if (e == null) return;
            if (e.InnerException != null)
            {
                HandleException(e.InnerException);
            }
            else
            {
                Logger.Error(e);
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            HandleException(e.Exception);
        }

        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleException(e.Exception);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
            if (e.IsTerminating)
                MessageBox.Show("There was an unexpected error. Please check the log for more details.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // ...
            return true;
        }

        private static string CleanReleaseNotes(string value)
        {
            var r = new Regex(@".*<p>(?<content>.*)</p>.*", RegexOptions.Singleline);
            var match = r.Match(value);
            return match.Success ? ScrubHtml(match.Groups["content"].Value) : value;
        }

        private static string ScrubHtml(string value)
        {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }

        #region Copy Directory Recursively
        /// <summary>
        /// This is a hack because the git libs require the appropriate OS specific dll's to
        /// be placed in a lib folder of the executing assembly. However, since this folder is
        /// called lib, it conflicts with Squirrel's auto update mechanism, which searches for
        /// a lib/net45 folder.  As a result, we use a post-build event in the main project
        /// to move all the libs to a folder called git, and have the execution copy the files
        /// the folders as necessary.
        /// </summary>
        private static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion
    }
}
