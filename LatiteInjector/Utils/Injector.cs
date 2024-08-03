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

    private static void ApplyAppPackages(string path)
    {
        FileInfo infoFile = new(path);
        FileSecurity fSecurity = infoFile.GetAccessControl();
        fSecurity.AddAccessRule(
            new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
                FileSystemRights.FullControl, InheritanceFlags.None,
                PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

        infoFile.SetAccessControl(fSecurity);
    }

    public static void OpenMinecraft()
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "explorer.exe",
                Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App"
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
                    Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                    if (Minecraft.MainModule?.FileVersionInfo.FileVersion != null)
                        MinecraftVersion = Minecraft.MainModule?.FileVersionInfo.FileVersion;
                    break;
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

        if (!isCompatible)
        {
            string warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess =
                $"Your Minecraft version, {MinecraftVersion}, is not in the supported versions list for Latite Client. It is VERY likely that you will run into crashes or other types of bugs! " +
                $"The supported versions are:\n{supportedVersionsString}\n\n" +
                "Look at the #announcements channel in the Discord for directions on how to change your Minecraft version to a compatible one.";

            Logging.WarnLogging(warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess);

            MessageBox.Show(
                Application.Current.MainWindow, // put messagebox on top of the main window
                warningMessageThatNobodyWillReadBecauseReadingIsForCasualsIGuess,
                "Minecraft version not supported!!",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    // App suspension code made by  (https://github.com/flarialmc/launcher/pull/7/commits/cf11941c79fe5fe64625e3e7731c7ec51dc7ed50)
    // They also have very good material on how this works and the reasoning behind it on their own project's README (https://github.com/Aetopia/AppLifecycleOptOut)
    private static void PreventAppSuspension()
    {
        if (IsMinecraftRunning())
        {
            Api.IPackageDebugSettings pPackageDebugSettings = (Api.IPackageDebugSettings)Activator.CreateInstance(
                Type.GetTypeFromCLSID(new Guid(0xb1aec16f, 0x2383, 0x4852, 0xb0, 0xe9, 0x8f, 0x0b, 0x1d, 0xc6, 0x6b,
                    0x4d)));
            uint count = 0, bufferLength = 0;
            Api.GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, IntPtr.Zero, ref bufferLength,
                IntPtr.Zero);
            IntPtr packageFullNames = Marshal.AllocHGlobal((int)(count * IntPtr.Size)),
                buffer = Marshal.AllocHGlobal((int)(bufferLength * 2));
            Api.GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, packageFullNames,
                ref bufferLength, buffer);
            for (int i = 0; i < count; i++)
            {
                pPackageDebugSettings.EnableDebugging(Marshal.PtrToStringUni(Marshal.ReadIntPtr(packageFullNames)),
                    null, null);
                packageFullNames += IntPtr.Size;
            }

            Marshal.FreeHGlobal(packageFullNames);
            Marshal.FreeHGlobal(buffer);

            Logging.WarnLogging("Disable app suspension has been enabled.");
        }
    }

    public static bool Inject(string path)
    {
        if (SettingsWindow.IsDisableAppSuspensionEnabled)
            PreventAppSuspension();

        try
        {
            // a lot of this is from https://github.com/JiayiSoftware/JiayiLauncher/blob/master/JiayiLauncher/Features/Launch/Injector.cs
            // we <3 jiayi and phase

            ApplyAppPackages(path);

            IntPtr procHandle = Api.OpenProcess(
                Api.PROCESS_CREATE_THREAD |
                Api.PROCESS_QUERY_INFORMATION |
                Api.PROCESS_VM_OPERATION |
                Api.PROCESS_VM_WRITE |
                Api.PROCESS_VM_READ,
                false, Minecraft.Id);
            if (procHandle == IntPtr.Zero)
            {
                Logging.ErrorLogging($"OpenProcess is null, Minecraft ProcId is {Minecraft.Id}");
                return false;
            }

            IntPtr loadLibraryAddress = Api.GetProcAddress(Api.GetModuleHandleW("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddress == IntPtr.Zero)
            {
                Logging.ErrorLogging("Couldn't get LoadLibraryA address, try disabling antivirus");
                return false;
            }

            IntPtr allocMemAddress = Api.VirtualAllocEx(procHandle,
                IntPtr.Zero,
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))),
                Api.MEM_COMMIT | Api.MEM_RESERVE,
                Api.PAGE_READWRITE);
            if (allocMemAddress == IntPtr.Zero)
            {
                Logging.ErrorLogging("Failed to allocate DLL memory");
                return false;
            }

            bool result = Api.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path),
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
            if (!result)
            {
                Logging.ErrorLogging("Failed to write allocated DLL memory");
                return false;
            }

            IntPtr thread = Api.CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddress,
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
            SetStatusLabel.Error("Ran into error on inject!");
            Logging.ExceptionLogging(ex);
        }

        return true;
    }

    public static async Task WaitForModules()
    {
        Application.Current.Dispatcher.Invoke(() => { SetStatusLabel.Pending("Waiting for Minecraft..."); });
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
        Application.Current.Dispatcher.Invoke(() => { SetStatusLabel.Completed("Minecraft has finished loading!"); });
    }
}
