using WpfTextBox = System.Windows.Controls.TextBox;

namespace IsleServerLauncher
{
    public partial class MainWindow
    {
        private WpfTextBox txtServerName => serverSettingsView.txtServerName;
        private WpfTextBox txtMaxPlayers => serverSettingsView.txtMaxPlayers;
        private WpfTextBox txtServerPass => serverSettingsView.txtServerPass;
        private WpfTextBox txtRconPass => serverSettingsView.txtRconPass;
        private WpfTextBox txtRconPort => serverSettingsView.txtRconPort;
        private WpfTextBox txtGamePort => serverSettingsView.txtGamePort;
        private WpfTextBox txtQueuePort => serverSettingsView.txtQueuePort;
        private WpfTextBox txtCustomArgs => serverSettingsView.txtCustomArgs;
        private WpfTextBox txtDayLength => serverSettingsView.txtDayLength;
        private WpfTextBox txtNightLength => serverSettingsView.txtNightLength;
        private WpfTextBox txtGrowth => serverSettingsView.txtGrowth;
        private WpfTextBox txtDecay => serverSettingsView.txtDecay;
        private WpfTextBox txtMigrationTime => serverSettingsView.txtMigrationTime;
        private WpfTextBox txtAISpawn => serverSettingsView.txtAISpawn;
        private WpfTextBox txtAIDensity => serverSettingsView.txtAIDensity;
        private WpfTextBox txtRegionSpawnCooldown => serverSettingsView.txtRegionSpawnCooldown;
        private WpfTextBox txtPlantSpawnMultiplier => serverSettingsView.txtPlantSpawnMultiplier;
        private WpfTextBox txtMassMigrationTime => serverSettingsView.txtMassMigrationTime;
        private WpfTextBox txtMassMigrationDisableTime => serverSettingsView.txtMassMigrationDisableTime;
        private WpfTextBox txtSpeciesMigrationTime => serverSettingsView.txtSpeciesMigrationTime;
        private WpfTextBox txtMinWeatherVariationInterval => serverSettingsView.txtMinWeatherVariationInterval;
        private WpfTextBox txtMaxWeatherVariationInterval => serverSettingsView.txtMaxWeatherVariationInterval;
        private WpfTextBox txtQueueJoinTimeoutSeconds => serverSettingsView.txtQueueJoinTimeoutSeconds;
        private WpfTextBox txtQueueHeartbeatIntervalSeconds => serverSettingsView.txtQueueHeartbeatIntervalSeconds;
        private WpfTextBox txtQueueHeartbeatTimeoutSeconds => serverSettingsView.txtQueueHeartbeatTimeoutSeconds;
        private WpfTextBox txtQueueHeartbeatMaxMisses => serverSettingsView.txtQueueHeartbeatMaxMisses;
        private WpfTextBox txtAdminSteamIds => serverSettingsView.txtAdminSteamIds;
        private WpfTextBox txtWhitelistIds => serverSettingsView.txtWhitelistIds;
        private WpfTextBox txtVipIds => serverSettingsView.txtVipIds;
        private WpfTextBox txtDiscordInvite => serverSettingsView.txtDiscordInvite;
        private WpfTextBox txtModLoaderPath => serverSettingsView.txtModLoaderPath;
        private WpfTextBox txtModDllPath => serverSettingsView.txtModDllPath;
        private WpfTextBox txtModConfigDir => serverSettingsView.txtModConfigDir;
        private WpfTextBox txtModLog => serverSettingsView.txtModLog;
        private WpfTextBox txtAutoInjectDelaySeconds => serverSettingsView.txtAutoInjectDelaySeconds;
        private RadioButton rdoInjectBuiltIn => serverSettingsView.rdoInjectBuiltIn;
        private RadioButton rdoInjectBat => serverSettingsView.rdoInjectBat;

        private CheckBox chkEnableRcon => serverSettingsView.chkEnableRcon;
        private CheckBox chkWhitelist => serverSettingsView.chkWhitelist;
        private CheckBox chkQueueEnabled => serverSettingsView.chkQueueEnabled;
        private CheckBox chkGlobalChat => serverSettingsView.chkGlobalChat;
        private CheckBox chkHumans => serverSettingsView.chkHumans;
        private CheckBox chkMutations => serverSettingsView.chkMutations;
        private CheckBox chkMigration => serverSettingsView.chkMigration;
        private CheckBox chkFallDamage => serverSettingsView.chkFallDamage;
        private CheckBox chkSpawnAI => serverSettingsView.chkSpawnAI;
        private CheckBox chkPlants => serverSettingsView.chkPlants;
        private CheckBox chkDynamicWeather => serverSettingsView.chkDynamicWeather;
        private CheckBox chkUseRegionSpawnCooldown => serverSettingsView.chkUseRegionSpawnCooldown;
        private CheckBox chkUseRegionSpawning => serverSettingsView.chkUseRegionSpawning;
        private CheckBox chkAllowRecordingReplay => serverSettingsView.chkAllowRecordingReplay;
        private CheckBox chkEnableDiets => serverSettingsView.chkEnableDiets;
        private CheckBox chkEnablePatrolZones => serverSettingsView.chkEnablePatrolZones;
        private CheckBox chkEnableMassMigration => serverSettingsView.chkEnableMassMigration;
        private CheckBox chkAutoInjectAfterRestart => serverSettingsView.chkAutoInjectAfterRestart;

