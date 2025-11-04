using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
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

    private static bool IsInjected(string dllPath)
    {
        Minecraft.Refresh();
        return Minecraft.Modules.Cast<ProcessModule>().Any(m => m.FileName == dllPath);
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
                        // TODO: Fix version detection, FileVersionInfo.FileVersion seems to be null
                        if (Minecraft.MainModule?.FileName != null)
                        {
                            string FileName = Minecraft.MainModule.FileName;
                            MinecraftVersion = System.Text.RegularExpressions.Regex.Match(FileName, @"\d+\.\d+\.\d+\.\d+").Value;
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

        // Partially check if MinecraftVersion matches currently supported versions list
        bool isCompatible =
            supportedVersionsString.Contains(MinecraftVersion.Substring(0,
                MinecraftVersion.LastIndexOf(".",
                    StringComparison.Ordinal)));

        if (!isCompatible && !SettingsWindow.IsLatiteBetaEnabled)
        {
            string warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess =
                $"Your Minecraft version, {MinecraftVersion}, is not in the supported versions list for Latite Client. It is VERY likely that you will run into crashes or other types of bugs! " +
                $"The supported versions are:\n{supportedVersionsString}\n\n" +
                "Look at the #announcements channel in the Discord for directions on how to change your Minecraft version to a compatible one.";
            Logging.WarnLogging(warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess);

            MessageBox.Show(
                Application.Current.MainWindow, // put messagebox on top of the main window
                App.GetTranslation("Your Minecraft version, {0}, is not in the supported versions list for Latite Client. It is VERY likely that you will run into crashes or other types of bugs! The supported versions are:\\n{1}\\n\\nLook at the #announcements channel in the Discord for directions on how to change your Minecraft version to a compatible one.", [MinecraftVersion, supportedVersionsString]),
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

            if (SettingsWindow.IsDiscordPresenceEnabled)
                DiscordPresence.PlayingPresence();
            if (SettingsWindow.IsCloseAfterInjectedEnabled)
                Application.Current.Shutdown();
        }
        catch (Exception? ex)
        {
            SetStatusLabel.Error(App.GetTranslation("Ran into error on inject!"));
            Logging.ExceptionLogging(ex);
        }

        return true;
    }

    public static async Task WaitForModules()
    {
        Application.Current.Dispatcher.Invoke(() => { SetStatusLabel.Pending(App.GetTranslation("Waiting for Minecraft...")); });
        await Task.Run(() =>
        {
            while (true)
            {
                Minecraft.Refresh();

                try
                {
                    if (!IsMinecraftRunning()) break;
                    if (Minecraft.Modules.Count > 165) break;
                }
                catch (Win32Exception e)
                {
                    if (e.NativeErrorCode != 0)
                    {
                        if (!IsMinecraftRunning()) break;

                        throw;
                    }
                }

                Task.Delay(500);
            }
        });
        Application.Current.Dispatcher.Invoke(() => { SetStatusLabel.Completed(App.GetTranslation("Minecraft has finished loading!")); });
    }

    // Not sure if this is necessary, gdk apps seem to use a launcher before the actual process spawns
    public static async Task WaitForMinecraft()
    {
        Process? minecraft = null;

        while (minecraft == null)
        {
            minecraft = Process.GetProcessesByName("Minecraft.Windows").FirstOrDefault();
            await Task.Delay(100);
        }

        while (minecraft.MainWindowHandle == IntPtr.Zero)
        {
            minecraft.Refresh();
            await Task.Delay(100);
        }

        Minecraft = minecraft;
        await Task.Delay(100);
    }
}
