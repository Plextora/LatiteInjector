using LatiteInjector.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Updater
{
    public const int InjectorCurrentVersion = 32;

    private static readonly Uri InjectorVersionUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/launcher_version");

    private static readonly Uri StableDllUrl =
        new("https://github.com/Imrglop/Latite-Releases/releases/latest/download/Latite.dll");
    private static readonly Uri NightlyDllUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/nightly/LatiteNightly.dll");
    private static readonly Uri DebugDllUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/debug/LatiteDebug.dll");
    private static readonly Uri DebugPdbUrl =
        new("https://github.com/LatiteClient/Latite/releases/download/debug/LatiteDebug.pdb");

    private static readonly Uri InstallerExecutableUrl =
        new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Installer.exe");
    private static readonly Uri SupportedVersionListUrl =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/supported_versions");

    public static async Task<string[]> GetSupportedVersionList()
    {
        var raw = await FileHelper.DownloadString(SupportedVersionListUrl);
        return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    private static async Task<int?> GetLatestInjectorVersion()
    {
        var raw = (await FileHelper.DownloadString(InjectorVersionUrl)).Trim();
        return int.TryParse(raw, out var version) ? version : null;
    }

    public static async Task UpdateInjector()
    {
        var latestVersion = await GetLatestInjectorVersion();
        if (latestVersion is null)
        {
            Logging.ErrorLogging("Failed to parse latest injector version.");
            return;
        }

        if (InjectorCurrentVersion >= latestVersion) return;

        MessageBoxResult result = MessageBox.Show(
            App.GetTranslation("The injector is outdated! Do you want to download the newest version?"),
            App.GetTranslation("Injector outdated!"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);

        if (result != MessageBoxResult.Yes) return;

        string fileName = $"LatiteInstaller_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.exe";
        string path = Path.Combine(Path.GetTempPath(), fileName);

        if (File.Exists(path)) File.Delete(path);

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
        if (!Directory.Exists(Logging.LatiteFolder))
            Directory.CreateDirectory(Logging.LatiteFolder);

        string stablePath = Path.Combine(Logging.LatiteFolder, "Latite.dll");
        string nightlyPath = Path.Combine(Logging.LatiteFolder, "LatiteNightly.dll");
        string debugDllPath = Path.Combine(Logging.LatiteFolder, "LatiteDebug.dll");
        string debugPdbPath = Path.Combine(Logging.LatiteFolder, "LatiteDebug.pdb");
        string customDllPath = Path.Combine(Logging.LatiteFolder, "Custom_DLL.dll");

        stablePath = await EnsureDeletable(stablePath, "Latite");
        nightlyPath = await EnsureDeletable(nightlyPath, "Latite_Nightly");
        debugDllPath = await EnsureDeletable(debugDllPath, "Latite_Debug");
        debugPdbPath = await EnsureDeletable(debugPdbPath, "Latite_Debug", ".pdb");
        customDllPath = await EnsureDeletable(customDllPath, "Custom_DLL");

        SetStatusLabel.Pending("Downloading Latite DLL");

        Settings settings = Settings.Default;

        if (!string.IsNullOrWhiteSpace(settings.CustomDllUrl)) {
            Logging.InfoLogging($"Using custom DLL URL: {settings.CustomDllUrl}");
            await FileHelper.DownloadFile(new Uri(settings.CustomDllUrl), customDllPath);
            return customDllPath;
        }

        if (settings.Nightly)
        {
            Logging.InfoLogging("Using latest Latite Nightly build");
            await FileHelper.DownloadFile(NightlyDllUrl, nightlyPath);
            return nightlyPath;
        }

        if (settings.Debug)
        {
            Logging.InfoLogging("Using latest Latite Debug build");
            await FileHelper.DownloadFile(DebugDllUrl, debugDllPath);
            await FileHelper.DownloadFile(DebugPdbUrl, debugPdbPath);
            return debugDllPath;
        }

        await FileHelper.DownloadFile(StableDllUrl, stablePath);
        return stablePath;
    }

    private static async Task<string> EnsureDeletable(string path, string baseName, string extension = ".dll")
    {
        try {
            if (await FileHelper.DeleteFile(path)) return path;
            Logging.ErrorLogging($"Failed to delete file: '{path}'");
        }
        catch (Exception ex) {
            Logging.ErrorLogging($"Error deleting '{path}': {ex.Message}");
        }

        string fallback = Path.Combine(Path.GetDirectoryName(path)!, $"{baseName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{extension}");

        return fallback;
    }
}
