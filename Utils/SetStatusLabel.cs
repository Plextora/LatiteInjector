using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LatiteInjector.Utils;

public static class SetStatusLabel
{
    private static readonly MainWindow? Form = Application.Current.Windows[1] as MainWindow;
    private static readonly Label? StatusLabel = Form?.StatusLabel;

    public static void Default()
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.AliceBlue;
        StatusLabel.Content = "Status: Idle";
    }

    public static void Pending(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.Khaki;
        StatusLabel.Content = $"Status: {statusText}";
    }

    public static void Completed(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.LightGreen;
        StatusLabel.Content = $"Status: {statusText}";
    }

    public static void Error(string statusText)
    {
        if (StatusLabel == null) return;
        StatusLabel.Foreground = Brushes.Crimson;
        StatusLabel.Content = $"Status: {statusText}";
    }
}