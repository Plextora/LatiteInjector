using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Updater
{
    public const string InjectorCurrentVersion = "28";

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
    private static readonly Uri DllDownloadUrl =
        new("https://github.com/Imrglop/Latite-Releases/releases/latest/download/Latite.dll");
    private static readonly Uri NightlyDownloadUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/nightly/LatiteNightly.dll");
    private static readonly Uri DebugDllDownloadUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/debug/LatiteDebug.dll");
    private static readonly Uri DebugPdbDownloadUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/debug/LatiteDebug.pdb");
    private static readonly Uri InstallerExecutableUrl =
        new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Installer.exe");
    private static readonly Uri SupportedVersionList =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/supported_versions");

    public static async Task<string[]> GetSupportedVersionList()
    {
        string rawSupportedVersions = await FileHelper.DownloadString(SupportedVersionList);
        string[] supportedVersionList = rawSupportedVersions.Split('\n');
        return supportedVersionList;
    }

    private static async Task<string> GetLatestInjectorVersion()
    {
        string latestVersion = await FileHelper.DownloadString(InjectorVersionUrl);
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
            MessageBox.Show(App.GetTranslation("The injector is outdated! Do you want to download the newest version?"),
                App.GetTranslation("Injector outdated!"), MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result != MessageBoxResult.Yes) return;

        string fileName = $"LatiteInstaller_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.exe";
        string path = $"{Path.GetTempPath()}{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        await FileHelper.DownloadFile(InstallerExecutableUrl, path);
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "runas",
            Arguments = "--injectorAutoUpdate"
        });
        Application.Current.Shutdown();
    }

    public static async Task<string> DownloadDll()
    {
        /*
        string latestVersion = GetLatestDllVersion();

        string dllPath = $"{Path.GetTempPath()}Latite_{latestVersion}.dll";
        if (File.Exists(dllPath)) return dllPath;
        */

        string dllPath = $"{LatiteInjectorDataFolder}\\Latite.dll";

        bool useBeta = SettingsWindow.IsLatiteBetaEnabled;
        string betaDllPath = $"{LatiteInjectorDataFolder}\\LatiteNightly.dll";

        bool useDebug = SettingsWindow.IsLatiteDebugEnabled;
        string debugDllPath = $"{Logging.LatiteFolder}\\LatiteDebug.dll";
        string debugPdbPath = $"{Logging.LatiteFolder}\\LatiteDebug.pdb";

        string customDllUrl = SettingsWindow.CustomDLLURL;
        string customDllPath = $"{Logging.LatiteFolder}\\Custom_DLL.dll";

        // This whole section looks like a schizophrenic wrote it
        // but I have to do this because Windows doesn't listen when you ask
        // nicely to delete a file
        try
        {
            if (!await FileHelper.DeleteFile(dllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{dllPath}'");
                dllPath = $"{LatiteInjectorDataFolder}\\Latite_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            }
            if (!await FileHelper.DeleteFile(betaDllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{betaDllPath}'");
                betaDllPath = $"{LatiteInjectorDataFolder}\\Latite_Beta_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            }
            if (!await FileHelper.DeleteFile(debugDllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{debugDllPath}'");
                debugDllPath = $"{Logging.LatiteFolder}\\Latite_Debug_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            }
            if (!await FileHelper.DeleteFile(debugDllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{debugPdbPath}'");
                debugPdbPath = $"{Logging.LatiteFolder}\\Latite_Debug_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdb";
            }
            if (!await FileHelper.DeleteFile(customDllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{customDllPath}'");
                customDllPath = $"{Logging.LatiteFolder}\\Latite_Debug_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdb";
            }
        }
        catch (Exception ex)
        {
            Logging.ErrorLogging(
                $"The injector ran into an error downloading the latest Latite DLL. The error is as follows: {ex.Message}");
            dllPath = $"{LatiteInjectorDataFolder}\\Latite_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            betaDllPath = $"{LatiteInjectorDataFolder}\\Latite_Beta_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            debugDllPath = $"{Logging.LatiteFolder}\\Latite_Debug_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            debugPdbPath = $"{Logging.LatiteFolder}\\Latite_Debug_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.pdb";
            customDllPath = $"{LatiteInjectorDataFolder}\\Custom_DLL_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
        }

        SetStatusLabel.Pending("Downloading Latite DLL");
        if (!string.IsNullOrEmpty(customDllUrl))
        {
            Logging.InfoLogging($"Using custom DLL URL: {customDllUrl}");
            await FileHelper.DownloadFile(new Uri(customDllUrl), customDllPath);
            return customDllPath;
        }
        else if (useBeta)
        {
            Logging.InfoLogging("Using latest Latite Nightly build (LatiteNightly.dll)");
            await FileHelper.DownloadFile(NightlyDownloadUrl, betaDllPath);
            return betaDllPath;
        }
        else if (useDebug)
        {
            Logging.InfoLogging("Using latest Latite Debug build (LatiteDebug.dll)");
            await FileHelper.DownloadFile(DebugDllDownloadUrl, debugDllPath);
            await FileHelper.DownloadFile(DebugPdbDownloadUrl, debugPdbPath);
            return debugDllPath;
        }

        await FileHelper.DownloadFile(DllDownloadUrl, dllPath);
        return dllPath;
    }
}