using IsleServerLauncher.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace IsleServerLauncher
{
    public partial class MainWindow : Window
    // System setup handlers

    {
        private const string GitHubLatestReleaseApi = "https://api.github.com/repos/sibercat/The-Isle-Evrima-Server-Launcher/releases/latest";
        private const string GitHubReleasesPage = "https://github.com/sibercat/The-Isle-Evrima-Server-Launcher/releases";
        private static readonly HttpClient _httpClient = new HttpClient();
        // SYSTEM SETUP HANDLERS
        // ==========================================

        private async void btnServerSetup_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will configure your Windows Server for The Isle:\n\n• Open firewall ports\n• Install VC++\n• Disable IE Security\n\nAdministrator privileges required. Continue?",
                "Server Setup", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (!_systemSetup.IsAdministrator()) { MessageBox.Show("Run as Admin required.", "Error"); return; }
                await _systemSetup.RunServerSetupAsync();
                ShowToast("✓ Server Setup Complete");
            }
            catch (Exception ex) { MessageBox.Show($"Setup failed: {ex.Message}", "Error"); }
        }

        private async void btnNetworkFix_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This optimizes drivers for VPS/Cloud Servers (VirtIO fix).\n\nRequires restart. Continue?",
                "Network Optimization", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (!_systemSetup.IsAdministrator()) { MessageBox.Show("Run as Admin required.", "Error"); return; }
                await _systemSetup.RunNetworkOptimizationAsync();
                ShowToast("✓ Network Optimization Complete");
                MessageBox.Show("Please restart your VPS/Server for changes to take effect.", "Restart Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Optimization failed: {ex.Message}", "Error"); }
        }

        private async void btnFixSSL_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Install Amazon Root CA 1 to fix SSL errors?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                if (!_systemSetup.IsAdministrator()) { MessageBox.Show("Run as Admin required.", "Error"); return; }
                await _systemSetup.FixSSLCertificateAsync();
                ShowToast("✓ SSL Fix Applied");
            }
            catch (Exception ex) { MessageBox.Show($"SSL fix failed: {ex.Message}", "Error"); }
        }

        private void btnTroubleshooting_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1. Client stuck connecting? Run InstallAntiCheat.bat in game folder.\n2. SSL Errors? Use the Help menu SSL fix.", "Troubleshooting");
        }

        internal async void btnCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item) item.IsEnabled = false;

            try
            {
                await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update check failed:\n\n{ex.Message}", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sender is MenuItem itemRestore) itemRestore.IsEnabled = true;
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GitHubLatestReleaseApi);
            request.Headers.UserAgent.ParseAdd("IsleServerLauncher/1.0");
            request.Headers.Accept.ParseAdd("application/vnd.github+json");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Update check failed: {response.StatusCode}", "Update Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            string latestTag = doc.RootElement.TryGetProperty("tag_name", out var tagEl) ? (tagEl.GetString() ?? "") : "";
            string latestUrl = doc.RootElement.TryGetProperty("html_url", out var urlEl) ? (urlEl.GetString() ?? GitHubReleasesPage) : GitHubReleasesPage;

            var currentVersion = GetCurrentVersion();
            var latestVersion = ParseVersion(latestTag);

            if (currentVersion != null && latestVersion != null)
            {
                if (latestVersion > currentVersion)
                {
                    var result = MessageBox.Show(
                        $"A new version is available.\n\nCurrent: v{currentVersion}\nLatest: v{latestVersion}\n\nOpen download page?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo { FileName = latestUrl, UseShellExecute = true });
                    }
                }
                else
                {
                    MessageBox.Show($"You're up to date.\n\nCurrent: v{currentVersion}", "Update Check",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }

            var fallbackResult = MessageBox.Show(
                $"Latest release: {latestTag}\n\nOpen download page?",
                "Update Check",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            if (fallbackResult == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { FileName = latestUrl, UseShellExecute = true });
            }
        }

        private Version? GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null) return version;

            return ParseVersion(Title);
        }

        private static Version? ParseVersion(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            string cleaned = value.Trim();
            if (cleaned.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[1..];
            }

            if (Version.TryParse(cleaned, out var v))
            {
                return v;
            }

            var match = System.Text.RegularExpressions.Regex.Match(cleaned, @"\d+(\.\d+)+");
            return match.Success && Version.TryParse(match.Value, out v) ? v : null;
        }

        internal void btnOpenAiAdminUi_Click(object sender, RoutedEventArgs e)
        {
            string toolPath = Path.Combine(_serverFolder, "tools", "AiAdminUi", "AiAdminUi.exe");
            if (!File.Exists(toolPath))
            {
                MessageBox.Show(
                    "AI Admin UI not found.\nExpected path:\n" + toolPath,
                    "AI Admin UI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = toolPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open AI Admin UI:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new Window
            {
                Title = "About",
                Width = 350,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "The Isle Evrima Server Launcher",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Version: 1.0.3",
                Margin = new Thickness(0, 0, 0, 10)
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Created by: Sibercat.",
                Margin = new Thickness(0, 0, 0, 10)
            });

            var linkTextBlock = new TextBlock { Margin = new Thickness(0, 0, 0, 10) };
            linkTextBlock.Inlines.Add(new Run("GitHub: "));
            var hyperlink = new Hyperlink(new Run("https://github.com/sibercat/The-Isle-Evrima-Server-Launcher"))
            {
                NavigateUri = new Uri("https://github.com/sibercat/The-Isle-Evrima-Server-Launcher")
            };
            hyperlink.RequestNavigate += (s, args) =>
            {
                Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute = true });
                args.Handled = true;
            };
            linkTextBlock.Inlines.Add(hyperlink);
            stackPanel.Children.Add(linkTextBlock);

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            okButton.Click += (s, args) => aboutWindow.Close();
            stackPanel.Children.Add(okButton);

            aboutWindow.Content = stackPanel;
            aboutWindow.ShowDialog();
        }

        private async void btnTestAnnounce_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) { MessageBox.Show("Server must be running.", "Error"); return; }

            try
            {
                var config = GetCurrentConfiguration();
                using (var tempRcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    string message = string.IsNullOrWhiteSpace(txtRestartMessage.Text) ? "Test Announcement" : txtRestartMessage.Text.Replace("{minutes}", "TEST");
                    await tempRcon.SendAnnounceAsync(message);
                    ShowToast("✓ Announcement Sent");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
        }

        private async void btnTestWebhook_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDiscordWebhookUrl.Text)) return;
            btnTestWebhook.IsEnabled = false;
            try
            {
                await _discordWebhookService.SendTestNotificationAsync(txtDiscordWebhookUrl.Text, txtServerName.Text);
                ShowToast("✓ Test Notification Sent");
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
            finally { btnTestWebhook.IsEnabled = true; }
        }

        // ==========================================
    }
}
