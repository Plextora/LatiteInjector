using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;

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
            DiscordPresence.DiscordClient.UpdateState("Idling in the client");
        }

        private void WindowToolbar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