        private CheckBox chkEnableLogRedpointEOSVerbose => serverSettingsView.chkEnableLogRedpointEOSVerbose;
        private CheckBox chkEnableLogOnlineVerbose => serverSettingsView.chkEnableLogOnlineVerbose;
        private CheckBox chkEnableLogOnlineGameVerbose => serverSettingsView.chkEnableLogOnlineGameVerbose;
        private CheckBox chkEnableLogNetVerbose => serverSettingsView.chkEnableLogNetVerbose;
        private CheckBox chkEnableLogNetTrafficVerbose => serverSettingsView.chkEnableLogNetTrafficVerbose;
        private CheckBox chkEnableLogReplicationGraphVerbose => serverSettingsView.chkEnableLogReplicationGraphVerbose;
        private CheckBox chkEnableLogTheIsleVerbose => serverSettingsView.chkEnableLogTheIsleVerbose;
        private CheckBox chkEnableLogTheIsleAdminVerbose => serverSettingsView.chkEnableLogTheIsleAdminVerbose;
        private CheckBox chkEnableLogTheIsleAIVerbose => serverSettingsView.chkEnableLogTheIsleAIVerbose;
        private CheckBox chkEnableLogTheIsleAnimInstanceVerbose => serverSettingsView.chkEnableLogTheIsleAnimInstanceVerbose;
        private CheckBox chkEnableLogTheIsleAudioVerbose => serverSettingsView.chkEnableLogTheIsleAudioVerbose;
        private CheckBox chkEnableLogTheIsleAuthVerbose => serverSettingsView.chkEnableLogTheIsleAuthVerbose;
        private CheckBox chkEnableLogTheIsleCharacterVerbose => serverSettingsView.chkEnableLogTheIsleCharacterVerbose;
        private CheckBox chkEnableLogTheIsleCharacterMovementVerbose => serverSettingsView.chkEnableLogTheIsleCharacterMovementVerbose;
        private CheckBox chkEnableLogTheIsleDatabaseVerbose => serverSettingsView.chkEnableLogTheIsleDatabaseVerbose;
        private CheckBox chkEnableLogTheIsleEnvironmentVerbose => serverSettingsView.chkEnableLogTheIsleEnvironmentVerbose;
        private CheckBox chkEnableLogTheIsleGameVerbose => serverSettingsView.chkEnableLogTheIsleGameVerbose;
        private CheckBox chkEnableLogTheIsleNetworkVerbose => serverSettingsView.chkEnableLogTheIsleNetworkVerbose;
        private CheckBox chkEnableLogTheIsleServerVerbose => serverSettingsView.chkEnableLogTheIsleServerVerbose;
        private CheckBox chkEnableLogTheIslePlayerControllerVerbose => serverSettingsView.chkEnableLogTheIslePlayerControllerVerbose;
        private CheckBox chkEnableLogTheIsleUIVerbose => serverSettingsView.chkEnableLogTheIsleUIVerbose;
        private CheckBox chkEnableLogTheIsleWorldVerbose => serverSettingsView.chkEnableLogTheIsleWorldVerbose;
        private CheckBox chkEnableLogTheIsleJoinDataVerbose => serverSettingsView.chkEnableLogTheIsleJoinDataVerbose;
        private CheckBox chkEnableLogTheIsleChatDataVerbose => serverSettingsView.chkEnableLogTheIsleChatDataVerbose;
        private CheckBox chkEnableLogTheIsleKillDataVerbose => serverSettingsView.chkEnableLogTheIsleKillDataVerbose;
        private CheckBox chkEnableLogTheIsleCommandDataVerbose => serverSettingsView.chkEnableLogTheIsleCommandDataVerbose;
        private CheckBox chkEnableLogTheIsleAntiCheatVerbose => serverSettingsView.chkEnableLogTheIsleAntiCheatVerbose;

        private ListBox lstDinos => serverSettingsView.lstDinos;
        private ListBox lstDisallowedAI => serverSettingsView.lstDisallowedAI;
    }
}
