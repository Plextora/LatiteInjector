using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;
using static LatiteInjector.MainWindow;

namespace LatiteInjector
{
    /// <summary>
    /// Interaction logic for CreditWindow.xaml
    /// </summary>
    public partial class CreditWindow
    {
        public CreditWindow()
        {
            InitializeComponent();
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
    }
}
