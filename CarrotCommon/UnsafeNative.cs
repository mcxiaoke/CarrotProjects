using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CarrotCommon {

    internal static class ERROR {
        public const int ACCESS_DENIED = 0x0005;
        public const int INVALID_HANDLE = 0x0006;
        public const int INVALID_PARAMETER = 0x0057;
        public const int INSUFFICIENT_BUFFER = 0x007A;
    }

    internal static class UnsafeNative {
        public static IntPtr InvalidIntPtr = (IntPtr)(-1);
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport("user32.dll")]
        public static extern int GetClassName(HandleRef hwnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

        public static StringBuilder GetModuleFileNameLong() {
            var hModule = NullHandleRef;
            StringBuilder buffer = new StringBuilder(260);
            int noOfTimes = 1;
            int length = 0;
            // Iterating by allocating chunk of memory each time we find the length is not sufficient.
            // Performance should not be an issue for current MAX_PATH length due to this change.
            while (((length = GetModuleFileName(hModule, buffer, buffer.Capacity)) == buffer.Capacity)
                && Marshal.GetLastWin32Error() == ERROR.INSUFFICIENT_BUFFER
                && buffer.Capacity < short.MaxValue) {
                noOfTimes += 2; // Increasing buffer size by 520 in each iteration.
                int capacity = noOfTimes * 260 < short.MaxValue ? noOfTimes * 260 : short.MaxValue;
                buffer.EnsureCapacity(capacity);
            }

            buffer.Length = length;
            return buffer;
        }
    }
}