using System;
using System.Runtime.InteropServices;

namespace GenshinNotifier {
    public class NativeHelper {

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

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

        // https://stackoverflow.com/questions/21884406
        // https://dev.to/mhmd_azeez/why-my-console-app-freezes-randomly-and-i-need-to-press-a-key-for-it-to-continue-44h9
        public static void AllocConsoleWithFix() {
            AllocConsole();
            FixConsoleMode();
        }

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        public static int RegisterWindowMessage(string format, params object[] args) {
            string message = string.Format(format, args);
            return RegisterWindowMessage(message);
        }

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowToFront(IntPtr window) {
            ShowWindow(window, SW_SHOWNORMAL);
            SetForegroundWindow(window);
        }

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

    }
}
