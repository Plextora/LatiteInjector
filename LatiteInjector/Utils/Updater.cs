using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils;

public static class Updater
{
    public const string InjectorCurrentVersion = "24";

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
    private static readonly Uri SupportedVersionList =
        new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/supported_versions");

    private static readonly HttpClient Client = new();

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
            MessageBox.Show(App.GetTranslation("The injector is outdated! Do you want to download the newest version?"),
                App.GetTranslation("Injector outdated!"), MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result != MessageBoxResult.Yes) return;

        string fileName = $"LatiteInstaller_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.exe";
        string path = $"{Path.GetTempPath()}{fileName}";
        if (File.Exists(path))
            File.Delete(path);
        await DownloadFile(InstallerExecutableUrl, path);
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
        string betaDllPath = $"{LatiteInjectorDataFolder}\\Latite (Beta).dll";
        string betaFolderPath = $"{LatiteInjectorDataFolder}\\Latite-Release";
        string betaDllFolderPath = $@"{LatiteInjectorDataFolder}\Latite-Release\LatiteRewrite.dll";
        string betaZipPath = $"{LatiteInjectorDataFolder}\\Latite-Release.zip";

        // This whole section looks like a schizophrenic wrote it
        // but I have to do this because Windows doesn't listen when you ask
        // nicely to delete a file
        try
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
            if (File.Exists(betaDllPath))
                File.Delete(betaDllPath);
            if (File.Exists(betaZipPath))
                File.Delete(betaZipPath);

            while (File.Exists(dllPath))
                File.Delete(dllPath);
            while (File.Exists(betaDllPath))
                File.Delete(betaDllPath);
            while (File.Exists(betaZipPath))
                File.Delete(betaZipPath);

            if (File.Exists(dllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{dllPath}'.");
                dllPath = $"{LatiteInjectorDataFolder}\\Latite_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            }
            if (File.Exists(betaDllPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{betaDllPath}'.");
                betaDllPath = $"{LatiteInjectorDataFolder}\\Latite_Beta_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            }
            if (File.Exists(betaZipPath))
            {
                Logging.ErrorLogging($"Failed to delete file: '{betaZipPath}'.");
                useBeta = false;
            }
        }
        catch (Exception ex)
        {
            Logging.ErrorLogging($"The injector ran into an error downloading the latest Latite DLL. The error is as follows: {ex.Message}");
            dllPath = $"{LatiteInjectorDataFolder}\\Latite_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
            betaDllPath = $"{LatiteInjectorDataFolder}\\Latite_Beta_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.dll";
        }

        SetStatusLabel.Pending("Downloading Latite DLL");
        if (useBeta)
        {
            Logging.InfoLogging("Using latest Latite Beta (Latite-Release.zip)");
            await DownloadFile(
                new Uri("https://nightly.link/LatiteClient/Latite/workflows/releasebuild/master/Latite-Release.zip"),
                betaZipPath);
            ZipFile.ExtractToDirectory(betaZipPath, betaFolderPath);
            File.Copy(betaDllFolderPath, betaDllPath);
            Directory.Delete(betaFolderPath, true);
            File.Delete(betaZipPath);

            return betaDllPath;
        }

        await DownloadFile(
            new Uri("https://github.com/Imrglop/Latite-Releases/releases/latest/download/Latite.dll"),
            dllPath);

        return dllPath;
    }
}