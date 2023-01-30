using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using LatiteInjector.Utils;
using Application = System.Windows.Application;
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
    public static bool IsMinecraftRunning;
    public static bool IsCustomDll;
    public static string? CustomDllName;
    
    private NotifyIcon? _notifyIcon;
    private readonly ContextMenu _contextMenu = new();
    private readonly MenuItem _menuItem = new();
    private WindowState _storedWindowState = WindowState.Normal;

    public MainWindow()
    {
        InitializeComponent();
        
        _notifyIcon = new NotifyIcon();
        if (!File.Exists("first_run"))
        {
            _notifyIcon.BalloonTipText =
                "Latite Client has been minimized. Click the tray icon to bring back Latite Client. Right click the tray icon to exit Latite Client";
            _notifyIcon.BalloonTipTitle = "I'm over here!";
            File.Create("first_run");
        }
        else
        {
            File.Create("first_run");
            _notifyIcon.BalloonTipText = null;
            _notifyIcon.BalloonTipTitle = null;
        }
        _notifyIcon.Text = "Latite Client";
        _notifyIcon.Icon = new System.Drawing.Icon(@"..\..\..\latite.ico");
        _notifyIcon.Click += NotifyIconClick;
        
        _contextMenu.MenuItems.AddRange(new[] {_menuItem});
        _menuItem.Index = 0;
        _menuItem.Text = "Exit Latite Client";
        _menuItem.Click += MenuExitItem_Click;

        _notifyIcon.ContextMenu = _contextMenu;
        
        Updater.UpdateInjector();
        DiscordPresence.DiscordClient.Initialize();
        DiscordPresence.IdlePresence();
        ChangelogWindow.Closing += OnClosing;
        CreditWindow.Closing += OnClosing;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        Updater.GetInjectorChangelog();
    }

    private void OnStateChanged(object sender, EventArgs args)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            DiscordPresence.DiscordClient.UpdateState("Minimized to tray");
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
            DiscordPresence.DiscordClient.UpdateState("Idling in the client");
        else if (IsMinecraftRunning)
            DiscordPresence.DiscordClient.UpdateState(
                IsCustomDll
                    ? $"Playing Minecraft {Updater.GetSelectedVersion()} with {CustomDllName}"
                    : $"Playing Minecraft {Updater.GetSelectedVersion()} with Latite");
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

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;
    private void WindowToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    public static string? GetLine(string? text, int lineNo)
    {
        string?[] lines = text?.Replace("\r","").Split('\n') ?? Array.Empty<string>();
        return lines.Length >= lineNo ? lines[lineNo-1] : null;
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
        CustomDllName = CustomDllName.Replace(".dll", "");
        
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
        DiscordPresence.DiscordClient.UpdateState("Idling in the client");
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
        IsMinecraftRunning = false;
        if (IsCustomDll) IsCustomDll = false;
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

    private void DiscordIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Process.Start("discord://-/invite/zcJfXxKTA4");

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
    private static void MenuExitItem_Click(object sender, EventArgs e) => Application.Current.Shutdown();

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        DiscordPresence.StopPresence();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
    }
}