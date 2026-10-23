using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace Panel_Splitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateInstalledVersionsDisplay();
            // Load saved settings for checkboxes
            UpdatesCheckBox.IsChecked = Properties.Settings.Default.AutoUpdates;
            AnalyticsCheckBox.IsChecked = Properties.Settings.Default.AnalyticsEnabled;

            // Send logs as a fallback if analytics is enabled
            System.Threading.Tasks.Task.Run(async () =>
            {
                await AnalyticsHelper.SendLogsAsync();
                await AnalyticsHelper.CaptureEvent("App-Startup");
            });

            // Check for updates on startup if automatic updates are enabled
            if (Properties.Settings.Default.AutoUpdates)
            {
                UpdateBtn_Click(null, null);
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Install Automatically button click.
        /// Detects Photoshop versions and installs the script into each.
        /// </summary>
        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> paths = DetectPhotoshopVersions();
            List<string> installedVersions = [];

            foreach (string path in paths)
            {
                try
                {
                    InstallScript(path);
                    string version = GetVersionFromPath(path);
                    installedVersions.Add(version);
                }
                catch (Exception ex)
                {
                    _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                    {
                        { "app_error_type", "Script-Installation-Error" },
                        { "app_error_msg", ex.Message },
                        { "app_ps_paths", paths }
                    });
                    System.Windows.MessageBox.Show($"Failed to install to {path}: {ex.Message}", "Installation Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Update the UI with the installation status
            InstalledVersionsTextBlock.Text = installedVersions.Count != 0
                ? "🗹 Installed in: " + string.Join(", ", installedVersions)
                : "No Photoshop versions found.";

            _ = AnalyticsHelper.CaptureEvent("Script-Installation", new Dictionary<string, object>
                {
                    { "app_ps_versions", string.Join(", ", installedVersions) }
                });
        }

        /// <summary>
        /// Handles the Extract Manually button click.
        /// Allows the user to select a folder and extracts the script there.
        /// </summary>
        private void ExtractBtn_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sourceFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Panel Splitter");
                string targetFolder = System.IO.Path.Combine(dialog.SelectedPath, "Panel Splitter");

                if (!Directory.Exists(sourceFolder))
                {
                    throw new DirectoryNotFoundException("Panel Splitter folder not found in the application directory.\nPlease reinstall the application!");
                }

                ProcessStartInfo psi = new()
                {
                    FileName = "Panel Splitter Helper.exe",
                    Arguments = $"/install \"{targetFolder}\" \"{sourceFolder}\"",
                    Verb = IsAdminRequired(targetFolder) ? "runas" : null, // Requests admin privileges if required
                    UseShellExecute = true
                };
                try
                {
                    Process.Start(psi).WaitForExit();
                    System.Windows.MessageBox.Show("Folder extracted successfully.", "Success | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to extract folder: {ex.Message}", "Extraction Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                    _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                    {
                        { "app_error_type", "Script-Extract-Error" },
                        { "app_error_msg", ex.Message }
                    });
                }
            }
        }

        /// <summary>
        /// Handles the Check for Updates button click.
        /// Queries GitHub for the latest release and offers to install it if newer.
        /// </summary>
        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateBtn.Content = "🔄 Checking";
            UpdateBtn.IsEnabled = false;

            var (updateAvailable, latestVersion, setupUrl, error) = await UpdateHelper.CheckForUpdatesAsync();

            if (error != null)
            {
                DownloadProgressBar.Visibility = Visibility.Collapsed;
                UpdateBtn.Content = "Check for updates";
                UpdateBtn.IsEnabled = true;
                _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                {
                    { "app_error_type", "App-Update-Failure" },
                    { "app_error_msg", error.Message }
                });
                System.Windows.MessageBox.Show($"Failed to check for updates: {error.Message}", "Update Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (updateAvailable)
            {
                UpdateBtn.Content = "✨ Update Available!";
                MessageBoxResult result = System.Windows.MessageBox.Show($"Update available: v{latestVersion}. Download and install?", "Update Available | Panel Splitter", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    string tempSetupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PanelSplitterSetup.exe");
                    UpdateBtn.Content = "⬇ Downloading...";
                    DownloadProgressBar.Visibility = Visibility.Visible;

#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    using (var wc = new System.Net.WebClient())
                    {
                        wc.DownloadProgressChanged += (s, args) => DownloadProgressBar.Value = args.ProgressPercentage;
                        wc.DownloadFileCompleted += (s, args) => DownloadProgressBar.Visibility = Visibility.Collapsed;
                        await wc.DownloadFileTaskAsync(setupUrl, tempSetupPath);
                    }
#pragma warning restore SYSLIB0014 // Type or member is obsolete

                    UpdateBtn.Content = "✔ Downloaded";
                    _ = AnalyticsHelper.CaptureEvent("App-Update-Download");

                    System.Windows.MessageBox.Show("Download complete. The installer will now start, and this application will close.", "Ready to Install | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(tempSetupPath);
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    _ = AnalyticsHelper.CaptureEvent("App-Update-Refused");
                    UpdateBtn.Content = "✨ Update Available!";
                    UpdateBtn.IsEnabled = true;
                }
            }
            else
            {
                UpdateBtn.Content = "✔ Up to Date";
                UpdateBtn.IsEnabled = false;
                UpdateBtn.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#265E0F"));
            }
        }

        /// <summary>
        /// Handles checking the Automatic Updates checkbox.
        /// Saves the preference to settings.
        /// </summary>
        private void UpdatesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoUpdates = true;
            Properties.Settings.Default.Save();

            try
            {
                // Create a new task definition for the local machine and assign properties
                TaskDefinition td = TaskService.Instance.NewTask();
                td.RegistrationInfo.Description = "Runs the Panel Splitter update check";

                DailyTrigger dt = new()
                {
                    StartBoundary = DateTime.Today.AddDays(1).AddHours(9).AddMinutes(0),
                    DaysInterval = 1
                };
                td.Triggers.Add(dt);

                string exePath = Environment.ProcessPath;
                td.Actions.Add(new ExecAction(exePath, "/background", null));

                TaskService.Instance.RootFolder.RegisterTaskDefinition("Panel Splitter", td);
            }
            catch (Exception ex)
            {
                _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                {
                    { "app_error_type", "App-Task-Creation-Failure" },
                    { "app_exe_path", string.IsNullOrEmpty(Environment.ProcessPath) ? "Undefined" : Environment.ProcessPath},
                    { "app_error_msg", ex.Message }
                });
            }
        }

        /// <summary>
        /// Handles checking the Analytics checkbox.
        /// Saves the preference to settings and toggles analytics.
        /// </summary>
        private void AnalyticsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AnalyticsEnabled = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles unchecking the Automatic Updates checkbox.
        /// Saves the preference to settings.
        /// </summary>
        private void UpdatesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoUpdates = false;
            Properties.Settings.Default.Save();
            _ = AnalyticsHelper.CaptureEvent("Opt-Out-Updates");
        }

        /// <summary>
        /// Handles unchecking the Analytics checkbox.
        /// Saves the preference to settings and toggles analytics.
        /// </summary>
        private void AnalyticsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _ = AnalyticsHelper.CaptureEvent("Opt-Out-Analytics");
            Properties.Settings.Default.AnalyticsEnabled = false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles clicking the footer link to open the GitHub repository.
        /// </summary>
        private void FooterTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/dilshan-h/Panel-Splitter",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to open GitHub: {ex.Message}", "Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                {
                    { "app_error_type", "App-Process-Start-Failure" },
                    { "app_process_type", "GH-Link" },
                    { "app_error_msg", ex.Message }
                });
            }
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.buymeacoffee.com/dilshanh",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to open BMC: {ex.Message}", "Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                {
                    { "app_error_type", "App-Process-Start-Failure" },
                    { "app_process_type", "BMC-Link" },
                    { "app_error_msg", ex.Message }
                });
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Detects installed Photoshop versions by checking the Windows Registry and file system.
        /// </summary>
        /// <returns>A list of paths to Photoshop installations.</returns>
        private List<string> DetectPhotoshopVersions()
        {
            List<string> paths = [];
            string baseKey = @"SOFTWARE\Adobe\Photoshop";

            // Scan Registry Keys
            InstalledVersionsTextBlock.Text = "Hang on a minute! Scanning Registry...";
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(baseKey))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using RegistryKey subKey = key.OpenSubKey(subKeyName);
                        string path = subKey?.GetValue("ApplicationPath") as string;
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            paths.Add(path);
                        }
                    }
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Fall back to file system scan
            if (paths.Count == 0)
            {
                InstalledVersionsTextBlock.Text += "\nRegistry scan failed! Now scanning filesystem...";
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string adobePath = System.IO.Path.Combine(programFiles, "Adobe");

                if (Directory.Exists(adobePath))
                {
                    foreach (string dir in Directory.GetDirectories(adobePath, "Adobe Photoshop *"))
                    {
                        string exePath = System.IO.Path.Combine(dir, "Photoshop.exe");
                        if (File.Exists(exePath))
                        {
                            paths.Add(dir);
                        }
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// Installs the script to the specified Photoshop installation path.
        /// </summary>
        /// <param name="photoshopPath">The path to the Photoshop installation.</param>
        private static void InstallScript(string photoshopPath)
        {
            string sourceFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Panel Splitter");
            string targetDir = System.IO.Path.Combine(photoshopPath, "Presets", "Scripts", "Panel Splitter");

            if (!Directory.Exists(sourceFolder))
            {
                throw new DirectoryNotFoundException("Panel Splitter folder not found in the application directory.\nPlease reinstall the application!");
            }

            ProcessStartInfo psi = new()
            {
                FileName = "Panel Splitter Helper.exe",
                Arguments = $"/install \"{targetDir}\" \"{sourceFolder}\"",
                Verb = IsAdminRequired(targetDir) ? "runas" : null, // Requests admin privileges if required
                UseShellExecute = true
            };
            try
            {
                Process.Start(psi).WaitForExit();
                System.Windows.MessageBox.Show("Installation completed!", "Success | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Installation failed: " + ex.Message, "Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                {
                    { "app_error_type", "Script-Installation-Failure" },
                    { "app_error_msg", ex.Message }
                });
            }
        }

        /// <summary>
        /// Extracts the Photoshop version from the installation path.
        /// </summary>
        /// <param name="path">The path to the Photoshop installation.</param>
        /// <returns>The version string (e.g., "CC 2023").</returns>
        private static string GetVersionFromPath(string path)
        {
            string folderName = new DirectoryInfo(path).Name;
            return folderName.Replace("Adobe Photoshop ", "").Trim();
        }

        /// <summary>
        /// Updates the installed versions display in the UI.
        /// </summary>
        private void UpdateInstalledVersionsDisplay()
        {
            List<string> installedVersions = [];
            List<string> paths = DetectPhotoshopVersions();

            foreach (string path in paths)
            {
                string targetDir = System.IO.Path.Combine(path, "Presets", "Scripts", "Panel Splitter");
                if (Directory.Exists(targetDir))
                {
                    string version = GetVersionFromPath(path);
                    installedVersions.Add(version);
                }
            }

            InstalledVersionsTextBlock.Text = installedVersions.Count != 0
                ? "🗹 Installed in: " + string.Join(", ", installedVersions)
                : "No installations found.";
        }

        /// <summary>
        /// Check whether the file copy operation requires admin privileges.
        /// </summary>
        /// <param name="targetFolder">The target folder where the script will be installed.</param>
        /// <returns>True if admin privileges are required; otherwise, false.</returns>
        private static bool IsAdminRequired(string targetFolder)
        {
            string[] protectedPaths = [@"C:\Program Files", @"C:\Program Files (x86)", @"C:\Windows"];
            return protectedPaths.Any(path => targetFolder.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }

        public void ShowUpdatePrompt()
        {
            UpdateBtn_Click(null, null);
        }
        #endregion
    }
}