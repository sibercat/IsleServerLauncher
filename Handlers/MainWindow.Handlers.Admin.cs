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

namespace IsleServerLauncher
{
    public partial class MainWindow : Window
    // Administrator tab handlers

    {
        // ADMINISTRATOR TAB HANDLERS
        // ==========================================

        private async void btnSendAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running)
            {
                MessageBox.Show("Server must be running to send announcements.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAnnouncementMessage.Text))
            {
                MessageBox.Show("Please enter an announcement message.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnSendAnnouncement.IsEnabled = false;
            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    await rcon.SendAnnounceAsync(txtAnnouncementMessage.Text);
                    AppendRconOutput($"✓ Announcement sent: {txtAnnouncementMessage.Text}");
                    txtAnnouncementMessage.Clear();
                    ShowToast("✓ Announcement Sent");
                }
            }
            catch (Exception ex)
            {
                AppendRconOutput($"✗ Announcement failed: {ex.Message}");
                MessageBox.Show($"Failed to send announcement:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSendAnnouncement.IsEnabled = true;
            }
        }

        private async void btnTestRcon_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running)
            {
                MessageBox.Show("Server must be running to test RCON connection.", "Server Stopped", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnTestRcon.IsEnabled = false;
            btnTestRcon.Content = "Checking...";

            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    var players = await rcon.GetPlayerListAsync();

                    txtRconStatus.Text = $"Status: Connected ✓ ({players.Count} players)";
                    txtRconStatus.Foreground = Brushes.Green;

                    btnTestRcon.Content = "Connected";
                    btnTestRcon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
                }
            }
            catch
            {
                txtRconStatus.Text = "Status: Failed ✗";
                txtRconStatus.Foreground = Brushes.Red;

                btnTestRcon.Content = "Connect";
                btnTestRcon.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            }
            finally
            {
                btnTestRcon.IsEnabled = true;
            }
        }

