using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;

namespace LatiteInjector;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public static string ConfigFilePath =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector\\config.txt";
    private static readonly string LatiteInjectorFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector";

    public static bool IsCloseAfterInjectedEnabled;
    public static bool IsDiscordPresenceEnabled;
    public static bool IsDisableAppSuspensionEnabled;

    public void ConfigSetup()
    {
        if (!File.Exists(ConfigFilePath))
        {
            Directory.CreateDirectory(LatiteInjectorFolder);
            File.Create(ConfigFilePath).Close();
            string defaultConfigText =
                "discordstatus:true\n" +
                "closeafterinjected:false\n" +
                "disableappsuspension:true\n";

            File.WriteAllText(ConfigFilePath, defaultConfigText);

            // set default config values
            IsDiscordPresenceEnabled = true;
            IsCloseAfterInjectedEnabled = false;
            IsDisableAppSuspensionEnabled = true;
        }
        else
        {
            LoadConfig();
        }
    }

    private void LoadConfig()
    {
        string config = File.ReadAllText(ConfigFilePath);
        IsDiscordPresenceEnabled = MainWindow.GetLine(config, 1) == "discordstatus:true";
        IsCloseAfterInjectedEnabled = MainWindow.GetLine(config, 2) == "closeafterinjected:true";
        IsDisableAppSuspensionEnabled = MainWindow.GetLine(config, 3) == "disableappsuspension:true";
        DiscordPresenceCheckBox.IsChecked = IsDiscordPresenceEnabled;
        CloseAfterInjectedCheckBox.IsChecked = IsCloseAfterInjectedEnabled;
        DisableAppSuspensionCheckBox.IsChecked = IsDisableAppSuspensionEnabled;
    }

    public void ModifyConfig(string newText, int lineToEdit)
    {
        string[] arrLine = File.ReadAllLines(ConfigFilePath);
        arrLine[lineToEdit - 1] = newText;
        File.WriteAllLines(ConfigFilePath, arrLine);
    } // https://stackoverflow.com/a/35496185

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        if (IsDiscordPresenceEnabled)
        {
            if (!Injector.IsMinecraftRunning())
            {
                DiscordPresence.IdlePresence();
                return;
            }
            DiscordPresence.PlayingPresence();
        }
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void DiscordPresenceCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsDiscordPresenceEnabled = (bool)DiscordPresenceCheckBox.IsChecked;
        if (IsDiscordPresenceEnabled)
        {
            DiscordPresence.DefaultPresence();
            ModifyConfig("discordstatus:true", 1);
        }
        else if (!IsDiscordPresenceEnabled)
        {
            DiscordPresence.StopPresence();
            ModifyConfig("discordstatus:false", 1);
        }
    }

    private void CloseAfterInjectedCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsCloseAfterInjectedEnabled = (bool)CloseAfterInjectedCheckBox.IsChecked;
        if (IsCloseAfterInjectedEnabled)
            ModifyConfig("closeafterinjected:true", 2);
        else if (!IsCloseAfterInjectedEnabled)
            ModifyConfig("closeafterinjected:false", 2);
    }

    private void DisableAppSuspensionCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsDisableAppSuspensionEnabled = (bool)DisableAppSuspensionCheckBox.IsChecked;
        if (IsDisableAppSuspensionEnabled)
            ModifyConfig("disableappsuspension:true", 3);
        else if (!IsDisableAppSuspensionEnabled)
            ModifyConfig("disableappsuspension:false", 3);
    }
}