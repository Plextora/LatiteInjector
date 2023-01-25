using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Logging
{
    private static readonly string RoamingStateDirectory =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState";
    
    public static void ErrorLogging(Exception error)
    {
        var filePath = $@"{RoamingStateDirectory}\Latite\Latite_Injector_Error_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
        
        if (File.Exists(filePath))
            File.Create(filePath).Close();
        
        File.WriteAllText(filePath, error.ToString());

        var result = MessageBox.Show(
            "An error has occurred! Please report this error to the developers! If you don't know how to report errors, click the \"Yes\" button to visit the #bugs forum in the Discord!",
            "An unhandled error has occurred!",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);

        if (result == MessageBoxResult.Yes)
            Process.Start("discord://-/invite/2ZFsuTsfeX"); // discord protocols for the win!
    }
}