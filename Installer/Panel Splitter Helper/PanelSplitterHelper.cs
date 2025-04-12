using System;
using System.IO;

namespace Panel_Splitter_Helper
{
    /// <summary>
    /// Provides functionality to copy the script files using provided paths.
    /// </summary>
    internal class PanelSplitterHelper
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">An array of command-line arguments. The first argument must be "/install", second and third for file paths.</param>
        static void Main(string[] args)
        {
            if (args.Length < 3 || args[0] != "/install") return;
            string destDir = args[1];
            string sourceDir = args[2];
            try
            {
                InstallScript(destDir, sourceDir);
                Console.WriteLine("Script installed successfully.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Installs the script to the specified target path.
        /// This method requires administrative privileges to copy files to a protected directory.
        /// </summary>
        /// <param name="destDir">The path where the script will be installed.</param>
        /// <param name="sourceDir">The path where the script is located within main app.</param>
        static void InstallScript(string destDir, string sourceDir)
        {
            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir))
            {
                throw new ArgumentException("Source and destination paths cannot be null or empty.");
            }

            DirectoryInfo dir = new DirectoryInfo(sourceDir);
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
                InstallScript(subdir.FullName, tempPath);
            }
        }
    }
}
