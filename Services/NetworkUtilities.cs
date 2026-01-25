using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IsleServerLauncher.Services
{
    public static class NetworkUtilities
    {
        /// <summary>
        /// Validates if a string is a valid IPv4 address
        /// </summary>
        public static bool IsValidIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (string part in parts)
            {
                if (!byte.TryParse(part, out byte _))
                    return false;
            }

            return true;
        }
    }
}