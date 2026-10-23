using System.Windows;
using Panel_Splitter.Properties;

namespace Panel_Splitter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Enumeration for update check results
        /// </summary>
        enum UpdateCheckResult
        {
            None,
            Exit,
            ShowNotification
        }

        /// <summary>
        /// Handles application startup overriding the default behavior
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Prevent default shutdown behavior
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (e.Args.Contains("/background"))
            {
                // Background mode: no UI
                if (Settings.Default.AnalyticsEnabled)
                {
                    SendLogs();
                }

                // Check for updates if auto update enabled
                if (Settings.Default.AutoUpdates)
                {
                    var result = CheckForUpdates();

                    switch (result)
                    {
                        case UpdateCheckResult.Exit:
                            Shutdown();
                            break;

                        case UpdateCheckResult.ShowNotification:
                            // Show UI ~ notification window
                            UpdateNotificationWindow notificationWindow = new();
                            notificationWindow.Show();
                            break;

                        case UpdateCheckResult.None:
                        default:
                            Shutdown();
                            break;
                    }
                }
                else
                {
                    Shutdown();
                }

            }
            else
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();

                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
        }

        /// <summary>
        /// Checks for updates and return the result
        /// </summary>
        /// <returns></returns>
        private static UpdateCheckResult CheckForUpdates()
        {
            return Task.Run(async () =>
            {
                var (updateAvailable, latestVersion, _, error) = await UpdateHelper.CheckForUpdatesAsync();
                if (error != null)
                {
                    await AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                    {
                        { "app_error_type", "Background-Update-Failure" },
                        { "app_error_msg", error.Message }
                    });

                    return UpdateCheckResult.Exit;

                }
                else if (updateAvailable)
                {
                    await AnalyticsHelper.CaptureEvent("App-Background-Stat", new Dictionary<string, object>
                    {
                        { "stat_type", "Update-Available" },
                        { "latest_version", latestVersion }
                    });

                    return UpdateCheckResult.ShowNotification;
                }
                else
                {
                    await AnalyticsHelper.CaptureEvent("App-Background-Stat", new Dictionary<string, object>
                    {
                        { "stat_type", "Up-to-Date" },
                        { "latest_version", latestVersion }
                    });

                    return UpdateCheckResult.Exit;
                }
            }).Result;
        }

        private static void SendLogs()
        {
            Task.Run(async () =>
            {
                await AnalyticsHelper.SendLogsAsync();
            }).Wait();
        }
    }

}
