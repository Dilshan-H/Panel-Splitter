using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Panel_Splitter
{
    /// <summary>
    /// Provides methods for checking for updates to the Panel Splitter application.
    /// </summary>
    public static class UpdateHelper
    {
        public const string CurrentVersion = "2.1";

        /// <summary>
        /// Checks for available updates for the application asynchronously.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>updateAvailable</c>: A boolean indicating if an update is available.</description></item>
        /// <item><description><c>latestVersion</c>: The latest version number available.</description></item>
        /// <item><description><c>setupUrl</c>: The URL to download the latest setup file.</description></item>
        /// <item><description><c>error</c>: An exception that occurred during the update check, or <c>null</c> if no error occurred.</description></item>
        /// </list>
        /// </returns>
        public static async Task<(bool updateAvailable, string latestVersion, string setupUrl, Exception error)> CheckForUpdatesAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PanelSplitterApp");
            try
            {
                string json = await client.GetStringAsync("https://api.github.com/repos/dilshan-h/Panel-Splitter/releases/latest");
                JObject release = JObject.Parse(json);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string latestVersion = release["tag_name"].ToString().TrimStart('v');

                if (new Version(latestVersion) > new Version(CurrentVersion))
                {
#pragma warning disable CS8604 // Possible null reference argument.

                    string setupUrl = release["assets"]
                        .First(static a => a["name"].ToString().EndsWith(".exe"))["browser_download_url"]
                        .ToString();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    return (true, latestVersion, setupUrl, null);

                }

                return (false, latestVersion, null, null);

            }
            catch (Exception ex)
            {
                return (false, null, null, ex);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }
    }
}