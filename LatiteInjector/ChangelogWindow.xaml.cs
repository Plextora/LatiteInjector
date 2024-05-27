using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;
using static LatiteInjector.MainWindow;

namespace LatiteInjector
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow
    {
        public ChangelogWindow()
        {
            InitializeComponent();
            InjectorVersionLabel.Content = $"Injector version: {Updater.InjectorCurrentVersion}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (SettingsWindow.IsDiscordPresenceEnabled)
            {
                if (!Injector.IsMinecraftRunning())
                {
                    DiscordPresence.IdlePresence();
                    return;
                }
                DiscordPresence.PlayingPresence();
            }
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
