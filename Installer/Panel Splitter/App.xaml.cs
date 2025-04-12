using System.IO;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using Panel_Splitter.Properties;

namespace Panel_Splitter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly string UpdateStatusFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Panel Splitter Logs", "update_status.txt");
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Contains("/background"))
            {
                ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

                // Background mode: no UI
                CheckForUpdates();
                SendLogs();
                Shutdown();
            }
            else
            {
                CheckForPendingUpdate();
                base.OnStartup(e);
            }
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            // Handle toast activation -> install
            if (e.Argument.Contains("action=install"))
            {
                Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = new();
                    mainWindow.ShowUpdatePrompt();
                    Current.MainWindow = mainWindow;
                    mainWindow.Show();
                });
            }
        }

        private static void CheckForUpdates()
        {

            Task.Run(async () =>
            {
                var (updateAvailable, latestVersion, _, error) = await UpdateHelper.CheckForUpdatesAsync();
                if (error != null)
                {
                    await AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                    {
                        { "app_error_type", "App-Background-Update-Failure" },
                        { "app_error_msg", error.Message }
                    });
                }
                else if (updateAvailable)
                {
                    // Save update status to file
                    File.WriteAllText(UpdateStatusFile, latestVersion);
                    ShowUpdateNotification(latestVersion);

                    await AnalyticsHelper.CaptureEvent("App-Background-Stat", new Dictionary<string, object>
                    {
                        { "stat_type", "Update-Available" },
                        { "latest_version", latestVersion }
                    });
                }
                else
                {
                    // Clear update status if no update is available
                    if (File.Exists(UpdateStatusFile)) File.Delete(UpdateStatusFile);

                    await AnalyticsHelper.CaptureEvent("App-Background-Stat", new Dictionary<string, object>
                    {
                        { "stat_type", "Up-to-Date" },
                        { "latest_version", latestVersion }
                    });
                }
            }).Wait();
        }

        private static void SendLogs()
        {
            Task.Run(async () =>
            {
                await AnalyticsHelper.SendLogsAsync();
            }).Wait();
        }

        private static void CheckForPendingUpdate()
        {
            if (File.Exists(UpdateStatusFile) && Settings.Default.AutoUpdates)
            {
                string latestVersion = File.ReadAllText(UpdateStatusFile).Trim();
                if (!string.IsNullOrEmpty(latestVersion) && new Version(latestVersion) > new Version(UpdateHelper.CurrentVersion))
                {
                    // Trigger update check in MainWindow
                    MainWindow mainWindow = new();
                    mainWindow.ShowUpdatePrompt();
                    Current.MainWindow = mainWindow;
                    mainWindow.Show();
                }
            }
        }

        private static void ShowUpdateNotification(string latestVersion)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Panel Splitter Update Available")
                    .AddText($"Version {latestVersion} is ready to install.")
                    .AddButton(new ToastButton()
                        .SetContent("Install")
                        .AddArgument("action", "install"))
                    .AddButton(new ToastButton()
                        .SetContent("Dismiss"))
                    .Show();
            }
            catch (Exception ex)
            {
                File.AppendAllText("error_log.txt", $"{DateTime.UtcNow}: Failed to show update notification: {ex.Message}\n");
            }
        }
    }

}
