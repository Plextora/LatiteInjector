using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LatiteInjector.Utils;

public static class SetStatusLabel
{
    private static readonly MainWindow? Form = Application.Current.Windows[3] as MainWindow;
    private static readonly Label? StatusLabel = Form?.StatusLabel;

    public static void Default()
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.White;
        StatusLabel.Content = App.GetTranslation("Status: Idle");
    }

    public static void Pending(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.Khaki;
        StatusLabel.Content = App.GetTranslation("Status: {0}", [statusText]);
    }

    public static void Completed(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.LightGreen;
        StatusLabel.Content = App.GetTranslation("Status: {0}", [statusText]);
    }

    public static void Error(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.Crimson;
        StatusLabel.Content = App.GetTranslation("Status: {0}", [statusText]);
    }
}