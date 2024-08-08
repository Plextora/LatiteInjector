using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DiscordRPC;
using LatiteInjector.Utils;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LatiteInjector;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        await Updater.UpdateInjector();
        await Updater.GetInjectorChangelog();
        await Updater.GetClientChangelog();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        Application.Current.Shutdown();

    private void MinimizeButton_OnClick(object sender, MouseButtonEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    public static string? GetLine(string? text, int lineNo)
    {
        string?[] lines = text?.Replace("\r", "").Split('\n') ?? Array.Empty<string>();
        return lines.Length >= lineNo ? lines[lineNo - 1] : null;
    } // https://stackoverflow.com/a/2606405/20083929

    private async void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Injector.OpenMinecraft();

        await Injector.InjectionPrep();
        await Injector.CheckVersionCompatibility();

        Injector.Inject(await Updater.DownloadDll());
        SetStatusLabel.Completed("Injected Latite Client!");
        DiscordPresence.CurrentTimestamp = Timestamps.Now;

        Injector.Minecraft.EnableRaisingEvents = true;
        Injector.Minecraft.Exited += IfMinecraftExited;
    }

    private async void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        SetStatusLabel.Pending("User is selecting DLL...");

        OpenFileDialog openFileDialog = new()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true)
        {
            SetStatusLabel.Default();
            return;
        }

        Injector.CustomDllName = openFileDialog.SafeFileName;

        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Injector.OpenMinecraft();

        await Injector.InjectionPrep();

        Injector.IsCustomDll = true;
        await Injector.WaitForModules();
        Injector.Inject(openFileDialog.FileName);
        SetStatusLabel.Completed("Injected custom DLL!");
        DiscordPresence.CurrentTimestamp = Timestamps.Now;

        Injector.Minecraft.EnableRaisingEvents = true;
        Injector.Minecraft.Exited += IfMinecraftExited;
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        if (SettingsWindow.IsDiscordPresenceEnabled)
        {
            DiscordPresence.CurrentTimestamp = Timestamps.Now;
            DiscordPresence.IdlePresence();
        }
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
        if (Injector.IsCustomDll) Injector.IsCustomDll = false;
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.ChangelogWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.ChangelogPresence();
    }

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.CreditWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.CreditsPresence();
    }

    private void OpenLatiteFolderLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        Process.Start("explorer.exe", Logging.LatiteFolder);

    private void SettingsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        App.SettingsWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.SettingsPresence();
    }

    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Process.GetProcessesByName("Discord").Length > 0)
            Process.Start(new ProcessStartInfo
            {
                FileName = "discord://-/invite/zcJfXxKTA4",
                UseShellExecute = true
            });
        else
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/zcJfXxKTA4",
                UseShellExecute = true
            });
    }

    private void Window_Closing(object sender, CancelEventArgs e) => DiscordPresence.ShutdownPresence();
}