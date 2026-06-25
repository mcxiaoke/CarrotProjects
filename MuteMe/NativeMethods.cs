using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MuteMe {
    public static class NativeMethods {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static string? GetForegroundProcessName() {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) {
                return null;
            }

            GetWindowThreadProcessId(hWnd, out var processId);
            try {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            } catch {
                return null;
            }
        }

        public static string NormalizeProcessName(string processName) {
            return processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? processName[..^4]
                : processName;
        }
    }
}
