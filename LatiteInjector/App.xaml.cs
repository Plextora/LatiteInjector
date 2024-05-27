using LatiteInjector.Utils;
using System.Globalization;
using System.Threading;
using System;
using System.Windows;
using System.ComponentModel;

namespace LatiteInjector;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static readonly SettingsWindow SettingsWindow = new();
    public static readonly ChangelogWindow ChangelogWindow = new();
    public static readonly CreditWindow CreditWindow = new();

    private void App_OnStartup(object sender, StartupEventArgs startupEventArgs)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", false);

        if (!Environment.Is64BitOperatingSystem)
        {
            MessageBox.Show(
                "It looks like you're running a 32 bit OS/Computer. Sadly, you cannot use Latite Client with a 32 bit OS/Computer. Please do not report this as a bug, make a ticket, or ask how to switch to 64 bit in the Discord, you cannot use Latite Client AT ALL!!!",
                "32 bit OS/Computer", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }

        SettingsWindow.ConfigSetup();

        DiscordPresence.InitializePresence();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.DefaultPresence();

        // is this probably a bad practice? yes! do i care? no!
        System.Timers.Timer detailedPresenceTimer = new(5000);
        detailedPresenceTimer.AutoReset = true;
        detailedPresenceTimer.Elapsed += DiscordPresence.DetailedPlayingPresence;
        detailedPresenceTimer.Start();

        SettingsWindow.Closing += OnClosing;
        ChangelogWindow.Closing += OnClosing;
        CreditWindow.Closing += OnClosing;

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private static void OnUnhandledException(object sender,
        UnhandledExceptionEventArgs ex) =>
        Logging.ExceptionLogging(ex.ExceptionObject as Exception);

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}