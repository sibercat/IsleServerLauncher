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
using Microsoft.Win32;

namespace IsleServerLauncher
{
    public partial class MainWindow : Window
    // Configuration handlers

    {
        // CONFIGURATION HANDLERS
        // ==========================================

        private bool SaveSettings(bool silent)
        {
            try
            {
                _logger.Info("Save settings initiated");

                var validation = ValidateAllInputs();
                if (!validation.IsValid)
                {
                    _logger.Warning($"Configuration validation failed: {validation.GetErrorMessage()}");
                    MessageBox.Show(
                        $"Please fix the following errors:\n\n{validation.GetErrorMessage()}",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }

                // FIXED: Added zombie check parameters AND currentTheme
                var config = _uiMapper.BuildConfigurationFromUI(
                    txtServerName, txtMaxPlayers, txtServerPass,
                    txtRconPass, txtRconPort, chkEnableRcon, chkWhitelist,
                    txtGamePort, txtQueuePort, chkQueueEnabled, txtCustomArgs,
                    txtDayLength, txtNightLength,
                    chkGlobalChat, chkHumans, chkMutations, chkMigration, chkFallDamage,
                    txtGrowth, txtDecay, txtMigrationTime,
                    chkSpawnAI, chkPlants, chkDynamicWeather, txtAISpawn, txtAIDensity,
                    txtRegionSpawnCooldown, chkUseRegionSpawnCooldown, chkUseRegionSpawning,
                    txtPlantSpawnMultiplier, chkAllowRecordingReplay, chkEnableDiets, chkEnablePatrolZones,
                    txtMassMigrationTime, txtMassMigrationDisableTime, chkEnableMassMigration,
                    txtSpeciesMigrationTime, txtMinWeatherVariationInterval, txtMaxWeatherVariationInterval,
                    txtQueueJoinTimeoutSeconds, txtQueueHeartbeatIntervalSeconds, txtQueueHeartbeatTimeoutSeconds, txtQueueHeartbeatMaxMisses,
                    chkValidateFiles, chkDisableStreaming, cmbPriority, chkUseAllCores, pnlCpuCores,
                    chkEnableCrashDetection, chkAutoRestart, cmbMaxRestartAttempts,
                    chkScheduledRestartEnabled, cmbRestartInterval, cmbWarningMinutes,
                    txtRestartMessage, chkRestartScriptEnabled, txtRestartScriptPath, txtRestartScriptDelaySeconds,
                    chkEnableDiscordWebhook, txtDiscordWebhookUrl, txtDiscordInvite,
                    txtModLoaderPath, txtModDllPath, txtModConfigDir,
                    rdoInjectBuiltIn, rdoInjectBat, chkAutoInjectAfterRestart, txtAutoInjectDelaySeconds,
                    chkAutoBackupEnabled, cmbBackupInterval,
                    chkEnableChatMonitor, chkEnableChatWebhook, txtChatWebhookUrl,
                    txtAdminSteamIds, txtWhitelistIds, txtVipIds, lstDinos, lstDisallowedAI,
                    cmbChatRefreshInterval,
                    chkEnableZombieCheck, txtZombieTimeout,
                    chkAutoWipeCorpses, txtWipeInterval, txtWipeDelay, txtWipeWarningMessage, txtWipeCompleteMessage,
                    chkAutoRconSave, txtRconSaveInterval,
                    chkEnableLogRedpointEOSVerbose, chkEnableLogOnlineVerbose,
                    chkEnableLogOnlineGameVerbose, chkEnableLogNetVerbose,
                    chkEnableLogNetTrafficVerbose, chkEnableLogReplicationGraphVerbose,
                    chkEnableLogTheIsleVerbose, chkEnableLogTheIsleAdminVerbose,
                    chkEnableLogTheIsleAIVerbose, chkEnableLogTheIsleAnimInstanceVerbose,
                    chkEnableLogTheIsleAudioVerbose, chkEnableLogTheIsleAuthVerbose,
                    chkEnableLogTheIsleCharacterVerbose, chkEnableLogTheIsleCharacterMovementVerbose,
                    chkEnableLogTheIsleDatabaseVerbose, chkEnableLogTheIsleEnvironmentVerbose,
                    chkEnableLogTheIsleGameVerbose, chkEnableLogTheIsleNetworkVerbose,
                    chkEnableLogTheIsleServerVerbose, chkEnableLogTheIslePlayerControllerVerbose,
                    chkEnableLogTheIsleUIVerbose, chkEnableLogTheIsleWorldVerbose,
                    chkEnableLogTheIsleJoinDataVerbose, chkEnableLogTheIsleChatDataVerbose,
                    chkEnableLogTheIsleKillDataVerbose, chkEnableLogTheIsleCommandDataVerbose,
                    chkEnableLogTheIsleAntiCheatVerbose,
                    _currentTheme); // <--- Added missing argument here

                _configManager.SaveConfiguration(config);
                UpdateRconClient(config.RconPort, config.RconPassword);

                _currentRestartMessage = config.RestartMessage;
                _scheduledRestartService.Configure(
                    config.ScheduledRestartEnabled,
                    config.RestartIntervalHours,
                    config.RestartWarningMinutes,
                    config.RestartMessage);

                UpdateMaintenanceTimers();

                _serverManager.ConfigureWebhook(
                    _discordWebhookService,
                    config.EnableDiscordWebhook,
                    config.DiscordWebhookUrl);

                _lastSavedConfigSignature = BuildConfigSignature(config);
                SetDirty(false);

                _logger.Info("Configuration saved successfully");

                if (!silent)
                {
                    ShowToast("✓ Configuration Saved Successfully");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving configuration: {ex.Message}", ex);
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings(false);
        }

        private void btnBrowseRestartScript_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Batch Files (*.bat)|*.bat|All Files (*.*)|*.*",
                Title = "Select Post-Restart Script"
            };

            if (dialog.ShowDialog() == true)
            {
                txtRestartScriptPath.Text = dialog.FileName;
            }
        }

        // ==========================================
    }
}
