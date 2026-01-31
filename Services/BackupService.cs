using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public class BackupService
    {
        private readonly string _serverFolder;
        private readonly ILogger _logger;
        private System.Threading.Timer? _backupTimer;
        private bool _isEnabled;
        private int _intervalHours;
        private DateTime _nextBackupTime;

        public event EventHandler? BackupCompleted;
        public event EventHandler<string>? BackupFailed;

        public bool IsEnabled => _isEnabled;
        public DateTime NextBackupTime => _nextBackupTime;

        public BackupService(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Info($"BackupService initialized. Server folder: {_serverFolder}");
        }

        /// <summary>
        /// Configures automatic backups
        /// </summary>
        public void ConfigureAutomaticBackups(bool enabled, int intervalHours)
        {
            _isEnabled = enabled;
            _intervalHours = Math.Max(1, Math.Min(intervalHours, 168)); // 1 hour to 1 week

            Stop();

            if (_isEnabled)
            {
                Start();
            }
        }

        /// <summary>
        /// Starts the automatic backup timer
        /// </summary>
        private void Start()
        {
            TimeSpan interval = TimeSpan.FromHours(_intervalHours);
            _nextBackupTime = DateTime.Now.Add(interval);

            _logger.Info($"Automatic backups enabled: Every {_intervalHours} hours");
            _logger.Info($"Next backup scheduled for: {_nextBackupTime:yyyy-MM-dd HH:mm:ss}");

            _backupTimer = new System.Threading.Timer(CheckBackupTime, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Stops the automatic backup timer
        /// </summary>
        public void Stop()
        {
            _backupTimer?.Dispose();
            _backupTimer = null;

            if (_isEnabled)
            {
                _logger.Info("Automatic backups stopped");
            }
        }

        /// <summary>
        /// Resets the backup timer
        /// </summary>
        public void ResetTimer()
        {
            if (_isEnabled)
            {
                TimeSpan interval = TimeSpan.FromHours(_intervalHours);
                _nextBackupTime = DateTime.Now.Add(interval);
                _logger.Info($"Backup timer reset. Next backup: {_nextBackupTime:yyyy-MM-dd HH:mm:ss}");
            }
        }

        /// <summary>
        /// Gets time remaining until next backup
        /// </summary>
        public TimeSpan GetTimeUntilBackup()
        {
            if (!_isEnabled) return TimeSpan.Zero;
            TimeSpan remaining = _nextBackupTime - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Timer callback to check if backup is due
        /// </summary>
        private async void CheckBackupTime(object? state)
        {
            if (!_isEnabled) return;

            TimeSpan timeUntilBackup = _nextBackupTime - DateTime.Now;

            if (timeUntilBackup <= TimeSpan.Zero)
            {
                _logger.Info("=== AUTOMATIC BACKUP TRIGGERED ===");

                try
                {
                    await CreateBackupAsync();
                    CleanupOldBackups(10);
                    BackupCompleted?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Automatic backup failed: {ex.Message}", ex);
                    BackupFailed?.Invoke(this, ex.Message);
                }

                // Schedule next backup
                ResetTimer();
            }
        }

        /// <summary>
        /// Creates a compressed backup of the Saved folder
        /// </summary>
        public async Task<string> CreateBackupAsync()
        {
            string savedFolder = Path.Combine(_serverFolder, "TheIsle", "Saved");

            if (!Directory.Exists(savedFolder))
            {
                _logger.Error($"Saved folder not found at: {savedFolder}");
                throw new DirectoryNotFoundException($"Saved folder not found: {savedFolder}");
            }

            try
            {
                _logger.Info("Starting backup creation");

                // Create backups directory if it doesn't exist
                string backupDir = Path.Combine(_serverFolder, "Backups");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    _logger.Debug($"Created backups directory: {backupDir}");
                }

                // Generate backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFileName = $"SavedBackup_{timestamp}.zip";
                string backupPath = Path.Combine(backupDir, backupFileName);

                _logger.Info($"Creating backup: {backupFileName}");

                // Create the zip file asynchronously, excluding the Logs directory
                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
                    {
                        // Add all files and directories except the Logs folder
                        foreach (string filePath in Directory.GetFiles(savedFolder, "*", SearchOption.AllDirectories))
                        {
                            // Skip files in the Logs directory
                            if (filePath.Contains(Path.Combine("Saved", "Logs")))
                            {
                                _logger.Debug($"Skipping log file: {Path.GetFileName(filePath)}");
                                continue;
                            }

                            try
                            {
                                string entryName = filePath.Substring(savedFolder.Length + 1);
                                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                            }
                            catch (IOException ex)
                            {
                                // Log but don't fail the entire backup for locked files
                                _logger.Warning($"Skipped locked file: {Path.GetFileName(filePath)} - {ex.Message}");
                            }
                        }
                    }
                });

                // Get backup size
                FileInfo backupInfo = new FileInfo(backupPath);
                double sizeInMB = backupInfo.Length / (1024.0 * 1024.0);

                _logger.Info($"Backup created successfully: {backupFileName} ({sizeInMB:F2} MB)");

                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.Error($"Backup creation failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Restores a backup from a zip file
        /// </summary>
        public async Task RestoreBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                _logger.Error($"Backup file not found: {backupPath}");
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            string savedFolder = Path.Combine(_serverFolder, "TheIsle", "Saved");

            try
            {
                _logger.Info($"Starting backup restoration from: {Path.GetFileName(backupPath)}");

                // Create temporary restore directory
                string tempRestoreDir = Path.Combine(_serverFolder, "temp_restore");
                if (Directory.Exists(tempRestoreDir))
                {
                    Directory.Delete(tempRestoreDir, true);
                }
                Directory.CreateDirectory(tempRestoreDir);

                // Extract backup to temp directory
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(backupPath, tempRestoreDir);
                });

                _logger.Debug("Backup extracted to temporary directory");

                // Backup current Saved folder before restoration
                if (Directory.Exists(savedFolder))
                {
                    string preRestoreBackup = Path.Combine(_serverFolder, "Backups", $"PreRestore_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");
                    await Task.Run(() =>
                    {
                        ZipFile.CreateFromDirectory(savedFolder, preRestoreBackup, CompressionLevel.Optimal, false);
                    });
                    _logger.Info($"Created pre-restore backup: {Path.GetFileName(preRestoreBackup)}");

                    // Delete current Saved folder
                    Directory.Delete(savedFolder, true);
                    _logger.Debug("Removed existing Saved folder");
                }

                // Move restored data to Saved folder
                Directory.Move(tempRestoreDir, savedFolder);
                _logger.Info("Backup restored successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Backup restoration failed: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets list of available backups
        /// </summary>
        public string[] GetAvailableBackups()
        {
            string backupDir = Path.Combine(_serverFolder, "Backups");

            if (!Directory.Exists(backupDir))
            {
                return Array.Empty<string>();
            }

            try
            {
                var backups = Directory.GetFiles(backupDir, "SavedBackup_*.zip");
                Array.Sort(backups);
                Array.Reverse(backups); // Most recent first
                return backups;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error listing backups: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Deletes old backups, keeping only the specified number of most recent backups
        /// </summary>
        public void CleanupOldBackups(int keepCount = 10)
        {
            try
            {
                var backups = GetAvailableBackups();

                if (backups.Length <= keepCount)
                {
                    _logger.Debug($"Only {backups.Length} backups found, no cleanup needed");
                    return;
                }

                _logger.Info($"Cleaning up old backups, keeping {keepCount} most recent");

                // Delete old backups
                for (int i = keepCount; i < backups.Length; i++)
                {
                    try
                    {
                        File.Delete(backups[i]);
                        _logger.Debug($"Deleted old backup: {Path.GetFileName(backups[i])}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning($"Failed to delete backup {Path.GetFileName(backups[i])}: {ex.Message}");
                    }
                }

                _logger.Info($"Backup cleanup complete, kept {keepCount} backups");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Backup cleanup failed: {ex.Message}");
            }
        }
    }
}
