using System.Windows;
using System.Windows.Controls;

namespace IsleServerLauncher
{
    public partial class ServerSettingsView : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty RconEnabledProperty =
            DependencyProperty.Register(
                nameof(RconEnabled),
                typeof(bool),
                typeof(ServerSettingsView),
                new PropertyMetadata(false));

        public bool RconEnabled
        {
            get => (bool)GetValue(RconEnabledProperty);
            set => SetValue(RconEnabledProperty, value);
        }

        public ServerSettingsView()
        {
            InitializeComponent();
        }

        private MainWindow? OwnerWindow => Window.GetWindow(this) as MainWindow;

        private void btnBrowseModLoader_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnBrowseModLoader_Click(sender, e);
        }

        private void btnBrowseModDll_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnBrowseModDll_Click(sender, e);
        }

        private void btnBrowseModConfig_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnBrowseModConfig_Click(sender, e);
        }

        private void btnAutoFillModPaths_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnAutoFillModPaths_Click(sender, e);
        }

        private void btnInjectMod_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnInjectMod_Click(sender, e);
        }

        private void btnOpenModConfig_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnOpenModConfig_Click(sender, e);
        }

        private void btnOpenAiAdminUi_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.btnOpenAiAdminUi_Click(sender, e);
        }

        private void chkAutoInjectAfterRestart_Checked(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.chkAutoInjectAfterRestart_Checked(sender, e);
        }

        private void chkAutoInjectAfterRestart_Unchecked(object sender, RoutedEventArgs e)
        {
            OwnerWindow?.chkAutoInjectAfterRestart_Unchecked(sender, e);
        }

        private void txtAutoInjectDelaySeconds_TextChanged(object sender, TextChangedEventArgs e)
        {
            OwnerWindow?.txtAutoInjectDelaySeconds_TextChanged(sender, e);
        }
    }
}
