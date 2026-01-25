using System;
using System.Collections.Generic;
using System.Linq;

namespace IsleServerLauncher.Services
{
    public static class InputValidator
    {
        // Port validation
        public static bool IsValidPort(string portText, out int port)
        {
            port = 0;
            if (string.IsNullOrWhiteSpace(portText))
                return false;

            if (!int.TryParse(portText, out port))
                return false;

            return port >= 1024 && port <= 65535;
        }

        // IP address validation
        public static bool IsValidIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            return parts.All(part => byte.TryParse(part, out _));
        }

        // Server name validation
        public static bool IsValidServerName(string name, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Server name cannot be empty";
                return false;
            }

            if (name.Length > 100)
            {
                error = "Server name must be 100 characters or less";
                return false;
            }

            // Check for invalid characters that could break config files
            char[] invalidChars = new[] { '\n', '\r', '\0', '=', '[', ']' };
            if (name.Any(c => invalidChars.Contains(c)))
            {
                error = "Server name contains invalid characters";
                return false;
            }

            return true;
        }

        // Numeric range validation
        public static bool IsValidNumber(string text, int min, int max, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (!int.TryParse(text, out value))
                return false;

            return value >= min && value <= max;
        }

        // Decimal validation (for multipliers)
        public static bool IsValidDecimal(string text, decimal min, decimal max, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (!decimal.TryParse(text, out value))
                return false;

            return value >= min && value <= max;
        }

        // Steam ID validation (17-digit number)
        public static bool IsValidSteamId(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return false;

            steamId = steamId.Trim();

            if (steamId.Length != 17)
                return false;

            return steamId.All(char.IsDigit);
        }

        // Path validation (prevents directory traversal)
        public static bool IsValidFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // Check for path separators or invalid characters
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return !filename.Any(c => invalidChars.Contains(c) || c == '/' || c == '\\');
        }

        // Validate CPU core selection
        public static bool IsValidCpuAffinity(string cpuAffinity, int totalCores, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(cpuAffinity))
                return true; // Empty is valid (use all cores)

            var parts = cpuAffinity.Split(',');
            var cores = new HashSet<int>();

            foreach (var part in parts)
            {
                if (!int.TryParse(part.Trim(), out int core))
                {
                    error = $"Invalid core number: {part}";
                    return false;
                }

                if (core < 0 || core >= totalCores)
                {
                    error = $"Core {core} is out of range (0-{totalCores - 1})";
                    return false;
                }

                if (!cores.Add(core))
                {
                    error = $"Core {core} is listed multiple times";
                    return false;
                }
            }

            if (cores.Count == 0)
            {
                error = "At least one CPU core must be selected";
                return false;
            }

            return true;
        }
    }

    public class ConfigValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        public string GetErrorMessage()
        {
            return string.Join("\n", Errors);
        }
    }
}
