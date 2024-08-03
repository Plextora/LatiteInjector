using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Logging
{
    public static string LoggingFolder =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\LatiteInjector\Logs";
    public static string InjectorFolder =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\LatiteInjector";
    public static string LatiteFolder =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState\LatiteRecode";
    public static readonly string RoamingStateDirectory =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState";

    public static void ExceptionLogging(Exception? ex)
    {
        Directory.CreateDirectory(LoggingFolder);
        string filePath = $@"{LoggingFolder}\Latite_Injector_Exception_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";

        if (!File.Exists(filePath))
            File.Create(filePath).Close();
        
        File.WriteAllText(filePath, ex?.ToString());

        MessageBoxResult result = MessageBox.Show(
            "An error has occurred! Please report this error to the developers!\nClick Yes to go to the Latite Discord.\n\n" + ex?.ToString().Substring(0, 500) + "...",
            "An unhandled error has occurred!",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);

        if (result == MessageBoxResult.Yes)
        {
            if (Process.GetProcessesByName("Discord").Length > 0)
                Process.Start(new ProcessStartInfo
                {
                    FileName = "discord://-/invite/2ZFsuTsfeX",
                    UseShellExecute = true
                });
            else
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://discord.gg/2ZFsuTsfeX",
                    UseShellExecute = true
                });
        }
    }

    public static void ErrorLogging(string log)
    {
        string timestamp = $"{DateTime.Now:HH:mm:ss yyyy/MM/dd}";
        string filePath = $@"{LoggingFolder}\Latite_Injector_Log_{DateTime.Now:yyyy_MM_dd}.txt";

        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(LoggingFolder);
            File.Create(filePath).Close();
        }

        File.AppendAllLines(filePath, new[] { $"{timestamp} | ERROR: {log}" });
    }

    public static void WarnLogging(string log)
    {
        string timestamp = $"{DateTime.Now:HH:mm:ss yyyy/MM/dd}";
        string filePath = $@"{LoggingFolder}\Latite_Injector_Log_{DateTime.Now:yyyy_MM_dd}.txt";

        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(LoggingFolder);
            File.Create(filePath).Close();
        }

        File.AppendAllLines(filePath, new[] { $"{timestamp} | WARN: {log}" });
    }

    public static void InfoLogging(string log)
    {
        string timestamp = $"{DateTime.Now:HH:mm:ss yyyy/MM/dd}";
        string filePath = $@"{LoggingFolder}\Latite_Injector_Log_{DateTime.Now:yyyy_MM_dd}.txt";

        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(LoggingFolder);
            File.Create(filePath).Close();
        }

        File.AppendAllLines(filePath, new[] { $"{timestamp} | INFO: {log}" });
    }
}