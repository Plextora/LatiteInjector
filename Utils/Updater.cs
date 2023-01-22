﻿using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Updater
{
    private const string CurrentVersion = "v1.2.3";
    private static string? _selectedVersion;
    private const string InjectorVersionUrl =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/latest_version.txt";
    private const string InjectorExecutableUrl =
        "https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe";

    private static readonly WebClient? Client = new WebClient();
    private static readonly MainWindow? Form = Application.Current.Windows[0] as MainWindow;

    private static string? GetLatestVersion()
    {
        try
        {
            var latestVersion = Client?.DownloadString(
                InjectorVersionUrl);
            latestVersion = latestVersion?.Replace("\n", "");
            return latestVersion;
        }
        catch
        {
            SetStatusLabel.Error("Failed to check latest version of injector. Are you connected to the internet?");
            return "Couldn't get latest version";
        }
    }
    
    public static void UpdateInjector()
    {
        var latestVersion = GetLatestVersion();
        
        if (CurrentVersion == latestVersion) return;
        var result = MessageBox.Show("The injector is outdated! Do you want to download the newest version?", "Injector outdated", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        var fileName = $"Injector_{latestVersion}.exe";
        var path = $"./{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        Client?.DownloadFile(InjectorExecutableUrl, path);
        Process.Start(fileName);
        Application.Current.Shutdown();
    }

    public static string DownloadDll()
    {
        var latestVersion = GetLatestVersion();
        
        _selectedVersion = Form?.VersionSelectionComboBox.SelectedIndex switch
        {
            0 => "1.19.51",
            1 => "1.18.12",
            2 => "1.18",
            3 => "1.17.41",
            _ => _selectedVersion
        }; // woah cool switch expression

        var dllPath = $"{Path.GetTempPath()}Latite_{latestVersion}_{_selectedVersion}.dll";
        if (File.Exists(dllPath)) return dllPath;
        SetStatusLabel.Pending($"Downloading Latite's {_selectedVersion} DLL");
        Client?.DownloadFile(
            $"https://github.com/Imrglop/Latite-Releases/releases/download/{latestVersion}/Latite.{_selectedVersion}.dll",
            dllPath);

        return dllPath;
    }
}