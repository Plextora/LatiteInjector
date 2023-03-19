using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils
{
    internal static class Injector
    {

        public static bool Inject(string path, string application)
        {
            SetStatusLabel.Pending($"Injecting {path} into Minecraft!");

            var procs = Process.GetProcessesByName(application);
            if (procs.Length == 0)
            {
                MessageBox.Show("Minecraft is not open!");
                return false;
            }

            var proc = procs[0];
            var procId = proc.Id;
            var hProc = Api.OpenProcess((IntPtr)2035711, false, (uint)procId);
            if (hProc == IntPtr.Zero) return false;

            var loc = Api.VirtualAllocEx(hProc, IntPtr.Zero, (uint)(path.Length + 1), 12288, 64);
            if (loc == IntPtr.Zero)
                MessageBox.Show("Could not allocate!");
            IntPtr _;
            if (!Api.WriteProcessMemory(hProc, loc, Marshal.StringToHGlobalUni(path), (ulong)(path.Length * 2 + 2),
                    out _)) MessageBox.Show("Could not write process memory!");
            var hThread = Api.CreateRemoteThread(hProc, IntPtr.Zero, 0,
                Api.GetProcAddress(Api.GetModuleHandle("Kernel32.dll"), "LoadLibraryW"), loc, 0, ref _);
            if (!IsCustomDll)
                SetStatusLabel.Completed("Injected Latite Client into Minecraft successfully!");
            else if (IsCustomDll)
                SetStatusLabel.Completed($"Injected {CustomDllName} into Minecraft successfully!");

            Thread.Sleep(500); // good enough for now

            Api.VirtualFreeEx(hProc, loc, 0, 0x8000 /*fully release*/);

            if (hThread == IntPtr.Zero)
            {
                MessageBox.Show("Could not create remote thread!");
                return false;
            }

            Api.CloseHandle(hThread);

            Api.CloseHandle(hProc);

            return true;
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
}