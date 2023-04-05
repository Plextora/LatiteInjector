using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Logging
{
    public static readonly string RoamingStateDirectory =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState";
    
    public static void ErrorLogging(Exception? error)
    {
        var filePath = $@"{RoamingStateDirectory}\Latite\Latite_Injector_Error_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
        
        if (File.Exists(filePath))
            File.Create(filePath).Close();
        
        File.WriteAllText(filePath, error?.ToString());

        var result = MessageBox.Show(
            "An error has occurred! Please report this error to the developers!\nClick Yes to go to the Latite Discord.\n\n" + error?.ToString().Substring(0, 500) + "...",
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
                    FileName = "https://discord.gg/zcJfXxKTA4",
                    UseShellExecute = true
                });
        }
    }
}