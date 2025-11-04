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

    public static bool IsDiscordPresenceEnabled;
    public static bool IsCloseAfterInjectedEnabled;
    public static bool IsDisableAppSuspensionEnabled;
    public static string SelectedLanguage = string.Empty;
    public static bool IsLatiteBetaEnabled;
    public static bool IsLatiteDebugEnabled;
    public static string CustomDLLURL = string.Empty;

    public void ConfigSetup()
    {
        if (!File.Exists(ConfigFilePath))
        {
            Directory.CreateDirectory(LatiteInjectorFolder);
            File.Create(ConfigFilePath).Close();
            string defaultConfigText =
                "discordstatus:true\n" +
                "closeafterinjected:false\n" +
                "disableappsuspension:true\n" +
                "selectedlanguage:pack://application:,,,/Latite Injector;component//Assets/Translations/English.xaml\n" +
                "latitebeta:false\n" +
                "latitedebug:false" +
                "customdllurl:";

            File.WriteAllText(ConfigFilePath, defaultConfigText);

            // set default config values
            IsDiscordPresenceEnabled = true;
            IsCloseAfterInjectedEnabled = false;
            IsDisableAppSuspensionEnabled = true;
            SelectedLanguage = "pack://application:,,,/Latite Injector;component//Assets/Translations/English.xaml";
            IsLatiteBetaEnabled = false;
            IsLatiteDebugEnabled = false;
            CustomDLLURL = string.Empty;
        }
        else
        {
            LoadConfig();
        }
    }

    private void LoadConfig()
    {
        if (File.ReadAllLines(ConfigFilePath).Length != 7)
        {
            string defaultConfigText =
                "discordstatus:true\n" +
                "closeafterinjected:false\n" +
                "disableappsuspension:true\n" +
                "selectedlanguage:pack://application:,,,/Latite Injector;component//Assets/Translations/English.xaml\n" +
                "latitebeta:false\n" +
                "latitedebug:false\n" +
                "customdllurl:";

            File.WriteAllText(ConfigFilePath, defaultConfigText);

            // set default config values
            IsDiscordPresenceEnabled = true;
            IsCloseAfterInjectedEnabled = false;
            IsDisableAppSuspensionEnabled = true;
            SelectedLanguage = "pack://application:,,,/Latite Injector;component//Assets/Translations/English.xaml";
            IsLatiteBetaEnabled = false;
            IsLatiteDebugEnabled = false;
            CustomDLLURL = string.Empty;
        }

        string config = File.ReadAllText(ConfigFilePath);
        IsDiscordPresenceEnabled = MainWindow.GetLine(config, 1) == "discordstatus:true";
        IsCloseAfterInjectedEnabled = MainWindow.GetLine(config, 2) == "closeafterinjected:true";
        IsDisableAppSuspensionEnabled = MainWindow.GetLine(config, 3) == "disableappsuspension:true";
        SelectedLanguage = MainWindow.GetLine(config, 4)?.Replace("selectedlanguage:", "") ??
                           "pack://application:,,,/Latite Injector;component//Assets/Translations/English.xaml";
        IsLatiteBetaEnabled = MainWindow.GetLine(config, 5) == "latitebeta:true";
        IsLatiteDebugEnabled = MainWindow.GetLine(config, 6) == "latitedebug:true";
        CustomDLLURL = MainWindow.GetLine(config, 7)?.Replace("customdllurl:", "") ?? string.Empty;

        DiscordPresenceCheckBox.IsChecked = IsDiscordPresenceEnabled;
        CloseAfterInjectedCheckBox.IsChecked = IsCloseAfterInjectedEnabled;
        DisableAppSuspensionCheckBox.IsChecked = IsDisableAppSuspensionEnabled;
        LatiteBetaCheckBox.IsChecked = IsLatiteBetaEnabled;
        CustomDLLInput.Text = CustomDLLURL;
    }

    public static void ModifyConfig(string newText, int lineToEdit)
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

    private void LatiteBetaCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsLatiteBetaEnabled = (bool)LatiteBetaCheckBox.IsChecked;

        if (IsLatiteDebugEnabled)
        {
            LatiteDebugCheckBox.IsChecked = false;
            IsLatiteDebugEnabled = false;
            ModifyConfig("latitedebug:false", 6);
        }

        if (IsLatiteBetaEnabled)
        {
            MessageBoxResult result = MessageBox.Show(
                App.GetTranslation(@"WARNING: This option lets you use experimental builds of Latite Client that have a high chance of containing bugs or crashes\nDo you still want to use Latite Nightly?"),
                App.GetTranslation("Latite Nightly disclaimer"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                ModifyConfig("latitebeta:true", 5);
            else
                LatiteBetaCheckBox.IsChecked = false;
        }
        else if (!IsLatiteBetaEnabled)
            ModifyConfig("latitebeta:false", 5);
    }

    private void LatiteDebugCheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        IsLatiteDebugEnabled = (bool)LatiteDebugCheckBox.IsChecked;

        if (IsLatiteBetaEnabled)
        {
            LatiteBetaCheckBox.IsChecked = false;
            IsLatiteBetaEnabled = false;
            ModifyConfig("latitebeta:false", 5);
        }

        if (IsLatiteDebugEnabled)
        {
            MessageBoxResult result = MessageBox.Show(
                App.GetTranslation(
                    @"WARNING: This option is only meant for detailed reporting of bugs in Latite Client.\nThis version may run slower than production builds.\nDo you still want to use Latite Debug?"),
           App.GetTranslation("Latite Debug disclaimer"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                ModifyConfig("latitedebug:true", 6);
            else
                LatiteDebugCheckBox.IsChecked = false;
        }
        else if (!IsLatiteDebugEnabled)
            ModifyConfig("latitedebug:false", 6);
    }

    private void CustomDLLInput_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        bool isValid =
            Uri.TryCreate(CustomDLLInput.Text, UriKind.Absolute, out Uri uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
            uri.AbsolutePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

        if (isValid)
        {
            CustomDLLURL = CustomDLLInput.Text;
            ModifyConfig($"customdllurl:{CustomDLLURL}", 7);
        }
    }

    private void SwitchLanguageButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.LanguageWindow.Show();
        if (SettingsWindow.IsDiscordPresenceEnabled)
            DiscordPresence.LanguagesPresence();
    }
}