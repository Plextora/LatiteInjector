using System.IO;
using System.Windows;
using System.Windows.Input;
using LatiteInjector.Utils;
using static LatiteInjector.MainWindow;

namespace LatiteInjector
{
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

        private void ConfigSetup()
        {
            if (!File.Exists("config.txt"))
            {
                File.Create("config.txt").Close();
                string defaultConfigText =
                    "discordstatus:true\n" +
                    "hidetotray:true\n" +
                    "data:true";
                
                File.WriteAllText("config.txt", defaultConfigText);

                // set default config values
                IsDiscordPresenceEnabled = true;
                IsHideToTrayEnabled = true;
                IsLoggingEnabled = true;
            }
            else
                LoadConfig();
        }

        private void LoadConfig()
        {
            string config = File.ReadAllText("config.txt");
            IsDiscordPresenceEnabled = GetLine(config, 1) == "discordstatus:true";
            IsHideToTrayEnabled = GetLine(config, 2) == "hidetotray:true";
            IsLoggingEnabled = GetLine(config, 3) == "data:true";
            DiscordPresenceCheckBox.IsChecked = IsDiscordPresenceEnabled;
            HideToTrayCheckBox.IsChecked = IsHideToTrayEnabled;
            LogInjectionCheckBox.IsChecked = IsLoggingEnabled;
        }

        private void ModifyConfig(string newText, int lineToEdit)
        {
            string[] arrLine = File.ReadAllLines("config.txt");
            arrLine[lineToEdit - 1] = newText;
            File.WriteAllLines("config.txt", arrLine);
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

        private void LogInjectionCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            IsLoggingEnabled = (bool)LogInjectionCheckBox.IsChecked;
            if (IsLoggingEnabled)
                ModifyConfig("data:true", 3);
            else if (!IsLoggingEnabled)
                ModifyConfig("data:false", 3);
        }
    }
}
