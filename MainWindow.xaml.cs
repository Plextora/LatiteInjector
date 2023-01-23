using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Normal,
            FileName = "explorer.exe",
            Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App",
        };
        process.StartInfo = startInfo;
        process.Start();

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

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Launcher made by Plextora (Plextora#0033)\nLatite Client made by Imrglop (JayRSky#9295)\nSome injector code made by carlton (baseballer#4451)");
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        DiscordPresence.DiscordClient.UpdateState("Idling in the client");
        SetStatusLabel.Default();
    }
}