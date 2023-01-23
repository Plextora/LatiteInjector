using System.Windows;
using LatiteInjector.Utils;

namespace LatiteInjector;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void App_OnExit(object sender, ExitEventArgs e) => DiscordPresence.StopPresence();
}