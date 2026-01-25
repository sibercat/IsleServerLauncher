using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public class SteamCmdService
    {
        private const string SteamAppId = "412680";

        private readonly string _serverFolder;
        private readonly string _steamCmdPath;
        private readonly ILogger _logger;

        public SteamCmdService(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _steamCmdPath = Path.Combine(serverFolder, "steamcmd.exe");

            _logger.Info($"SteamCmdService initialized. Server folder: {_serverFolder}");
        }

        /// <summary>
        /// Installs or updates The Isle server files
        /// </summary>
        public async Task InstallOrUpdateServerAsync()
        {
            _logger.Info("Starting server installation/update");

            try
            {
                // Create required directories
                if (!Directory.Exists(_serverFolder))
                {
                    Directory.CreateDirectory(_serverFolder);
                    _logger.Debug($"Created server folder: {_serverFolder}");
                }

                string steamappsPath = Path.Combine(_serverFolder, "steamapps");
                string logsPath = Path.Combine(_serverFolder, "logs");

                Directory.CreateDirectory(steamappsPath);
                Directory.CreateDirectory(logsPath);
                _logger.Debug("Required directories created");

                // Extract embedded steamcmd.exe if needed
                if (!File.Exists(_steamCmdPath))
                {
                    _logger.Info("SteamCMD not found, extracting embedded resource");
                    await ExtractSteamCmdAsync();
                }
                else
                {
                    _logger.Debug("SteamCMD executable found");
                }

                // Create and run update batch file
                await RunSteamUpdateAsync(validate: true);

                // Apply post-installation fixes
                ApplySteamFixes();

                _logger.Info("Server installation/update completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Server installation/update failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Validates server files using SteamCMD
        /// </summary>
        public async Task ValidateServerFilesAsync()
        {
            if (!File.Exists(_steamCmdPath))
            {
                _logger.Warning("Cannot validate - SteamCMD not found");
                return;
            }

            _logger.Info("Starting server file validation");

            try
            {
                await RunSteamUpdateAsync(validate: true);
                _logger.Info("Server file validation completed");
            }
            catch (Exception ex)
            {
                _logger.Error($"Server file validation failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Extracts the embedded steamcmd.exe resource
        /// </summary>
        private async Task ExtractSteamCmdAsync()
        {
            string resourceName = "IsleServerLauncher.steamcmd.exe";
            var assembly = Assembly.GetExecutingAssembly();

            try
            {
                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (FileStream fileStream = new FileStream(_steamCmdPath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                        _logger.Info($"SteamCMD extracted to: {_steamCmdPath}");
                    }
                    else
                    {
                        _logger.Error($"Embedded resource not found: {resourceName}");
                        throw new FileNotFoundException("Embedded steamcmd.exe resource not found", resourceName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to extract SteamCMD: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Runs SteamCMD update with optional validation
        /// </summary>
        private async Task RunSteamUpdateAsync(bool validate)
        {
            string affinityHex = CalculateAffinityMask();
            string validateFlag = validate ? "validate" : "";
            string operationType = validate ? "Validation" : "Update";

            string batchFilePath = Path.Combine(_serverFolder, validate ? "ValidateServer.bat" : "UpdateServer.bat");

            try
            {
                string batchContent = $@"@ECHO OFF
CD /D ""{_serverFolder}""
TITLE SteamCMD - The Isle {operationType}
start /B /WAIT /AFFINITY {affinityHex} steamcmd.exe +force_install_dir ""{_serverFolder}"" +login anonymous +app_update {SteamAppId} -beta evrima {validateFlag} +quit
IF !ERRORLEVEL! NEQ 0 ( TIMEOUT /T 5 /NOBREAK >NUL & start /B /WAIT /AFFINITY {affinityHex} steamcmd.exe +force_install_dir ""{_serverFolder}"" +login anonymous +app_update {SteamAppId} -beta evrima {validateFlag} +quit )
TIMEOUT /T {(validate ? 3 : 5)}";

                await File.WriteAllTextAsync(batchFilePath, batchContent);
                _logger.Debug($"Created batch file: {batchFilePath}");

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C \"{batchFilePath}\"";
                process.StartInfo.WorkingDirectory = _serverFolder;
                process.StartInfo.UseShellExecute = true;

                _logger.Info($"Starting SteamCMD {operationType.ToLower()}...");
                process.Start();

                await process.WaitForExitAsync();
                _logger.Info($"SteamCMD {operationType.ToLower()} process completed with exit code: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    _logger.Warning($"SteamCMD exited with non-zero code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error running SteamCMD {operationType.ToLower()}: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Applies necessary fixes after SteamCMD installation
        /// </summary>
        private void ApplySteamFixes()
        {
            try
            {
                _logger.Debug("Applying post-installation fixes");

                string binFolder = Path.Combine(_serverFolder, "TheIsle", "Binaries", "Win64");

                if (!Directory.Exists(binFolder))
                {
                    _logger.Warning($"Binaries folder not found: {binFolder}");
                    return;
                }

                string appIdFile = Path.Combine(binFolder, "steam_appid.txt");
                File.WriteAllText(appIdFile, SteamAppId);
                _logger.Info($"Created steam_appid.txt with App ID: {SteamAppId}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Steam fix warning: {ex.Message}", ex);
                // Don't throw - this is not critical to functionality
            }
        }

        /// <summary>
        /// Calculates CPU affinity mask for SteamCMD process (leaves core 0 free)
        /// </summary>
        private string CalculateAffinityMask()
        {
            int coreCount = Environment.ProcessorCount;

            if (coreCount <= 1)
            {
                _logger.Debug("Single core system - using core 0 for SteamCMD");
                return "1";
            }

            // Use all cores except core 0
            long affinityMask = (1L << coreCount) - 2;
            _logger.Debug($"SteamCMD affinity mask calculated: 0x{affinityMask:X} (cores 1-{coreCount - 1})");
            return affinityMask.ToString("X");
        }

        /// <summary>
        /// Checks if server executable exists
        /// </summary>
        public bool IsServerInstalled()
        {
            string gameExePath = Path.Combine(_serverFolder, "TheIsleServer.exe");
            bool installed = File.Exists(gameExePath);

            _logger.Debug($"Server installation check: {(installed ? "INSTALLED" : "NOT INSTALLED")}");
            return installed;
        }
    }
}