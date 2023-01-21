using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils;

public static class Injector
{
    public static void Inject(string path)
    {
        // a lot of this is from https://github.com/notcarlton

        SetStatusLabel.Pending($"Injecting {path} into Minecraft!");

        try
        {
            if (Process.GetProcessesByName("Minecaft.Windows").Length == 0)
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = "explorer.exe",
                    Arguments = "shell:appsFolder\\Microsoft.MinecraftUWP_8wekyb3d8bbwe!App",
                };
                process.StartInfo = startInfo;
                process.Start();
            }

            ApplyAppPackages(path);

            var targetProcess = Process.GetProcessesByName("Minecraft.Windows")[0];

            var procHandle = Api.OpenProcess(Api.PROCESS_CREATE_THREAD | Api.PROCESS_QUERY_INFORMATION |
                                             Api.PROCESS_VM_OPERATION | Api.PROCESS_VM_WRITE | Api.PROCESS_VM_READ,
                false, targetProcess.Id);

            var loadLibraryAddress = Api.GetProcAddress(Api.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            var allocMemAddress = Api.VirtualAllocEx(procHandle, nint.Zero,
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), Api.MEM_COMMIT
                                                                          | Api.MEM_RESERVE, Api.PAGE_READWRITE);

            Api.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path),
                (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
            Api.CreateRemoteThread(procHandle, nint.Zero, 0, loadLibraryAddress,
                allocMemAddress, 0, nint.Zero);
            
            SetStatusLabel.Completed($"Injected {path} into Minecraft successfully!");
        }
        catch (Exception e)
        {
            SetStatusLabel.Error("Ran into an error while injecting!");
            MessageBox.Show(e.ToString());
        }

        void ApplyAppPackages(string path)
        {
            var infoFile = new FileInfo(path);
            var fSecurity = infoFile.GetAccessControl();
            fSecurity.AddAccessRule(
                new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
                    FileSystemRights.FullControl, InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

            infoFile.SetAccessControl(fSecurity);
        }
    }
    
    public static async Task WaitForModules()
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SetStatusLabel.Pending("Waiting for Minecraft to finish loading...");
            });
            while (true)
            {
                Minecraft?.Refresh();
                if (Minecraft is { Modules.Count: > 160 }) break;
                Thread.Sleep(4000);
            }
        });
        Application.Current.Dispatcher.Invoke(() =>
        {
            SetStatusLabel.Completed("Minecraft has finished loading!");
        });
    }
}