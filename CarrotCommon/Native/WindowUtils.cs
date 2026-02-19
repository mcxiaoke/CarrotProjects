using System;
using static Carrot.Common.Native.UnsafeTypes;
using static Carrot.Common.Native.UnsafeNative;

namespace Carrot.Common.Native;

/// <summary>
/// Provides utility methods for window management.
/// 提供窗口管理的实用程序方法。
/// </summary>
public static class WindowUtils {

    /// <summary>
    /// Makes the window a tool window (hidden from Alt-Tab and taskbar) and non-activatable.
    /// 将窗口设置为工具窗口（从 Alt-Tab 和任务栏中隐藏）且不可激活。
    /// Reference: https://stackoverflow.com/questions/6804251
    /// </summary>
    /// <param name="handle">The window handle. 窗口句柄。</param>
    public static void MakeWindowSpecial(IntPtr handle) {
        int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
        exStyle |= WS_EX_TOOLWINDOW;
        exStyle |= WS_EX_NOACTIVATE;
        SetWindowLong(handle, GWL_EXSTYLE, exStyle);
    }

    /// <summary>
    /// Sets common styles for the window (no activate, bottommost).
    /// 设置窗口的常用样式（不激活，最底层）。
    /// </summary>
    /// <param name="hwnd">The window handle. 窗口句柄。</param>
    public static void SetCommonStyles(IntPtr hwnd) {
        SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        SetWindowPos(hwnd, new IntPtr(HWND_BOTTOM), 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);
    }

    /// <summary>
    /// Shows the window always on the desktop (behind icons).
    /// 将窗口始终显示在桌面（图标后面）。
    /// </summary>
    /// <param name="hwnd">The window handle. 窗口句柄。</param>
    public static void ShowAlwaysOnDesktop(IntPtr hwnd) {
        var progmanHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
        var workerWHandle = IntPtr.Zero;
        
        EnumWindows(new EnumWindowsProc((topHandle, topParamHandle) => {
            IntPtr shellHandle = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellHandle != IntPtr.Zero) {
                workerWHandle = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
            }
            return true;
        }), IntPtr.Zero);
        
        workerWHandle = workerWHandle == IntPtr.Zero ? progmanHandle : workerWHandle;
        SetParent(hwnd, workerWHandle);
    }

    /// <summary>
    /// Special hack to draw behind Desktop Icons in Windows.
    /// 在 Windows 桌面图标后面绘制的特殊 Hack。
    /// Reference: https://www.codeproject.com/Articles/856020/Draw-behind-Desktop-Icons-in-Windows
    /// </summary>
    /// <param name="hwnd">The window handle. 窗口句柄。</param>
    public static void ShowBehindDesktopIcons(IntPtr hwnd) {
        // Send 0x052C to Progman. This message directs Progman to spawn a 
        // WorkerW behind the desktop icons. If it is already there, nothing happens.
        var progmanHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
        SendMessage(progmanHandle, 0x052C, 0x0000000D, 0);
        SendMessage(progmanHandle, 0x052C, 0x0000000D, 1);
    }
}