        private async void btnRefreshPlayers_Click(object? sender, RoutedEventArgs? e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;

            btnRefreshPlayers.IsEnabled = false;

            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    lstPlayers.ItemsSource = await rcon.GetPlayerListAsync();
                    AppendRconOutput("✓ Player list refreshed");
                }
            }
            catch (Exception ex)
            {
                AppendRconOutput($"✗ Error refreshing players: {ex.Message}");
            }
            finally
            {
                btnRefreshPlayers.IsEnabled = true;
            }
        }

        private async void btnKickPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (lstPlayers.SelectedItem == null) { MessageBox.Show("Select a player."); return; }
            var player = (EvrimaRconPlayer)lstPlayers.SelectedItem;

            var dialog = new Window { Title = $"Kick {player.PlayerName}", Width = 350, Height = 180, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock { Text = "Reason:", FontWeight = FontWeights.Bold });
            var txtReason = new System.Windows.Controls.TextBox { Text = "Rule Violation", Margin = new Thickness(0, 5, 0, 10), Height = 25 };
            stack.Children.Add(txtReason);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnOk = new Button { Content = "Kick", Width = 80, Margin = new Thickness(5) };
            btnOk.Click += (s, args) => dialog.DialogResult = true;
            btnPanel.Children.Add(btnOk);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;

            if (dialog.ShowDialog() != true) return;

            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    await rcon.KickPlayerAsync(player.EosId, txtReason.Text);
                    AppendRconOutput($"✓ Kicked {player.PlayerName}");
                    await Task.Delay(500);
                    btnRefreshPlayers_Click(null, null);
                }
            }
            catch (Exception ex) { AppendRconOutput($"✗ Kick failed: {ex.Message}"); }
        }

        private async void btnBanPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (lstPlayers.SelectedItem == null) { MessageBox.Show("Select a player."); return; }
            var player = (EvrimaRconPlayer)lstPlayers.SelectedItem;

            var dialog = new Window { Title = $"Ban {player.PlayerName}", Width = 400, Height = 250, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var stack = new StackPanel { Margin = new Thickness(10) };

            stack.Children.Add(new TextBlock { Text = $"Banning: {player.PlayerName}", Foreground = Brushes.Red, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

            stack.Children.Add(new TextBlock { Text = "Reason:" });
            var txtReason = new System.Windows.Controls.TextBox { Text = "Rule Violation", Margin = new Thickness(0, 2, 0, 10), Height = 25 };
            stack.Children.Add(txtReason);

            stack.Children.Add(new TextBlock { Text = "Duration (Minutes, 0 = Permanent):" });
            var txtDuration = new System.Windows.Controls.TextBox { Text = "0", Margin = new Thickness(0, 2, 0, 10), Height = 25 };
            stack.Children.Add(txtDuration);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnOk = new Button { Content = "BAN PLAYER", Background = Brushes.Red, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Width = 100, Margin = new Thickness(5) };
            btnOk.Click += (s, args) => dialog.DialogResult = true;
            btnPanel.Children.Add(btnOk);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;

            if (dialog.ShowDialog() != true) return;

            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    await rcon.BanPlayerAsync(player.PlayerName, player.EosId, txtReason.Text, txtDuration.Text);

                    AppendRconOutput($"✓ Banned {player.PlayerName} for {txtDuration.Text} mins");
                    await Task.Delay(500);
                    btnRefreshPlayers_Click(null, null);
                }
            }
            catch (Exception ex) { AppendRconOutput($"✗ Ban failed: {ex.Message}"); }
        }

        private async void btnMessagePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (lstPlayers.SelectedItem == null) { MessageBox.Show("Select a player."); return; }
            var player = (EvrimaRconPlayer)lstPlayers.SelectedItem;

            var dialog = new Window { Title = $"Message {player.PlayerName}", Width = 350, Height = 180, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var stack = new StackPanel { Margin = new Thickness(10) };
            stack.Children.Add(new TextBlock { Text = "Enter Message:", FontWeight = FontWeights.Bold });
            var txtMessage = new System.Windows.Controls.TextBox { Text = "", Margin = new Thickness(0, 5, 0, 10), Height = 25 };
            stack.Children.Add(txtMessage);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnOk = new Button { Content = "Send", Width = 80, Margin = new Thickness(5) };
            btnOk.Click += (s, args) => dialog.DialogResult = true;
            btnPanel.Children.Add(btnOk);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;
            dialog.Loaded += (s, args) => txtMessage.Focus();

            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(txtMessage.Text)) return;

            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    await rcon.SendDirectMessageAsync(player.EosId, txtMessage.Text);
                    AppendRconOutput($"✓ DM to {player.PlayerName}: {txtMessage.Text}");
                }
            }
            catch (Exception ex) { AppendRconOutput($"✗ Message failed: {ex.Message}"); }
        }

        private async void btnGetPlayerData_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;
            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    string? data = await rcon.GetPlayerDataAsync();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        ShowDataDialog("Detailed Player Data", data);
                    }
                    else
                    {
                        MessageBox.Show("No player data available.", "Player Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving player data:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEnableWhitelist_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleWhitelistAsync(true), "Enable Whitelist");
        private async void btnDisableWhitelist_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleWhitelistAsync(false), "Disable Whitelist");
        private async void btnAddToWhitelist_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(txtWhitelistId.Text)) await RunRconCommand(async r => await r.AddWhitelistIdAsync(txtWhitelistId.Text), "Add Whitelist"); }
        private async void btnRemoveFromWhitelist_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(txtWhitelistId.Text)) await RunRconCommand(async r => await r.RemoveWhitelistIdAsync(txtWhitelistId.Text), "Remove Whitelist"); }

        private async void btnEnableGlobalChat_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleGlobalChatAsync(true), "Enable Global Chat");
        private async void btnDisableGlobalChat_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleGlobalChatAsync(false), "Disable Global Chat");
        private async void btnEnableHumans_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleHumansAsync(true), "Enable Humans");
        private async void btnDisableHumans_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleHumansAsync(false), "Disable Humans");
        private async void btnEnableAI_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleAIAsync(true), "Enable AI");
        private async void btnDisableAI_Click(object sender, RoutedEventArgs e) => await RunRconCommand(async r => await r.ToggleAIAsync(false), "Disable AI");

        private async void btnSetAIDensity_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(txtLiveAIDensity.Text, out float d)) await RunRconCommand(async r => await r.SetAIDensityAsync(d), "Set AI Density");
        }

        private async void btnWipeCorpses_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wipe all corpses?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                await RunRconCommand(async r => await r.WipeCorpsesAsync(), "Wipe Corpses");
        }

        private async void btnGetServerDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;
            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    string? details = await rcon.GetServerDetailsAsync();
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        ShowDataDialog("Server Details", details);
                    }
                    else
                    {
                        MessageBox.Show("No server details available.", "Server Details", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving server details:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearOutput_Click(object sender, RoutedEventArgs e) => txtRconOutput.Clear();

        private async Task RunRconCommand<T>(Func<RconClient, Task<T>> command, string actionName)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;
            try
            {
                var config = GetCurrentConfiguration();
                using (var rcon = new RconClient("127.0.0.1", int.Parse(config.RconPort), config.RconPassword, _logger))
                {
                    var result = await command(rcon);
                    AppendRconOutput($"✓ {actionName}: {result}");
                }
            }
            catch (Exception ex) { AppendRconOutput($"✗ {actionName} Failed: {ex.Message}"); }
        }

        private void AppendRconOutput(string message)
        {
            Dispatcher.Invoke(() => {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtRconOutput.AppendText($"[{timestamp}] {message}\n");
                txtRconOutput.ScrollToEnd();
            });
        }

        // ==========================================
    }
}
