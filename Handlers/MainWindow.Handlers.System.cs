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

namespace IsleServerLauncher
{
    public partial class MainWindow : Window
    // System setup handlers

    {
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

        private void btnOpenAiAdminUi_Click(object sender, RoutedEventArgs e)
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
                Text = "Version: 1.0.1",
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
