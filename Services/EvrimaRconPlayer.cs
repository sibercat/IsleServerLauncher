namespace IsleServerLauncher.Services
{
    public class EvrimaRconPlayer
    {
        public string PlayerName { get; set; } = "";
        public string EosId { get; set; } = "";

        public EvrimaRconPlayer(string playerName, string eosId)
        {
            PlayerName = playerName;
            EosId = eosId;
        }
    }
}
