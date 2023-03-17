using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using LatiteInjector.Utils;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace LatiteInjector;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public static Process? Minecraft;
    private static readonly ChangelogWindow ChangelogWindow = new();
    private static readonly CreditWindow CreditWindow = new();
    private static readonly WebClient? Client = new WebClient();
    public static bool IsMinecraftRunning;
    public static bool IsCustomDll;
    public static string? CustomDllName;
    public static readonly List<string> VersionList = new();

    private NotifyIcon? _notifyIcon;
    private readonly ContextMenu _contextMenu = new();
    private readonly MenuItem _menuItem = new();
    private WindowState _storedWindowState = WindowState.Normal;

    public MainWindow()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        InitializeComponent();

        if (!Environment.Is64BitOperatingSystem)
        {
            MessageBox.Show(
                "It looks like you're running a 32 bit OS/Computer. Sadly, you cannot use Latite Client with a 32 bit OS/Computer. Please do not report this as a bug, make a ticket, or ask how to switch to 64 bit in the Discord, you cannot use Latite Client AT ALl!!!",
                "32 bit OS/Computer", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
        
        if (!FontManager.IsFontInstalled("Inter"))
        {
            var result = MessageBox.Show(
                "The font that Latite Injector uses (Inter) is not installed. Do you want to install it?",
                "Install font", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                Client?.DownloadFile("https://github.com/Imrglop/Latite-Releases/raw/main/injector/InterFont.ttf",
                    "Inter.ttf");
                FontManager.InstallFont($"{Directory.GetCurrentDirectory()}\\Inter.ttf");
                File.Delete($"{Directory.GetCurrentDirectory()}\\Inter.ttf");
            }
        }

        Updater.UpdateInjector();
        DiscordPresence.InitalizePresence();
        ChangelogWindow.Closing += OnClosing;
        CreditWindow.Closing += OnClosing;
        Updater.GetInjectorChangelog();
        Updater.GetClientChangelog();
        Updater.FetchVersionList();

        _notifyIcon = new NotifyIcon();
        if (!File.Exists("first_run"))
        {
            _notifyIcon.BalloonTipText =
                "Latite Injector has been minimized. Click the tray icon to bring back the Latite Injector. Right click the tray icon to exit the Latite Injector";
            _notifyIcon.BalloonTipTitle = "I'm over here!";
            File.Create("first_run").Close();
        }
        else
        {
            _notifyIcon.BalloonTipText = null;
            _notifyIcon.BalloonTipTitle = null;
        }

        _notifyIcon.Text = "Latite Client";
        var stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/latite.ico"))?.Stream;
        if (stream != null)
            _notifyIcon.Icon =
                new Icon(stream);
        _notifyIcon.Click += NotifyIconClick;

        _contextMenu.MenuItems.AddRange(new[] { _menuItem });
        _menuItem.Index = 0;
        _menuItem.Text = "Exit Latite Client";
        _menuItem.Click += MenuExitItem_Click;

        _notifyIcon.ContextMenu = _contextMenu;
    }

    private void OnStateChanged(object sender, EventArgs args)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            DiscordPresence.MinimizeToTrayPresence();
            if (_notifyIcon?.BalloonTipText == null) return;
            _notifyIcon.ShowBalloonTip(2000);
            _notifyIcon.BalloonTipText = null;
            _notifyIcon.BalloonTipTitle = null;
        }
        else
            _storedWindowState = WindowState;
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args) => CheckTrayIcon();

    private void NotifyIconClick(object sender, EventArgs e)
    {
        Show();
        if (!IsMinecraftRunning)
            DiscordPresence.IdlePresence();
        else if (IsMinecraftRunning)
            DiscordPresence.PlayingPresence();
        WindowState = _storedWindowState;
    }

    private void CheckTrayIcon() => ShowTrayIcon(!IsVisible);

    private void ShowTrayIcon(bool show)
    {
        if (_notifyIcon != null)
            _notifyIcon.Visible = show;
    }

    private static void OnUnhandledException(object sender,
        UnhandledExceptionEventArgs e) =>
        Logging.ErrorLogging(e.ExceptionObject as Exception);

    private void CloseButton_LeftClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void CloseButton_RightClick(object sender, RoutedEventArgs e) =>
        Application.Current.Shutdown();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    public static string? GetLine(string? text, int lineNo)
    {
        string?[] lines = text?.Replace("\r", "").Split('\n') ?? Array.Empty<string>();
        return lines.Length >= lineNo ? lines[lineNo - 1] : null;
    } // https://stackoverflow.com/a/2606405/20083929

    private async void LaunchButton_OnLeftClick(object sender, RoutedEventArgs e)
    {
        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }

        if (IsCustomDll)
            await Injector.WaitForModules();
        Injector.Inject(Updater.DownloadDll());
        IsMinecraftRunning = true;

        Minecraft.EnableRaisingEvents = true;
        Minecraft.Exited += IfMinecraftExited;
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

        CustomDllName = openFileDialog.SafeFileName;

        if (Process.GetProcessesByName("Minecaft.Windows").Length != 0) return;

        Process.Start("minecraft:");

        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0) continue;
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            break;
        }

        IsCustomDll = true;
        await Injector.WaitForModules();
        Injector.Inject(openFileDialog.FileName);
        IsMinecraftRunning = true;

        Minecraft.EnableRaisingEvents = true;
        Minecraft.Exited += IfMinecraftExited;
    }

    private static void IfMinecraftExited(object sender, EventArgs e)
    {
        DiscordPresence.IdlePresence();
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
        IsMinecraftRunning = false;
        if (IsCustomDll) IsCustomDll = false;
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChangelogWindow.Show();
        DiscordPresence.ChangelogPresence();
    }

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreditWindow.Show();
        DiscordPresence.CreditsPresence();
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

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
    private static void MenuExitItem_Click(object sender, EventArgs e) => Application.Current.Shutdown();

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        DiscordPresence.StopPresence();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
    }
}