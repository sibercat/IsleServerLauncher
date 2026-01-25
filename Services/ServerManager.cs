using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public enum ServerState
    {
        NotInstalled,
        Installing,
        Stopped,
        Starting,
        Running,
        Stopping,
        Crashed
    }

    public class ServerManager
    {
        // Native API for graceful shutdown
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? handler, bool add);

        private delegate bool ConsoleCtrlDelegate(uint ctrlType);

        private const string ServerExeName = "TheIsleServer-Win64-Shipping";
        private const string ServerExePathRel = @"TheIsleServer.exe";
        private const string MapPath = "/Game/TheIsle/Maps/Game/Gateway/Gateway";

        private readonly string _serverFolder;
        private readonly string _gameExePath;
        private readonly ILogger _logger;
        private readonly object _processLock = new object();
        private Process? _serverProcess;

        // Crash detection fields
        private bool _enableCrashDetection;
        private bool _autoRestart;
        private int _maxRestartAttempts;
        private int _currentRestartAttempts;
        private DateTime _lastCrashTime;
        private string? _lastGamePort;
        private string? _lastCustomArgs;
        private string? _lastPriority;
        private string? _lastCpuAffinity;
        private bool _lastUseAllCores;
        private DiscordWebhookService? _discordWebhookService;
        private bool _enableDiscordWebhook;
        private string _discordWebhookUrl = "";
        private DateTime _serverStartTime;

        // Zombie check fields
        private bool _enableZombieCheck;
        private int _zombieTimeoutSeconds;

        public event EventHandler<ServerState>? StateChanged;
        public event EventHandler<string>? CrashDetected;

        public ServerState CurrentState { get; private set; }

        public ServerManager(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameExePath = Path.Combine(serverFolder, ServerExePathRel);
            CurrentState = ServerState.Stopped;

            // Default crash detection settings
            _enableCrashDetection = true;
            _autoRestart = false;
            _maxRestartAttempts = 3;
            _currentRestartAttempts = 0;
            _lastCrashTime = DateTime.MinValue;

            // Default zombie check settings
            _enableZombieCheck = false;
            _zombieTimeoutSeconds = 60;

            _logger.Info($"ServerManager initialized. Server folder: {_serverFolder}");
        }

        /// <summary>
        /// Configures Discord webhook notifications
        /// </summary>
        public void ConfigureWebhook(DiscordWebhookService webhookService, bool enableWebhook, string webhookUrl)
        {
            _discordWebhookService = webhookService;
            _enableDiscordWebhook = enableWebhook;
            _discordWebhookUrl = webhookUrl ?? "";
            _logger.Info($"Discord webhook configured: Enabled={enableWebhook}");
        }

        /// <summary>
        /// Configures crash detection and auto-restart settings
        /// </summary>
        public void ConfigureCrashDetection(bool enableCrashDetection, bool autoRestart, int maxAttempts)
        {
            _enableCrashDetection = enableCrashDetection;
            _autoRestart = autoRestart;
            _maxRestartAttempts = Math.Max(1, Math.Min(maxAttempts, 10)); // Limit to 1-10

            _logger.Info($"Crash detection configured: Enabled={enableCrashDetection}, AutoRestart={autoRestart}, MaxAttempts={_maxRestartAttempts}");
        }

        /// <summary>
        /// Configures zombie process protection settings
        /// </summary>
        public void ConfigureZombieCheck(bool enableZombieCheck, int timeoutSeconds)
        {
            _enableZombieCheck = enableZombieCheck;
            _zombieTimeoutSeconds = Math.Max(30, Math.Min(timeoutSeconds, 300)); // Clamp 30-300s
            _logger.Info($"Zombie check configured: Enabled={enableZombieCheck}, Timeout={_zombieTimeoutSeconds}s");
        }

        /// <summary>
        /// Updates launch parameters (used when detecting an existing process or loading config)
        /// </summary>
        public void UpdateLaunchParameters(string gamePort, string? customArgs, string priority, string cpuAffinity, bool useAllCores)
        {
            _lastGamePort = gamePort;
            _lastCustomArgs = customArgs;
            _lastPriority = priority;
            _lastCpuAffinity = cpuAffinity;
            _lastUseAllCores = useAllCores;
            _logger.Debug($"Launch parameters updated: Port={gamePort}, Priority={priority}");
        }

        /// <summary>
        /// Resets restart attempt counter (call when server starts successfully)
        /// </summary>
        public void ResetRestartCounter()
        {
            _currentRestartAttempts = 0;
            _logger.Debug("Restart attempt counter reset");
        }

        /// <summary>
        /// Checks for existing server process and updates state
        /// </summary>
        public void CheckInitialState()
        {
            try
            {
                _logger.Debug("Checking for existing server process");
                var processes = Process.GetProcessesByName(ServerExeName);

                if (processes.Length > 0)
                {
                    lock (_processLock)
                    {
                        _serverProcess = processes[0];
                        _serverProcess.EnableRaisingEvents = true;
                        _serverProcess.Exited += OnProcessExited;

                        // Capture start time from existing process for accurate Uptime reporting
                        try { _serverStartTime = _serverProcess.StartTime; }
                        catch { _serverStartTime = DateTime.Now; }
                    }

                    _logger.Info($"Found existing server process (PID: {processes[0].Id})");
                    SetState(ServerState.Running);

                    // Dispose other processes
                    for (int i = 1; i < processes.Length; i++)
                    {
                        processes[i].Dispose();
                    }
                    return;
                }

                if (!File.Exists(_gameExePath))
                {
                    _logger.Info("Server executable not found - server not installed");
                    SetState(ServerState.NotInstalled);
                }
                else
                {
                    _logger.Info("Server executable found - server is stopped");
                    SetState(ServerState.Stopped);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking initial state: {ex.Message}", ex);
                SetState(ServerState.Stopped);
            }
        }

        /// <summary>
        /// Starts the server with specified configuration
        /// </summary>
        public void StartServer(string gamePort, string? customArgs, string processPriority, string cpuAffinity, bool useAllCores)
        {
            if (!File.Exists(_gameExePath))
            {
                _logger.Error($"Server executable not found at: {_gameExePath}");
                throw new FileNotFoundException("Server executable not found", _gameExePath);
            }

            // Validate port
            if (!InputValidator.IsValidPort(gamePort, out int port))
            {
                _logger.Error($"Invalid game port: {gamePort}");
                throw new ArgumentException($"Invalid game port: {gamePort}", nameof(gamePort));
            }

            // Store launch parameters for potential auto-restart
            _lastGamePort = gamePort;
            _lastCustomArgs = customArgs;
            _lastPriority = processPriority;
            _lastCpuAffinity = cpuAffinity;
            _lastUseAllCores = useAllCores;

            try
            {
                lock (_processLock)
                {
                    _serverStartTime = DateTime.Now;
                    // Clean up any stale handles/processes
                    var existing = Process.GetProcessesByName(ServerExeName);
                    foreach (var p in existing) { try { p.Kill(); } catch { } p.Dispose(); }

                    _serverProcess = new Process();
                    _serverProcess.StartInfo.FileName = _gameExePath;

                    // Build arguments
                    string args = $"-ini:Engine:[EpicOnlineServices]:DedicatedServerClientId=xyza7891gk5PRo3J7G9puCJGFJjmEguW " +
                                 $"-ini:Engine:[EpicOnlineServices]:DedicatedServerClientSecret=pKWl6t5i9NJK8gTpVlAxzENZ65P8hYzodV8Dqe5Rlc8 " +
                                 $"{MapPath} -log -Port={port}";

                    if (!string.IsNullOrWhiteSpace(customArgs))
                        args += $" {customArgs}";

                    _serverProcess.StartInfo.Arguments = args;
                    _serverProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_gameExePath);
                    _serverProcess.StartInfo.UseShellExecute = true;
                    _serverProcess.EnableRaisingEvents = true;
                    _serverProcess.Exited += OnProcessExited;

                    _logger.Info($"Starting server with arguments: {args}");
                    _serverProcess.Start();
                    _logger.Info($"Server process started (Wrapper PID: {_serverProcess.Id})");

                    // Apply performance settings
                    try
                    {
                        ApplyProcessPriority(processPriority);
                        ApplyCpuAffinity(cpuAffinity, useAllCores);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning($"Could not apply performance settings: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to start server: {ex.Message}", ex);
                lock (_processLock)
                {
                    _serverProcess?.Dispose();
                    _serverProcess = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Stops the server using "Double Tap" Ctrl+C Logic with optional zombie timeout
        /// </summary>
        public async Task StopServerAsync(RconClient? rconClient = null)
        {
            // 1. Find the REAL server process (TheIsleServer-Win64-Shipping)
            Process? targetProcess = null;
            bool isOwnProcess = false;

            try
            {
                var shippingProcs = Process.GetProcessesByName(ServerExeName);
                if (shippingProcs.Length > 0)
                {
                    targetProcess = shippingProcs[0];
                    // Clean up duplicates if any
                    for (int i = 1; i < shippingProcs.Length; i++) shippingProcs[i].Dispose();
                    _logger.Info($"Found Game Server process (PID: {targetProcess.Id})");
                }
                else
                {
                    // Fallback to wrapper if shipping exe not found
                    lock (_processLock)
                    {
                        if (_serverProcess != null && !_serverProcess.HasExited)
                        {
                            targetProcess = _serverProcess;
                            isOwnProcess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error finding process to stop: {ex.Message}");
            }

            if (targetProcess == null)
            {
                _logger.Info("No server process found to stop.");
                SetState(ServerState.Stopped);
                return;
            }

            SetState(ServerState.Stopping);
            int targetPid = targetProcess.Id;

            try
            {
                // 2. RCON Save (Best practice)
                if (rconClient != null)
                {
                    _logger.Info("Attempting RCON save...");
                    bool saved = await Task.Run(() => rconClient.TrySendSave());
                    if (saved)
                    {
                        _logger.Info("RCON Save sent. Waiting 5s for disk write...");
                        await Task.Delay(5000);
                    }
                }

                // 3. Graceful Shutdown Loop ("Double Tap" Logic with Zombie Check)
                int attempts = 0;
                const int maxAttempts = 2;

                while (attempts < maxAttempts)
                {
                    targetProcess.Refresh();
                    if (targetProcess.HasExited) break;

                    attempts++;
                    _logger.Info($"[Attempt {attempts}/{maxAttempts}] Sending Ctrl+C signal to PID {targetPid}...");

                    bool signalSent = await Task.Run(() => SendCtrlC(targetPid));

                    if (signalSent)
                    {
                        _logger.Info("Signal sent. Waiting for exit...");
                        try
                        {
                            await Task.Run(() => targetProcess.WaitForExit(5000));
                        }
                        catch { }
                    }
                    else
                    {
                        _logger.Warning("Failed to attach console for Ctrl+C.");
                    }

                    targetProcess.Refresh();
                    if (targetProcess.HasExited)
                    {
                        _logger.Info("Server stopped gracefully.");
                        break;
                    }
                    else
                    {
                        if (attempts < maxAttempts)
                            _logger.Warning("Server still running. Retrying Ctrl+C...");
                    }
                }

                // 4. Zombie Check or Force Kill
                targetProcess.Refresh();
                if (!targetProcess.HasExited)
                {
                    if (_enableZombieCheck)
                    {
                        _logger.Warning($"Graceful shutdown failed. Zombie check enabled - waiting {_zombieTimeoutSeconds} seconds before force kill...");

                        bool processExited = await Task.Run(() =>
                        {
                            return targetProcess.WaitForExit(_zombieTimeoutSeconds * 1000);
                        });

                        targetProcess.Refresh();
                        if (!processExited && !targetProcess.HasExited)
                        {
                            _logger.Warning($"Process still alive after {_zombieTimeoutSeconds}s timeout. Force killing...");
                            targetProcess.Kill();
                            await Task.Delay(2000);
                        }
                        else
                        {
                            _logger.Info("Process exited during zombie timeout window.");
                        }
                    }
                    else
                    {
                        _logger.Warning("Graceful shutdown failed. Force killing immediately (zombie check disabled)...");
                        targetProcess.Kill();
                        await Task.Delay(2000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error stopping server: {ex.Message}", ex);
            }
            finally
            {
                // Cleanup
                if (targetProcess != null && !isOwnProcess)
                {
                    targetProcess.Dispose();
                }

                lock (_processLock)
                {
                    _serverProcess?.Dispose();
                    _serverProcess = null;
                }

                // Final verify to ensure state is clean
                await VerifyProcessTermination();
                SetState(ServerState.Stopped);
            }
        }

        /// <summary>
        /// Final check to ensure no zombies remain
        /// </summary>
        private async Task VerifyProcessTermination()
        {
            await Task.Run(() =>
            {
                try
                {
                    var remaining = Process.GetProcessesByName(ServerExeName);
                    if (remaining.Length == 0) return;

                    _logger.Warning($"Cleaning up {remaining.Length} zombie process(es)...");
                    foreach (var p in remaining)
                    {
                        try { p.Kill(); } catch { }
                        p.Dispose();
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Sets server state and raises event
        /// </summary>
        public void SetState(ServerState state)
        {
            _logger.Debug($"State change: {CurrentState} -> {state}");
            CurrentState = state;
            StateChanged?.Invoke(this, state);
        }

        private void ApplyProcessPriority(string priority)
        {
            Process? process;
            lock (_processLock) { process = _serverProcess; }
            if (process == null || process.HasExited) return;

            try
            {
                var priorityClass = priority switch
                {
                    "Normal" => ProcessPriorityClass.Normal,
                    "AboveNormal" => ProcessPriorityClass.AboveNormal,
                    "High" => ProcessPriorityClass.High,
                    _ => ProcessPriorityClass.Normal
                };
                process.PriorityClass = priorityClass;
            }
            catch (Exception ex) { _logger.Warning($"Failed to set priority: {ex.Message}"); }
        }

        private void ApplyCpuAffinity(string cpuAffinity, bool useAllCores)
        {
            Process? process;
            lock (_processLock) { process = _serverProcess; }
            if (process == null || process.HasExited) return;
            if (useAllCores || string.IsNullOrWhiteSpace(cpuAffinity)) return;

            if (!InputValidator.IsValidCpuAffinity(cpuAffinity, Environment.ProcessorCount, out string? error)) return;

            try
            {
                long affinityMask = 0;
                foreach (var core in cpuAffinity.Split(','))
                {
                    if (int.TryParse(core.Trim(), out int coreNum)) affinityMask |= (1L << coreNum);
                }
                if (affinityMask > 0) process.ProcessorAffinity = new IntPtr(affinityMask);
            }
            catch (Exception ex) { _logger.Warning($"Failed to set affinity: {ex.Message}"); }
        }

        private bool SendCtrlC(int processId)
        {
            try
            {
                FreeConsole();
                if (!AttachConsole(processId)) return false;

                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(0, 0);

                Thread.Sleep(2000);

                FreeConsole();
                SetConsoleCtrlHandler(null, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Monitors the server log for "Load map complete" to confirm recovery
        /// </summary>
        private async Task MonitorServerRecoveryAsync(int attemptNumber)
        {
            var timeout = TimeSpan.FromMinutes(5);
            var startTime = DateTime.Now;
            var logPath = Path.Combine(_serverFolder, "TheIsle", "Saved", "Logs", "TheIsle.log");
            bool recovered = false;

            _logger.Info("Monitoring server log for recovery confirmation...");

            try
            {
                // Wait for log file to be created
                while (!File.Exists(logPath))
                {
                    if (DateTime.Now - startTime > timeout)
                    {
                        _logger.Warning("Recovery Monitor: Log file was not created within timeout.");
                        return;
                    }
                    await Task.Delay(1000);
                }

                // Read the log file in real-time
                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    while (DateTime.Now - startTime < timeout)
                    {
                        string? line = await reader.ReadLineAsync();

                        if (line != null)
                        {
                            if (line.Contains("Load map complete", StringComparison.OrdinalIgnoreCase))
                            {
                                recovered = true;
                                break;
                            }
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error monitoring log file for recovery: {ex.Message}");
            }

            if (recovered)
            {
                _logger.Info("✔ Server recovery confirmed (Map Loaded).");
                ResetRestartCounter();

                if (_enableDiscordWebhook && _discordWebhookService != null)
                {
                    await _discordWebhookService.SendAutoRestartSuccessAsync(
                        _discordWebhookUrl,
                        "Isle Server",
                        attemptNumber);
                }
            }
            else
            {
                _logger.Warning("Server restart initiated, but 'Load Map Complete' was not detected within 5 minutes.");
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            if (CurrentState == ServerState.Stopping)
            {
                _logger.Debug("Process exited during managed shutdown - ignoring");
                return;
            }

            if (!_enableCrashDetection)
            {
                _logger.Info("Server process exited (crash detection disabled)");
                SetState(ServerState.Stopped);
                return;
            }

            Process? crashedProcess;
            lock (_processLock) { crashedProcess = _serverProcess; }

            int? exitCode = null;
            DateTime crashTime = DateTime.Now;

            try
            {
                if (crashedProcess != null && crashedProcess.HasExited)
                {
                    exitCode = crashedProcess.ExitCode;
                }
            }
            catch { }

            if (_enableDiscordWebhook && _discordWebhookService != null && !string.IsNullOrWhiteSpace(_discordWebhookUrl))
            {
                try
                {
                    TimeSpan uptime = crashTime - _serverStartTime;
                    Task.Run(async () =>
                    {
                        await _discordWebhookService.SendCrashNotificationAsync(
                            _discordWebhookUrl,
                            "Isle Server",
                            crashTime,
                            exitCode,
                            uptime);
                    });
                }
                catch (Exception webhookEx)
                {
                    _logger.Warning($"Failed to send Discord webhook: {webhookEx.Message}");
                }
            }

            _logger.Error($"=== SERVER CRASH DETECTED ===");
            _logger.Error($"Crash Time: {crashTime}");
            _logger.Error($"Exit Code: {exitCode?.ToString() ?? "Unknown"}");
            _logger.Error($"Uptime: {(crashTime - _lastCrashTime).TotalMinutes:F2} minutes since last event");

            _lastCrashTime = crashTime;

            string crashMessage = $"Server crashed at {crashTime:HH:mm:ss}\n" +
                                $"Exit Code: {exitCode?.ToString() ?? "Unknown"}";

            CrashDetected?.Invoke(this, crashMessage);

            if (_autoRestart && _currentRestartAttempts < _maxRestartAttempts)
            {
                _currentRestartAttempts++;
                _logger.Info($"Auto-restart attempt {_currentRestartAttempts}/{_maxRestartAttempts}");

                SetState(ServerState.Crashed);

                Task.Run(async () =>
                {
                    await Task.Delay(5000);

                    try
                    {
                        _logger.Info("Initiating auto-restart...");

                        if (_enableDiscordWebhook && _discordWebhookService != null && !string.IsNullOrWhiteSpace(_discordWebhookUrl))
                        {
                            try
                            {
                                await _discordWebhookService.SendRestartNotificationAsync(
                                    _discordWebhookUrl,
                                    "Isle Server",
                                    true,
                                    _currentRestartAttempts,
                                    _maxRestartAttempts);
                            }
                            catch { }
                        }

                        SetState(ServerState.Starting);

                        StartServer(
                            _lastGamePort ?? "7777",
                            _lastCustomArgs,
                            _lastPriority ?? "Normal",
                            _lastCpuAffinity ?? "",
                            _lastUseAllCores
                        );

                        SetState(ServerState.Running);
                        _logger.Info("Auto-restart process launched. Waiting for map load...");

                        int attempt = _currentRestartAttempts;
                        _ = Task.Run(() => MonitorServerRecoveryAsync(attempt));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Auto-restart failed: {ex.Message}", ex);
                        SetState(ServerState.Stopped);

                        if (_enableDiscordWebhook && _discordWebhookService != null)
                        {
                            await _discordWebhookService.SendAutoRestartFailureAsync(_discordWebhookUrl, "Isle Server", _maxRestartAttempts);
                        }
                    }
                });
            }
            else
            {
                if (_autoRestart)
                {
                    _logger.Warning($"Max restart attempts ({_maxRestartAttempts}) reached. Auto-restart disabled.");

                    if (_enableDiscordWebhook && _discordWebhookService != null)
                    {
                        Task.Run(async () => await _discordWebhookService.SendAutoRestartFailureAsync(_discordWebhookUrl, "Isle Server", _maxRestartAttempts));
                    }
                }

                SetState(ServerState.Crashed);
            }
        }
    }
}