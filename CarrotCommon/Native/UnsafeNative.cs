using System.Runtime.InteropServices;
using System.Text;
using static Carrot.Common.Native.UnsafeTypes;

namespace Carrot.Common.Native;

internal static class NativeError {
    public const int ACCESS_DENIED = 0x0005;
    public const int INVALID_HANDLE = 0x0006;
    public const int INVALID_PARAMETER = 0x0057;
    public const int INSUFFICIENT_BUFFER = 0x007A;
}

/// <summary>
/// Provides access to native methods from user32, kernel32, gdi32, etc.
/// 提供对 user32、kernel32、gdi32 等原生方法的访问。
/// </summary>
public static class UnsafeNative {
    /// <summary>
    /// Represents an invalid handle value (-1).
    /// 表示无效的句柄值 (-1)。
    /// </summary>
    public static readonly IntPtr InvalidIntPtr = (IntPtr)(-1);

    /// <summary>
    /// Represents a null handle reference.
    /// 表示空句柄引用。
    /// </summary>
    public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

    public delegate bool EnumMonitorProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT rcMonitor, IntPtr data);

    public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int dx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hDC, IntPtr clipRect, EnumMonitorProc proc, IntPtr data);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo info);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int index, int value);

    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool GetPerformanceInfo(ref PerformanceInformation pi, int size);

    [DllImport("kernel32.dll")]
    public static extern bool GetVersionEx(ref OSVersionInfoEx versionInfo);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    // https://www.cnblogs.com/hbccdf/p/csharp_debug_induction.html
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void OutputDebugString(string message);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    /// <summary>
    /// Gets the actual screen resolution in pixels.
    /// 获取屏幕的实际分辨率（像素）。
    /// </summary>
    public static (int Width, int Height) GetDisplayRealSize() {
        IntPtr primary = GetDC(IntPtr.Zero);
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;
        int actualPixelsX = GetDeviceCaps(primary, DESKTOPHORZRES);
        int actualPixelsY = GetDeviceCaps(primary, DESKTOPVERTRES);
        ReleaseDC(IntPtr.Zero, primary);
        return (actualPixelsX, actualPixelsY);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private const uint ENABLE_QUICK_EDIT = 0x0040;

    // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
    private const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    /// <summary>
    /// Disables Quick Edit mode for the console to prevent freezing.
    /// 禁用控制台的快速编辑模式以防止冻结。
    /// </summary>
    public static bool FixConsoleMode() {
        IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
        // get current console mode
        if (!GetConsoleMode(consoleHandle, out uint consoleMode)) {
            // ERROR: Unable to get console mode.
            return false;
        }
        // Clear the quick edit bit in the mode flags
        consoleMode &= ~ENABLE_QUICK_EDIT;

        // set the new mode
        if (!SetConsoleMode(consoleHandle, consoleMode)) {
            // ERROR: Unable to set console mode
            return false;
        }
        return true;
    }

    /// <summary>
    /// Allocates a console and fixes the Quick Edit mode issue.
    /// 分配控制台并修复快速编辑模式问题。
    /// </summary>
    public static void AllocConsoleWithFix() {
        AllocConsole();
        FixConsoleMode();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int RegisterWindowMessage(string message);

    public static int RegisterWindowMessage(string format, params object[] args) {
        string message = string.Format(format, args);
        return RegisterWindowMessage(message);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// Shows the window and brings it to the front.
    /// 显示窗口并将其带到前台。
    /// </summary>
    public static void ShowToFront(IntPtr window) {
        ShowWindow(window, SW_SHOWNORMAL);
        SetForegroundWindow(window);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int ShowWindow(IntPtr hWnd, uint Msg);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string? windowTitle);

    public const int GWL_HWNDPARENT = -8;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(HandleRef hwnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

    /// <summary>
    /// Gets the full path of the module file, supporting long paths.
    /// 获取模块文件的完整路径，支持长路径。
    /// </summary>
    public static StringBuilder GetModuleFileNameLong() {
        var hModule = NullHandleRef;
        StringBuilder buffer = new StringBuilder(260);
        int noOfTimes = 1;
        int length = 0;
        // Iterating by allocating chunk of memory each time we find the length is not sufficient.
        // Performance should not be an issue for current MAX_PATH length due to this change.
        while ((length = GetModuleFileName(hModule, buffer, buffer.Capacity)) == buffer.Capacity
            && Marshal.GetLastWin32Error() == NativeError.INSUFFICIENT_BUFFER
            && buffer.Capacity < short.MaxValue) {
            noOfTimes += 2; // Increasing buffer size by 520 in each iteration.
            int capacity = noOfTimes * 260 < short.MaxValue ? noOfTimes * 260 : short.MaxValue;
            buffer.EnsureCapacity(capacity);
        }

        buffer.Length = length;
        return buffer;
    }
}