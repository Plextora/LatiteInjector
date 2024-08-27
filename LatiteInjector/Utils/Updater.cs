using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils;

public static class Updater
{
    public const string InjectorCurrentVersion = "22";

    private static readonly string LatiteInjectorDataFolder =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector";

    private static readonly Uri InjectorVersionUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/launcher_version");
    /*
    private static readonly Uri DllVersionUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/latest_version.txt");
    private static readonly Uri InjectorExecutableUrl =
        new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe");
    */
    private static readonly Uri InstallerExecutableUrl =
        new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Installer.exe");
    private static readonly Uri InjectorChangelogUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/injector_changelog");
    private static readonly Uri ClientChangelogUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/client_changelog");
    private static readonly Uri SupportedVersionList =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/supported_versions");

    private static readonly HttpClient Client = new();
    private static readonly ChangelogWindow? ChangelogForm = Application.Current.Windows[1] as ChangelogWindow;

    private static async Task DownloadFile(Uri uri, string fileName)
    {
        await using Stream asyncStream = await Client.GetStreamAsync(uri);
        await using FileStream fs = new(fileName, FileMode.CreateNew);
        await asyncStream.CopyToAsync(fs);
    }

    private static async Task<string> DownloadString(Uri uri) => await Client.GetStringAsync(uri);

    public static async Task<string[]> GetSupportedVersionList()
    {
        string rawSupportedVersions = await DownloadString(SupportedVersionList);
        string[] supportedVersionList = rawSupportedVersions.Split('\n');
        return supportedVersionList;
    }

    private static async Task<string> GetLatestInjectorVersion()
    {
        string latestVersion = await DownloadString(InjectorVersionUrl);
        latestVersion = latestVersion.Replace("\n", "");
        return latestVersion;
    }

    /*
    private static string GetLatestDllVersion()
    {
        try
        {
            string latestVersion = DownloadString(DllVersionUrl);
            latestVersion = latestVersion.Replace("\n", "");
            return latestVersion;
        }
        catch
        {
            SetStatusLabel.Error("Failed to check latest version of DLL. Are you connected to the internet?");
            throw new Exception("Cannot get latest DLL!");
        }
    }
    */
    
    public static async Task UpdateInjector()
    {
        string latestVersion = await GetLatestInjectorVersion();
        
        try
        {
            if (Convert.ToInt32(InjectorCurrentVersion) >= Convert.ToInt32(latestVersion)) return;
        }
        catch
        {
            Logging.ErrorLogging("Failed to convert injector current version or latest injector version when attempting to update.");
        }

        MessageBoxResult result =
            MessageBox.Show("The injector is outdated! Do you want to download the newest version?",
                "Injector outdated!", MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result != MessageBoxResult.Yes) return;

        string fileName = $"LatiteInstaller_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.exe";
        string path = $"{Path.GetTempPath()}{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        await DownloadFile(InstallerExecutableUrl, path);
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            Arguments = "--injectorAutoUpdate"
        });
        Application.Current.Shutdown();
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

    public static async Task GetInjectorChangelog()
    {
        string? rawChangelog = null;
        
        try
        {
            rawChangelog = await DownloadString(
                InjectorChangelogUrl);
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
    
    public static async Task GetClientChangelog()
    {
        string? rawChangelog = null;
        
        try
        {
            rawChangelog = await DownloadString(
                ClientChangelogUrl);
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

    public static async Task<string> DownloadDll()
    {
        /*
        string latestVersion = GetLatestDllVersion();

        string dllPath = $"{Path.GetTempPath()}Latite_{latestVersion}.dll";
        if (File.Exists(dllPath)) return dllPath;
        */

        string dllPath = $"{LatiteInjectorDataFolder}\\Latite.dll";
        try
        {
            if (File.Exists(dllPath)) File.Delete(dllPath);
        }
        catch (Exception ex)
        {
            Logging.ErrorLogging($"The injector ran into an error downloading the latest Latite DLL. The error is as follows: {ex.Message}");
        }

        SetStatusLabel.Pending("Downloading Latite DLL");
        await DownloadFile(
            new Uri("https://github.com/Imrglop/Latite-Releases/releases/latest/download/Latite.dll"),
            dllPath);

        return dllPath;
    }
}