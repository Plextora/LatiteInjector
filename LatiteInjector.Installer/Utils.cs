﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LatiteInjector.Installer
{
    public class Utils
    {
        // carlton you were right fuck com what the fuck is this
        
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        private static HttpClient _client = new();

        public static void ErrorDump(Exception err)
        {
            if (!File.Exists("err.txt")) File.Create("err.txt").Close();

            File.WriteAllText("err.txt", err.ToString());

            Console.WriteLine("Wrote error to err.txt! (same directory this exe is in) please report this error to the developers in the Discord server!\nMAKE SURE TO SEND THE err.txt FILE WHEN REPORTING!!!!!!!!!!!!!!!!");
            Console.ReadKey();
        }

        public static async Task InjectorAutoUpdate(string[] args)
        {
            foreach (string arg in args)
            {
                if (!arg.Contains("--injectorAutoUpdate")) continue;

                Uri latiteInjectorLink =
                    new("https://github.com/Imrglop/Latite-Releases/raw/main/injector/Injector.exe");
                Uri latiteInjectorVersion =
                    new("https://raw.githubusercontent.com/Imrglop/Latite-Releases/main/launcher_version");

                string versionNumber = await Utils.DownloadString(latiteInjectorVersion);
                versionNumber = versionNumber.Replace("\n", "");

                Utils.WriteColor("The Latite Injector update will begin in 5 seconds...", ConsoleColor.White);
                Thread.Sleep(5000);

                if (Directory.Exists(Program.LatiteInjectorExeFolder) && File.Exists(Program.LatiteInjectorExePath))
                {
                    Utils.WriteColor(
                        "Deleting current Latite Injector version..",
                        ConsoleColor.Yellow);
                    File.Delete(Program.LatiteInjectorExePath);
                }
                else
                {
                    Utils.WriteColor(
                        "For some reason you don't have Latite Injector downloaded?? (at least not in the path it should be)\n" + 
                        "Cancelling auto-update and proceeding with regular install process", ConsoleColor.Red);
                    return;
                }

                Utils.WriteColor($"Downloading Latite Injector (version {versionNumber})..", ConsoleColor.Yellow);
                await Utils.DownloadFile(latiteInjectorLink, Program.LatiteInjectorExePath);
                Utils.WriteColor($"Downloaded Latite Injector to directory {Program.LatiteInjectorExeFolder}!",
                    ConsoleColor.Green);

                Utils.WriteColor($"Latite Injector has been successfully updated to version {versionNumber}!", ConsoleColor.Green);
                Utils.WriteColor("Press any key to open Latite Injector.", ConsoleColor.White);

                Console.ReadKey();
                Process.Start(Program.LatiteInjectorExePath);
                Environment.Exit(0);
            }
        }

        public static void WriteColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static async Task DownloadFile(Uri uri, string fileName)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            using Stream asyncStream = await _client.GetStreamAsync(uri);
            using FileStream fs = new(fileName, FileMode.CreateNew);
            await asyncStream.CopyToAsync(fs);
        }

        private static async Task<string> DownloadString(Uri uri)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            return await _client.GetStringAsync(uri);
        }

        private static bool CheckRegistry(RegistryView view, string path, string versionPrefix)
        {
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using RegistryKey subKey = baseKey.OpenSubKey(path);
                if (subKey != null && subKey.GetSubKeyNames().Any(v => v.StartsWith(versionPrefix)))
                    return true;
            }
            catch (Exception)
            { }

            return false;
        }

        // this just WILL NOT work so im just gonna attempt .net 8 installation every time
        /*
        public static bool IsNet8Installed()
        {
            const string versionPrefix = "8.";

            const string runtimePath64 =
                @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App";
            const string sdkPath64 = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sdk";

            if (CheckRegistry(RegistryView.Registry64, runtimePath64, versionPrefix)) return true;
            if (CheckRegistry(RegistryView.Registry64, sdkPath64, versionPrefix)) return true;

            if (CheckRegistry(RegistryView.Registry32, runtimePath64, versionPrefix)) return true;
            if (CheckRegistry(RegistryView.Registry32, sdkPath64, versionPrefix)) return true;

            return false;
        }
        */

        public static bool IsLatiteInstalled() => File.Exists(Program.LatiteInjectorExePath);

        public static void CreateShortcut(string shortcutPath, string shortcutDescription, string shortcutIconPath, string targetPath)
        {
            IShellLink link = (IShellLink)new ShellLink();

            // setup shortcut information
            link.SetPath(shortcutPath);
            link.SetDescription(shortcutDescription);
            link.SetIconLocation(shortcutIconPath, 0);

            // save it
            IPersistFile file = link as IPersistFile;
            file.Save(targetPath, false);
        } // https://stackoverflow.com/a/14632782
    }
}
