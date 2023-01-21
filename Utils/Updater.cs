using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Updater
{
    private static string? _latestVersion;
    private const string CurrentVersion = "1.0.0";
    private static readonly WebClient? Client = new WebClient();

    public static void UpdateInjector()
    {
        try
        {
            _latestVersion =
                Client?.DownloadString(
                    "https://raw.githubusercontent.com/Plextora/LatiteUtil/master/InjectorVersion.txt");
            _latestVersion = _latestVersion?.Replace("\n", "");
        }
        catch
        {
            SetStatusLabel.Error("Failed to check latest version of injector. Are you connected to the internet?");
            return;
        }

        if (CurrentVersion == _latestVersion) return;
        var result = MessageBox.Show("The injector is outdated! Do you want to download the newest version?", "Injector outdated", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        var fileName = $"Injector_{_latestVersion}.exe";
        var path = $"./{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        Client?.DownloadFile("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe", path);
        Process.Start(fileName);
        Application.Current.Shutdown();
    }
}