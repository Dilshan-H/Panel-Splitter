using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Panel_Splitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Application version
        private const string currentVersion = "2.0";

        public MainWindow()
        {
            InitializeComponent();
            UpdateInstalledVersionsDisplay();
            // Load saved settings for checkboxes
            UpdatesCheckBox.IsChecked = Properties.Settings.Default.AutoUpdates;
            AnalyticsCheckBox.IsChecked = Properties.Settings.Default.AnalyticsEnabled;

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
                    System.Windows.MessageBox.Show($"Failed to install to {path}: {ex.Message}", "Installation Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Update the UI with the installation status
            InstalledVersionsTextBlock.Text = installedVersions.Count != 0
                ? "🗹 Installed in: " + string.Join(", ", installedVersions)
                : "No Photoshop versions found.";
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
                try
                {
                    DirectoryCopy(sourceFolder, targetFolder);
                    System.Windows.MessageBox.Show("Folder extracted successfully.", "Success | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to extract folder: {ex.Message}", "Extraction Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PanelSplitterApp");
            try
            {
                string json = await client.GetStringAsync("https://api.github.com/repos/dilshan-h/Panel-Splitter/releases/latest");
                JObject release = JObject.Parse(json);
                string latestVersion = release["tag_name"].ToString().TrimStart('v');

                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    UpdateBtn.Content = "✨ Update Available!";
                    MessageBoxResult result = System.Windows.MessageBox.Show($"Update available: v{latestVersion}. Download and install?", "Update Available | Panel Splitter", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Find the setup executable in the release assets
                        #pragma warning disable CS8602 // Dereference of a possibly null reference.
                        #pragma warning disable CS8604 // Possible null reference argument.
                        string setupUrl = release["assets"]
                            .First(a => a["name"].ToString().EndsWith(".exe"))["browser_download_url"]
                            .ToString();
                        #pragma warning restore CS8604 // Possible null reference argument.
                        #pragma warning restore CS8602 // Dereference of a possibly null reference.
                        string tempSetupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PanelSplitterSetup.exe");

                        UpdateBtn.Content = "⬇ Downloading...";
                        DownloadProgressBar.Visibility = Visibility.Visible;
                        #pragma warning disable SYSLIB0014 // Type or member is obsolete
                        using (var wc = new System.Net.WebClient())
                        {
                            wc.DownloadProgressChanged += (s, args) =>
                            {
                                DownloadProgressBar.Value = args.ProgressPercentage;
                            };
                            wc.DownloadFileCompleted += (s, args) =>
                            {
                                DownloadProgressBar.Visibility = Visibility.Collapsed;
                            };
                            await wc.DownloadFileTaskAsync(setupUrl, tempSetupPath);
                        }
                        #pragma warning restore SYSLIB0014 // Type or member is obsolete

                        UpdateBtn.Content = "✔ Downloaded";

                        System.Windows.MessageBox.Show("Download complete. The installer will now start, and this application will close.","Ready to Install | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Run the setup and close the application
                        Process.Start(tempSetupPath);
                        System.Windows.Application.Current.Shutdown();
                    }
                    else
                    {
                        UpdateBtn.Content = "✨ Update Available!";
                        UpdateBtn.IsEnabled = true;
                    }
                }
                else
                {
                    UpdateBtn.Content = "✔ Up to Date";
                    UpdateBtn.IsEnabled = false;
                    UpdateBtn.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF53A85F"));
                }
            }
            catch (Exception ex)
            {
                DownloadProgressBar.Visibility = Visibility.Collapsed;
                UpdateBtn.Content = "Check for updates";
                UpdateBtn.IsEnabled = true;
                System.Windows.MessageBox.Show($"Failed to check for updates: {ex.Message}", "Update Error | Panel Splitter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles changes to the Automatic Updates checkbox.
        /// Saves the preference to settings.
        /// </summary>
        private void UpdatesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoUpdates = UpdatesCheckBox.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles changes to the Analytics checkbox.
        /// Saves the preference to settings and toggles analytics.
        /// </summary>
        private void AnalyticsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AnalyticsEnabled = AnalyticsCheckBox.IsChecked == true;
            Properties.Settings.Default.Save();

            // TODO
            // Analytics initialization
            // if (Properties.Settings.Default.AnalyticsEnabled)
            // {
            //     posthog stuff goes here
            // }
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
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Detects installed Photoshop versions by checking the Windows Registry.
        /// </summary>
        /// <returns>A list of paths to Photoshop installations.</returns>
        private List<string> DetectPhotoshopVersions()
        {
            List<string> paths = [];
            string baseKey = @"SOFTWARE\Adobe\Photoshop";

            // Scan Registry Keys
            InstalledVersionsTextBlock.Text = "Hang on a minute! Scanning Registry...";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(baseKey))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                        {
                            string path = subKey?.GetValue("ApplicationPath") as string;
                            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                            {
                                paths.Add(path);
                            }
                        }
                    }
                }
            }

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
        /// Installs the Panel Splitter.jsx script into the specified Photoshop path.
        /// </summary>
        /// <param name="photoshopPath">The path to the Photoshop installation.</param>
        private static void InstallScript(string photoshopPath)
        {
            string sourceFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Panel Splitter");
            string targetDir = System.IO.Path.Combine(photoshopPath, "Presets", "Scripts", "Panel Splitter");

            if (!Directory.Exists(sourceFolder))
            {
                throw new DirectoryNotFoundException("Panel Splitter folder not found in the application directory.");
            }

            DirectoryCopy(sourceFolder, targetDir);
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

            InstalledVersionsTextBlock.Text = installedVersions.Any()
                ? "🗹 Installed in: " + string.Join(", ", installedVersions)
                : "No installations found.";
        }

        private static void DirectoryCopy(string sourceDir, string destDir)
        {
            DirectoryInfo dir = new(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist: " + sourceDir);

            Directory.CreateDirectory(destDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = System.IO.Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                string tempPath = System.IO.Path.Combine(destDir, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath);
            }
        }

        #endregion
    }
}