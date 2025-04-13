using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Panel_Splitter
{
    /// <summary>
    /// Provides methods for sending analytics data and capturing events.
    /// </summary>
    public static class AnalyticsHelper
    {
        // User role status
        private static readonly bool IsTestUser = true;
        private static readonly string TimestampFormat = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz";

        private static readonly string LogFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Panel Splitter Logs");
        private static readonly string LogFilePath = Path.Combine(LogFolderPath, "PanelSplitterDataLog.txt");
        private static readonly string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Panel Splitter Logs", "AppErrorLog.txt");

        /// <summary>
        /// Asynchronously sends log data to PostHog if analytics is enabled.
        /// </summary>
        public static async Task SendLogsAsync()
        {
            // Check if analytics is enabled
            if (!Properties.Settings.Default.AnalyticsEnabled)
            {
                File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Analytics disabled, skipping log sending.\n");
                return;
            }

            if (!Directory.Exists(LogFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(LogFolderPath);
                }
                catch (Exception ex)
                {
                    File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Failed to create log directory '{LogFolderPath}': {ex.Message}\n");
                    return;
                }
            }

            if (!File.Exists(LogFilePath)) return; // No logs to send

            try
            {
                string[] logEntries = File.ReadAllLines(LogFilePath);
                var batchEvents = new List<object>();

                foreach (string entry in logEntries)
                {
                    string[] parts = entry.Split('|');
                    if (parts.Length < 5) continue; // Minimum: timestamp, event, ps_version, script_version, test_user

                    string rawTimestamp = parts[0].Trim();
                    string eventType = parts[1].Trim(' ', '[', ']');
                    string appPsVersion = parts[2].Trim();
                    string scriptVersion = parts[3].Trim();
                    bool testUser = bool.Parse(parts[4].Trim());

                    // Convert timestamp to ISO 8601 for PostHog compatibility
                    string logTimestamp;
                    try
                    {
                        DateTimeOffset dto = DateTimeOffset.ParseExact(rawTimestamp, TimestampFormat, CultureInfo.InvariantCulture);
                        logTimestamp = dto.ToString("o"); // ISO 8601 format (like "2025-04-04T23:48:30+05:30")
                    }
                    catch (FormatException ex)
                    {
                        File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Failed to parse timestamp '{rawTimestamp}': {ex.Message}\n");
                        if (eventType != "App-Error")
                            _ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                            {
                                { "app_error_type", "App-AH-Timestamp-Conversion" },
                                { "app_error_msg", ex.Message },
                                { "app_error_timestamp", rawTimestamp }
                            });
                        continue; // Skip this entry if timestamp parsing fails
                    }

                    var properties = new Dictionary<string, object>
                    {
                        { "app_ps_version", appPsVersion },
                        { "script_version", scriptVersion },
                        { "script_test_user", testUser },
                        { "log_timestamp", logTimestamp }
                    };

                    switch (eventType)
                    {
                        case "Script-Startup":
                            // No additional fields
                            break;

                        case "Script-Error":
                            if (parts.Length > 5)
                                properties["script_error"] = parts[5].Trim();
                            break;

                        case "Script-Usage-Start":
                            if (parts.Length > 5)
                                properties["canvas_size"] = parts[5].Trim();
                            break;

                        case "Script-Usage-End":
                            if (parts.Length > 8)
                            {
                                properties["row_count"] = int.Parse(parts[5].Trim());
                                properties["column_count"] = int.Parse(parts[6].Trim());
                                properties["total_panels"] = int.Parse(parts[7].Trim());
                                properties["time_taken"] = double.Parse(parts[8].Trim());
                            }
                            break;

                        default:
                            File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Unknown event type '[{eventType}]' in log: {entry}\n");
                            continue;
                    }

                    batchEvents.Add(new
                    {
                        @event = eventType,
                        distinct_id = Properties.Settings.Default.UserDistinctId,
                        properties = MergeProperties(properties),
                        timestamp = logTimestamp
                    });
                }

                if (batchEvents.Count > 0)
                {
                    await SendBatchEvents(batchEvents);
                    File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Successfully sent {batchEvents.Count} log entries.\n");
                }
                else
                {
                    File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: No valid log entries found to send.\n");
                }

                // Clear the log file after successful sending
                File.WriteAllText(LogFilePath, "");
            }
            catch (Exception ex)
            {
                File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Failed to send logs: {ex.Message}\n");
                //_ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                //{
                //    { "app_error_type", "App-AH-Send-Log-Failure" },
                //    { "app_error_msg", ex.Message }
                //});
            }
        }

        /// <summary>
        /// Asynchronously sends a batch of events to PostHog.
        /// </summary>
        /// <param name="batchEvents">List of events to send.</param>
        /// <returns></returns>
        private static async Task SendBatchEvents(List<object> batchEvents)
        {
            var payload = new
            {
                api_key = Encoding.UTF8.GetString
                (
                    Convert.FromBase64String("API_KEY_BASE64")
                ),
                historical_migration = false,
                batch = batchEvents
            };

            try
            {
                using var client = new HttpClient();
                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://eu.i.posthog.com/batch/", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Failed to send batch to PostHog: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Asynchronously captures an event for analytics purposes.
        /// </summary>
        /// <param name="eventType">The name of the event to capture.</param>
        /// <param name="properties">Optional dictionary of additional properties associated with the event.</param>
        public static async Task CaptureEvent(string eventType, Dictionary<string, object> properties = null)
        {
            // Check if analytics is enabled
            if (!Properties.Settings.Default.AnalyticsEnabled) return;

            // Ensure distinct ID exists
            string distinctId = Properties.Settings.Default.UserDistinctId;
            if (string.IsNullOrEmpty(distinctId))
            {
                distinctId = Guid.NewGuid().ToString();
                Properties.Settings.Default.UserDistinctId = distinctId;
                Properties.Settings.Default.Save();
                await CaptureEvent("App-Installation", new Dictionary<string, object>
                {
                    { "app_os", Environment.OSVersion.ToString() },
                    { "app_locale", System.Globalization.CultureInfo.CurrentCulture.Name },
                    { "app_architecture", Environment.Is64BitOperatingSystem ? "x64" : "x86" },
                    { "app_language", System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName },
                    { "app_time_zone", TimeZoneInfo.Local.StandardName },
                    { "app_country", System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName },
                    { "app_device_os_build", Environment.OSVersion.Version.Build.ToString() },
                    { "test_user", IsTestUser }
                });
            }

            var payload = new
            {
                api_key = Encoding.UTF8.GetString
                (
                    Convert.FromBase64String("API_KEY_BASE64")
                ),
                @event = eventType,
                distinct_id = distinctId,
                properties = MergeProperties(properties ?? new Dictionary<string, object>()),

                // If event type is 'Script-Usage', use the received timestamp; otherwise use time now
                timestamp = eventType == "Script-Usage" && properties != null && properties.TryGetValue("log_timestamp", out object? value)
                    ? value.ToString()
                    : DateTime.UtcNow.ToString("o")
            };

            try
            {
                using var client = new HttpClient();
                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("https://eu.i.posthog.com/capture/", content);
            }
            catch (Exception ex)
            {
                File.AppendAllText(ErrorLogPath, $"{DateTime.UtcNow}: Failed to capture event '[{eventType}]': {ex.Message}\n");
                //_ = AnalyticsHelper.CaptureEvent("App-Error", new Dictionary<string, object>
                //{
                //    { "app_error_type", "App-AH-Capture-Event-Failure" },
                //    { "app_error_msg", ex.Message },
                //    { "event_type", eventType }
                //});
            }
        }

        private static object MergeProperties(Dictionary<string, object> customProperties)
        {
            var baseProperties = new Dictionary<string, object>
            {
                { "process_person_profile", false },
                { "app_version", UpdateHelper.CurrentVersion },
                { "app_os", Environment.OSVersion.ToString() },
                { "app_device_os_build", Environment.OSVersion.Version.Build.ToString() },
                { "app_locale", CultureInfo.CurrentCulture.Name },
                { "app_architecture", Environment.Is64BitOperatingSystem ? "x64" : "x86" },
                { "app_language", CultureInfo.CurrentCulture.TwoLetterISOLanguageName },
                { "app_time_zone", TimeZoneInfo.Local.StandardName },
                { "app_country", RegionInfo.CurrentRegion.TwoLetterISORegionName },
                { "test_user", IsTestUser }
            };

            // Merge custom properties into base properties, overriding if keys overlap
            foreach (var kvp in customProperties)
            {
                baseProperties[kvp.Key] = kvp.Value;
            }

            return baseProperties;
        }
    }
}