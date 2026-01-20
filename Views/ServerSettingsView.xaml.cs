using System.Windows;
using System.Windows.Controls;

namespace IsleServerLauncher
{
    public partial class ServerSettingsView : UserControl
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
    }
}
