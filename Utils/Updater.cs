using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils;

public static class Updater
{
    public const string InjectorCurrentVersion = "12";

    private const string INJECTOR_VERSION_URL =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/launcher_version";
    private const string DLL_VERSION_URL =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/latest_version.txt";
    private const string INJECTOR_EXECUTABLE_URL =
        "https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe";
    private const string INJECTOR_CHANGELOG_URL =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/injector_changelog";
    private const string CLIENT_CHANGELOG_URL =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/client_changelog";
    private const string GAME_VERSIONS_URL =
        "https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/game_versions";
    private static string? _selectedVersion;

    private static readonly WebClient? Client = new WebClient();
    private static readonly MainWindow? Form = Application.Current.Windows[3] as MainWindow;
    private static readonly ChangelogWindow? ChangelogForm = Application.Current.Windows[1] as ChangelogWindow;

    private static string? GetLatestInjectorVersion()
    {
        try
        {
            var latestVersion = Client?.DownloadString(
                INJECTOR_VERSION_URL);
            latestVersion = latestVersion?.Replace("\n", "");
            return latestVersion;
        }
        catch
        {
            SetStatusLabel.Error("Failed to check latest version of injector. Are you connected to the internet?");
            throw new Exception("Cannot get latest injector version!");
        }
    }

    private static string? GetLatestDllVersion()
    {
        try
        {
            var latestVersion = Client?.DownloadString(
                DLL_VERSION_URL);
            latestVersion = latestVersion?.Replace("\n", "");
            return latestVersion;
        }
        catch
        {
            SetStatusLabel.Error("Failed to check latest version of dll. Are you connected to the internet?");
            throw new Exception("Cannot get latest DLL!");
        }
    }

    public static bool IsVersionSimilar(string version1, string version2)
    {
        // version1: 1.19.63
        // version2: 1.19.63.01
        var split1 = version1.Split('.');
        var split2 = version2.Split('.');

        if (split1.Length != split2.Length + 1) return false;

        return (split1[0] == split2[0] && split1[1] == split1[1] && split1[2][0] == split2[2][0]);
    }
    
    public static void UpdateInjector()
    {
        var latestVersion = GetLatestInjectorVersion();
        
        try
        {
            if (Convert.ToInt32(InjectorCurrentVersion) >= Convert.ToInt32(latestVersion)) return;
        }
        catch
        {
            throw new Exception("Failed to convert injector current version or latest injector version");
        }

        var result =
            MessageBox.Show("The injector is outdated! Do you want to download the newest version?",
                "Injector outdated!", MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result != MessageBoxResult.Yes) return;

        var fileName = $"Injector_{latestVersion}.exe";
        var path = $"./{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        Client?.DownloadFile(INJECTOR_EXECUTABLE_URL, path);
        Process.Start(fileName);
        Application.Current.Shutdown();
    }

    public static void FetchVersionList()
    {
        if (Form != null) {
            Form.VersionSelectionComboBox.Items.Clear();
            VersionList.Clear();
            if (Client != null)
            {
                string str = Client.DownloadString(
                    GAME_VERSIONS_URL);
                if (str != null)
                {
                    str.Replace("\r", ""); // Remove RETURN so the string is clean.
                    string[] lines = str.Split('\n');

                    foreach (string line in lines)
                    {
                        VersionList.Add(line);
                        string displayStr = "Version " + line;
                        Form.VersionSelectionComboBox.Items.Add(displayStr);
                    }
                }
            }
            Form.VersionSelectionComboBox.SelectedIndex = 0;
        }
    }

    public static string GetSelectedVersion()
    {
        return VersionList[Form.VersionSelectionComboBox.SelectedIndex];
    }

    private static string? GetChangelogLine(string? changelog, int line, string changelogNum)
    {
        if (changelog != null && GetLine(changelog, line).StartsWith($"{changelogNum} "))
            return GetLine(changelog, line)?.Replace($"{changelogNum} ", "");
        return "Couldn't get changelog line";
    }
    
    private static string? GetClientChangelogLine(string? changelog, int line, string changelogNum)
    {
        if (changelog != null && GetLine(changelog, line).StartsWith($"{changelogNum} "))
            return GetLine(changelog, line)?.Replace($"{changelogNum} ", "");
        return "";
    } // temporary function until imrglop actually add more to the changelog

    public static void GetInjectorChangelog()
    {
        string? rawChangelog = null;
        
        try
        {
            rawChangelog = Client?.DownloadString(
                INJECTOR_CHANGELOG_URL);
        }
        catch
        {
            SetStatusLabel.Error("Failed to obtain injector changelog. Are you connected to the internet?");
        }
        
        if (rawChangelog == "\n")
        {
            SetStatusLabel.Error("Failed to obtain client changelog. Please report error to devs");
            throw new Exception("The injector changelog on Latite-Releases is (probably) empty");
        }

        if (ChangelogForm == null) return;
        ChangelogForm.InjectorChangelogLine1.Content = GetChangelogLine(rawChangelog, 1, "1.");
        ChangelogForm.InjectorChangelogLine2.Content = GetChangelogLine(rawChangelog, 2, "2.");
        ChangelogForm.InjectorChangelogLine3.Content = GetChangelogLine(rawChangelog, 3, "3.");
        ChangelogForm.InjectorChangelogLine4.Content = GetChangelogLine(rawChangelog, 4, "4.");
    }
    
    public static void GetClientChangelog()
    {
        string? rawChangelog = null;
        
        try
        {
            rawChangelog = Client?.DownloadString(
                CLIENT_CHANGELOG_URL);
        }
        catch
        {
            SetStatusLabel.Error("Failed to obtain client changelog. Are you connected to the internet?");
        }

        if (rawChangelog == "\n")
        {
            SetStatusLabel.Error("Failed to obtain client changelog. Please report error to devs");
            throw new Exception("The client changelog on Latite-Releases is (probably) empty");
        }

        if (ChangelogForm == null) return;
        ChangelogForm.ClientChangelogLine1.Content = GetClientChangelogLine(rawChangelog, 1, "1.");
        ChangelogForm.ClientChangelogLine2.Content = GetClientChangelogLine(rawChangelog, 2, "2.");
        ChangelogForm.ClientChangelogLine3.Content = GetClientChangelogLine(rawChangelog, 3, "3.");
        ChangelogForm.ClientChangelogLine4.Content = GetClientChangelogLine(rawChangelog, 4, "4.");
    }

    public static string DownloadDll()
    {
        var latestVersion = GetLatestDllVersion();

        _selectedVersion = GetSelectedVersion();
        
        var dllPath = $"{Path.GetTempPath()}Latite_{latestVersion}_{_selectedVersion}.dll";
        if (File.Exists(dllPath)) return dllPath;
        SetStatusLabel.Pending($"Downloading Latite's {_selectedVersion} DLL");
        Client?.DownloadFile(
            $"https://github.com/Imrglop/Latite-Releases/releases/download/{latestVersion}/Latite.{_selectedVersion}.dll",
            dllPath);

        return dllPath;
    }
}