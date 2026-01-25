using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public class RconClient : IDisposable
    {
        // ==========================================
        // EVRIMA RAW PROTOCOL BYTES
        // ==========================================
        private const byte PACKET_AUTH = 0x01;
        private const byte PACKET_EXEC = 0x02;

        // Command Types
        private const byte CMD_ANNOUNCE = 0x10;
        private const byte CMD_DIRECTMESSAGE = 0x11;
        private const byte CMD_SERVERDETAILS = 0x12;
        private const byte CMD_WIPECORPSES = 0x13;
        private const byte CMD_UPDATEPLAYABLES = 0x15;
        private const byte CMD_BAN = 0x20;
        private const byte CMD_KICK = 0x30;
        private const byte CMD_PLAYERLIST = 0x40;
        private const byte CMD_SAVE = 0x50;
        private const byte CMD_GETPLAYERDATA = 0x77;
        private const byte CMD_TOGGLEWHITELIST = 0x81;
        private const byte CMD_ADDWHITELISTID = 0x82;
        private const byte CMD_REMOVEWHITELISTID = 0x83;
        private const byte CMD_TOGGLEGLOBALCHAT = 0x84;
        private const byte CMD_TOGGLEHUMANS = 0x86;
        private const byte CMD_TOGGLEAI = 0x90;
        private const byte CMD_DISABLEAICLASSES = 0x91;
        private const byte CMD_AIDENSITY = 0x92;

        private readonly string _host;
        private readonly int _port;
        private readonly string _password;
        private readonly ILogger _logger;

        public RconClient(string host, int port, string password, ILogger logger)
        {
            _host = host;
            _port = port;
            _password = password;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==========================================
        // PUBLIC METHODS
        // ==========================================

        public async Task<bool> SendAnnounceAsync(string message)
        {
            var response = await SendCommandAsync(CMD_ANNOUNCE, message);
            return response != null;
        }

        // === FIXED METHOD ===
        public async Task<string?> SendDirectMessageAsync(string playerId, string message)
        {
            // Fixed: Changed space to comma separator
            return await SendCommandAsync(CMD_DIRECTMESSAGE, $"{playerId},{message}");
        }

        public async Task<string?> GetServerDetailsAsync()
        {
            return await SendCommandAsync(CMD_SERVERDETAILS, "");
        }

        public async Task<bool> WipeCorpsesAsync()
        {
            var response = await SendCommandAsync(CMD_WIPECORPSES, "");
            return response != null;
        }

        public async Task<string?> UpdatePlayablesAsync(string playablesList)
        {
            return await SendCommandAsync(CMD_UPDATEPLAYABLES, playablesList);
        }

        public async Task<bool> TrySendSave()
        {
            var response = await SendCommandAsync(CMD_SAVE, "");
            return response != null && !response.Contains("Error", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string?> GetPlayerDataAsync()
        {
            return await SendCommandAsync(CMD_GETPLAYERDATA, "");
        }

        public async Task<List<EvrimaRconPlayer>> GetPlayerListAsync()
        {
            var players = new List<EvrimaRconPlayer>();

            // Use CMD_GETPLAYERDATA since CMD_PLAYERLIST returns empty on this server
            var response = await SendCommandAsync(CMD_GETPLAYERDATA, "");

            if (string.IsNullOrWhiteSpace(response))
            {
                return players;
            }

            // Parse: "Name: xxx, PlayerID: yyy"
            var matches = Regex.Matches(response, @"Name:\s*([^,]+),\s*PlayerID:\s*(\d+)", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                players.Add(new EvrimaRconPlayer(
                    match.Groups[1].Value.Trim(),
                    match.Groups[2].Value.Trim()
                ));
            }

            return players;
        }

        public async Task<string> KickPlayerAsync(string playerId, string reason)
        {
            return await SendCommandAsync(CMD_KICK, $"{playerId},{reason}") ?? "Connection Failed";
        }

        public async Task<string> BanPlayerAsync(string playerName, string playerId, string reason, string duration)
        {
            return await SendCommandAsync(CMD_BAN, $"{playerName},{playerId},{reason},{duration}") ?? "Connection Failed";
        }

        public async Task<bool> ToggleWhitelistAsync(bool enable)
        {
            var response = await SendCommandAsync(CMD_TOGGLEWHITELIST, enable ? "1" : "0");
            return response != null;
        }

        public async Task<bool> AddWhitelistIdAsync(string playerId)
        {
            var response = await SendCommandAsync(CMD_ADDWHITELISTID, playerId);
            return response != null;
        }

        public async Task<bool> RemoveWhitelistIdAsync(string playerId)
        {
            var response = await SendCommandAsync(CMD_REMOVEWHITELISTID, playerId);
            return response != null;
        }

        public async Task<bool> ToggleGlobalChatAsync(bool enable)
        {
            var response = await SendCommandAsync(CMD_TOGGLEGLOBALCHAT, enable ? "1" : "0");
            return response != null;
        }

        public async Task<bool> ToggleHumansAsync(bool enable)
        {
            var response = await SendCommandAsync(CMD_TOGGLEHUMANS, enable ? "1" : "0");
            return response != null;
        }

        public async Task<bool> ToggleAIAsync(bool enable)
        {
            var response = await SendCommandAsync(CMD_TOGGLEAI, enable ? "1" : "0");
            return response != null;
        }

        public async Task<bool> DisableAIClassesAsync(string classList)
        {
            var response = await SendCommandAsync(CMD_DISABLEAICLASSES, classList);
            return response != null;
        }

        public async Task<bool> SetAIDensityAsync(float density)
        {
            var response = await SendCommandAsync(CMD_AIDENSITY, density.ToString("F2"));
            return response != null;
        }

        // ==========================================
        // PROTOCOL IMPLEMENTATION
        // ==========================================

        private async Task<string?> SendCommandAsync(byte commandType, string args)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // 1. Connect
                    var connectTask = client.ConnectAsync(_host, _port);
                    if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                    {
                        _logger.Error($"RCON Connection timeout to {_host}:{_port}");
                        return null;
                    }

                    using (var stream = client.GetStream())
                    {
                        stream.ReadTimeout = 5000;
                        stream.WriteTimeout = 5000;

                        // 2. Authenticate
                        if (!await AuthenticateAsync(stream))
                        {
                            _logger.Error("RCON Authentication failed");
                            return null;
                        }

                        // 3. Send Command
                        var payload = new List<byte>();
                        payload.Add(PACKET_EXEC);       // 0x02
                        payload.Add(commandType);       // Sub-type

                        if (!string.IsNullOrEmpty(args))
                        {
                            payload.AddRange(Encoding.UTF8.GetBytes(args));
                        }

                        payload.Add(0x00); // Null Terminator

                        await stream.WriteAsync(payload.ToArray(), 0, payload.Count);

                        // 4. Read Response
                        return await ReadResponseAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"RCON Error (Cmd: {commandType:X2}): {ex.Message}");
                return null;
            }
        }

        private async Task<bool> AuthenticateAsync(NetworkStream stream)
        {
            var payload = new List<byte>();
            payload.Add(PACKET_AUTH);
            payload.AddRange(Encoding.UTF8.GetBytes(_password));
            payload.Add(0x00);

            await stream.WriteAsync(payload.ToArray(), 0, payload.Count);

            string response = await ReadResponseAsync(stream);
            return response.Contains("Accepted", StringComparison.OrdinalIgnoreCase) ||
                   response.Contains("Logged in", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> ReadResponseAsync(NetworkStream stream)
        {
            var buffer = new byte[8192];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0) return string.Empty;

            return Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd('\0');
        }

        public void Dispose()
        {
            // Stateless
        }
    }
}
