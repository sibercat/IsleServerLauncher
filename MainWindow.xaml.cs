﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IsleServerLauncher.Services;

namespace IsleServerLauncher
{
    public partial class MainWindow : Window
    {
        // ==========================================
        // SERVICES
        // ==========================================
        private readonly ServerManager _serverManager;
        private readonly ConfigurationManager _configManager;
        private readonly SteamCmdService _steamCmdService;
        private readonly ThemeManager _themeManager;
        private readonly SystemSetupService _systemSetup;
        private readonly UIConfigurationMapper _uiMapper;
        private readonly ILogger _logger;
        private RconClient? _rconClient;
        private readonly ScheduledRestartService _scheduledRestartService;
        private readonly DiscordWebhookService _discordWebhookService;
        private readonly BackupService _backupService;
        private readonly PresetManager _presetManager;
        private System.Windows.Threading.DispatcherTimer? _nextRestartUpdateTimer;

        // Maintenance Timers
        private System.Windows.Threading.DispatcherTimer? _wipeCorpsesTimer;
        private System.Windows.Threading.DispatcherTimer? _rconSaveTimer;
        private string _currentRestartMessage = "Server will restart in {minutes} minute(s)!";
        private DateTime? _nextWipeCorpsesAt;
        private readonly HashSet<int> _sentWipeWarnings = new HashSet<int>();

        // Chat Monitor
        private System.Windows.Threading.DispatcherTimer? _chatMonitorTimer;
        private bool _chatMonitorPaused = false;
        private bool _isLoadingConfig = false;
        private long _lastChatPosition = 0;
        private System.Threading.Timer? _chatWebhookBatchTimer;
        private readonly List<string> _pendingChatMessages = new List<string>();
        private readonly object _chatBatchLock = new object();

        // State Tracking
        private string _currentTheme = "Light";
        private bool _isDirty = false;
        private Brush? _saveButtonNormalBrush;
        private readonly Brush _saveButtonDirtyBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
        private string _lastSavedConfigSignature = "";
        private readonly HashSet<string> _dirtyIgnoreNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "txtAnnouncementMessage",
            "txtWhitelistId",
            "txtLiveAIDensity",
            "chkMonitorGlobal",
            "chkMonitorSpatial",
            "chkMonitorAdmin"
        };

        // Paths
        private readonly string _baseFolder = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _serverFolder;

        // Available dinosaurs
        private readonly List<string> _allDinos = new List<string>
        {
            "Dryosaurus", "Hypsilophodon", "Pachycephalosaurus", "Stegosaurus", "Tenontosaurus",
            "Carnotaurus", "Ceratosaurus", "Deinosuchus", "Diabloceratops", "Omniraptor",
            "Pteranodon", "Troodon", "Beipiaosaurus", "Gallimimus", "Dilophosaurus",
            "Herrerasaurus", "Maiasaura", "Triceratops", "Allosaurus", "Tyrannosaurus"
        };

