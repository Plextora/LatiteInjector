using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LatiteInjector.Installer
{
    internal class Program
    {
        public static readonly string LatiteInjectorDataFolder =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LatiteInjector";
        public static readonly string LatiteInjectorExeFolder =
            $"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\\Latite Injector";
        public static readonly string LatiteInjectorExePath =
            $@"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\Latite Injector\Latite Injector.exe";
        public static readonly string LatiteInjectorStartMenuFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Latite Injector");
        public static readonly string LatiteInjectorStartMenuShortcutPath =
            Path.Combine(LatiteInjectorStartMenuFolder, "Latite Injector.lnk");


        private static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", false);

            static void OnUnhandledException(object sender,
                UnhandledExceptionEventArgs e) =>
                Utils.ErrorDump(e.ExceptionObject as Exception);

            Console.Title = "Latite Injector Installer";

            if (!Environment.Is64BitOperatingSystem)
            {
                Utils.WriteColor(
                    "It looks like you're running a 32 bit OS/Computer.\nSadly, you cannot use Latite Client with a 32 bit OS/Computer.\nPlease do not report this as a bug, make a ticket, or ask how to switch to 64 bit in the Discord, you cannot use Latite Client AT ALL!!!",
                    ConsoleColor.Red);
                Console.ReadLine();
                Environment.Exit(1);
            }

            await Utils.InjectorAutoUpdate(args);

            if (Utils.IsLatiteInstalled())
            {
                Utils.WriteColor("Latite Injector has already been installed. Do you want to uninstall it? (Y/N)",
                    ConsoleColor.DarkGray);
                Console.Write("> ");
                string input = Console.ReadLine();
                // multiple cases because for some godforsaken reason users can't be trusted to type Y correctly
                if (input == "Y" || input == "y" || input == "yes" || input == "Yes" || input == "YES")
                {
                    if (Directory.Exists(LatiteInjectorExeFolder))
                    {
                        Directory.Delete(LatiteInjectorExeFolder, true);
                        Utils.WriteColor("\nDeleted Latite Injector .exe folder", ConsoleColor.Green);
                    }
                    if (Directory.Exists(LatiteInjectorDataFolder))
                    {
                        Directory.Delete(LatiteInjectorDataFolder, true);
                        Utils.WriteColor("Deleted Latite Injector data folder", ConsoleColor.Green);
                    }
                    string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        "Latite Injector.lnk");
                    if (File.Exists(desktopShortcut))
                    {
                        File.Delete(desktopShortcut);
                        Utils.WriteColor("Deleted Latite Injector desktop shortcut", ConsoleColor.Green);
                    }
                    if (Directory.Exists(LatiteInjectorStartMenuFolder))
                    {
                        Directory.Delete(LatiteInjectorStartMenuFolder, true);
                        Utils.WriteColor("Deleted Latite Injector Start Menu folder and shortcut", ConsoleColor.Green);
                    }

                    Utils.WriteColor($"\nLatite Injector has been uninstalled. There may be leftover Installer files downloaded by Latite Injector in your Temporary folder ({Path.GetTempPath()}).\nPress any key to close this window.", ConsoleColor.White);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                else
                    Console.Clear();
            }

            Utils.WriteColor("Welcome to the Latite Injector Installer!", ConsoleColor.White);
            Utils.WriteColor("The installer will now start..", ConsoleColor.White);
            Thread.Sleep(4000);

            Console.Clear();
            Utils.WriteColor("[1/3] Installing .NET 8\n", ConsoleColor.White);

            if (Utils.IsNet8Installed())
            {
                Utils.WriteColor(".NET 8 is already installed. Skipping .NET 8 installation", ConsoleColor.Green);
                Thread.Sleep(4000);
            }
            else
            {
                Utils.WriteColor(
                    "It looks like .NET 8, which Latite Injector needs to run, isn't installed on your system. Installing .NET 8 now..",
                    ConsoleColor.Red);
                string downloadURL =
                    "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe";
                string downloadPath = Path.Combine(Path.GetTempPath(), "dotnet8.exe");
                if (File.Exists(downloadPath))
                {
                    Utils.WriteColor("The .NET 8 installer file already exists (possibly from a previous failed install), deleting and redownloading file...", ConsoleColor.DarkGray);
                    File.Delete(downloadPath);
                }
                Utils.WriteColor("Downloading the .NET 8 installer..", ConsoleColor.Yellow);
                await Utils.DownloadFile(new Uri(downloadURL), downloadPath);
                Utils.WriteColor("Downloaded .NET 8 installer!", ConsoleColor.Green);

                Process dotnetInstaller = Process.Start(new ProcessStartInfo
                {
                    FileName = downloadPath,
                    Arguments = "/passive /norestart" // run installer quietly
                });

                dotnetInstaller?.WaitForExit();
                File.Delete(downloadPath);
                if (dotnetInstaller?.ExitCode == 0)
                {
                    Utils.WriteColor(".NET 8 has been installed!", ConsoleColor.Green);
                    Thread.Sleep(4000);
                }
                else
                {
                    Utils.WriteColor(".NET 8 installation has failed in some way. Please try manually installing .NET 8", ConsoleColor.Red);
                    Console.ReadLine();
                }
            }

            Console.Clear();
            Utils.WriteColor("[2/3] Downloading Latite Injector\n", ConsoleColor.White);

            if (!Directory.Exists(LatiteInjectorDataFolder))
            {
                Utils.WriteColor("The Latite Injector config/logging directory does not exist. Creating it now..",
                    ConsoleColor.Yellow);
                Directory.CreateDirectory(LatiteInjectorDataFolder);
                Utils.WriteColor("Created directory!", ConsoleColor.Green);
            }

            if (Directory.Exists(LatiteInjectorExeFolder) && File.Exists(LatiteInjectorExePath))
            {
                Utils.WriteColor("Latite Injector is already downloaded, deleting Latite Injector file and downloading latest version..", ConsoleColor.DarkGray);
                File.Delete(LatiteInjectorExePath);
            }
            else if (!Directory.Exists(LatiteInjectorExeFolder))
            {
                Utils.WriteColor("The Latite Injector .exe directory does not exist. Creating it now..", ConsoleColor.Yellow);
                Directory.CreateDirectory(LatiteInjectorExeFolder);
                Utils.WriteColor("Created directory!", ConsoleColor.Green);
            }

            Uri latiteInjectorLink = new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe");
            Utils.WriteColor("Downloading Latite Injector..", ConsoleColor.Yellow);
            await Utils.DownloadFile(latiteInjectorLink, LatiteInjectorExePath);
            Utils.WriteColor($"Downloaded Latite Injector to directory {LatiteInjectorExeFolder}!", ConsoleColor.Green);

            Thread.Sleep(4000);

            Console.Clear();
            Utils.WriteColor("[3/3] Extra", ConsoleColor.White);

            Utils.WriteColor(
                "The installer will now create a Desktop shortcut to Latite Injector.\nYou can delete this shortcut off of your Desktop if you don't want it.",
                ConsoleColor.White);

            Thread.Sleep(4000);

            Utils.CreateShortcut(LatiteInjectorExePath,
                "Latite Client's new and improved injector!",
                LatiteInjectorExePath,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "Latite Injector.lnk"));

            Utils.WriteColor("Added shortcut to Desktop!", ConsoleColor.Green);

            Utils.WriteColor("The installer will now create a Start Menu shortcut.", ConsoleColor.White);
            Thread.Sleep(4000);

            if (!Directory.Exists(LatiteInjectorStartMenuFolder))
            {
                Directory.CreateDirectory(LatiteInjectorStartMenuFolder);
            }
            Utils.CreateShortcut(LatiteInjectorExePath,
                "Latite Client's new and improved injector!",
                LatiteInjectorExePath,
                LatiteInjectorStartMenuShortcutPath);
            Utils.WriteColor("Added shortcut to the Start Menu!", ConsoleColor.Green);


            Utils.WriteColor(
                $"Latite Injector's installation has completed!\nLatite Injector's .exe is now located in {LatiteInjectorExePath}.",
                ConsoleColor.Green);
            Utils.WriteColor("Press any key to close this window.", ConsoleColor.White);

            Console.ReadKey();
        }
    }
}