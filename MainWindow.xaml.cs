using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
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
    public static string MinecraftVersion = "";
    public static string LatiteFolder =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState\LatiteRecode";
    private static readonly SettingsWindow SettingsWindow = new();
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

    public static bool IsDiscordPresenceEnabled;
    public static bool IsHideToTrayEnabled;
    public static bool IsCloseAfterInjectedEnabled;

    public MainWindow()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        InitializeComponent();

        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        if (!Environment.Is64BitOperatingSystem)
        {
            MessageBox.Show(
                "It looks like you're running a 32 bit OS/Computer. Sadly, you cannot use Latite Client with a 32 bit OS/Computer. Please do not report this as a bug, make a ticket, or ask how to switch to 64 bit in the Discord, you cannot use Latite Client AT ALL!!!",
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
        DiscordPresence.InitializePresence();
        if (IsDiscordPresenceEnabled)
            DiscordPresence.DefaultPresence();
        SettingsWindow.Closing += OnClosing;
        ChangelogWindow.Closing += OnClosing;
        CreditWindow.Closing += OnClosing;
        Updater.GetInjectorChangelog();
        Updater.GetClientChangelog();

        // is this probably a bad practice? yes! do i care? no!
        System.Timers.Timer detailedPresenceTimer = new(5000);
        detailedPresenceTimer.AutoReset = true;
        detailedPresenceTimer.Elapsed += DiscordPresence.DetailedPlayingPresence;
        detailedPresenceTimer.Start();

        _notifyIcon = new NotifyIcon();
        if (GetLine(File.ReadAllText(SettingsWindow.ConfigFilePath), 4) == "firstrun:true")
        {
            _notifyIcon.BalloonTipText =
                "Latite Injector has been minimized. Click the tray icon to bring back the Latite Injector. Right click the tray icon to exit the Latite Injector";
            _notifyIcon.BalloonTipTitle = "I'm over here!";
            SettingsWindow.ModifyConfig("firstrun:false", 4);
        } /* I really need to find a better way to do this
           * with this method if you open the config file on the first run, it will say "firstrun:false"
           * even though it IS the first run. */
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

    private static void OpenGame()
    {
	    var process = new Process
	    {
		    StartInfo = new ProcessStartInfo
		    {
			    WindowStyle = ProcessWindowStyle.Normal,
			    FileName = "explorer.exe",
			    Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App"
		    }
	    };
	    
	    process.Start();
    }

    private void OnStateChanged(object sender, EventArgs args)
    {
        if (IsHideToTrayEnabled)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (IsDiscordPresenceEnabled)
                    DiscordPresence.MinimizeToTrayPresence();
                if (_notifyIcon?.BalloonTipText == null) return;
                _notifyIcon.ShowBalloonTip(2000);
                _notifyIcon.BalloonTipText = null;
                _notifyIcon.BalloonTipTitle = null;
            }
            else
                _storedWindowState = WindowState;
        }
        else if (!IsHideToTrayEnabled)
            Application.Current.Shutdown();
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args) => CheckTrayIcon();

    private void NotifyIconClick(object sender, EventArgs e)
    {
        Show();
        if (IsDiscordPresenceEnabled)
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

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
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

        OpenGame();
        
        while (true)
        {
            if (Process.GetProcessesByName("Minecraft.Windows").Length == 0)
                continue; // skip this execution of loop if true
            Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
            bool shouldGo = true;
            while (MinecraftVersion.Length == 0)
            {
                int myCount = 0;
            retry:
                myCount++;
                // this is cringe but I have no clue why its being cringe without this
                try
                {
                    MinecraftVersion = Minecraft.MainModule?.FileVersionInfo.FileVersion;
                } catch
                {
                    if (myCount > 10)
                    {
                        MessageBox.Show("Could not inject. Please try again, or inject while Minecraft is already open.");
                        return;
                    }
                    goto retry;
                }
            }

            if (shouldGo)
            {
                if (IsCustomDll)
                    await Injector.WaitForModules();
                Injector.Inject(Updater.DownloadDll());
                IsMinecraftRunning = true;

                Minecraft.EnableRaisingEvents = true;
                Minecraft.Exited += IfMinecraftExited;
            }

            return;
        }
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

        OpenGame();

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
        if (IsDiscordPresenceEnabled)
            DiscordPresence.IdlePresence();
        Application.Current.Dispatcher.Invoke(SetStatusLabel.Default);
        IsMinecraftRunning = false;
        if (IsCustomDll) IsCustomDll = false;
    }

    private void ChangelogButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChangelogWindow.Show();
        if (IsDiscordPresenceEnabled)
            DiscordPresence.ChangelogPresence();
    }

    private void CreditButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreditWindow.Show();
        if (IsDiscordPresenceEnabled)
            DiscordPresence.CreditsPresence();
    }

    private void OpenLatiteFolderLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Process.Start(LatiteFolder);

    private void SettingsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SettingsWindow.Show();
        if (IsDiscordPresenceEnabled)
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

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
    private static void MenuExitItem_Click(object sender, EventArgs e) => Application.Current.Shutdown();

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        DiscordPresence.ShutdownPresence();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
    }
}