using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;
using static LatiteInjector.MainWindow;

namespace LatiteInjector
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            IsDiscordPresenceEnabled = (bool)DiscordPresenceCheckBox.IsChecked;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (IsDiscordPresenceEnabled)
            {
                if (!IsMinecraftRunning)
                {
                    DiscordPresence.IdlePresence();
                    return;
                }
                DiscordPresence.PlayingPresence();
            }
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void DiscordPresenceCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            IsDiscordPresenceEnabled = (bool)DiscordPresenceCheckBox.IsChecked;
            if (IsDiscordPresenceEnabled)
                DiscordPresence.DefaultPresence();
            else if (!IsDiscordPresenceEnabled)
                DiscordPresence.StopPresence();
        }
    }
}
