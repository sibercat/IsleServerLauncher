using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IsleServerLauncher.Services
{
    public class DinoOption
    {
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
    }

    public class AiOption
    {
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; } = false;
    }

    public class ServerConfiguration
    {
        // Identity
        public string ServerName { get; set; } = "My Amazing Server";
        public string MaxPlayers { get; set; } = "100";
        public string ServerPassword { get; set; } = "";

        // Security & RCON
        public string RconPassword { get; set; } = "ChangeMe123";
        public string RconPort { get; set; } = "8888";
        public bool RconEnabled { get; set; } = false;
        public bool Whitelist { get; set; } = false;

        // Network
        public string GamePort { get; set; } = "7777";
        public string QueuePort { get; set; } = "10000";
        public bool QueueEnabled { get; set; } = false;
        public string CustomArgs { get; set; } = "";

        // Time
        public string DayLength { get; set; } = "45";
        public string NightLength { get; set; } = "20";

        // Gameplay
        public bool GlobalChat { get; set; } = false;
        public bool Humans { get; set; } = false;
        public bool Mutations { get; set; } = false;
        public bool Migration { get; set; } = false;
        public bool FallDamage { get; set; } = false;
        public string GrowthMultiplier { get; set; } = "1";
        public string CorpseDecay { get; set; } = "1";
        public string MigrationTime { get; set; } = "5400";

        // AI & Environment
        public bool SpawnAI { get; set; } = false;
        public bool SpawnPlants { get; set; } = false;
        public bool DynamicWeather { get; set; } = false;
        public string AISpawnInterval { get; set; } = "40";
        public string AIDensity { get; set; } = "1";
        public List<AiOption> DisallowedAIClasses { get; set; } = new List<AiOption>();
        public string RegionSpawnCooldownTimeSeconds { get; set; } = "30";
        public bool UseRegionSpawnCooldown { get; set; } = false;
        public bool UseRegionSpawning { get; set; } = false;
        public string PlantSpawnMultiplier { get; set; } = "1";
        public bool AllowRecordingReplay { get; set; } = true;
        public bool EnableDiets { get; set; } = true;
        public bool EnablePatrolZones { get; set; } = true;
        public string MassMigrationTime { get; set; } = "43200";
        public string MassMigrationDisableTime { get; set; } = "7200";
        public bool EnableMassMigration { get; set; } = false;
        public string SpeciesMigrationTime { get; set; } = "10800";
        public string MinWeatherVariationInterval { get; set; } = "600";
        public string MaxWeatherVariationInterval { get; set; } = "900";
        public string QueueJoinTimeoutSeconds { get; set; } = "30";
        public string QueueHeartbeatIntervalSeconds { get; set; } = "8";
        public string QueueHeartbeatTimeoutSeconds { get; set; } = "5";
        public string QueueHeartbeatMaxMisses { get; set; } = "2";

        // Performance
        public bool ValidateFiles { get; set; } = false;
        public bool DisableStreaming { get; set; } = false;
        public string ProcessPriority { get; set; } = "Normal";
        public string CpuAffinity { get; set; } = "";

        // Crash Detection
        public bool EnableCrashDetection { get; set; } = true;
        public bool AutoRestart { get; set; } = false;
        public int MaxRestartAttempts { get; set; } = 3;

        // Scheduled Restarts
        public bool ScheduledRestartEnabled { get; set; } = false;
        public int RestartIntervalHours { get; set; } = 6;
        public int RestartWarningMinutes { get; set; } = 15;
        public string RestartMessage { get; set; } = "Server will restart in {minutes} minute(s)!";

        // Discord Webhook
        public bool EnableDiscordWebhook { get; set; } = false;
        public string DiscordWebhookUrl { get; set; } = "";
        public string DiscordInvite { get; set; } = "";

        // Automatic Backups
        public bool AutoBackupEnabled { get; set; } = false;
        public int BackupIntervalHours { get; set; } = 6;

        // Maintenance
        public bool AutoWipeCorpsesEnabled { get; set; } = false;
        public int WipeCorpsesIntervalMinutes { get; set; } = 60;
        public int WipeCorpsesDelayMinutes { get; set; } = 0;
        public string WipeWarningMessage { get; set; } = "Warning: All Corpses will be wiped in {minutes} minute(s)!";
        public string WipeCompleteMessage { get; set; } = "All Corpses have been wiped.";
        public bool AutoRconSaveEnabled { get; set; } = false;
        public int RconSaveIntervalMinutes { get; set; } = 30;

        // Chat Monitor
        public bool EnableChatMonitor { get; set; } = false;
        public bool EnableChatWebhook { get; set; } = false;
        public string ChatWebhookUrl { get; set; } = "";
        public int ChatRefreshInterval { get; set; } = 2;

        // Zombie Check
        public bool EnableZombieCheck { get; set; } = false;
        public int ZombieTimeoutSeconds { get; set; } = 60;

        // Debug Logging
        public bool EnableLogRedpointEOSVerbose { get; set; } = false;
        public bool EnableLogOnlineVerbose { get; set; } = false;
        public bool EnableLogOnlineGameVerbose { get; set; } = false;
        public bool EnableLogNetVerbose { get; set; } = false;
        public bool EnableLogNetTrafficVerbose { get; set; } = false;
        public bool EnableLogReplicationGraphVerbose { get; set; } = false;
        public bool EnableLogTheIsleVerbose { get; set; } = false;
        public bool EnableLogTheIsleAdminVerbose { get; set; } = false;
        public bool EnableLogTheIsleAIVerbose { get; set; } = false;
        public bool EnableLogTheIsleAnimInstanceVerbose { get; set; } = false;
        public bool EnableLogTheIsleAudioVerbose { get; set; } = false;
        public bool EnableLogTheIsleAuthVerbose { get; set; } = false;
        public bool EnableLogTheIsleCharacterVerbose { get; set; } = false;
        public bool EnableLogTheIsleCharacterMovementVerbose { get; set; } = false;
        public bool EnableLogTheIsleDatabaseVerbose { get; set; } = false;
        public bool EnableLogTheIsleEnvironmentVerbose { get; set; } = false;
        public bool EnableLogTheIsleGameVerbose { get; set; } = false;
        public bool EnableLogTheIsleNetworkVerbose { get; set; } = false;
        public bool EnableLogTheIsleServerVerbose { get; set; } = false;
        public bool EnableLogTheIslePlayerControllerVerbose { get; set; } = false;
        public bool EnableLogTheIsleUIVerbose { get; set; } = false;
        public bool EnableLogTheIsleWorldVerbose { get; set; } = false;
        public bool EnableLogTheIsleJoinDataVerbose { get; set; } = false;
        public bool EnableLogTheIsleChatDataVerbose { get; set; } = false;
        public bool EnableLogTheIsleKillDataVerbose { get; set; } = false;
        public bool EnableLogTheIsleCommandDataVerbose { get; set; } = false;
        public bool EnableLogTheIsleAntiCheatVerbose { get; set; } = false;

        // Application Settings
        public string Theme { get; set; } = "Light";

        // Lists
        public List<string> AdminSteamIds { get; set; } = new List<string>();
        public List<string> WhitelistIds { get; set; } = new List<string>();
        public List<string> VipIds { get; set; } = new List<string>();
        public List<DinoOption> Dinosaurs { get; set; } = new List<DinoOption>();
    }

    public class ConfigurationManager
    {
        private readonly string _serverFolder;
        private readonly string _configPath;
        private readonly string _engineConfigPath;
        private readonly string _settingsPath;
        private readonly ILogger _logger;

        public ConfigurationManager(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _configPath = Path.Combine(serverFolder, @"TheIsle\Saved\Config\WindowsServer\Game.ini");
            _engineConfigPath = Path.Combine(serverFolder, @"TheIsle\Saved\Config\WindowsServer\Engine.ini");
            _settingsPath = Path.Combine(serverFolder, "launcher_settings.ini");

            _logger.Info($"ConfigurationManager initialized. Config path: {_configPath}");
        }

        /// <summary>
        /// Loads configuration from all config files
        /// </summary>
        public ServerConfiguration LoadConfiguration(List<string> allDinos, List<string> allAi)
        {
            _logger.Info("Loading configuration");
            var config = new ServerConfiguration();

            // Initialize dinosaur list
            config.Dinosaurs = allDinos.Select(d => new DinoOption { Name = d, IsEnabled = false }).ToList();
            config.DisallowedAIClasses = allAi.Select(ai => new AiOption { Name = ai, IsEnabled = false }).ToList();

            // Load Game.ini
            if (!File.Exists(_configPath))
            {
                _logger.Warning($"Game.ini not found at {_configPath}, creating default");
                CreateDefaultGameConfig();
            }

            try
            {
                string content = File.ReadAllText(_configPath);
                _logger.Debug("Game.ini loaded successfully");

                // Identity
                config.ServerName = GetConfigValue(content, "ServerName") ?? "My Amazing Server";
                config.MaxPlayers = GetConfigValue(content, "MaxPlayerCount") ?? "100";
                config.ServerPassword = GetConfigValue(content, "ServerPassword")?.Replace("\"", "") ?? "";

                // Security
                config.RconPassword = GetConfigValue(content, "RconPassword")?.Replace("\"", "") ?? "ChangeMe123";
                config.RconPort = GetConfigValue(content, "RconPort") ?? "8888";
                config.RconEnabled = GetBoolValue(content, "bRconEnabled", defaultValue: false);
                config.Whitelist = GetBoolValue(content, "bServerWhitelist");

                // Network
                config.QueuePort = GetConfigValue(content, "QueuePort") ?? "10000";
                config.QueueEnabled = GetBoolValue(content, "bQueueEnabled");

                // Time
                config.DayLength = GetConfigValue(content, "ServerDayLengthMinutes") ?? "45";
                config.NightLength = GetConfigValue(content, "ServerNightLengthMinutes") ?? "20";

                // Gameplay
                config.GlobalChat = GetBoolValue(content, "bEnableGlobalChat") || GetBoolValue(content, "bServerGlobalChat");
                config.Humans = GetBoolValue(content, "bEnableHumans");
                config.Mutations = GetBoolValue(content, "bEnableMutations");
                config.Migration = GetBoolValue(content, "bEnableMigration");
                config.FallDamage = GetBoolValue(content, "bServerFallDamage");
                config.GrowthMultiplier = GetConfigValue(content, "GrowthMultiplier") ?? "1";
                config.CorpseDecay = GetConfigValue(content, "CorpseDecayMultiplier") ?? "1";
                config.MigrationTime = GetConfigValue(content, "MaxMigrationTime") ?? "5400";

                // AI & Environment
                config.SpawnAI = GetBoolValue(content, "bSpawnAI");
                config.SpawnPlants = GetBoolValue(content, "bSpawnPlants");
                config.DynamicWeather = GetBoolValue(content, "bServerDynamicWeather");
                config.AISpawnInterval = GetConfigValue(content, "AISpawnInterval") ?? "40";
                config.AIDensity = GetConfigValue(content, "AIDensity") ?? "1";
                config.DiscordInvite = GetConfigValue(content, "Discord") ?? "";
                config.RegionSpawnCooldownTimeSeconds = GetConfigValue(content, "RegionSpawnCooldownTimeSeconds") ?? "30";
                config.UseRegionSpawnCooldown = GetBoolValue(content, "bUseRegionSpawnCooldown");
                config.UseRegionSpawning = GetBoolValue(content, "bUseRegionSpawning");
                config.PlantSpawnMultiplier = GetConfigValue(content, "PlantSpawnMultiplier") ?? "1";
                config.AllowRecordingReplay = GetBoolValue(content, "bAllowRecordingReplay", defaultValue: true);
                config.EnableDiets = GetBoolValue(content, "bEnableDiets", defaultValue: true);
                config.EnablePatrolZones = GetBoolValue(content, "bEnablePatrolZones", defaultValue: true);
                config.MassMigrationTime = GetConfigValue(content, "MassMigrationTime") ?? "43200";
                config.MassMigrationDisableTime = GetConfigValue(content, "MassMigrationDisableTime") ?? "7200";
                config.EnableMassMigration = GetBoolValue(content, "bEnableMassMigration");
                config.SpeciesMigrationTime = GetConfigValue(content, "SpeciesMigrationTime") ?? "10800";
                config.MinWeatherVariationInterval = GetConfigValue(content, "MinWeatherVariationInterval") ?? "600";
                config.MaxWeatherVariationInterval = GetConfigValue(content, "MaxWeatherVariationInterval") ?? "900";
                config.QueueJoinTimeoutSeconds = GetConfigValue(content, "QueueJoinTimeoutSeconds") ?? "30";
                config.QueueHeartbeatIntervalSeconds = GetConfigValue(content, "QueueHeartbeatIntervalSeconds") ?? "8";
                config.QueueHeartbeatTimeoutSeconds = GetConfigValue(content, "QueueHeartbeatTimeoutSeconds") ?? "5";
                config.QueueHeartbeatMaxMisses = GetConfigValue(content, "QueueHeartbeatMaxMisses") ?? "2";

                var disallowedAiRaw = GetConfigValue(content, "DisallowedAIClasses");
                if (!string.IsNullOrWhiteSpace(disallowedAiRaw))
                {
                    var disallowed = disallowedAiRaw
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => item.Trim())
                        .Where(item => item.Length > 0);

                    foreach (var name in disallowed)
                    {
                        var option = config.DisallowedAIClasses.FirstOrDefault(ai =>
                            ai.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (option != null) option.IsEnabled = true;
                    }
                }
                
                // Admin IDs
                var adminMatches = Regex.Matches(content, @"AdminsSteamIDs=(\d+)");
                foreach (Match m in adminMatches)
                {
                    config.AdminSteamIds.Add(m.Groups[1].Value);
                }
                _logger.Debug($"Loaded {config.AdminSteamIds.Count} admin Steam IDs");

                // Whitelist IDs
                var whitelistMatches = Regex.Matches(content, @"WhitelistIDs=(\d+)");
                foreach (Match m in whitelistMatches)
                {
                    config.WhitelistIds.Add(m.Groups[1].Value);
                }
                _logger.Debug($"Loaded {config.WhitelistIds.Count} whitelist IDs");

                // VIP IDs
                var vipMatches = Regex.Matches(content, @"VIPs=(\d+)");
                foreach (Match m in vipMatches)
                {
                    config.VipIds.Add(m.Groups[1].Value);
                }
                _logger.Debug($"Loaded {config.VipIds.Count} VIP IDs");

                // Load dinosaurs from launcher settings first (priority)
                bool dinosLoadedFromSettings = false;
                if (File.Exists(_settingsPath))
                {
                    string settings = File.ReadAllText(_settingsPath);
                    _logger.Debug("Launcher settings loaded");

                    config.ValidateFiles = GetBoolValue(settings, "ValidateFiles");
                    config.DisableStreaming = GetBoolValue(settings, "DisableStreaming");
                    config.ProcessPriority = GetConfigValue(settings, "ProcessPriority") ?? "Normal";
                    config.CpuAffinity = GetConfigValue(settings, "CpuAffinity") ?? "";

                    // Theme
                    config.Theme = GetConfigValue(settings, "Theme") ?? "Light";

                    // Crash Detection
                    config.EnableCrashDetection = GetBoolValue(settings, "EnableCrashDetection", defaultValue: true);
                    config.AutoRestart = GetBoolValue(settings, "AutoRestart");
                    config.MaxRestartAttempts = int.TryParse(GetConfigValue(settings, "MaxRestartAttempts") ?? "3", out int maxAttempts) ? maxAttempts : 3;

                    // Scheduled Restarts
                    config.ScheduledRestartEnabled = GetBoolValue(settings, "ScheduledRestartEnabled");
                    config.RestartIntervalHours = int.TryParse(GetConfigValue(settings, "RestartIntervalHours") ?? "6", out int intervalHours) ? intervalHours : 6;
                    config.RestartWarningMinutes = int.TryParse(GetConfigValue(settings, "RestartWarningMinutes") ?? "15", out int warningMinutes) ? warningMinutes : 15;
                    config.RestartMessage = GetConfigValue(settings, "RestartMessage") ?? "Server will restart in {minutes} minute(s)!";

                    // Discord Webhook
                    config.EnableDiscordWebhook = GetBoolValue(settings, "EnableDiscordWebhook");
                    config.DiscordWebhookUrl = GetConfigValue(settings, "DiscordWebhookUrl") ?? "";

                    // Automatic Backups
                    config.AutoBackupEnabled = GetBoolValue(settings, "AutoBackupEnabled");
                    config.BackupIntervalHours = int.TryParse(GetConfigValue(settings, "BackupIntervalHours") ?? "6", out int backupInterval) ? backupInterval : 6;

                    // Maintenance
                    config.AutoWipeCorpsesEnabled = GetBoolValue(settings, "AutoWipeCorpsesEnabled");
                    config.WipeCorpsesIntervalMinutes = int.TryParse(GetConfigValue(settings, "WipeCorpsesIntervalMinutes") ?? "60", out int wipeInt) ? wipeInt : 60;
                    config.WipeCorpsesDelayMinutes = int.TryParse(GetConfigValue(settings, "WipeCorpsesDelayMinutes") ?? "0", out int wipeDelay) ? wipeDelay : 0;
                    config.WipeWarningMessage = GetConfigValue(settings, "WipeWarningMessage") ?? "Warning: All Corpses will be wiped in {minutes} minute(s)!";
                    config.WipeCompleteMessage = GetConfigValue(settings, "WipeCompleteMessage") ?? "All Corpses have been wiped.";
                    config.AutoRconSaveEnabled = GetBoolValue(settings, "AutoRconSaveEnabled");
                    config.RconSaveIntervalMinutes = int.TryParse(GetConfigValue(settings, "RconSaveIntervalMinutes") ?? "30", out int saveInt) ? saveInt : 30;

                    // Chat Monitor
                    config.EnableChatMonitor = GetBoolValue(settings, "EnableChatMonitor");
                    config.EnableChatWebhook = GetBoolValue(settings, "EnableChatWebhook");
                    config.ChatWebhookUrl = GetConfigValue(settings, "ChatWebhookUrl") ?? "";
                    config.ChatRefreshInterval = int.TryParse(GetConfigValue(settings, "ChatRefreshInterval") ?? "2", out int chatInterval) ? chatInterval : 2;

                    // Zombie Check
                    config.EnableZombieCheck = GetBoolValue(settings, "EnableZombieCheck");
                    config.ZombieTimeoutSeconds = int.TryParse(GetConfigValue(settings, "ZombieTimeoutSeconds") ?? "60", out int zombieTimeout) ? zombieTimeout : 60;

                    // Debug Logging
                    config.EnableLogRedpointEOSVerbose = GetBoolValue(settings, "EnableLogRedpointEOSVerbose");
                    config.EnableLogOnlineVerbose = GetBoolValue(settings, "EnableLogOnlineVerbose");
                    config.EnableLogOnlineGameVerbose = GetBoolValue(settings, "EnableLogOnlineGameVerbose");
                    config.EnableLogNetVerbose = GetBoolValue(settings, "EnableLogNetVerbose");
                    config.EnableLogNetTrafficVerbose = GetBoolValue(settings, "EnableLogNetTrafficVerbose");
                    config.EnableLogReplicationGraphVerbose = GetBoolValue(settings, "EnableLogReplicationGraphVerbose");
                    config.EnableLogTheIsleVerbose = GetBoolValue(settings, "EnableLogTheIsleVerbose");
                    config.EnableLogTheIsleAdminVerbose = GetBoolValue(settings, "EnableLogTheIsleAdminVerbose");
                    config.EnableLogTheIsleAIVerbose = GetBoolValue(settings, "EnableLogTheIsleAIVerbose");
                    config.EnableLogTheIsleAnimInstanceVerbose = GetBoolValue(settings, "EnableLogTheIsleAnimInstanceVerbose");
                    config.EnableLogTheIsleAudioVerbose = GetBoolValue(settings, "EnableLogTheIsleAudioVerbose");
                    config.EnableLogTheIsleAuthVerbose = GetBoolValue(settings, "EnableLogTheIsleAuthVerbose");
                    config.EnableLogTheIsleCharacterVerbose = GetBoolValue(settings, "EnableLogTheIsleCharacterVerbose");
                    config.EnableLogTheIsleCharacterMovementVerbose = GetBoolValue(settings, "EnableLogTheIsleCharacterMovementVerbose");
                    config.EnableLogTheIsleDatabaseVerbose = GetBoolValue(settings, "EnableLogTheIsleDatabaseVerbose");
                    config.EnableLogTheIsleEnvironmentVerbose = GetBoolValue(settings, "EnableLogTheIsleEnvironmentVerbose");
                    config.EnableLogTheIsleGameVerbose = GetBoolValue(settings, "EnableLogTheIsleGameVerbose");
                    config.EnableLogTheIsleNetworkVerbose = GetBoolValue(settings, "EnableLogTheIsleNetworkVerbose");
                    config.EnableLogTheIsleServerVerbose = GetBoolValue(settings, "EnableLogTheIsleServerVerbose");
                    config.EnableLogTheIslePlayerControllerVerbose = GetBoolValue(settings, "EnableLogTheIslePlayerControllerVerbose");
                    config.EnableLogTheIsleUIVerbose = GetBoolValue(settings, "EnableLogTheIsleUIVerbose");
                    config.EnableLogTheIsleWorldVerbose = GetBoolValue(settings, "EnableLogTheIsleWorldVerbose");
                    config.EnableLogTheIsleJoinDataVerbose = GetBoolValue(settings, "EnableLogTheIsleJoinDataVerbose");
                    config.EnableLogTheIsleChatDataVerbose = GetBoolValue(settings, "EnableLogTheIsleChatDataVerbose");
                    config.EnableLogTheIsleKillDataVerbose = GetBoolValue(settings, "EnableLogTheIsleKillDataVerbose");
                    config.EnableLogTheIsleCommandDataVerbose = GetBoolValue(settings, "EnableLogTheIsleCommandDataVerbose");
                    config.EnableLogTheIsleAntiCheatVerbose = GetBoolValue(settings, "EnableLogTheIsleAntiCheatVerbose");

                    string customArgs = GetConfigValue(settings, "CustomArgs") ?? "";
                    config.CustomArgs = customArgs.StartsWith("Example:") ? "" : customArgs;

                    string? enabledDinos = GetConfigValue(settings, "EnabledDinos");
                    if (enabledDinos != null)
                    {
                        dinosLoadedFromSettings = true;
                        if (!string.IsNullOrWhiteSpace(enabledDinos))
                        {
                            var enabled = enabledDinos.Split(',');
                            foreach (var dino in config.Dinosaurs)
                            {
                                dino.IsEnabled = enabled.Contains(dino.Name);
                            }
                            _logger.Debug($"Loaded {enabled.Length} enabled dinosaurs from settings");
                        }
                        else
                        {
                            foreach (var dino in config.Dinosaurs)
                            {
                                dino.IsEnabled = false;
                            }
                            _logger.Debug("No dinosaurs enabled (from settings)");
                        }
                    }
                }

                // Load dinos from Game.ini only if not loaded from settings
                if (!dinosLoadedFromSettings)
                {
                    if (content.Contains("AllowedClasses="))
                    {
                        foreach (var dino in config.Dinosaurs) dino.IsEnabled = false;
                        var matches = Regex.Matches(content, @"AllowedClasses=([a-zA-Z]+)");
                        foreach (Match m in matches)
                        {
                            var item = config.Dinosaurs.FirstOrDefault(d =>
                                d.Name.Equals(m.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                            if (item != null) item.IsEnabled = true;
                        }
                        _logger.Debug($"Loaded {matches.Count} enabled dinosaurs from Game.ini");
                    }
                    else
                    {
                        // No saved data - enable all by default
                        foreach (var dino in config.Dinosaurs)
                        {
                            dino.IsEnabled = true;
                        }
                        _logger.Debug("All dinosaurs enabled by default");
                    }
                }

                _logger.Info("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading configuration: {ex.Message}", ex);
                // Return default configuration on error
                config = new ServerConfiguration();
                config.Dinosaurs = allDinos.Select(d => new DinoOption { Name = d, IsEnabled = true }).ToList();
                config.DisallowedAIClasses = allAi.Select(ai => new AiOption { Name = ai, IsEnabled = false }).ToList();
            }

            return config;
        }

        /// <summary>
        /// Saves configuration to all config files
        /// </summary>
        public void SaveConfiguration(ServerConfiguration config)
        {
            _logger.Info("Saving configuration");

            try
            {
                // Save Game.ini
                UpdateGameConfig(config);

                // Save Engine.ini
                UpdateEngineConfig(config);

                // Save launcher settings
                SaveLauncherSettings(config);

                _logger.Info("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving configuration: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Updates Game.ini with safe editing
        /// </summary>
        private void UpdateGameConfig(ServerConfiguration config)
        {
            try
            {
                _logger.Debug("Updating Game.ini");

                string? dir = Path.GetDirectoryName(_configPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    _logger.Debug($"Created directory: {dir}");
                }

                List<string> lines = File.Exists(_configPath)
                    ? File.ReadAllLines(_configPath).ToList()
                    : new List<string>();

                string section = "/Script/TheIsle.TIGameSession";

                // Identity
                UpdateIniValue(lines, section, "ServerName", config.ServerName);
                UpdateIniValue(lines, section, "MaxPlayerCount", config.MaxPlayers);
                UpdateIniValue(lines, section, "MapName", "Gateway");

                // Security
                bool hasPass = !string.IsNullOrWhiteSpace(config.ServerPassword);
                UpdateIniValue(lines, section, "bServerPassword", hasPass.ToString().ToLower());
                if (hasPass) UpdateIniValue(lines, section, "ServerPassword", config.ServerPassword);

                UpdateIniValue(lines, section, "bRconEnabled", config.RconEnabled.ToString().ToLower());
                UpdateIniValue(lines, section, "RconPassword", config.RconPassword);
                UpdateIniValue(lines, section, "RconPort", config.RconPort);
                UpdateIniValue(lines, section, "bServerWhitelist", config.Whitelist.ToString().ToLower());

                // Queue
                UpdateIniValue(lines, section, "bQueueEnabled", config.QueueEnabled.ToString().ToLower());
                UpdateIniValue(lines, section, "QueuePort", config.QueuePort);

                // Gameplay
                UpdateIniValue(lines, section, "bEnableHumans", config.Humans.ToString().ToLower());
                UpdateIniValue(lines, section, "bEnableMutations", config.Mutations.ToString().ToLower());
                UpdateIniValue(lines, section, "bEnableGlobalChat", config.GlobalChat.ToString().ToLower());
                UpdateIniValue(lines, section, "bEnableMigration", config.Migration.ToString().ToLower());
                UpdateIniValue(lines, section, "bServerFallDamage", config.FallDamage.ToString().ToLower());
                UpdateIniValue(lines, section, "GrowthMultiplier", config.GrowthMultiplier);
                UpdateIniValue(lines, section, "CorpseDecayMultiplier", config.CorpseDecay);
                UpdateIniValue(lines, section, "ServerDayLengthMinutes", config.DayLength);
                UpdateIniValue(lines, section, "ServerNightLengthMinutes", config.NightLength);
                UpdateIniValue(lines, section, "MaxMigrationTime", config.MigrationTime);

                // AI & Environment
                UpdateIniValue(lines, section, "bSpawnAI", config.SpawnAI.ToString().ToLower());
                UpdateIniValue(lines, section, "bSpawnPlants", config.SpawnPlants.ToString().ToLower());
                UpdateIniValue(lines, section, "bServerDynamicWeather", config.DynamicWeather.ToString().ToLower());
                UpdateIniValue(lines, section, "AISpawnInterval", config.AISpawnInterval);
                UpdateIniValue(lines, section, "AIDensity", config.AIDensity);
                UpdateIniValue(lines, section, "Discord", string.IsNullOrWhiteSpace(config.DiscordInvite) ? null : config.DiscordInvite);
                UpdateIniValue(lines, section, "RegionSpawnCooldownTimeSeconds", config.RegionSpawnCooldownTimeSeconds);
                UpdateIniValue(lines, section, "bUseRegionSpawnCooldown", config.UseRegionSpawnCooldown.ToString().ToLower());
                UpdateIniValue(lines, section, "bUseRegionSpawning", config.UseRegionSpawning.ToString().ToLower());
                UpdateIniValue(lines, section, "PlantSpawnMultiplier", config.PlantSpawnMultiplier);
                UpdateIniValue(lines, section, "bAllowRecordingReplay", config.AllowRecordingReplay.ToString().ToLower());
                UpdateIniValue(lines, section, "bEnableDiets", config.EnableDiets.ToString().ToLower());
                UpdateIniValue(lines, section, "bEnablePatrolZones", config.EnablePatrolZones.ToString().ToLower());
                UpdateIniValue(lines, section, "MassMigrationTime", config.MassMigrationTime);
                UpdateIniValue(lines, section, "MassMigrationDisableTime", config.MassMigrationDisableTime);
                UpdateIniValue(lines, section, "bEnableMassMigration", config.EnableMassMigration.ToString().ToLower());
                UpdateIniValue(lines, section, "SpeciesMigrationTime", config.SpeciesMigrationTime);
                UpdateIniValue(lines, section, "MinWeatherVariationInterval", config.MinWeatherVariationInterval);
                UpdateIniValue(lines, section, "MaxWeatherVariationInterval", config.MaxWeatherVariationInterval);
                UpdateIniValue(lines, section, "QueueJoinTimeoutSeconds", config.QueueJoinTimeoutSeconds);
                UpdateIniValue(lines, section, "QueueHeartbeatIntervalSeconds", config.QueueHeartbeatIntervalSeconds);
                UpdateIniValue(lines, section, "QueueHeartbeatTimeoutSeconds", config.QueueHeartbeatTimeoutSeconds);
                UpdateIniValue(lines, section, "QueueHeartbeatMaxMisses", config.QueueHeartbeatMaxMisses);

                // Lists
                string stateSection = "/Script/TheIsle.TIGameStateBase";

                var adminList = config.AdminSteamIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => $"AdminsSteamIDs={id.Trim()}")
                    .ToList();

                var whitelistList = config.WhitelistIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => $"WhitelistIDs={id.Trim()}")
                    .ToList();

                var vipList = config.VipIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => $"VIPs={id.Trim()}")
                    .ToList();

                var dinoList = config.Dinosaurs
                    .Where(d => d.IsEnabled)
                    .Select(d => $"AllowedClasses={d.Name}")
                    .ToList();

                UpdateIniList(lines, stateSection, "AdminsSteamIDs", adminList);
                UpdateIniList(lines, stateSection, "WhitelistIDs", whitelistList);
                UpdateIniList(lines, stateSection, "VIPs", vipList);
                UpdateIniList(lines, stateSection, "AllowedClasses", dinoList);

                var disallowedAi = config.DisallowedAIClasses
                    .Where(ai => ai.IsEnabled)
                    .Select(ai => ai.Name)
                    .ToList();
                UpdateIniValue(lines, stateSection, "DisallowedAIClasses", string.Join(",", disallowedAi));

                File.WriteAllLines(_configPath, lines);
                _logger.Debug("Game.ini updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating Game.ini: {ex.Message}", ex);
                throw new InvalidOperationException($"Error updating Game.ini: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates Engine.ini with safe editing
        /// </summary>
        private void UpdateEngineConfig(ServerConfiguration config)
        {
            try
            {
                _logger.Debug("Updating Engine.ini");

                string? dir = Path.GetDirectoryName(_engineConfigPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    _logger.Debug($"Created directory: {dir}");
                }

                List<string> lines = File.Exists(_engineConfigPath)
                    ? File.ReadAllLines(_engineConfigPath).ToList()
                    : new List<string>();

                // Debug logging
                UpdateIniValue(lines, "Core.Log", "LogRedpointEOS", config.EnableLogRedpointEOSVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogOnline", config.EnableLogOnlineVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogOnlineGame", config.EnableLogOnlineGameVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogNet", config.EnableLogNetVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogNetTraffic", config.EnableLogNetTrafficVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogReplicationGraph", config.EnableLogReplicationGraphVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsle", config.EnableLogTheIsleVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAdmin", config.EnableLogTheIsleAdminVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAI", config.EnableLogTheIsleAIVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAnimInstance", config.EnableLogTheIsleAnimInstanceVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAudio", config.EnableLogTheIsleAudioVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAuth", config.EnableLogTheIsleAuthVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleCharacter", config.EnableLogTheIsleCharacterVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleCharacterMovement", config.EnableLogTheIsleCharacterMovementVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleDatabase", config.EnableLogTheIsleDatabaseVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleEnvironment", config.EnableLogTheIsleEnvironmentVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleGame", config.EnableLogTheIsleGameVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleNetwork", config.EnableLogTheIsleNetworkVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleServer", config.EnableLogTheIsleServerVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIslePlayerController", config.EnableLogTheIslePlayerControllerVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleUI", config.EnableLogTheIsleUIVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleWorld", config.EnableLogTheIsleWorldVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleJoinData", config.EnableLogTheIsleJoinDataVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleChatData", config.EnableLogTheIsleChatDataVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleKillData", config.EnableLogTheIsleKillDataVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleCommandData", config.EnableLogTheIsleCommandDataVerbose ? "Verbose" : null);
                UpdateIniValue(lines, "Core.Log", "LogTheIsleAntiCheat", config.EnableLogTheIsleAntiCheatVerbose ? "Verbose" : null);

                // Mandatory EOS keys
                UpdateIniValue(lines, "EpicOnlineServices", "DedicatedServerClientId", "xyza7891gk5PRo3J7G9puCJGFJjmEguW");
                UpdateIniValue(lines, "EpicOnlineServices", "DedicatedServerClientSecret", "pKWl6t5i9NJK8gTpVlAxzENZ65P8hYzodV8Dqe5Rlc8");

                // Streaming options
                if (config.DisableStreaming)
                {
                    UpdateIniValue(lines, "ConsoleVariables", "wp.Runtime.EnableServerStreaming", "0");
                    UpdateIniValue(lines, "ConsoleVariables", "wp.Runtime.EnableServerStreamingOut", "0");
                    _logger.Debug("World streaming disabled");
                }
                else
                {
                    UpdateIniValue(lines, "ConsoleVariables", "wp.Runtime.EnableServerStreaming", null);
                    UpdateIniValue(lines, "ConsoleVariables", "wp.Runtime.EnableServerStreamingOut", null);
                }

                File.WriteAllLines(_engineConfigPath, lines);
                _logger.Debug("Engine.ini updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating Engine.ini: {ex.Message}", ex);
                throw new InvalidOperationException($"Error updating Engine.ini: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves launcher-specific settings
        /// </summary>
        private void SaveLauncherSettings(ServerConfiguration config)
        {
            try
            {
                _logger.Debug("Saving launcher settings");

                string? dir = Path.GetDirectoryName(_settingsPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    _logger.Debug($"Created directory: {dir}");
                }

                StringBuilder settings = new StringBuilder();
                settings.AppendLine($"ValidateFiles={config.ValidateFiles.ToString().ToLower()}");
                settings.AppendLine($"DisableStreaming={config.DisableStreaming.ToString().ToLower()}");
                settings.AppendLine($"CustomArgs={config.CustomArgs}");
                settings.AppendLine($"ProcessPriority={config.ProcessPriority}");
                settings.AppendLine($"CpuAffinity={config.CpuAffinity}");

                // Theme
                settings.AppendLine($"Theme={config.Theme}");

                // Crash Detection
                settings.AppendLine($"EnableCrashDetection={config.EnableCrashDetection.ToString().ToLower()}");
                settings.AppendLine($"AutoRestart={config.AutoRestart.ToString().ToLower()}");
                settings.AppendLine($"MaxRestartAttempts={config.MaxRestartAttempts}");

                // Scheduled Restarts
                settings.AppendLine($"ScheduledRestartEnabled={config.ScheduledRestartEnabled.ToString().ToLower()}");
                settings.AppendLine($"RestartIntervalHours={config.RestartIntervalHours}");
                settings.AppendLine($"RestartWarningMinutes={config.RestartWarningMinutes}");
                settings.AppendLine($"RestartMessage={config.RestartMessage}");

                // Discord Webhook
                settings.AppendLine($"EnableDiscordWebhook={config.EnableDiscordWebhook.ToString().ToLower()}");
                settings.AppendLine($"DiscordWebhookUrl={config.DiscordWebhookUrl}");

                // Automatic Backups
                settings.AppendLine($"AutoBackupEnabled={config.AutoBackupEnabled.ToString().ToLower()}");
                settings.AppendLine($"BackupIntervalHours={config.BackupIntervalHours}");

                // Maintenance
                settings.AppendLine($"AutoWipeCorpsesEnabled={config.AutoWipeCorpsesEnabled.ToString().ToLower()}");
                settings.AppendLine($"WipeCorpsesIntervalMinutes={config.WipeCorpsesIntervalMinutes}");
                settings.AppendLine($"WipeCorpsesDelayMinutes={config.WipeCorpsesDelayMinutes}");
                settings.AppendLine($"WipeWarningMessage={config.WipeWarningMessage}");
                settings.AppendLine($"WipeCompleteMessage={config.WipeCompleteMessage}");
                settings.AppendLine($"AutoRconSaveEnabled={config.AutoRconSaveEnabled.ToString().ToLower()}");
                settings.AppendLine($"RconSaveIntervalMinutes={config.RconSaveIntervalMinutes}");

                // Chat Monitor
                settings.AppendLine($"EnableChatMonitor={config.EnableChatMonitor.ToString().ToLower()}");
                settings.AppendLine($"EnableChatWebhook={config.EnableChatWebhook.ToString().ToLower()}");
                settings.AppendLine($"ChatWebhookUrl={config.ChatWebhookUrl}");
                settings.AppendLine($"ChatRefreshInterval={config.ChatRefreshInterval}");

                // Zombie Check
                settings.AppendLine($"EnableZombieCheck={config.EnableZombieCheck.ToString().ToLower()}");
                settings.AppendLine($"ZombieTimeoutSeconds={config.ZombieTimeoutSeconds}");

                // Debug Logging
                settings.AppendLine($"EnableLogRedpointEOSVerbose={config.EnableLogRedpointEOSVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogOnlineVerbose={config.EnableLogOnlineVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogOnlineGameVerbose={config.EnableLogOnlineGameVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogNetVerbose={config.EnableLogNetVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogNetTrafficVerbose={config.EnableLogNetTrafficVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogReplicationGraphVerbose={config.EnableLogReplicationGraphVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleVerbose={config.EnableLogTheIsleVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAdminVerbose={config.EnableLogTheIsleAdminVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAIVerbose={config.EnableLogTheIsleAIVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAnimInstanceVerbose={config.EnableLogTheIsleAnimInstanceVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAudioVerbose={config.EnableLogTheIsleAudioVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAuthVerbose={config.EnableLogTheIsleAuthVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleCharacterVerbose={config.EnableLogTheIsleCharacterVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleCharacterMovementVerbose={config.EnableLogTheIsleCharacterMovementVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleDatabaseVerbose={config.EnableLogTheIsleDatabaseVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleEnvironmentVerbose={config.EnableLogTheIsleEnvironmentVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleGameVerbose={config.EnableLogTheIsleGameVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleNetworkVerbose={config.EnableLogTheIsleNetworkVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleServerVerbose={config.EnableLogTheIsleServerVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIslePlayerControllerVerbose={config.EnableLogTheIslePlayerControllerVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleUIVerbose={config.EnableLogTheIsleUIVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleWorldVerbose={config.EnableLogTheIsleWorldVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleJoinDataVerbose={config.EnableLogTheIsleJoinDataVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleChatDataVerbose={config.EnableLogTheIsleChatDataVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleKillDataVerbose={config.EnableLogTheIsleKillDataVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleCommandDataVerbose={config.EnableLogTheIsleCommandDataVerbose.ToString().ToLower()}");
                settings.AppendLine($"EnableLogTheIsleAntiCheatVerbose={config.EnableLogTheIsleAntiCheatVerbose.ToString().ToLower()}");

                var enabledDinos = config.Dinosaurs.Where(d => d.IsEnabled).Select(d => d.Name);
                settings.AppendLine($"EnabledDinos={string.Join(",", enabledDinos)}");

                File.WriteAllText(_settingsPath, settings.ToString());
                _logger.Debug("Launcher settings saved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving launcher settings: {ex.Message}", ex);
                throw new InvalidOperationException($"Error saving launcher settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates default Game.ini if it doesn't exist
        /// </summary>
        private void CreateDefaultGameConfig()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_configPath);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[/Script/TheIsle.TIGameSession]");
                sb.AppendLine("ServerName=My Amazing Server");
                sb.AppendLine("bQueueEnabled=false");

                File.WriteAllText(_configPath, sb.ToString());
                _logger.Info("Created default Game.ini");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating default Game.ini: {ex.Message}", ex);
                // Don't throw - this is not critical
            }
        }

        /// <summary>
        /// Safely updates or adds a single INI value
        /// </summary>
        private void UpdateIniValue(List<string> lines, string section, string key, string? value)
        {
            int sectionIdx = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                {
                    sectionIdx = i;
                    break;
                }
            }

            if (sectionIdx == -1)
            {
                if (value == null) return;
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                    lines.Add("");
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
                return;
            }

            int keyIdx = -1;
            int nextSectionIdx = lines.Count;

            for (int i = sectionIdx + 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    nextSectionIdx = i;
                    break;
                }

                if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith(key + " =", StringComparison.OrdinalIgnoreCase))
                {
                    keyIdx = i;
                    break;
                }
            }

            if (value == null)
            {
                if (keyIdx != -1)
                    lines.RemoveAt(keyIdx);
            }
            else
            {
                if (keyIdx != -1)
                    lines[keyIdx] = $"{key}={value}";
                else
                    lines.Insert(nextSectionIdx, $"{key}={value}");
            }
        }

        /// <summary>
        /// Safely updates or replaces a list of INI values (like AllowedClasses)
        /// </summary>
        private void UpdateIniList(List<string> lines, string section, string keyPrefix, List<string> newValues)
        {
            int sectionIdx = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                {
                    sectionIdx = i;
                    break;
                }
            }

            if (sectionIdx == -1)
            {
                if (newValues.Count == 0) return;
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                    lines.Add("");
                lines.Add($"[{section}]");
                lines.AddRange(newValues);
                return;
            }

            int nextSectionIdx = lines.Count;
            for (int i = sectionIdx + 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    nextSectionIdx = i;
                    break;
                }
            }

            // Remove existing entries
            for (int i = nextSectionIdx - 1; i > sectionIdx; i--)
            {
                if (lines[i].Trim().StartsWith(keyPrefix + "=", StringComparison.OrdinalIgnoreCase))
                {
                    lines.RemoveAt(i);
                    nextSectionIdx--;
                }
            }

            // Insert new values
            lines.InsertRange(nextSectionIdx, newValues);
        }

        private string? GetConfigValue(string content, string key)
        {
            var match = Regex.Match(
                content,
                $@"^{key}=""?([^""\r\n]*)""?",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private bool GetBoolValue(string content, string key, bool defaultValue = false)
        {
            var value = GetConfigValue(content, key);
            if (value == null) return defaultValue;
            return value.ToLower() == "true";
        }
    }
}
