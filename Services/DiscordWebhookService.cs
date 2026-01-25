using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public class DiscordWebhookService
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public DiscordWebhookService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends a crash notification to Discord via webhook
        /// </summary>
        public async Task<bool> SendCrashNotificationAsync(string webhookUrl, string serverName, DateTime crashTime, int? exitCode, TimeSpan uptime)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.Warning("Discord webhook URL is empty");
                return false;
            }

            if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Invalid Discord webhook URL: {webhookUrl}");
                return false;
            }

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "🔴 Server Crash Detected",
                            description = $"**{serverName}** has crashed unexpectedly.",
                            color = 15548997, // Red color (0xED4245)
                            fields = new[]
                            {
                                new { name = "Crash Time", value = crashTime.ToString("yyyy-MM-dd HH:mm:ss"), inline = true },
                                new { name = "Exit Code", value = exitCode?.ToString() ?? "Unknown", inline = true },
                                new { name = "Uptime", value = FormatUptime(uptime), inline = true }
                            },
                            timestamp = crashTime.ToString("o"),
                            footer = new
                            {
                                text = "Isle Server Launcher"
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Info($"Sending crash notification to Discord webhook");
                var response = await _httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Crash notification sent successfully to Discord");
                    return true;
                }
                else
                {
                    _logger.Error($"Discord webhook failed with status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending Discord webhook: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a test notification to verify webhook configuration
        /// </summary>
        public async Task<bool> SendTestNotificationAsync(string webhookUrl, string serverName)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.Warning("Discord webhook URL is empty");
                return false;
            }

            if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Invalid Discord webhook URL: {webhookUrl}");
                return false;
            }

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "✅ Test Notification",
                            description = $"Discord webhook is configured correctly for **{serverName}**!",
                            color = 5763719, // Green color (0x57F287)
                            timestamp = DateTime.Now.ToString("o"),
                            footer = new
                            {
                                text = "Isle Server Launcher"
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Info("Sending test notification to Discord webhook");
                var response = await _httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Test notification sent successfully to Discord");
                    return true;
                }
                else
                {
                    _logger.Error($"Discord webhook test failed with status: {response.StatusCode}");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.Error($"Response: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending test Discord webhook: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a server restart notification to Discord
        /// </summary>
        public async Task<bool> SendRestartNotificationAsync(string webhookUrl, string serverName, bool autoRestart, int attempt, int maxAttempts)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return false;

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "🔄 Server Restarting",
                            description = autoRestart
                                ? $"**{serverName}** is attempting automatic restart (Attempt {attempt}/{maxAttempts})"
                                : $"**{serverName}** is restarting",
                            color = 16776960, // Yellow color (0xFEE75C)
                            timestamp = DateTime.Now.ToString("o"),
                            footer = new
                            {
                                text = "Isle Server Launcher"
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending restart notification: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Sends auto-restart recovery notification to Discord
        /// </summary>
        public async Task<bool> SendAutoRestartSuccessAsync(string webhookUrl, string serverName, int attemptNumber)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return false;

            if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Invalid Discord webhook URL: {webhookUrl}");
                return false;
            }

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "✅ Server Recovered",
                            description = $"**{serverName}** has automatically restarted and recovered from crash (Attempt {attemptNumber})",
                            color = 5763719, // Green color (0x57F287)
                            timestamp = DateTime.Now.ToString("o"),
                            footer = new
                            {
                                text = "Isle Server Launcher"
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Info("Sending auto-restart success notification to Discord");
                var response = await _httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Auto-restart success notification sent to Discord");
                    return true;
                }
                else
                {
                    _logger.Error($"Discord webhook failed with status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending recovery notification: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Sends auto-restart failure notification
        /// </summary>
        public async Task<bool> SendAutoRestartFailureAsync(string webhookUrl, string serverName, int maxAttempts)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return false;

            if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Invalid Discord webhook URL: {webhookUrl}");
                return false;
            }

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "❌ Auto-Restart Failed",
                            description = $"**{serverName}** failed to restart after {maxAttempts} attempts. Manual intervention required.",
                            color = 15548997, // Red color (0xED4245)
                            timestamp = DateTime.Now.ToString("o"),
                            footer = new
                            {
                                text = "Isle Server Launcher"
                            }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Info("Sending auto-restart failure notification to Discord");
                var response = await _httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Auto-restart failure notification sent to Discord");
                    return true;
                }
                else
                {
                    _logger.Error($"Discord webhook failed with status: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending failure notification: {ex.Message}", ex);
                return false;
            }
        }


        /// <summary>
        /// Sends batched chat messages to Discord
        /// </summary>
        public async Task<bool> SendBatchedChatMessagesAsync(string webhookUrl, string serverName, string[] messages)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl) || messages.Length == 0)
                return false;

            if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error($"Invalid Discord webhook URL: {webhookUrl}");
                return false;
            }

            try
            {
                // Batch messages into groups of 10 to avoid hitting embed limits
                var batches = messages.Select((msg, idx) => new { msg, idx })
                    .GroupBy(x => x.idx / 10)
                    .Select(g => g.Select(x => x.msg).ToArray());

                foreach (var batch in batches)
                {
                    var embed = new
                    {
                        embeds = new[]
                        {
                            new
                            {
                                title = $"💬 {serverName} - Chat",
                                description = string.Join("\n", batch),
                                color = 3447003, // Blue
                                timestamp = DateTime.Now.ToString("o"),
                                footer = new { text = "Isle Server Launcher" }
                            }
                        }
                    };

                    string jsonContent = JsonSerializer.Serialize(embed);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(webhookUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.Warning($"Discord chat webhook failed with status: {response.StatusCode}");
                    }

                    // Small delay to avoid rate limits if sending multiple batches
                    if (batches.Count() > 1)
                        await Task.Delay(1000);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending chat batch to Discord: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a test chat notification
        /// </summary>
        public async Task<bool> SendChatTestNotificationAsync(string webhookUrl, string serverName)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return false;

            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = "✅ Chat Monitor Test",
                            description = $"Chat webhook is configured correctly for **{serverName}**!\n\n" +
                                         "**[Global]** TestPlayer: This is a test message\n" +
                                         "**[Local]** AdminUser: Testing spatial chat",
                            color = 5763719, // Green
                            timestamp = DateTime.Now.ToString("o"),
                            footer = new { text = "Isle Server Launcher" }
                        }
                    }
                };

                string jsonContent = JsonSerializer.Serialize(embed);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending chat test notification: {ex.Message}", ex);
                return false;
            }
        }

        private string FormatUptime(TimeSpan uptime)
        {
            if (uptime.TotalMinutes < 1)
                return "Less than 1 minute";
            else if (uptime.TotalHours < 1)
                return $"{(int)uptime.TotalMinutes} minute(s)";
            else if (uptime.TotalDays < 1)
                return $"{(int)uptime.TotalHours} hour(s), {uptime.Minutes} minute(s)";
            else
                return $"{(int)uptime.TotalDays} day(s), {uptime.Hours} hour(s)";
        }
    }
}