using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LatiteInjector.Utils;
using Microsoft.Win32;

namespace LatiteInjector
{
    /// <summary>
    /// Interaction logic for CreditWindow.xaml
    /// </summary>
    public partial class LanguageWindow
    {
        public LanguageWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            if (!SettingsWindow.IsDiscordPresenceEnabled) return;
            if (!Injector.IsMinecraftRunning())
            {
                DiscordPresence.IdlePresence();
                return;
            }
            DiscordPresence.PlayingPresence();
        }

        private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private RadioButton _languageSelected = new();
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "XAML files (*.xaml)|*.xaml",
                RestoreDirectory = true
            };

            if (!(openFileDialog.ShowDialog() ?? false))
            {
                _languageSelected.IsChecked = true;   
                return;
            }
            CustomLanguageRadioButton.Content = openFileDialog.FileName;
            try
            {
                App.ChangeLanguage(new Uri(openFileDialog.FileName, UriKind.Absolute));
                SettingsWindow.ModifyConfig(openFileDialog.FileName, 4);
            }
            catch (Exception)
            {
                App.ChangeLanguage(new Uri("pack://application:,,,/Latite Injector;component//Translations/English.xaml", UriKind.Absolute));
                SettingsWindow.ModifyConfig("pack://application:,,,/Latite Injector;component//Translations/English.xaml", 4);
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _languageSelected = (RadioButton)sender;
            App.ChangeLanguage(new Uri($"pack://application:,,,/Latite Injector;component//Translations/{((RadioButton)sender).Content}.xaml"));
            SettingsWindow.ModifyConfig($"pack://application:,,,/Latite Injector;component//Translations/{((RadioButton)sender).Content}.xaml", 4);
        }

        private void LanguageWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement uiElement in LanguagesStackPanel.Children)
            {
                if (uiElement is not RadioButton radioButton) continue;
                if ((string)radioButton.Content != SettingsWindow.SelectedLanguage
                        .Replace("pack://application:,,,/Latite Injector;component//Translations/", "")
                        .Replace(".xaml", "")) continue;
                radioButton.IsChecked = true;
                return;
            }
            
            CustomLanguageRadioButton.IsChecked = true;
            CustomLanguageRadioButton.Content = SettingsWindow.SelectedLanguage;
        }
    }
}
