using LatiteInjector.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LatiteInjector.Utils;

public static class Injector
{
    public static Process Minecraft = null!;
    public static string MinecraftVersion = "";
    public static bool IsCustomDll;
    public static string? CustomDllName;

    public static void OpenMinecraft()
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "explorer.exe",
                Arguments = "minecraft:"
            }
        };

        process.Start();
    }

    public static bool IsMinecraftRunning()
    {
        Process[] minecraftProcesses = Process.GetProcessesByName("Minecraft.Windows");
        if (minecraftProcesses.Length == 0) return false;

        Minecraft = minecraftProcesses[0];
        return true;
    }

    public static async Task InjectionPrep()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                if (IsMinecraftRunning())
                {
                    try
                    {
                        if (Minecraft.MainModule?.FileName != null)
                        {
                            string FileName = Minecraft.MainModule.FileName;
                            MinecraftVersion = System.Text.RegularExpressions.Regex
                                .Match(FileName, @"\d+\.\d+\.\d+\.\d+").Value;
                            break;
                        }
                    }
                    catch (Win32Exception e)
                    {
                        if (e.NativeErrorCode != 299)
                        {
                            Logging.ErrorLogging($"Error accessing Minecraft modules: {e.Message}");
                        }
                    }
                }

                await Task.Delay(500);
            }
        });
    }

    public static async Task CheckVersionCompatibility()
    {
        string[] supportedVersions = await Updater.GetSupportedVersionList();
        string supportedVersionsString = string.Join("\n", supportedVersions).Replace("\n\n", "");

        string versionToCheck = MinecraftVersion;
        Match match = Regex.Match(MinecraftVersion, @"^(\d+\.\d+\.\d+)(?:\.\d+)?$");
        if (match.Success)
        {
            versionToCheck = match.Groups[1].Value;
        }

        bool hasValidFormat = Regex.IsMatch(versionToCheck, @"^\d+\.\d+\.\d+$");

        bool isCompatible = hasValidFormat &&
                            supportedVersions.Any(v =>
                                string.Equals(v.Trim(), versionToCheck, StringComparison.Ordinal));

        if (!isCompatible && !Settings.Default.Nightly && !Settings.Default.Debug)
        {
            string warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess =
                $"Your Minecraft version, {MinecraftVersion}, is not in the supported versions list for Latite Client. It is VERY likely that you will run into crashes or other types of bugs! " +
                $"The supported versions are:\n{supportedVersionsString}\n\n" +
                "Look at the #announcements channel in the Discord for directions on how to change your Minecraft version to a compatible one.";
            Logging.WarnLogging(warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess);

            MessageBox.Show(
                Application.Current.MainWindow,
                App.GetTranslation(
                    "Your Minecraft version, {0}, is not in the supported versions list for Latite Client. It is VERY likely that you will run into crashes or other types of bugs! The supported versions are:\\n{1}\\n\\nLook at the #announcements channel in the Discord for directions on how to change your Minecraft version to a compatible one.",
                    [MinecraftVersion, supportedVersionsString]),
                App.GetTranslation("Minecraft version not supported!!"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    public static bool Inject(string path)
    {
        try
        {
            // a lot of this is from https://github.com/JiayiSoftware/JiayiLauncher/blob/master/JiayiLauncher/Features/Launch/Injector.cs
            // we <3 jiayi and phase

            IntPtr procHandle = WinAPI.OpenProcess(
                WinAPI.PROCESS_CREATE_THREAD |
                WinAPI.PROCESS_QUERY_INFORMATION |
                WinAPI.PROCESS_VM_OPERATION |
                WinAPI.PROCESS_VM_WRITE |
                WinAPI.PROCESS_VM_READ,
                false, Minecraft.Id);
            if (procHandle == IntPtr.Zero)
            {
                Logging.ErrorLogging($"OpenProcess is null, Minecraft ProcId is {Minecraft.Id}");
                return false;
            }

            IntPtr loadLibraryAddress = WinAPI.GetProcAddress(WinAPI.GetModuleHandleW("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddress == IntPtr.Zero)
            {
                Logging.ErrorLogging("Couldn't get LoadLibraryA address, try disabling antivirus");
                return false;
            }

            IntPtr allocMemAddress = WinAPI.VirtualAllocEx(procHandle,
                IntPtr.Zero,
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))),
                WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                WinAPI.PAGE_READWRITE);
            if (allocMemAddress == IntPtr.Zero)
            {
                Logging.ErrorLogging("Failed to allocate DLL memory");
                return false;
            }

            bool result = WinAPI.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path),
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
            if (!result)
            {
                Logging.ErrorLogging("Failed to write allocated DLL memory");
                return false;
            }

            IntPtr thread = WinAPI.CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddress,
                allocMemAddress, 0, IntPtr.Zero);
            if (thread == IntPtr.Zero)
            {
                Logging.ErrorLogging("Failed to create remote thread");
                return false;
            }

            if (Settings.Default.DiscordPresence)
                DiscordPresence.PlayingPresence();
            if (Settings.Default.CloseAfterInjected)
                Application.Current.Shutdown();
        }
        catch (Exception? ex)
        {
            SetStatusLabel.Error(App.GetTranslation("Ran into error on inject!"));
            Logging.ExceptionLogging(ex);
        }

        return true;
    }
}