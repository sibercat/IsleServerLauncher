using System;
using System.IO;
using System.Linq;

namespace IsleServerLauncher.Services
{
    public class PresetManager
    {
        private readonly string _serverFolder;
        private readonly string _presetsFolder;
        private readonly string _configPath;
        private readonly string _engineConfigPath;
        private readonly string _settingsPath;
        private readonly ILogger _logger;

        public PresetManager(string serverFolder, ILogger logger)
        {
            _serverFolder = serverFolder ?? throw new ArgumentNullException(nameof(serverFolder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _presetsFolder = Path.Combine(_serverFolder, "Presets");
            _configPath = Path.Combine(serverFolder, @"TheIsle\Saved\Config\WindowsServer\Game.ini");
            _engineConfigPath = Path.Combine(serverFolder, @"TheIsle\Saved\Config\WindowsServer\Engine.ini");
            _settingsPath = Path.Combine(serverFolder, "launcher_settings.ini");
        }

        public string PresetsFolder => _presetsFolder;

        public string[] GetPresetFolders()
        {
            if (!Directory.Exists(_presetsFolder)) return Array.Empty<string>();
            return Directory.GetDirectories(_presetsFolder);
        }

        public bool SavePreset(string name, bool overwrite, out string error)
        {
            error = "";
            if (!TryNormalizeName(name, out string safeName, out error)) return false;

            try
            {
                EnsurePresetsFolder();
                string presetFolder = Path.Combine(_presetsFolder, safeName);

                if (Directory.Exists(presetFolder))
                {
                    if (!overwrite)
                    {
                        error = "Preset already exists.";
                        return false;
                    }
                    Directory.Delete(presetFolder, true);
                }

                Directory.CreateDirectory(presetFolder);

                if (!File.Exists(_configPath) || !File.Exists(_engineConfigPath) || !File.Exists(_settingsPath))
                {
                    error = "Configuration files are missing. Save settings first.";
                    return false;
                }

                File.Copy(_configPath, Path.Combine(presetFolder, "Game.ini"), true);
                File.Copy(_engineConfigPath, Path.Combine(presetFolder, "Engine.ini"), true);
                File.Copy(_settingsPath, Path.Combine(presetFolder, "launcher_settings.ini"), true);

                _logger.Info($"Preset saved: {presetFolder}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        public bool LoadPreset(string name, out string error)
        {
            error = "";
            if (!TryNormalizeName(name, out string safeName, out error)) return false;

            string presetFolder = Path.Combine(_presetsFolder, safeName);
            if (!Directory.Exists(presetFolder))
            {
                error = "Preset not found.";
                return false;
            }

            try
            {
                string presetGame = Path.Combine(presetFolder, "Game.ini");
                string presetEngine = Path.Combine(presetFolder, "Engine.ini");
                string presetSettings = Path.Combine(presetFolder, "launcher_settings.ini");

                if (!File.Exists(presetGame) || !File.Exists(presetEngine) || !File.Exists(presetSettings))
                {
                    error = "Preset is missing required files.";
                    return false;
                }

                EnsureConfigFolders();
                File.Copy(presetGame, _configPath, true);
                File.Copy(presetEngine, _engineConfigPath, true);
                File.Copy(presetSettings, _settingsPath, true);

                _logger.Info($"Preset loaded: {presetFolder}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        public bool ExportPreset(string name, string exportPath, bool overwrite, out string error)
        {
            error = "";
            if (!TryNormalizeName(name, out string safeName, out error)) return false;

            string presetFolder = Path.Combine(_presetsFolder, safeName);
            if (!Directory.Exists(presetFolder))
            {
                error = "Preset not found.";
                return false;
            }

            try
            {
                if (File.Exists(exportPath))
                {
                    if (!overwrite)
                    {
                        error = "Export path already exists.";
                        return false;
                    }
                    File.Delete(exportPath);
                }

                string presetGame = Path.Combine(presetFolder, "Game.ini");
                string presetEngine = Path.Combine(presetFolder, "Engine.ini");
                string presetSettings = Path.Combine(presetFolder, "launcher_settings.ini");

                if (!File.Exists(presetGame) || !File.Exists(presetEngine) || !File.Exists(presetSettings))
                {
                    error = "Preset is missing required files.";
                    return false;
                }

                string gameContent = File.ReadAllText(presetGame);
                string engineContent = File.ReadAllText(presetEngine);
                string settingsContent = File.ReadAllText(presetSettings);

                string payload = BuildPresetPayload(gameContent, engineContent, settingsContent);
                File.WriteAllText(exportPath, payload);

                _logger.Info($"Preset exported: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to export preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        public bool ImportPreset(string archivePath, string name, bool overwrite, out string error)
        {
            error = "";
            if (!TryNormalizeName(name, out string safeName, out error)) return false;

            if (!File.Exists(archivePath))
            {
                error = "Preset file not found.";
                return false;
            }

            string presetFolder = Path.Combine(_presetsFolder, safeName);

            try
            {
                EnsurePresetsFolder();
                if (Directory.Exists(presetFolder))
                {
                    if (!overwrite)
                    {
                        error = "Preset already exists.";
                        return false;
                    }
                    Directory.Delete(presetFolder, true);
                }

                string payload = File.ReadAllText(archivePath);
                if (!TryParsePresetPayload(payload, out string gameContent, out string engineContent, out string settingsContent, out error))
                {
                    return false;
                }

                Directory.CreateDirectory(presetFolder);
                File.WriteAllText(Path.Combine(presetFolder, "Game.ini"), gameContent);
                File.WriteAllText(Path.Combine(presetFolder, "Engine.ini"), engineContent);
                File.WriteAllText(Path.Combine(presetFolder, "launcher_settings.ini"), settingsContent);

                _logger.Info($"Preset imported: {presetFolder}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to import preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        public bool RenamePreset(string oldName, string newName, bool overwrite, out string error)
        {
            error = "";
            if (!TryNormalizeName(oldName, out string safeOld, out error)) return false;
            if (!TryNormalizeName(newName, out string safeNew, out error)) return false;

            string oldFolder = Path.Combine(_presetsFolder, safeOld);
            string newFolder = Path.Combine(_presetsFolder, safeNew);

            if (!Directory.Exists(oldFolder))
            {
                error = "Preset not found.";
                return false;
            }

            if (Directory.Exists(newFolder))
            {
                if (!overwrite)
                {
                    error = "Preset already exists.";
                    return false;
                }
                Directory.Delete(newFolder, true);
            }

            try
            {
                Directory.Move(oldFolder, newFolder);
                _logger.Info($"Preset renamed: {oldFolder} -> {newFolder}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to rename preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        public bool DeletePreset(string name, out string error)
        {
            error = "";
            if (!TryNormalizeName(name, out string safeName, out error)) return false;

            string presetFolder = Path.Combine(_presetsFolder, safeName);
            if (!Directory.Exists(presetFolder))
            {
                error = "Preset not found.";
                return false;
            }

            try
            {
                Directory.Delete(presetFolder, true);
                _logger.Info($"Preset deleted: {presetFolder}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete preset: {ex.Message}", ex);
                error = ex.Message;
                return false;
            }
        }

        private void EnsurePresetsFolder()
        {
            if (!Directory.Exists(_presetsFolder))
            {
                Directory.CreateDirectory(_presetsFolder);
            }
        }

        private void EnsureConfigFolders()
        {
            string? gameDir = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrWhiteSpace(gameDir) && !Directory.Exists(gameDir))
            {
                Directory.CreateDirectory(gameDir);
            }

            string? engineDir = Path.GetDirectoryName(_engineConfigPath);
            if (!string.IsNullOrWhiteSpace(engineDir) && !Directory.Exists(engineDir))
            {
                Directory.CreateDirectory(engineDir);
            }
        }

        private bool TryNormalizeName(string name, out string safeName, out string error)
        {
            error = "";
            safeName = (name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(safeName))
            {
                error = "Preset name is required.";
                return false;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = new string(safeName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            sanitized = sanitized.Trim();

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                error = "Preset name is invalid.";
                return false;
            }

            safeName = sanitized;
            return true;
        }

        private static string BuildPresetPayload(string gameContent, string engineContent, string settingsContent)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ISLE_PRESET_V1");
            sb.AppendLine(">>>BEGIN Game.ini");
            sb.AppendLine(gameContent ?? "");
            sb.AppendLine("<<<END Game.ini");
            sb.AppendLine(">>>BEGIN Engine.ini");
            sb.AppendLine(engineContent ?? "");
            sb.AppendLine("<<<END Engine.ini");
            sb.AppendLine(">>>BEGIN launcher_settings.ini");
            sb.AppendLine(settingsContent ?? "");
            sb.AppendLine("<<<END launcher_settings.ini");
            return sb.ToString();
        }

        private static bool TryParsePresetPayload(
            string payload,
            out string gameContent,
            out string engineContent,
            out string settingsContent,
            out string error)
        {
            gameContent = "";
            engineContent = "";
            settingsContent = "";
            error = "";

            if (string.IsNullOrWhiteSpace(payload))
            {
                error = "Preset file is empty.";
                return false;
            }

            if (!payload.StartsWith("ISLE_PRESET_V1", StringComparison.Ordinal))
            {
                error = "Preset format is not supported.";
                return false;
            }

            if (!TryExtractBlock(payload, ">>>BEGIN Game.ini", "<<<END Game.ini", out gameContent) ||
                !TryExtractBlock(payload, ">>>BEGIN Engine.ini", "<<<END Engine.ini", out engineContent) ||
                !TryExtractBlock(payload, ">>>BEGIN launcher_settings.ini", "<<<END launcher_settings.ini", out settingsContent))
            {
                error = "Preset file is missing required sections.";
                return false;
            }

            return true;
        }

        private static bool TryExtractBlock(string payload, string begin, string end, out string content)
        {
            content = "";
            int beginIndex = payload.IndexOf(begin, StringComparison.Ordinal);
            if (beginIndex < 0) return false;

            int start = beginIndex + begin.Length;
            if (start < payload.Length && payload[start] == '\r') start++;
            if (start < payload.Length && payload[start] == '\n') start++;

            int endIndex = payload.IndexOf(end, start, StringComparison.Ordinal);
            if (endIndex < 0) return false;

            content = payload.Substring(start, endIndex - start);
            return true;
        }
    }
}