        private readonly List<string> _allAI = new List<string>
        {
            "Boar", "Rabbit", "Deer", "Goat", "Chickens",
            "Turtles", "Frogs/Toads", "Various Fish", "Crabs",
            "Pterodactylus", "Psittacosaurus"
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeDirtyTracking();

            _serverFolder = System.IO.Path.Combine(_baseFolder, "TheIsleServerFiles");
            string logFolder = System.IO.Path.Combine(_serverFolder, "Logs");

            _logger = new FileLogger(logFolder, LogLevel.Info);
            _logger.Info("=== Launcher Started ===");

            try
            {
                // Initialize services
                _serverManager = new ServerManager(_serverFolder, _logger);
                _configManager = new ConfigurationManager(_serverFolder, _logger);
                _steamCmdService = new SteamCmdService(_serverFolder, _logger);
                _themeManager = new ThemeManager(_serverFolder); // No longer needs config logic inside
                _systemSetup = new SystemSetupService(_serverFolder, _logger);
                _uiMapper = new UIConfigurationMapper(_logger);
                _scheduledRestartService = new ScheduledRestartService(_logger);
                _discordWebhookService = new DiscordWebhookService(_logger);
                _backupService = new BackupService(_serverFolder, _logger);
                _presetManager = new PresetManager(_serverFolder, _logger);

                // Subscribe to events
                _serverManager.StateChanged += OnServerStateChanged;
                _serverManager.CrashDetected += OnServerCrashed;

                _scheduledRestartService.RestartScheduled += OnRestartScheduled;
                _scheduledRestartService.RestartTriggered += async (s, e) =>
                {
                    await Dispatcher.InvokeAsync(async () => await GracefulShutdownAsync(restartAfter: true));
                };
                _scheduledRestartService.WarningIssued += OnRestartWarningIssued;

                _backupService.BackupCompleted += OnBackupCompleted;
                _backupService.BackupFailed += OnBackupFailed;

                InitializeDinoList();
                InitializeDisallowedAiList();
                InitializeCpuCores();
                InitializeChatMonitor();
                InitializeMaintenanceTimers();

                // Load Configuration (This now handles Theme loading)
                LoadConfiguration();

                _serverManager.CheckInitialState();

                _nextRestartUpdateTimer = new System.Windows.Threading.DispatcherTimer();
                _nextRestartUpdateTimer.Interval = TimeSpan.FromSeconds(1);
                _nextRestartUpdateTimer.Tick += UpdateNextRestartDisplay;
                _nextRestartUpdateTimer.Tick += UpdateNextBackupDisplay;
                _nextRestartUpdateTimer.Start();

                _logger.Info("Launcher initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Critical("Failed to initialize launcher", ex);
                MessageBox.Show($"Critical error during initialization:\n{ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        // ==========================================
        // UI HELPERS (TOAST NOTIFICATIONS)
        // ==========================================
        public void ShowToast(string message, bool isError = false)
        {
            Dispatcher.Invoke(() =>
            {
                txtToastMessage.Text = message;
                bdrToast.Background = isError
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));

                bdrToast.Opacity = 1;
                var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(3)))
                {
                    BeginTime = TimeSpan.FromSeconds(0.5)
                };
                bdrToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            });
        }

