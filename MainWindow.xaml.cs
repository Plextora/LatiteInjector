using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;

namespace LatiteInjector;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static Process? Minecraft;
    private static readonly ChangelogWindow ChangelogWindow = new();
    private static readonly CreditWindow CreditWindow = new();

    public MainWindow()
    {
        InitializeComponent();
        Updater.UpdateInjector();
        DiscordPresence.DiscordClient.Initialize();
        DiscordPresence.IdlePresence();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private async void LaunchButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }

        await Injector.WaitForModules();
        Injector.Inject(Updater.DownloadDll());

        Minecraft.EnableRaisingEvents = true;
        Minecraft.Exited += IfMinecraftExited;
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        DiscordPresence.DiscordClient.UpdateState("Idling in the client");
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChangelogWindow.Show();
        DiscordPresence.DiscordClient.UpdateState("Reading the changelog");
    }
    
    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreditWindow.Show();
        DiscordPresence.DiscordClient.UpdateState("Reading the credits");
    }
}