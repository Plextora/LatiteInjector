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
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (!IsMinecraftRunning)
            {
                DiscordPresence.DiscordClient.UpdateState("Idling in the injector");
                return;
            }
            DiscordPresence.DiscordClient.UpdateState(
                IsCustomDll
                    ? $"Playing Minecraft {Updater.GetSelectedVersion()} with {CustomDllName}"
                    : $"Playing Minecraft {Updater.GetSelectedVersion()} with Latite");
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