        // ==========================================
        // GRACEFUL SHUTDOWN LOGIC
        // ==========================================
        public async Task GracefulShutdownAsync(bool restartAfter)
        {
            string action = restartAfter ? "RESTART" : "SHUTDOWN";
            _logger.Info($"=== INITIATING GRACEFUL {action} ===");

            if (_serverManager.CurrentState == ServerState.Running)
            {
                if (_rconClient == null)
                {
                    var config = GetCurrentConfiguration();
                    if (InputValidator.IsValidPort(config.RconPort, out int port))
                    {
                        _rconClient = new RconClient("127.0.0.1", port, config.RconPassword, _logger);
                    }
                }

                if (_rconClient != null)
                {
                    try
                    {
                        string msg = restartAfter ? "Server is RESTARTING" : "Server is SHUTTING DOWN";
                        await _rconClient.SendAnnounceAsync($"{msg} in 10 seconds (SAVING WORLD)...");

                        _logger.Info("Triggering Pre-Shutdown Save...");
                        bool saveSuccess = await _rconClient.TrySendSave();

                        if (saveSuccess) _logger.Info("✔ Server Save Acknowledged.");
                        else _logger.Warning("⚠ Save command failed. Proceeding anyway.");

                        await Task.Delay(10000);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error during RCON shutdown tasks: {ex.Message}");
                    }
                }
            }

            await _serverManager.StopServerAsync(_rconClient);

            if (restartAfter)
            {
                _logger.Info("Waiting 3 seconds for ports to clear...");
                await Task.Delay(3000);
                await HandleServerStartAsync();
            }
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        public ServerConfiguration GetCurrentConfiguration()
        {
            return _uiMapper.BuildConfigurationFromUI(
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
                txtRestartMessage,
                chkEnableDiscordWebhook, txtDiscordWebhookUrl, txtDiscordInvite,
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
                _currentTheme); // Pass current theme to ensure config is saved correctly
        }

        private void InitializeDinoList()
        {
            var list = _allDinos.Select(d => new DinoOption { Name = d, IsEnabled = false }).ToList();
            lstDinos.ItemsSource = list;
        }

        private void InitializeDisallowedAiList()
        {
            var list = _allAI.Select(ai => new AiOption { Name = ai, IsEnabled = false }).ToList();
            lstDisallowedAI.ItemsSource = list;
        }

        private void InitializeCpuCores()
        {
            pnlCpuCores.Children.Clear();
            int coreCount = Environment.ProcessorCount;
            for (int i = 0; i < coreCount; i++)
            {
                var checkbox = new CheckBox
                {
                    Content = $"{i}",
                    IsChecked = true,
                    Margin = new Thickness(5, 2, 5, 2),
                    Width = 45,
                    ToolTip = "Core " + i
                };
                pnlCpuCores.Children.Add(checkbox);
            }
            chkUseAllCores.Checked += (s, e) => { foreach (CheckBox cb in pnlCpuCores.Children) cb.IsChecked = true; };
        }

        private void LoadConfiguration()
        {
            try
            {
                _logger.Info("Loading configuration");
                var config = _configManager.LoadConfiguration(_allDinos, _allAI);

                _isLoadingConfig = true;
                _uiMapper.LoadConfigurationIntoUI(config,
                    txtServerName, txtMaxPlayers, txtServerPass, txtRconPass, txtRconPort, chkEnableRcon, chkWhitelist,
                    txtGamePort, txtQueuePort, chkQueueEnabled, txtCustomArgs,
                    txtDayLength, txtNightLength, chkGlobalChat, chkHumans, chkMutations, chkMigration, chkFallDamage,
                    txtGrowth, txtDecay, txtMigrationTime, chkSpawnAI, chkPlants, chkDynamicWeather, txtAISpawn, txtAIDensity,
                    txtRegionSpawnCooldown, chkUseRegionSpawnCooldown, chkUseRegionSpawning,
                    txtPlantSpawnMultiplier, chkAllowRecordingReplay, chkEnableDiets, chkEnablePatrolZones,
                    txtMassMigrationTime, txtMassMigrationDisableTime, chkEnableMassMigration,
                    txtSpeciesMigrationTime, txtMinWeatherVariationInterval, txtMaxWeatherVariationInterval,
                    txtQueueJoinTimeoutSeconds, txtQueueHeartbeatIntervalSeconds, txtQueueHeartbeatTimeoutSeconds, txtQueueHeartbeatMaxMisses,
                    chkValidateFiles, chkDisableStreaming, cmbPriority, chkUseAllCores, pnlCpuCores,
                    chkEnableCrashDetection, chkAutoRestart, cmbMaxRestartAttempts,
                    chkScheduledRestartEnabled, cmbRestartInterval, cmbWarningMinutes, txtRestartMessage,
                    chkEnableDiscordWebhook, txtDiscordWebhookUrl, txtDiscordInvite,
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
                    chkEnableLogTheIsleAntiCheatVerbose);

                _isLoadingConfig = false;

                // Apply Theme from Configuration
                _currentTheme = config.Theme ?? "Light";
                _themeManager.ApplyTheme(_currentTheme);
                mnuLightMode.IsChecked = _currentTheme == "Light";
                mnuDarkMode.IsChecked = _currentTheme == "Dark";

                UpdateRconClient(config.RconPort, config.RconPassword);

                if (config.EnableChatMonitor && _chatMonitorTimer != null)
                {
                    rtbChatMonitor.Document.Blocks.Clear();
                    LoadRecentChatHistory(100);

                    string logPath = System.IO.Path.Combine(_serverFolder, "TheIsle", "Saved", "Logs", "TheIsle.log");
                    if (System.IO.File.Exists(logPath))
                    {
                        var fileInfo = new System.IO.FileInfo(logPath);
                        _lastChatPosition = fileInfo.Length;
                        _logger.Info($"Chat monitor starting from log position: {_lastChatPosition}");
                    }
                    _chatMonitorTimer.Start();
                    _logger.Info("Chat monitor started from saved configuration");
                }
                else
                {
                    var paragraph = new Paragraph { Margin = new Thickness(0) };
                    paragraph.Inlines.Add(new Run("Chat Monitor is disabled. Enable above to start monitoring.")
                    {
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic
                    });
                    rtbChatMonitor.Document.Blocks.Clear();
                    rtbChatMonitor.Document.Blocks.Add(paragraph);
                }

                _serverManager.ConfigureCrashDetection(
                    config.EnableCrashDetection,
                    config.AutoRestart,
                    config.MaxRestartAttempts);

                _serverManager.ConfigureZombieCheck(
                    config.EnableZombieCheck,
                    config.ZombieTimeoutSeconds);

                _serverManager.ConfigureWebhook(
                    _discordWebhookService,
                    config.EnableDiscordWebhook,
                    config.DiscordWebhookUrl);

                bool useAllCores = string.IsNullOrWhiteSpace(config.CpuAffinity);
                _serverManager.UpdateLaunchParameters(
                    config.GamePort,
                    config.CustomArgs,
                    config.ProcessPriority,
                    config.CpuAffinity,
                    useAllCores
                );

                _backupService.ConfigureAutomaticBackups(
                    config.AutoBackupEnabled,
                    config.BackupIntervalHours);

                _currentRestartMessage = config.RestartMessage;
                _scheduledRestartService.Configure(
                    config.ScheduledRestartEnabled,
                    config.RestartIntervalHours,
                    config.RestartWarningMinutes,
                    config.RestartMessage);

                UpdateMaintenanceTimers();

                if (_serverManager.CurrentState == ServerState.Running && config.ScheduledRestartEnabled)
                {
                    _scheduledRestartService.ResetTimer();
                }

                _lastSavedConfigSignature = BuildConfigSignature(config);
                SetDirty(false);
                _logger.Info("Configuration applied to all services.");
            }
            catch (Exception ex) { _logger.Error($"Config Load Error: {ex.Message}", ex); }
        }

        private void InitializeDirtyTracking()
        {
            _saveButtonNormalBrush = btnSaveConfig.Background;
            AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnSettingTextChanged));
            AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnSettingToggleChanged));
            AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnSettingToggleChanged));
            AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnSettingSelectionChanged));
        }

        private bool ShouldIgnoreDirtyEvent(object? source)
        {
            if (source is FrameworkElement fe && !string.IsNullOrWhiteSpace(fe.Name))
            {
                return _dirtyIgnoreNames.Contains(fe.Name);
            }
            return false;
        }

        private void OnSettingTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (ShouldIgnoreDirtyEvent(e.OriginalSource)) return;
            if (e.OriginalSource is TextBox tb && tb.IsReadOnly) return;
            UpdateDirtyState();
        }

        private void OnSettingToggleChanged(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (ShouldIgnoreDirtyEvent(e.OriginalSource)) return;
            UpdateDirtyState();
        }

        private void OnSettingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (ShouldIgnoreDirtyEvent(e.OriginalSource)) return;
            if (e.OriginalSource is ComboBox) UpdateDirtyState();
        }

        private void SetDirty(bool isDirty)
        {
            if (_isDirty == isDirty) return;
            _isDirty = isDirty;
            _saveButtonNormalBrush ??= btnSaveConfig.Background;
            btnSaveConfig.Background = isDirty ? _saveButtonDirtyBrush : _saveButtonNormalBrush;
        }

        private void UpdateDirtyState()
        {
            if (_isLoadingConfig) return;

            var currentConfig = GetCurrentConfiguration();
            var currentSignature = BuildConfigSignature(currentConfig);
            
            bool isDirty = !string.Equals(currentSignature, _lastSavedConfigSignature, StringComparison.Ordinal);
            SetDirty(isDirty);
            _logger.Debug($"Dirty check: {isDirty}");
        }

        private string BuildConfigSignature(ServerConfiguration config)
        {
            var sb = new StringBuilder(2048);
            sb.Append(config.ServerName).Append('|');
            sb.Append(config.MaxPlayers).Append('|');
            sb.Append(config.ServerPassword).Append('|');
            sb.Append(config.RconPassword).Append('|');
            sb.Append(config.RconPort).Append('|');
            sb.Append(config.RconEnabled).Append('|');
            sb.Append(config.Whitelist).Append('|');
            sb.Append(config.GamePort).Append('|');
            sb.Append(config.QueuePort).Append('|');
            sb.Append(config.QueueEnabled).Append('|');
            sb.Append(config.CustomArgs).Append('|');
            sb.Append(config.DayLength).Append('|');
            sb.Append(config.NightLength).Append('|');
            sb.Append(config.GlobalChat).Append('|');
            sb.Append(config.Humans).Append('|');
            sb.Append(config.Mutations).Append('|');
            sb.Append(config.Migration).Append('|');
            sb.Append(config.FallDamage).Append('|');
            sb.Append(config.GrowthMultiplier).Append('|');
            sb.Append(config.CorpseDecay).Append('|');
            sb.Append(config.MigrationTime).Append('|');
            sb.Append(config.SpawnAI).Append('|');
            sb.Append(config.SpawnPlants).Append('|');
            sb.Append(config.DynamicWeather).Append('|');
            sb.Append(config.AISpawnInterval).Append('|');
            sb.Append(config.AIDensity).Append('|');
            sb.Append(config.RegionSpawnCooldownTimeSeconds).Append('|');
            sb.Append(config.UseRegionSpawnCooldown).Append('|');
            sb.Append(config.UseRegionSpawning).Append('|');
            sb.Append(config.PlantSpawnMultiplier).Append('|');
            sb.Append(config.AllowRecordingReplay).Append('|');
            sb.Append(config.EnableDiets).Append('|');
            sb.Append(config.EnablePatrolZones).Append('|');
            sb.Append(config.MassMigrationTime).Append('|');
            sb.Append(config.MassMigrationDisableTime).Append('|');
            sb.Append(config.EnableMassMigration).Append('|');
            sb.Append(config.SpeciesMigrationTime).Append('|');
            sb.Append(config.MinWeatherVariationInterval).Append('|');
            sb.Append(config.MaxWeatherVariationInterval).Append('|');
            sb.Append(config.QueueJoinTimeoutSeconds).Append('|');
            sb.Append(config.QueueHeartbeatIntervalSeconds).Append('|');
            sb.Append(config.QueueHeartbeatTimeoutSeconds).Append('|');
            sb.Append(config.QueueHeartbeatMaxMisses).Append('|');
            sb.Append(config.ValidateFiles).Append('|');
            sb.Append(config.DisableStreaming).Append('|');
            sb.Append(config.ProcessPriority).Append('|');
            sb.Append(config.CpuAffinity).Append('|');
            sb.Append(config.EnableCrashDetection).Append('|');
            sb.Append(config.AutoRestart).Append('|');
            sb.Append(config.MaxRestartAttempts).Append('|');
            sb.Append(config.ScheduledRestartEnabled).Append('|');
            sb.Append(config.RestartIntervalHours).Append('|');
            sb.Append(config.RestartWarningMinutes).Append('|');
            sb.Append(config.RestartMessage).Append('|');
            sb.Append(config.EnableDiscordWebhook).Append('|');
            sb.Append(config.DiscordWebhookUrl).Append('|');
            sb.Append(config.DiscordInvite).Append('|');
            sb.Append(config.AutoBackupEnabled).Append('|');
            sb.Append(config.BackupIntervalHours).Append('|');
            sb.Append(config.EnableChatMonitor).Append('|');
            sb.Append(config.EnableChatWebhook).Append('|');
            sb.Append(config.ChatWebhookUrl).Append('|');
            sb.Append(config.ChatRefreshInterval).Append('|');
            sb.Append(config.EnableZombieCheck).Append('|');
            sb.Append(config.ZombieTimeoutSeconds).Append('|');
            sb.Append(config.AutoWipeCorpsesEnabled).Append('|');
            sb.Append(config.WipeCorpsesIntervalMinutes).Append('|');
            sb.Append(config.WipeCorpsesDelayMinutes).Append('|');
            sb.Append(config.WipeWarningMessage).Append('|');
            sb.Append(config.WipeCompleteMessage).Append('|');
            sb.Append(config.AutoRconSaveEnabled).Append('|');
            sb.Append(config.RconSaveIntervalMinutes).Append('|');
            sb.Append(config.EnableLogRedpointEOSVerbose).Append('|');
            sb.Append(config.EnableLogOnlineVerbose).Append('|');
            sb.Append(config.EnableLogOnlineGameVerbose).Append('|');
            sb.Append(config.EnableLogNetVerbose).Append('|');
            sb.Append(config.EnableLogNetTrafficVerbose).Append('|');
            sb.Append(config.EnableLogReplicationGraphVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAdminVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAIVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAnimInstanceVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAudioVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAuthVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleCharacterVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleCharacterMovementVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleDatabaseVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleEnvironmentVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleGameVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleNetworkVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleServerVerbose).Append('|');
            sb.Append(config.EnableLogTheIslePlayerControllerVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleUIVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleWorldVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleJoinDataVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleChatDataVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleKillDataVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleCommandDataVerbose).Append('|');
            sb.Append(config.EnableLogTheIsleAntiCheatVerbose).Append('|');
            sb.Append(config.Theme).Append('|');

            sb.Append(string.Join(",", config.AdminSteamIds)).Append('|');
            sb.Append(string.Join(",", config.WhitelistIds)).Append('|');
            sb.Append(string.Join(",", config.VipIds)).Append('|');

            foreach (var dino in config.Dinosaurs)
            {
                sb.Append(dino.Name).Append(':').Append(dino.IsEnabled).Append(',');
            }
            sb.Append('|');

            foreach (var ai in config.DisallowedAIClasses)
            {
                sb.Append(ai.Name).Append(':').Append(ai.IsEnabled).Append(',');
            }

            return sb.ToString();
        }

        private void InitializeMaintenanceTimers()
        {
            _wipeCorpsesTimer = new System.Windows.Threading.DispatcherTimer();
            _wipeCorpsesTimer.Tick += WipeCorpsesTimer_Tick;

            _rconSaveTimer = new System.Windows.Threading.DispatcherTimer();
            _rconSaveTimer.Tick += RconSaveTimer_Tick;
        }

        private void UpdateMaintenanceTimers()
        {
            var config = GetCurrentConfiguration();

            // Wipe Corpses
            _wipeCorpsesTimer?.Stop();
            _nextWipeCorpsesAt = null;
            _sentWipeWarnings.Clear();
            if (config.AutoWipeCorpsesEnabled && config.WipeCorpsesIntervalMinutes > 0)
            {
                int delayMinutes = Math.Max(0, config.WipeCorpsesDelayMinutes);
                _nextWipeCorpsesAt = DateTime.Now.AddMinutes(delayMinutes + config.WipeCorpsesIntervalMinutes);
                _wipeCorpsesTimer!.Interval = TimeSpan.FromMinutes(1);
                _wipeCorpsesTimer.Start();
                _logger.Info($"Auto Wipe Corpses scheduled every {config.WipeCorpsesIntervalMinutes} minutes after {delayMinutes} minute delay.");
            }

            // RCON Save
            _rconSaveTimer?.Stop();
            if (config.AutoRconSaveEnabled && config.RconSaveIntervalMinutes > 0)
            {
                _rconSaveTimer!.Interval = TimeSpan.FromMinutes(config.RconSaveIntervalMinutes);
                _rconSaveTimer.Start();
                _logger.Info($"Auto RCON Save scheduled every {config.RconSaveIntervalMinutes} minutes.");
            }
        }

        private async void WipeCorpsesTimer_Tick(object? sender, EventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;
            if (_nextWipeCorpsesAt == null) return;

            try
            {
                var config = GetCurrentConfiguration();
                if (InputValidator.IsValidPort(config.RconPort, out int port))
                {
                    var remaining = _nextWipeCorpsesAt.Value - DateTime.Now;
                    int remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);

                    using (var rcon = new RconClient("127.0.0.1", port, config.RconPassword, _logger))
                    {
                        if (remainingMinutes <= 0)
                        {
                            await rcon.WipeCorpsesAsync();
                            string complete = string.IsNullOrWhiteSpace(config.WipeCompleteMessage)
                                ? "All Corpses have been wiped."
                                : config.WipeCompleteMessage;
                            await rcon.SendAnnounceAsync(complete);
                            _logger.Info("Automated task: Wipe Corpses executed.");
                            _sentWipeWarnings.Clear();
                            _nextWipeCorpsesAt = DateTime.Now.AddMinutes(Math.Max(1, config.WipeCorpsesIntervalMinutes));
                        }
                        else if (remainingMinutes == 10 || remainingMinutes == 5)
                        {
                            if (_sentWipeWarnings.Add(remainingMinutes))
                            {
                                string warning = string.IsNullOrWhiteSpace(config.WipeWarningMessage)
                                    ? "Warning: All Corpses will be wiped in {minutes} minute(s)!"
                                    : config.WipeWarningMessage;
                                await rcon.SendAnnounceAsync(warning.Replace("{minutes}", remainingMinutes.ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.Error($"Auto Wipe Corpses failed: {ex.Message}"); }
        }

        private async void RconSaveTimer_Tick(object? sender, EventArgs e)
        {
            if (_serverManager.CurrentState != ServerState.Running) return;

            try
            {
                var config = GetCurrentConfiguration();
                if (InputValidator.IsValidPort(config.RconPort, out int port))
                {
                    using (var rcon = new RconClient("127.0.0.1", port, config.RconPassword, _logger))
                    {
                        await rcon.TrySendSave();
                        _logger.Info("Automated task: RCON Save executed.");
                    }
                }
            }
            catch (Exception ex) { _logger.Error($"Auto RCON Save failed: {ex.Message}"); }
        }

        private ConfigValidationResult ValidateAllInputs()
        {
            var result = new ConfigValidationResult { IsValid = true };
            if (!InputValidator.IsValidServerName(txtServerName.Text, out string? nameError)) result.AddError($"Server Name: {nameError}");
            if (!InputValidator.IsValidPort(txtGamePort.Text, out _)) result.AddError("Game Port: Must be 1024-65535");
            if (!InputValidator.IsValidPort(txtRconPort.Text, out _)) result.AddError("RCON Port: Must be 1024-65535");
            return result;
        }

        private string GetSelectedCpuCores()
        {
            var selectedCores = new List<string>();
            foreach (CheckBox cb in pnlCpuCores.Children)
                if (cb.IsChecked == true) selectedCores.Add(cb.Content.ToString() ?? "");
            return string.Join(",", selectedCores);
        }

        private void UpdateRconClient(string rconPort, string rconPassword)
        {
            if (InputValidator.IsValidPort(rconPort, out int port))
                _rconClient = new RconClient("127.0.0.1", port, rconPassword, _logger);
        }

        private void OnServerCrashed(object? sender, string crashMessage)
        {
            Dispatcher.InvokeAsync(() => {
                _logger.Warning("Server crash event received");
                MessageBox.Show($"{crashMessage}\n\nCheck logs.", "Server Crashed", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private void OnRestartScheduled(object? sender, TimeSpan interval)
        {
            Dispatcher.InvokeAsync(() => _logger.Info($"Restart scheduled in {interval.TotalHours} hours"));
        }

        private async void OnRestartWarningIssued(object? sender, int minutesRemaining)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                if (_rconClient != null && _serverManager.CurrentState == ServerState.Running)
                {
                    string message = _currentRestartMessage.Replace("{minutes}", minutesRemaining.ToString());
                    await Task.Run(() => _rconClient.SendAnnounceAsync(message));
                }
            });
        }

        private void UpdateNextRestartDisplay(object? sender, EventArgs e)
        {
            if (_scheduledRestartService.IsEnabled)
            {
                TimeSpan remaining = _scheduledRestartService.GetTimeUntilRestart();
                txtNextRestart.Text = remaining > TimeSpan.Zero
                    ? $"Next restart: {_scheduledRestartService.NextRestartTime:HH:mm:ss} ({remaining.Hours}h {remaining.Minutes}m)"
                    : "Next restart: Calculating...";
            }
            else txtNextRestart.Text = "Next restart: Not scheduled";
        }

        private void OnBackupCompleted(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _logger.Info("Automatic backup completed successfully");
                ShowToast("Auto-Backup Completed");
            });
        }

        private void OnBackupFailed(object? sender, string errorMessage)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _logger.Error($"Automatic backup failed: {errorMessage}");
                ShowToast("Auto-Backup Failed", true);
            });
        }

        private void UpdateNextBackupDisplay(object? sender, EventArgs e)
        {
            if (_backupService.IsEnabled)
            {
                TimeSpan remaining = _backupService.GetTimeUntilBackup();
                txtNextBackup.Text = remaining > TimeSpan.Zero
                    ? $"Next backup: {_backupService.NextBackupTime:HH:mm:ss} ({remaining.Hours}h {remaining.Minutes}m)"
                    : "Next backup: Calculating...";
            }
            else
            {
                txtNextBackup.Text = "Next backup: Not scheduled";
            }
        }

        // ==========================================
        // CHAT MONITOR (RichTextBox Implementation)
        // ==========================================
        private void InitializeChatMonitor()
        {
            try
            {
                _logger.Info("InitializeChatMonitor started");

                _chatMonitorTimer = new System.Windows.Threading.DispatcherTimer();
                _chatMonitorTimer.Interval = TimeSpan.FromSeconds(1);
                _chatMonitorTimer.Tick += ChatMonitorTimer_Tick;
                _logger.Info("Chat monitor timer initialized (not started)");

                _chatWebhookBatchTimer = new System.Threading.Timer(
                    SendBatchedChatMessages,
                    null,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize chat monitor: {ex.Message}", ex);
            }
        }

        private void ChatMonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (!chkEnableChatMonitor.IsChecked.GetValueOrDefault()) return;
            if (_chatMonitorPaused || _serverManager.CurrentState != ServerState.Running) return;

            string logPath = System.IO.Path.Combine(_serverFolder, "TheIsle", "Saved", "Logs", "TheIsle.log");
            if (!System.IO.File.Exists(logPath)) return;

            try
            {
                using (var fs = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    if (fs.Length < _lastChatPosition) _lastChatPosition = 0;
                    fs.Seek(_lastChatPosition, System.IO.SeekOrigin.Begin);

                    using (var reader = new System.IO.StreamReader(fs))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("LogTheIsleChatData:"))
                            {
                                ParseAndDisplayChatLine(line);
                            }
                        }
                        _lastChatPosition = fs.Position;
                    }
                }
            }
            catch { }
        }

        private void SendBatchedChatMessages(object? state)
        {
            string[] messagesToSend;
            lock (_chatBatchLock)
            {
                if (_pendingChatMessages.Count == 0) return;
                messagesToSend = _pendingChatMessages.ToArray();
                _pendingChatMessages.Clear();
            }

            Dispatcher.Invoke(async () =>
            {
                if (chkEnableChatWebhook.IsChecked != true || string.IsNullOrWhiteSpace(txtChatWebhookUrl.Text)) return;

                try
                {
                    await _discordWebhookService.SendBatchedChatMessagesAsync(
                        txtChatWebhookUrl.Text,
                        txtServerName.Text,
                        messagesToSend);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to send chat batch to Discord: {ex.Message}", ex);
                }
            });
        }

        private void chkWhitelist_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
