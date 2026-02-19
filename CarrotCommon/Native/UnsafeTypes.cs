using System.Runtime.InteropServices;

namespace Carrot.Common.Native;

/// <summary>
/// Defining native constants and types for P/Invoke.
/// 定义用于 P/Invoke 的原生常量和类型。
/// </summary>
public static class UnsafeTypes {
    /// <summary>
    /// Extended window style.
    /// 扩展窗口样式。
    /// </summary>
    public const int GWL_EXSTYLE = -20;

    /// <summary>
    /// The window is a tool window. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
    /// 工具窗口。工具窗口不会出现在任务栏或 ALT+TAB 对话框中。
    /// </summary>
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    /// <summary>
    /// A top-level window created with this style does not become the foreground window when the user clicks it.
    /// 使用此样式创建的顶级窗口在用户单击时不会成为前台窗口。
    /// </summary>
    public const int WS_EX_NOACTIVATE = 0x8000000;

    /// <summary>
    /// Retains the current position (ignores X and Y parameters).
    /// 保持当前位置（忽略 X 和 Y 参数）。
    /// </summary>
    public const int SWP_NOMOVE = 2;

    /// <summary>
    /// Retains the current size (ignores cx and cy parameters).
    /// 保持当前大小（忽略 cx 和 cy 参数）。
    /// </summary>
    public const int SWP_NOSIZE = 1;

    /// <summary>
    /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
    /// 不激活窗口。如果未设置此标志，窗口将被激活并移动到最顶层或非最顶层组的顶部（取决于 hWndInsertAfter 参数的设置）。
    /// </summary>
    public const int SWP_NOACTIVATE = 4;

    /// <summary>
    /// Retains the current Z order (ignores the hWndInsertAfter parameter).
    /// 保持当前的 Z 顺序（忽略 hWndInsertAfter 参数）。
    /// </summary>
    public const int SWP_NOZORDER = 4;

    /// <summary>
    /// Activates the window and displays it in its current size and position.
    /// 激活窗口并以当前大小和位置显示它。
    /// </summary>
    public const int SW_SHOW = 5;

    /// <summary>
    /// Minimizes the specified window and activates the next top-level window in the Z order.
    /// 最小化指定窗口并激活 Z 顺序中的下一个顶级窗口。
    /// </summary>
    public const int SW_MINIMIZE = 6;

    /// <summary>
    /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
    /// 以最小化窗口显示窗口。该值类似于 SW_SHOWMINIMIZED，除了窗口不会被激活。
    /// </summary>
    public const int SW_SHOWNOACTIVE = 7;

    /// <summary>
    /// Sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or other window-management functions.
    /// 发送给因调用 SetWindowPos 函数或其他窗口管理函数而导致其大小、位置或 Z 顺序中的位置即将更改的窗口。
    /// </summary>
    public const int WM_WINDOWPOSCHANGING = 0x46;

    /// <summary>
    /// Sent when the effective dots per inch (dpi) for a window has changed.
    /// 当窗口的有效每英寸点数 (dpi) 发生变化时发送。
    /// </summary>
    public const int WM_DPICHANGED = 0x02E0;

    /// <summary>
    /// Posted when the user presses the left mouse button while the cursor is within the nonclient area of a window.
    /// 当光标位于窗口的非客户区内时，用户按下鼠标左键时发送。
    /// </summary>
    public const int WM_NCLBUTTONDOWN = 0xA1;

    /// <summary>
    /// In the title bar.
    /// 在标题栏中。
    /// </summary>
    public const int HT_CAPTION = 0x2;

    /// <summary>
    /// Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
    /// 将窗口放置在 Z 顺序的底部。如果 hWnd 参数标识了一个最顶层窗口，则该窗口将失去其最顶层状态并放置在所有其他窗口的底部。
    /// </summary>
    public const int HWND_BOTTOM = 1;

    /// <summary>
    /// The message is posted to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not posted to child windows.
    /// 消息发送到系统中的所有顶级窗口，包括禁用或不可见的无主窗口、重叠窗口和弹出窗口；但消息不会发送到子窗口。
    /// </summary>
    public const int HWND_BROADCAST = 0xffff;


    /// <summary>
    /// Hides the window and activates another window.
    /// 隐藏窗口并激活另一个窗口。
    /// </summary>
    public const int SW_HIDE = 0;

    /// <summary>
    /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
    /// 激活并显示一个窗口。如果窗口被最小化或最大化，系统将其恢复到原来的大小和位置。应用程序在首次显示窗口时应指定此标志。
    /// </summary>
    public const int SW_SHOWNORMAL = 1;

    /// <summary>
    /// Activates the window and displays it as a minimized window.
    /// 激活窗口并将其显示为最小化窗口。
    /// </summary>
    public const int SW_SHOWMINIMIZED = 2;

    /// <summary>
    /// Activates the window and displays it as a maximized window.
    /// 激活窗口并将其显示为最大化窗口。
    /// </summary>
    public const int SW_SHOWMAXIMIZED = 3;

    /// <summary>
    /// Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
    /// 以最近的大小和位置显示窗口。该值类似于 SW_SHOWNORMAL，除了窗口不会被激活。
    /// </summary>
    public const int SW_SHOWNOACTIVATE = 4;
}

/// <summary>
/// Contains performance information.
/// 包含性能信息。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PerformanceInformation {
    public int cb;
    public IntPtr CommitTotal;
    public IntPtr CommitLimit;
    public IntPtr CommitPeak;
    public IntPtr PhysicalTotal;
    public IntPtr PhysicalAvailable;
    public IntPtr SystemCache;
    public IntPtr KernelTotal;
    public IntPtr KernelPaged;
    public IntPtr KernelNonpaged;
    public IntPtr PageSize;
    public uint HandleCount;
    public uint ProcessCount;
    public uint ThreadCount;
}

/// <summary>
/// Contains operating system version information.
/// 包含操作系统版本信息。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct OSVersionInfoEx {
    public int dwOSVersionInfoSize;
    public uint dwMajorVersion;
    public uint dwMinorVersion;
    public uint dwBuildNumber;
    public uint dwPlatformId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
    public ushort wServicePackMajor;
    public ushort wServicePackMinor;
    public ushort wSuiteMask;
    public byte wProductType;
    public byte wReserved;
}

/// <summary>
/// Contains information about the size and position of a window.
/// 包含有关窗口大小和位置的信息。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WindowPos {
    public IntPtr hwnd;
    public IntPtr hwndInsertAfter;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public uint flags;
}

/// <summary>
/// Defines a rectangle by the coordinates of its upper-left and lower-right corners.
/// 通过左上角和右下角的坐标定义矩形。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RECT {
    public int Left, Top, Right, Bottom;

    /// <summary>
    /// Gets the width of the rectangle.
    /// 获取矩形的宽度。
    /// </summary>
    public int Width => Right - Left;

    /// <summary>
    /// Gets the height of the rectangle.
    /// 获取矩形的高度。
    /// </summary>
    public int Height => Bottom - Top;
    
    /// <inheritdoc />
    public override string ToString() => $"Left={Left},Top={Top},Right={Right},Bottom={Bottom},Width={Width},Height={Height}";
}

/// <summary>
/// Contains information about a display monitor.
/// 包含有关显示监视器的信息。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MonitorInfo {
    public uint cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;

    /// <summary>
    /// Initializes the structure.
    /// 初始化结构体。
    /// </summary>
    public void Init() {
        cbSize = (uint)Marshal.SizeOf<MonitorInfo>();
    }
}
