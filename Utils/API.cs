using System.Runtime.InteropServices;

namespace LatiteInjector.Utils;

public static class Api
{
    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION = 0x2;

    public const int PROCESS_CREATE_THREAD = 0x0002;
    public const int PROCESS_QUERY_INFORMATION = 0x0400;
    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_VM_WRITE = 0x0020;
    public const int PROCESS_VM_READ = 0x0010;

    public const uint MEM_COMMIT = 0x00001000;
    public const uint MEM_RESERVE = 0x00002000;
    public const uint PAGE_READWRITE = 4;

    [DllImport("user32.dll")]
    public static extern int SendMessage(nint hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("kernel32.dll")]
    public static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern nint GetProcAddress(nint hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    public static extern nint VirtualAllocEx(nint hProcess, nint lpAddress, uint dwSize,
        uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, uint nSize,
        out nuint lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    public static extern nint CreateRemoteThread(nint hProcess, nint lpThreadAttributes, uint dwStackSize,
        nint lpStartAddress, nint lpParameter, uint dwCreationFlags, nint lpThreadId);
}