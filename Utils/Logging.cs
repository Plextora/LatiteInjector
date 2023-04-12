using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace LatiteInjector.Utils;

public static class Logging
{
    public static readonly string RoamingStateDirectory =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState";
    private static readonly MainWindow? Form = Application.Current.Windows[3] as MainWindow;

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

    public static void LogInjection()
    {
        string URI = "https://latitelogging-1-t0943070.deta.app/api";
        string version = Form.VersionSelectionComboBox.SelectedValue.ToString().Replace("Version ", "");

        using WebClient wc = new WebClient();
        wc.Headers[HttpRequestHeader.ContentType] = "text/plain";
        wc.UploadString(URI, version);
    }
}