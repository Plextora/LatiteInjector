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
    public static readonly LanguageWindow LanguageWindow = new();

    private void App_OnStartup(object sender, StartupEventArgs startupEventArgs)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

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
        LanguageWindow.Closing += OnClosing;

        ChangeLanguage(new Uri(SettingsWindow.SelectedLanguage, UriKind.Absolute));
        
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    public static void ChangeLanguage(Uri uri) => Current.Resources.MergedDictionaries[0].Source = uri;

    // this function is hot dogshit that is in desperate need of a refactor
    // actually the entire translation system is probably in need of a complete rewrite from scratch.
    public static string GetTranslation(string input, string[]? args = null)
    {
        try
        {
            if (args is not null)
            {
                string? temp = (string)Current.TryFindResource(input);
                for (var i = 0; i < args.Length; i++)
                {
                    temp = temp.Replace($"{{{i}}}", args[i]);
                }
                // the replace is needed here since stuff like unhandled exception message
                // have their newlines escaped by default
                return temp.Replace("\\n", "\n");
            }
            var result = Current.TryFindResource(input);
            if (result is not null)
                return (string)result;
            return input;
        }
        catch (Exception)
        {
            return input;
        }
    }
    
    private static void OnUnhandledException(object sender,
        UnhandledExceptionEventArgs ex) =>
        Logging.ExceptionLogging(ex.ExceptionObject as Exception);

    private static void OnClosing(object sender, CancelEventArgs e) => e.Cancel = true;
}