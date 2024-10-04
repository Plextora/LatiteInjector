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

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e) => await Updater.UpdateInjector();

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
        SetStatusLabel.Completed(App.GetTranslation("Injected Latite Client!"));
        DiscordPresence.CurrentTimestamp = Timestamps.Now;

        Injector.Minecraft.EnableRaisingEvents = true;
        Injector.Minecraft.Exited += IfMinecraftExited;
    }

    private async void LaunchButton_OnRightClick(object sender, RoutedEventArgs e)
    {
        // The Spanish translation is SO long for some fucking reason so
        // this is a special case to try to make the text fit into the StatusLabel
        if (SettingsWindow.SelectedLanguage == "pack://application:,,,/Latite Injector;component//Assets/Translations/Spanish.xaml")
            StatusLabel.FontSize = 12;
        SetStatusLabel.Pending(App.GetTranslation("User is selecting DLL..."));

        OpenFileDialog openFileDialog = new()
        {
            Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true)
        {
            SetStatusLabel.Default();
            if (SettingsWindow.SelectedLanguage ==
                "pack://application:,,,/Latite Injector;component//Assets/Translations/Spanish.xaml")
                StatusLabel.FontSize = 15;
            return;
        }

        Injector.CustomDllName = openFileDialog.SafeFileName;

        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Injector.OpenMinecraft();
        if (SettingsWindow.SelectedLanguage ==
            "pack://application:,,,/Latite Injector;component//Assets/Translations/Spanish.xaml")
            StatusLabel.FontSize = 15;

        await Injector.InjectionPrep();

        Injector.IsCustomDll = true;
        await Injector.WaitForModules();
        Injector.Inject(openFileDialog.FileName);
        SetStatusLabel.Completed(App.GetTranslation("Injected custom DLL!"));
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

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.CreditWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.CreditsPresence();
    }
    
    private void LanguageButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.LanguageWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.LanguagesPresence();
    }

    private void OpenLatiteFolderLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        Process.Start("explorer.exe", Logging.LatiteFolder);

    private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        App.SettingsWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.SettingsPresence();
    }

    private void DiscordIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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