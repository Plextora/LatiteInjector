using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LatiteInjector.Installer.Utils;

namespace LatiteInjector.Installer
{
    internal class Program
    {
        public static void WriteColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static async Task Main(string[] args)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                WriteColor(
                    "It looks like you're running a 32 bit OS/Computer.\nSadly, you cannot use Latite Client with a 32 bit OS/Computer.\nPlease do not report this as a bug, make a ticket, or ask how to switch to 64 bit in the Discord, you cannot use Latite Client AT ALL!!!",
                    ConsoleColor.Red);
                Console.ReadLine();
                Environment.Exit(1);
            }

            WriteColor("Welcome to the Latite Injector Installer!", ConsoleColor.White);
            WriteColor("The Installer will now start..", ConsoleColor.White);
            Thread.Sleep(2000);

            WriteColor("[1/3] Installing .NET 8", ConsoleColor.White);

            Process dotnetVersionProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            dotnetVersionProcess?.WaitForExit();
            string output = dotnetVersionProcess?.StandardOutput.ReadToEnd();
            if (output != null && !output.StartsWith("8."))
            {
                WriteColor(".NET 8 is already installed. Skipping .NET 8 installation", ConsoleColor.Green);
                Thread.Sleep(2000);
            }
            else
            {
                WriteColor(
                    "It looks like .NET 8, which Latite Injector needs to run, isn't installed on your system. Installing .NET 8 now..",
                    ConsoleColor.Red);
                string downloadURL =
                    "https://download.visualstudio.microsoft.com/download/pr/0ff148e7-bbf6-48ed-bdb6-367f4c8ea14f/bd35d787171a1f0de7da6b57cc900ef5/windowsdesktop-runtime-8.0.5-win-x64.exe";
                string downloadPath = Path.Combine(Path.GetTempPath(), "dotnet.exe");
                WriteColor("Downloading the .NET 8 installer..", ConsoleColor.Yellow);
                await Download.DownloadFile(new Uri(downloadURL), downloadPath);
            }
        }
    }
}
