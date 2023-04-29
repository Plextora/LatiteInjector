using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;
using static LatiteInjector.MainWindow;

namespace LatiteInjector;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        ConfigSetup();
    }

    public static string ConfigFilePath =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector\\config.txt";
    private static readonly string LatiteInjectorFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector";

    private void ConfigSetup()
    {
        if (!File.Exists(ConfigFilePath))
        {
            Directory.CreateDirectory(LatiteInjectorFolder);
            File.Create(ConfigFilePath).Close();
            string defaultConfigText =
                "discordstatus:true\n" +
                "hidetotray:true\n" +
                "firstrun:true";

            File.WriteAllText(ConfigFilePath, defaultConfigText);

            // set default config values
            IsDiscordPresenceEnabled = true;
            IsHideToTrayEnabled = true;
        }
        else
            LoadConfig();
    }

    private void LoadConfig()
    {
        string config = File.ReadAllText(ConfigFilePath);
        IsDiscordPresenceEnabled = GetLine(config, 1) == "discordstatus:true";
        IsHideToTrayEnabled = GetLine(config, 2) == "hidetotray:true";
        DiscordPresenceCheckBox.IsChecked = IsDiscordPresenceEnabled;
        HideToTrayCheckBox.IsChecked = IsHideToTrayEnabled;
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
            if (!IsMinecraftRunning)
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

    private void HideToTrayCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsHideToTrayEnabled = (bool)HideToTrayCheckBox.IsChecked;
        if (IsHideToTrayEnabled)
            ModifyConfig("hidetotray:true", 2);
        else if (!IsHideToTrayEnabled)
            ModifyConfig("hidetotray:false", 2);
    }
}