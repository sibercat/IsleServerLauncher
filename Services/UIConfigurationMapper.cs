using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace IsleServerLauncher.Services
{
    public class UIConfigurationMapper
    {
        private readonly ILogger _logger;

        public UIConfigurationMapper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LoadConfigurationIntoUI(
            ServerConfiguration config,
            TextBox txtServerName, TextBox txtMaxPlayers, TextBox txtServerPass,
            TextBox txtRconPass, TextBox txtRconPort, CheckBox chkEnableRcon, CheckBox chkWhitelist,
            TextBox txtGamePort, TextBox txtQueuePort,
            CheckBox chkQueueEnabled, TextBox txtCustomArgs,
            TextBox txtDayLength, TextBox txtNightLength,
            CheckBox chkGlobalChat, CheckBox chkHumans, CheckBox chkMutations,
            CheckBox chkMigration, CheckBox chkFallDamage, TextBox txtGrowth,
            TextBox txtDecay, TextBox txtMigrationTime,
            CheckBox chkSpawnAI, CheckBox chkPlants, CheckBox chkDynamicWeather,
            TextBox txtAISpawn, TextBox txtAIDensity,
            TextBox txtRegionSpawnCooldown, CheckBox chkUseRegionSpawnCooldown, CheckBox chkUseRegionSpawning,
            TextBox txtPlantSpawnMultiplier, CheckBox chkAllowRecordingReplay, CheckBox chkEnableDiets, CheckBox chkEnablePatrolZones,
            TextBox txtMassMigrationTime, TextBox txtMassMigrationDisableTime, CheckBox chkEnableMassMigration,
            TextBox txtSpeciesMigrationTime, TextBox txtMinWeatherVariationInterval, TextBox txtMaxWeatherVariationInterval,
            TextBox txtQueueJoinTimeoutSeconds, TextBox txtQueueHeartbeatIntervalSeconds, TextBox txtQueueHeartbeatTimeoutSeconds, TextBox txtQueueHeartbeatMaxMisses,
            CheckBox chkValidateFiles, CheckBox chkDisableStreaming,
            ComboBox cmbPriority, CheckBox chkUseAllCores, Panel pnlCpuCores,
            CheckBox chkEnableCrashDetection, CheckBox chkAutoRestart, ComboBox cmbMaxRestartAttempts,
            CheckBox chkScheduledRestartEnabled, ComboBox cmbRestartInterval, ComboBox cmbWarningMinutes,
            TextBox txtRestartMessage,
            CheckBox chkEnableDiscordWebhook, TextBox txtDiscordWebhookUrl, TextBox txtDiscordInvite,
            CheckBox chkAutoBackupEnabled, ComboBox cmbBackupInterval,
            CheckBox chkEnableChatMonitor, CheckBox chkEnableChatWebhook, TextBox txtChatWebhookUrl,
            TextBox txtAdminSteamIds, TextBox txtWhitelistIds, TextBox txtVipIds, ListBox lstDinos, ListBox lstDisallowedAI,
            ComboBox cmbChatRefreshInterval,
            CheckBox chkEnableZombieCheck, TextBox txtZombieTimeout,
            CheckBox chkAutoWipeCorpses, TextBox txtWipeInterval, TextBox txtWipeDelay,
            TextBox txtWipeWarningMessage, TextBox txtWipeCompleteMessage,
            CheckBox chkAutoRconSave, TextBox txtRconSaveInterval,
            CheckBox chkEnableLogRedpointEOSVerbose, CheckBox chkEnableLogOnlineVerbose,
            CheckBox chkEnableLogOnlineGameVerbose, CheckBox chkEnableLogNetVerbose,
            CheckBox chkEnableLogNetTrafficVerbose, CheckBox chkEnableLogReplicationGraphVerbose,
            CheckBox chkEnableLogTheIsleVerbose, CheckBox chkEnableLogTheIsleAdminVerbose,
            CheckBox chkEnableLogTheIsleAIVerbose, CheckBox chkEnableLogTheIsleAnimInstanceVerbose,
            CheckBox chkEnableLogTheIsleAudioVerbose, CheckBox chkEnableLogTheIsleAuthVerbose,
            CheckBox chkEnableLogTheIsleCharacterVerbose, CheckBox chkEnableLogTheIsleCharacterMovementVerbose,
            CheckBox chkEnableLogTheIsleDatabaseVerbose, CheckBox chkEnableLogTheIsleEnvironmentVerbose,
            CheckBox chkEnableLogTheIsleGameVerbose, CheckBox chkEnableLogTheIsleNetworkVerbose,
            CheckBox chkEnableLogTheIsleServerVerbose, CheckBox chkEnableLogTheIslePlayerControllerVerbose,
            CheckBox chkEnableLogTheIsleUIVerbose, CheckBox chkEnableLogTheIsleWorldVerbose,
            CheckBox chkEnableLogTheIsleJoinDataVerbose, CheckBox chkEnableLogTheIsleChatDataVerbose,
            CheckBox chkEnableLogTheIsleKillDataVerbose, CheckBox chkEnableLogTheIsleCommandDataVerbose,
            CheckBox chkEnableLogTheIsleAntiCheatVerbose)
        {
            try
            {
                _logger.Info("Loading configuration into UI");

                txtServerName.Text = config.ServerName;
                txtMaxPlayers.Text = config.MaxPlayers;
                txtServerPass.Text = config.ServerPassword;
                txtRconPass.Text = config.RconPassword;
                txtRconPort.Text = config.RconPort;
                chkEnableRcon.IsChecked = config.RconEnabled;
                chkWhitelist.IsChecked = config.Whitelist;
                txtGamePort.Text = config.GamePort;
                txtQueuePort.Text = config.QueuePort;
                chkQueueEnabled.IsChecked = config.QueueEnabled;
                txtCustomArgs.Text = config.CustomArgs;
                txtDayLength.Text = config.DayLength;
                txtNightLength.Text = config.NightLength;
                chkGlobalChat.IsChecked = config.GlobalChat;
                chkHumans.IsChecked = config.Humans;
                chkMutations.IsChecked = config.Mutations;
                chkMigration.IsChecked = config.Migration;
                chkFallDamage.IsChecked = config.FallDamage;
                txtGrowth.Text = config.GrowthMultiplier;
                txtDecay.Text = config.CorpseDecay;
                txtMigrationTime.Text = config.MigrationTime;
                chkSpawnAI.IsChecked = config.SpawnAI;
                chkPlants.IsChecked = config.SpawnPlants;
                chkDynamicWeather.IsChecked = config.DynamicWeather;
                txtAISpawn.Text = config.AISpawnInterval;
                txtAIDensity.Text = config.AIDensity;
                txtRegionSpawnCooldown.Text = config.RegionSpawnCooldownTimeSeconds;
                chkUseRegionSpawnCooldown.IsChecked = config.UseRegionSpawnCooldown;
                chkUseRegionSpawning.IsChecked = config.UseRegionSpawning;
                txtPlantSpawnMultiplier.Text = config.PlantSpawnMultiplier;
                chkAllowRecordingReplay.IsChecked = config.AllowRecordingReplay;
                chkEnableDiets.IsChecked = config.EnableDiets;
                chkEnablePatrolZones.IsChecked = config.EnablePatrolZones;
                txtMassMigrationTime.Text = config.MassMigrationTime;
                txtMassMigrationDisableTime.Text = config.MassMigrationDisableTime;
                chkEnableMassMigration.IsChecked = config.EnableMassMigration;
                txtSpeciesMigrationTime.Text = config.SpeciesMigrationTime;
                txtMinWeatherVariationInterval.Text = config.MinWeatherVariationInterval;
                txtMaxWeatherVariationInterval.Text = config.MaxWeatherVariationInterval;
                txtQueueJoinTimeoutSeconds.Text = config.QueueJoinTimeoutSeconds;
                txtQueueHeartbeatIntervalSeconds.Text = config.QueueHeartbeatIntervalSeconds;
                txtQueueHeartbeatTimeoutSeconds.Text = config.QueueHeartbeatTimeoutSeconds;
                txtQueueHeartbeatMaxMisses.Text = config.QueueHeartbeatMaxMisses;
                chkValidateFiles.IsChecked = config.ValidateFiles;
                chkDisableStreaming.IsChecked = config.DisableStreaming;

                cmbPriority.SelectedIndex = config.ProcessPriority switch
                {
                    "Normal" => 0,
                    "AboveNormal" => 1,
                    "High" => 2,
                    _ => 0
                };

                if (!string.IsNullOrWhiteSpace(config.CpuAffinity))
                {
                    chkUseAllCores.IsChecked = false;
                    var cores = config.CpuAffinity.Split(',');
                    foreach (CheckBox cb in pnlCpuCores.Children)
                        cb.IsChecked = cores.Contains(cb.Content.ToString());
                }

                chkEnableCrashDetection.IsChecked = config.EnableCrashDetection;
                chkAutoRestart.IsChecked = config.AutoRestart;
                cmbMaxRestartAttempts.SelectedIndex = config.MaxRestartAttempts switch
                {
                    1 => 0,
                    2 => 1,
                    3 => 2,
                    5 => 3,
                    10 => 4,
                    _ => 2
                };

                chkScheduledRestartEnabled.IsChecked = config.ScheduledRestartEnabled;
                cmbRestartInterval.SelectedIndex = config.RestartIntervalHours switch
                {
                    1 => 0,
                    2 => 1,
                    3 => 2,
                    4 => 3,
                    6 => 4,
                    8 => 5,
                    12 => 6,
                    24 => 7,
                    _ => 4
                };

                cmbWarningMinutes.SelectedIndex = config.RestartWarningMinutes switch
                {
                    5 => 0,
                    10 => 1,
                    15 => 2,
                    30 => 3,
                    60 => 4,
                    _ => 2
                };

                txtRestartMessage.Text = config.RestartMessage;
                chkEnableDiscordWebhook.IsChecked = config.EnableDiscordWebhook;
                txtDiscordWebhookUrl.Text = config.DiscordWebhookUrl;
                txtDiscordInvite.Text = config.DiscordInvite;
                chkAutoBackupEnabled.IsChecked = config.AutoBackupEnabled;
                cmbBackupInterval.SelectedIndex = config.BackupIntervalHours switch
                {
                    1 => 0,
                    3 => 1,
                    6 => 2,
                    12 => 3,
                    24 => 4,
                    _ => 2
                };

                txtAdminSteamIds.Text = string.Join("\n", config.AdminSteamIds);
                txtWhitelistIds.Text = string.Join("\n", config.WhitelistIds);
                txtVipIds.Text = string.Join("\n", config.VipIds);

                // Chat Monitor
                chkEnableChatMonitor.IsChecked = config.EnableChatMonitor;
                chkEnableChatWebhook.IsChecked = config.EnableChatWebhook;
                txtChatWebhookUrl.Text = config.ChatWebhookUrl;
                cmbChatRefreshInterval.SelectedIndex = config.ChatRefreshInterval switch
                {
                    1 => 0,
                    2 => 1,
                    5 => 2,
                    10 => 3,
                    _ => 1
                };

                // Zombie Check
                chkEnableZombieCheck.IsChecked = config.EnableZombieCheck;
                txtZombieTimeout.Text = config.ZombieTimeoutSeconds.ToString();

                // Maintenance
                chkAutoWipeCorpses.IsChecked = config.AutoWipeCorpsesEnabled;
                txtWipeInterval.Text = config.WipeCorpsesIntervalMinutes.ToString();
                txtWipeDelay.Text = config.WipeCorpsesDelayMinutes.ToString();
                txtWipeWarningMessage.Text = config.WipeWarningMessage;
                txtWipeCompleteMessage.Text = config.WipeCompleteMessage;
                chkAutoRconSave.IsChecked = config.AutoRconSaveEnabled;
                txtRconSaveInterval.Text = config.RconSaveIntervalMinutes.ToString();

                // Debug Logging
                chkEnableLogRedpointEOSVerbose.IsChecked = config.EnableLogRedpointEOSVerbose;
                chkEnableLogOnlineVerbose.IsChecked = config.EnableLogOnlineVerbose;
                chkEnableLogOnlineGameVerbose.IsChecked = config.EnableLogOnlineGameVerbose;
                chkEnableLogNetVerbose.IsChecked = config.EnableLogNetVerbose;
                chkEnableLogNetTrafficVerbose.IsChecked = config.EnableLogNetTrafficVerbose;
                chkEnableLogReplicationGraphVerbose.IsChecked = config.EnableLogReplicationGraphVerbose;
                chkEnableLogTheIsleVerbose.IsChecked = config.EnableLogTheIsleVerbose;
                chkEnableLogTheIsleAdminVerbose.IsChecked = config.EnableLogTheIsleAdminVerbose;
                chkEnableLogTheIsleAIVerbose.IsChecked = config.EnableLogTheIsleAIVerbose;
                chkEnableLogTheIsleAnimInstanceVerbose.IsChecked = config.EnableLogTheIsleAnimInstanceVerbose;
                chkEnableLogTheIsleAudioVerbose.IsChecked = config.EnableLogTheIsleAudioVerbose;
                chkEnableLogTheIsleAuthVerbose.IsChecked = config.EnableLogTheIsleAuthVerbose;
                chkEnableLogTheIsleCharacterVerbose.IsChecked = config.EnableLogTheIsleCharacterVerbose;
                chkEnableLogTheIsleCharacterMovementVerbose.IsChecked = config.EnableLogTheIsleCharacterMovementVerbose;
                chkEnableLogTheIsleDatabaseVerbose.IsChecked = config.EnableLogTheIsleDatabaseVerbose;
                chkEnableLogTheIsleEnvironmentVerbose.IsChecked = config.EnableLogTheIsleEnvironmentVerbose;
                chkEnableLogTheIsleGameVerbose.IsChecked = config.EnableLogTheIsleGameVerbose;
                chkEnableLogTheIsleNetworkVerbose.IsChecked = config.EnableLogTheIsleNetworkVerbose;
                chkEnableLogTheIsleServerVerbose.IsChecked = config.EnableLogTheIsleServerVerbose;
                chkEnableLogTheIslePlayerControllerVerbose.IsChecked = config.EnableLogTheIslePlayerControllerVerbose;
                chkEnableLogTheIsleUIVerbose.IsChecked = config.EnableLogTheIsleUIVerbose;
                chkEnableLogTheIsleWorldVerbose.IsChecked = config.EnableLogTheIsleWorldVerbose;
                chkEnableLogTheIsleJoinDataVerbose.IsChecked = config.EnableLogTheIsleJoinDataVerbose;
                chkEnableLogTheIsleChatDataVerbose.IsChecked = config.EnableLogTheIsleChatDataVerbose;
                chkEnableLogTheIsleKillDataVerbose.IsChecked = config.EnableLogTheIsleKillDataVerbose;
                chkEnableLogTheIsleCommandDataVerbose.IsChecked = config.EnableLogTheIsleCommandDataVerbose;
                chkEnableLogTheIsleAntiCheatVerbose.IsChecked = config.EnableLogTheIsleAntiCheatVerbose;

                lstDinos.ItemsSource = config.Dinosaurs;
                lstDinos.Items.Refresh();
                lstDisallowedAI.ItemsSource = config.DisallowedAIClasses;
                lstDisallowedAI.Items.Refresh();

                _logger.Info("Configuration loaded into UI successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading configuration into UI: {ex.Message}", ex);
                throw;
            }
        }

        public ServerConfiguration BuildConfigurationFromUI(
            TextBox txtServerName, TextBox txtMaxPlayers, TextBox txtServerPass,
            TextBox txtRconPass, TextBox txtRconPort, CheckBox chkEnableRcon, CheckBox chkWhitelist,
            TextBox txtGamePort, TextBox txtQueuePort,
            CheckBox chkQueueEnabled, TextBox txtCustomArgs,
            TextBox txtDayLength, TextBox txtNightLength,
            CheckBox chkGlobalChat, CheckBox chkHumans, CheckBox chkMutations,
            CheckBox chkMigration, CheckBox chkFallDamage, TextBox txtGrowth,
            TextBox txtDecay, TextBox txtMigrationTime,
            CheckBox chkSpawnAI, CheckBox chkPlants, CheckBox chkDynamicWeather,
            TextBox txtAISpawn, TextBox txtAIDensity,
            TextBox txtRegionSpawnCooldown, CheckBox chkUseRegionSpawnCooldown, CheckBox chkUseRegionSpawning,
            TextBox txtPlantSpawnMultiplier, CheckBox chkAllowRecordingReplay, CheckBox chkEnableDiets, CheckBox chkEnablePatrolZones,
            TextBox txtMassMigrationTime, TextBox txtMassMigrationDisableTime, CheckBox chkEnableMassMigration,
            TextBox txtSpeciesMigrationTime, TextBox txtMinWeatherVariationInterval, TextBox txtMaxWeatherVariationInterval,
            TextBox txtQueueJoinTimeoutSeconds, TextBox txtQueueHeartbeatIntervalSeconds, TextBox txtQueueHeartbeatTimeoutSeconds, TextBox txtQueueHeartbeatMaxMisses,
            CheckBox chkValidateFiles, CheckBox chkDisableStreaming,
            ComboBox cmbPriority, CheckBox chkUseAllCores, Panel pnlCpuCores,
            CheckBox chkEnableCrashDetection, CheckBox chkAutoRestart, ComboBox cmbMaxRestartAttempts,
            CheckBox chkScheduledRestartEnabled, ComboBox cmbRestartInterval, ComboBox cmbWarningMinutes,
            TextBox txtRestartMessage,
            CheckBox chkEnableDiscordWebhook, TextBox txtDiscordWebhookUrl, TextBox txtDiscordInvite,
            CheckBox chkAutoBackupEnabled, ComboBox cmbBackupInterval,
            CheckBox chkEnableChatMonitor, CheckBox chkEnableChatWebhook, TextBox txtChatWebhookUrl,
            TextBox txtAdminSteamIds, TextBox txtWhitelistIds, TextBox txtVipIds, ListBox lstDinos, ListBox lstDisallowedAI,
            ComboBox cmbChatRefreshInterval,
            CheckBox chkEnableZombieCheck, TextBox txtZombieTimeout,
            CheckBox chkAutoWipeCorpses, TextBox txtWipeInterval, TextBox txtWipeDelay,
            TextBox txtWipeWarningMessage, TextBox txtWipeCompleteMessage,
            CheckBox chkAutoRconSave, TextBox txtRconSaveInterval,
            CheckBox chkEnableLogRedpointEOSVerbose, CheckBox chkEnableLogOnlineVerbose,
            CheckBox chkEnableLogOnlineGameVerbose, CheckBox chkEnableLogNetVerbose,
            CheckBox chkEnableLogNetTrafficVerbose, CheckBox chkEnableLogReplicationGraphVerbose,
            CheckBox chkEnableLogTheIsleVerbose, CheckBox chkEnableLogTheIsleAdminVerbose,
            CheckBox chkEnableLogTheIsleAIVerbose, CheckBox chkEnableLogTheIsleAnimInstanceVerbose,
            CheckBox chkEnableLogTheIsleAudioVerbose, CheckBox chkEnableLogTheIsleAuthVerbose,
            CheckBox chkEnableLogTheIsleCharacterVerbose, CheckBox chkEnableLogTheIsleCharacterMovementVerbose,
            CheckBox chkEnableLogTheIsleDatabaseVerbose, CheckBox chkEnableLogTheIsleEnvironmentVerbose,
            CheckBox chkEnableLogTheIsleGameVerbose, CheckBox chkEnableLogTheIsleNetworkVerbose,
            CheckBox chkEnableLogTheIsleServerVerbose, CheckBox chkEnableLogTheIslePlayerControllerVerbose,
            CheckBox chkEnableLogTheIsleUIVerbose, CheckBox chkEnableLogTheIsleWorldVerbose,
            CheckBox chkEnableLogTheIsleJoinDataVerbose, CheckBox chkEnableLogTheIsleChatDataVerbose,
            CheckBox chkEnableLogTheIsleKillDataVerbose, CheckBox chkEnableLogTheIsleCommandDataVerbose,
            CheckBox chkEnableLogTheIsleAntiCheatVerbose,
            string currentTheme) // <--- Added parameter for theme state
        {
            var config = new ServerConfiguration
            {
                ServerName = txtServerName.Text,
                MaxPlayers = txtMaxPlayers.Text,
                ServerPassword = txtServerPass.Text,
                RconPassword = txtRconPass.Text,
                RconPort = txtRconPort.Text,
                RconEnabled = chkEnableRcon.IsChecked.GetValueOrDefault(false),
                Whitelist = chkWhitelist.IsChecked.GetValueOrDefault(),
                GamePort = txtGamePort.Text,
                QueuePort = txtQueuePort.Text,
                QueueEnabled = chkQueueEnabled.IsChecked.GetValueOrDefault(),
                CustomArgs = txtCustomArgs.Text ?? "",
                DayLength = txtDayLength.Text,
                NightLength = txtNightLength.Text,
                GlobalChat = chkGlobalChat.IsChecked.GetValueOrDefault(),
                Humans = chkHumans.IsChecked.GetValueOrDefault(),
                Mutations = chkMutations.IsChecked.GetValueOrDefault(),
                Migration = chkMigration.IsChecked.GetValueOrDefault(),
                FallDamage = chkFallDamage.IsChecked.GetValueOrDefault(),
                GrowthMultiplier = txtGrowth.Text,
                CorpseDecay = txtDecay.Text,
                MigrationTime = txtMigrationTime.Text,
                SpawnAI = chkSpawnAI.IsChecked.GetValueOrDefault(),
                SpawnPlants = chkPlants.IsChecked.GetValueOrDefault(),
                DynamicWeather = chkDynamicWeather.IsChecked.GetValueOrDefault(),
                AISpawnInterval = txtAISpawn.Text,
                AIDensity = txtAIDensity.Text,
                RegionSpawnCooldownTimeSeconds = txtRegionSpawnCooldown.Text,
                UseRegionSpawnCooldown = chkUseRegionSpawnCooldown.IsChecked.GetValueOrDefault(),
                UseRegionSpawning = chkUseRegionSpawning.IsChecked.GetValueOrDefault(),
                PlantSpawnMultiplier = txtPlantSpawnMultiplier.Text,
                AllowRecordingReplay = chkAllowRecordingReplay.IsChecked.GetValueOrDefault(),
                EnableDiets = chkEnableDiets.IsChecked.GetValueOrDefault(),
                EnablePatrolZones = chkEnablePatrolZones.IsChecked.GetValueOrDefault(),
                MassMigrationTime = txtMassMigrationTime.Text,
                MassMigrationDisableTime = txtMassMigrationDisableTime.Text,
                EnableMassMigration = chkEnableMassMigration.IsChecked.GetValueOrDefault(),
                SpeciesMigrationTime = txtSpeciesMigrationTime.Text,
                MinWeatherVariationInterval = txtMinWeatherVariationInterval.Text,
                MaxWeatherVariationInterval = txtMaxWeatherVariationInterval.Text,
                QueueJoinTimeoutSeconds = txtQueueJoinTimeoutSeconds.Text,
                QueueHeartbeatIntervalSeconds = txtQueueHeartbeatIntervalSeconds.Text,
                QueueHeartbeatTimeoutSeconds = txtQueueHeartbeatTimeoutSeconds.Text,
                QueueHeartbeatMaxMisses = txtQueueHeartbeatMaxMisses.Text,
                ValidateFiles = chkValidateFiles.IsChecked.GetValueOrDefault(),
                DisableStreaming = chkDisableStreaming.IsChecked.GetValueOrDefault(),

                ProcessPriority = cmbPriority.SelectedIndex switch
                {
                    0 => "Normal",
                    1 => "AboveNormal",
                    2 => "High",
                    _ => "Normal"
                },

                CpuAffinity = chkUseAllCores.IsChecked == true ? "" : GetSelectedCpuCores(pnlCpuCores),

                EnableCrashDetection = chkEnableCrashDetection.IsChecked.GetValueOrDefault(true),
                AutoRestart = chkAutoRestart.IsChecked.GetValueOrDefault(),

                MaxRestartAttempts = cmbMaxRestartAttempts.SelectedIndex switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    3 => 5,
                    4 => 10,
                    _ => 3
                },

                ScheduledRestartEnabled = chkScheduledRestartEnabled.IsChecked.GetValueOrDefault(),

                RestartIntervalHours = cmbRestartInterval.SelectedIndex switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    4 => 6,
                    5 => 8,
                    6 => 12,
                    7 => 24,
                    _ => 6
                },

                RestartWarningMinutes = cmbWarningMinutes.SelectedIndex switch
                {
                    0 => 5,
                    1 => 10,
                    2 => 15,
                    3 => 30,
                    4 => 60,
                    _ => 15
                },

                RestartMessage = txtRestartMessage.Text ?? "Server will restart in {minutes} minute(s)!",
                EnableDiscordWebhook = chkEnableDiscordWebhook.IsChecked.GetValueOrDefault(),
                DiscordWebhookUrl = txtDiscordWebhookUrl.Text ?? "",
                DiscordInvite = txtDiscordInvite.Text ?? "",
                AdminSteamIds = txtAdminSteamIds.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(id => id.Trim()).ToList(),
                WhitelistIds = txtWhitelistIds.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(id => id.Trim()).ToList(),
                VipIds = txtVipIds.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(id => id.Trim()).ToList(),
                AutoBackupEnabled = chkAutoBackupEnabled.IsChecked.GetValueOrDefault(),

                BackupIntervalHours = cmbBackupInterval.SelectedIndex switch
                {
                    0 => 1,
                    1 => 3,
                    2 => 6,
                    3 => 12,
                    4 => 24,
                    _ => 6
                },

                EnableChatMonitor = chkEnableChatMonitor.IsChecked.GetValueOrDefault(),
                EnableChatWebhook = chkEnableChatWebhook.IsChecked.GetValueOrDefault(),
                ChatWebhookUrl = txtChatWebhookUrl.Text ?? "",

                ChatRefreshInterval = cmbChatRefreshInterval.SelectedIndex switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 5,
                    3 => 10,
                    _ => 2
                },

                EnableZombieCheck = chkEnableZombieCheck.IsChecked.GetValueOrDefault(),
                ZombieTimeoutSeconds = int.TryParse(txtZombieTimeout.Text, out int zt) && zt >= 30 && zt <= 300 ? zt : 60,

                // Maintenance
                AutoWipeCorpsesEnabled = chkAutoWipeCorpses.IsChecked.GetValueOrDefault(),
                WipeCorpsesIntervalMinutes = int.TryParse(txtWipeInterval.Text, out int wipeInt) ? wipeInt : 60,
                WipeCorpsesDelayMinutes = int.TryParse(txtWipeDelay.Text, out int wipeDelay) ? wipeDelay : 0,
                WipeWarningMessage = txtWipeWarningMessage.Text ?? "Warning: All Corpses will be wiped in {minutes} minute(s)!",
                WipeCompleteMessage = txtWipeCompleteMessage.Text ?? "All Corpses have been wiped.",
                AutoRconSaveEnabled = chkAutoRconSave.IsChecked.GetValueOrDefault(),
                RconSaveIntervalMinutes = int.TryParse(txtRconSaveInterval.Text, out int saveInt) ? saveInt : 30,

                // Debug Logging
                EnableLogRedpointEOSVerbose = chkEnableLogRedpointEOSVerbose.IsChecked.GetValueOrDefault(),
                EnableLogOnlineVerbose = chkEnableLogOnlineVerbose.IsChecked.GetValueOrDefault(),
                EnableLogOnlineGameVerbose = chkEnableLogOnlineGameVerbose.IsChecked.GetValueOrDefault(),
                EnableLogNetVerbose = chkEnableLogNetVerbose.IsChecked.GetValueOrDefault(),
                EnableLogNetTrafficVerbose = chkEnableLogNetTrafficVerbose.IsChecked.GetValueOrDefault(),
                EnableLogReplicationGraphVerbose = chkEnableLogReplicationGraphVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleVerbose = chkEnableLogTheIsleVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAdminVerbose = chkEnableLogTheIsleAdminVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAIVerbose = chkEnableLogTheIsleAIVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAnimInstanceVerbose = chkEnableLogTheIsleAnimInstanceVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAudioVerbose = chkEnableLogTheIsleAudioVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAuthVerbose = chkEnableLogTheIsleAuthVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleCharacterVerbose = chkEnableLogTheIsleCharacterVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleCharacterMovementVerbose = chkEnableLogTheIsleCharacterMovementVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleDatabaseVerbose = chkEnableLogTheIsleDatabaseVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleEnvironmentVerbose = chkEnableLogTheIsleEnvironmentVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleGameVerbose = chkEnableLogTheIsleGameVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleNetworkVerbose = chkEnableLogTheIsleNetworkVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleServerVerbose = chkEnableLogTheIsleServerVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIslePlayerControllerVerbose = chkEnableLogTheIslePlayerControllerVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleUIVerbose = chkEnableLogTheIsleUIVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleWorldVerbose = chkEnableLogTheIsleWorldVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleJoinDataVerbose = chkEnableLogTheIsleJoinDataVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleChatDataVerbose = chkEnableLogTheIsleChatDataVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleKillDataVerbose = chkEnableLogTheIsleKillDataVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleCommandDataVerbose = chkEnableLogTheIsleCommandDataVerbose.IsChecked.GetValueOrDefault(),
                EnableLogTheIsleAntiCheatVerbose = chkEnableLogTheIsleAntiCheatVerbose.IsChecked.GetValueOrDefault(),

                // Explicitly set the theme from the passed parameter
                Theme = currentTheme,

                Dinosaurs = lstDinos.ItemsSource as List<DinoOption> ?? new List<DinoOption>(),
                DisallowedAIClasses = lstDisallowedAI.ItemsSource as List<AiOption> ?? new List<AiOption>()
            };

            return config;
        }


        private string GetSelectedCpuCores(Panel pnlCpuCores)
        {
            var selectedCores = new List<string>();
            foreach (CheckBox cb in pnlCpuCores.Children)
            {
                if (cb.IsChecked == true)
                    selectedCores.Add(cb.Content.ToString() ?? "");
            }
            return string.Join(",", selectedCores);
        }
    }
}
