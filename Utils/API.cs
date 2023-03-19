using System;
using System.Runtime.InteropServices;

namespace LatiteInjector.Utils;

public static class Api
{
    [DllImport("Kernel32.dll")]
    public static extern IntPtr OpenProcess(IntPtr dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("Kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
        uint flAllocationType, uint flProtect);

    [DllImport("Kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer,
        ulong nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("Kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref IntPtr lpThreadId);

    [DllImport("Kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("Kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport("Kernel32.dll")]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, ulong dwAddress, uint dwFreeType);

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hWnd);
}