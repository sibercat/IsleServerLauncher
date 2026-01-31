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
    // Server action handlers

    {
        // SERVER ACTION HANDLERS
        // ==========================================

        private async void btnServerAction_Click(object sender, RoutedEventArgs e)
        {
            btnServerAction.IsEnabled = false;
            btnRestart.IsEnabled = false;

            try
            {
                _logger.Info($"Server action requested. Current state: {_serverManager.CurrentState}");

                switch (_serverManager.CurrentState)
                {
                    case ServerState.NotInstalled:
                        await HandleServerInstallAsync();
                        break;

                    case ServerState.Stopped:
                    case ServerState.Crashed:
                        await HandleServerStartAsync();
                        break;

                    case ServerState.Running:
                        await GracefulShutdownAsync(restartAfter: false);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Server action failed: {ex.Message}", ex);
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _serverManager.CheckInitialState();
            }
            finally
            {
                btnServerAction.IsEnabled = true;
            }
        }

        private async void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;

            var result = MessageBox.Show(
                "Are you sure you want to restart the server?\n\nThis will Announce, Save the World, and then Restart.",
                "Confirm Restart",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            btnRestart.IsEnabled = false;
            btnServerAction.IsEnabled = false;

            try
            {
                await GracefulShutdownAsync(restartAfter: true);

                if (_scheduledRestartService.IsEnabled)
                {
                    _scheduledRestartService.ResetTimer();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Restart sequence failed: {ex.Message}", ex);
                MessageBox.Show($"Restart failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _serverManager.CheckInitialState();
            }
            finally
            {
                btnServerAction.IsEnabled = true;
            }
        }

        private async Task HandleServerInstallAsync()
        {
            _serverManager.SetState(ServerState.Installing);
            _logger.Info("Starting server installation");

            await _steamCmdService.InstallOrUpdateServerAsync();

            if (_steamCmdService.IsServerInstalled())
            {
                _logger.Info("Server installation completed");
                _serverManager.SetState(ServerState.Stopped);
                LoadConfiguration();
                ShowToast("✓ Installation Completed");
            }
            else
            {
                _logger.Error("Server installation failed - executable not found");
                MessageBox.Show("Installation failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _serverManager.SetState(ServerState.NotInstalled);
            }
        }

        private async Task HandleServerStartAsync()
        {
            _serverManager.SetState(ServerState.Starting);
            _logger.Info("Starting server");

            if (!InputValidator.IsValidPort(txtGamePort.Text, out int gamePort))
            {
                _logger.Error($"Invalid game port: {txtGamePort.Text}");
                throw new ArgumentException("Invalid game port. Please check your settings.");
            }

            if (chkValidateFiles.IsChecked == true)
            {
                _logger.Info("Validating server files before start");
                await _steamCmdService.ValidateServerFilesAsync();
            }

            if (!SaveSettings(true))
            {
                _serverManager.SetState(ServerState.Stopped);
                return;
            }

            var config = GetCurrentConfiguration();

            _serverManager.ConfigureCrashDetection(
                config.EnableCrashDetection,
                config.AutoRestart,
                config.MaxRestartAttempts);

            string customArgs = txtCustomArgs.Text?.Trim() ?? "";
            string priority = cmbPriority.SelectedIndex switch { 0 => "Normal", 1 => "AboveNormal", 2 => "High", _ => "Normal" };
            string cpuAffinity = GetSelectedCpuCores();
            bool useAllCores = chkUseAllCores.IsChecked.GetValueOrDefault();

            _logger.Info($"Starting server on port {gamePort} with priority {priority}");
            _serverManager.StartServer(gamePort.ToString(), customArgs, priority, cpuAffinity, useAllCores);
            _serverManager.SetState(ServerState.Running);

            if (config.ScheduledRestartEnabled)
            {
                _scheduledRestartService.ResetTimer();
            }

            UpdateMaintenanceTimers();

            _serverManager.ResetRestartCounter();

            await RunPostRestartScriptIfNeededAsync();
        }

        private async Task HandleServerStopAsync()
        {
            _serverManager.SetState(ServerState.Stopping);
            _logger.Info("Stopping server");

            await _serverManager.StopServerAsync(_rconClient);

            if (_serverManager.CurrentState != ServerState.Stopped)
                _serverManager.SetState(ServerState.Stopped);
        }

        private async void OnServerStateChanged(object? sender, ServerState state)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                UpdateUIForServerState(state);
            });
        }

        private void UpdateUIForServerState(ServerState state)
        {
            string statusText = state switch
            {
                ServerState.NotInstalled => "Not Installed",
                ServerState.Installing => "Installing",
                ServerState.Stopped => "Stopped",
                ServerState.Starting => "Starting",
                ServerState.Running => "Running",
                ServerState.Stopping => "Stopping",
                ServerState.Crashed => "Crashed",
                _ => state.ToString()
            };

            txtStatus.Text = $"Status: {statusText}";

            txtStatus.Foreground = state switch
            {
                ServerState.Running => Brushes.Green,
                ServerState.Stopped => Brushes.Red,
                ServerState.NotInstalled => Brushes.Red,
                ServerState.Crashed => Brushes.Orange,
                _ => Brushes.Orange
            };

            if (state == ServerState.Running)
            {
                if (btnServerAction.Content is StackPanel sp && sp.Children.Count > 0 && sp.Children[0] is TextBlock tb)
                {
                    tb.Text = "Stop Server";
                }
                else
                {
                    btnServerAction.Content = "■ Stop Server";
                }

                btnServerAction.Foreground = Brushes.Red;
                btnRestart.IsEnabled = true;
            }
            else
            {
                btnRestart.IsEnabled = false;

                string buttonText = "Start Server";
                if (state == ServerState.Installing) buttonText = "Installing...";
                else if (state == ServerState.NotInstalled) buttonText = "Install Server";

                if (btnServerAction.Content is StackPanel sp && sp.Children.Count > 0 && sp.Children[0] is TextBlock tb)
                {
                    tb.Text = buttonText;
                }
                else
                {
                    btnServerAction.Content = "▶ " + buttonText;
                }

                btnServerAction.Foreground = state == ServerState.Installing ? Brushes.Blue : Brushes.Green;
            }
        }

        // ==========================================
    }
}
