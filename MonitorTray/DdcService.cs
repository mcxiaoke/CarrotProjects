using System.Management;
using System.Runtime.InteropServices;

namespace MonitorControlTray;

/// <summary>
/// DDC/CI 显示器控制服务
/// <para>使用 HMONITOR 直接调用 High Level Monitor Configuration API，失败时回退到 WMI</para>
/// </summary>
internal static class DdcService {
    #region Native Methods

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(
        IntPtr hMonitor, uint dwPhysicalMonitorArraySize,
        [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetMonitorBrightness(
        IntPtr hMonitor, out uint pdwMinimum, out uint pdwCurrent, out uint pdwMaximum);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetMonitorBrightness(
        IntPtr hMonitor, uint dwNewBrightness);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetMonitorContrast(
        IntPtr hMonitor, out uint pdwMinimum, out uint pdwCurrent, out uint pdwMaximum);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetMonitorContrast(
        IntPtr hMonitor, uint dwNewContrast);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetVCPFeatureAndVCPFeatureReply(
        IntPtr hMonitor, byte bVCPCode, out uint pvct,
        out uint pdwCurrentValue, out uint pdwMaximumValue);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetVCPFeature(
        IntPtr hMonitor, byte bVCPCode, uint dwNewValue);

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PHYSICAL_MONITOR {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    #endregion

    #region Constants

    private const byte VCP_BRIGHTNESS = 0x10;
    private const byte VCP_CONTRAST = 0x12;

    #endregion

    #region Public API

    /// <summary>
    /// 设置所有显示器的亮度和对比度
    /// </summary>
    /// <param name="brightness">亮度值（0-100）</param>
    /// <param name="contrast">对比度值（0-100）</param>
    /// <returns>成功设置的显示器数量</returns>
    public static int SetBrightnessAndContrast(int brightness, int contrast) {
        var physicalHandles = GetPhysicalMonitorHandles();

        if (physicalHandles.Count == 0) {
            Program.Log("DDC: 未检测到显示器，回退到 WMI 控制亮度");
            return SetBrightnessViaWmi(brightness) ? 1 : 0;
        }

        Program.Log($"DDC: 检测到 {physicalHandles.Count} 个显示器");
        int successCount = 0;

        try {
            foreach (var handle in physicalHandles) {
                Program.Log($"DDC: 开始设置显示器 (handle: {handle})");
                bool brightnessOk = SetBrightnessValue(handle, brightness);
                bool contrastOk = SetContrastValue(handle, contrast);

                Program.Log($"DDC: 设置结果 - 亮度: {brightnessOk}, 对比度: {contrastOk}");

                if (brightnessOk || contrastOk) {
                    successCount++;
                } else {
                    int error = Marshal.GetLastWin32Error();
                    Program.Log($"DDC: 显示器设置失败 (Win32 错误: {error})");
                }
            }
        } finally {
            foreach (var handle in physicalHandles) {
                DestroyPhysicalMonitor(handle);
            }
        }

        if (successCount == 0) {
            Program.Log("DDC: 所有显示器设置失败，回退到 WMI 控制亮度");
            return SetBrightnessViaWmi(brightness) ? 1 : 0;
        }

        return successCount;
    }

    /// <summary>
    /// 获取第一个显示器的当前亮度和对比度
    /// </summary>
    /// <param name="brightness">当前亮度值</param>
    /// <param name="contrast">当前对比度值</param>
    /// <returns>是否成功获取</returns>
    public static bool GetBrightnessAndContrast(out int brightness, out int contrast) {
        brightness = 0;
        contrast = 0;

        var physicalHandles = GetPhysicalMonitorHandles();

        try {
            if (physicalHandles.Count == 0) {
                if (GetBrightnessViaWmi(out brightness)) {
                    contrast = 50;
                    Program.Log("WMI: 对比度无法通过 WMI 获取，使用默认值 50");
                    return true;
                }
                return false;
            }

            bool brightnessOk = GetBrightnessValue(physicalHandles[0], out brightness);
            bool contrastOk = GetContrastValue(physicalHandles[0], out contrast);

            if (!brightnessOk && !contrastOk) {
                if (GetBrightnessViaWmi(out brightness)) {
                    contrast = 50;
                    return true;
                }
                return false;
            }

            return brightnessOk || contrastOk;
        } finally {
            foreach (var handle in physicalHandles) {
                DestroyPhysicalMonitor(handle);
            }
        }
    }

    /// <summary>
    /// 检测显示器数量
    /// </summary>
    /// <returns>检测到的显示器数量</returns>
    public static int DetectMonitorCount() {
        var physicalHandles = GetPhysicalMonitorHandles();

        try {
            if (physicalHandles.Count > 0)
                return physicalHandles.Count;
        } finally {
            foreach (var handle in physicalHandles) {
                DestroyPhysicalMonitor(handle);
            }
        }

        try {
            using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBrightness");
            using var results = searcher.Get();
            int count = results.Count;
            Program.Log($"WMI: 检测到 {count} 个支持亮度控制的显示器");
            return count;
        } catch (Exception ex) {
            Program.Log($"WMI: 检测显示器数量失败 - {ex.Message}");
            return 0;
        }
    }

    #endregion

    #region Monitor Handle Enumeration

    /// <summary>
    /// 获取所有物理显示器句柄（通过 HMONITOR → PHYSICAL_MONITOR）
    /// </summary>
    private static List<IntPtr> GetPhysicalMonitorHandles() {
        var handles = new List<IntPtr>();

        MonitorEnumProc callback = (IntPtr hMonitor, IntPtr _, ref RECT _, IntPtr _) => {
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count)) {
                int error = Marshal.GetLastWin32Error();
                Program.Log($"DDC: GetNumberOfPhysicalMonitorsFromHMONITOR 失败 (错误: {error})");
                return true;
            }

            if (count == 0) {
                Program.Log("DDC: HMONITOR 没有关联的物理显示器");
                return true;
            }

            var physicalMonitors = new PHYSICAL_MONITOR[count];
            if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors)) {
                int error = Marshal.GetLastWin32Error();
                Program.Log($"DDC: GetPhysicalMonitorsFromHMONITOR 失败 (错误: {error})");
                return true;
            }

            foreach (var pm in physicalMonitors) {
                handles.Add(pm.hPhysicalMonitor);
                Program.Log($"DDC: 物理显示器: {pm.szPhysicalMonitorDescription}");
            }

            return true;
        };

        if (!EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero)) {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: EnumDisplayMonitors 失败 (错误: {error})");
        }

        GC.KeepAlive(callback);

        if (handles.Count > 0) {
            Program.Log($"DDC: 枚举到 {handles.Count} 个物理显示器");
        }

        return handles;
    }

    #endregion

    #region Brightness & Contrast (High Level API)

    /// <summary>
    /// 设置亮度值
    /// </summary>
    private static bool SetBrightnessValue(IntPtr handle, int value) {
        Program.Log($"DDC: SetBrightnessValue 入口 (handle: {handle}, value: {value})");

        // 优先使用 High Level API
        if (GetMonitorBrightness(handle, out uint min, out _, out uint max)) {
            Program.Log($"DDC: 亮度范围 {min}-{max}");

            uint scaledValue = max > min
                ? (uint)Math.Round(min + (value / 100.0) * (max - min))
                : (uint)value;

            if (SetMonitorBrightness(handle, scaledValue)) {
                return true;
            }

            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: SetMonitorBrightness 失败 (错误: {error}, 值: {scaledValue}), 尝试 VCP 方式");
        } else {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: GetMonitorBrightness 失败 (错误: {error}), 尝试 VCP 方式");
        }

        // 回退到 VCP 方式
        return SetVcpValue(handle, VCP_BRIGHTNESS, value);
    }

    /// <summary>
    /// 获取亮度值（已缩放到 0-100）
    /// </summary>
    private static bool GetBrightnessValue(IntPtr handle, out int value) {
        value = 0;

        // 优先使用 High Level API
        if (GetMonitorBrightness(handle, out uint min, out uint current, out uint max)) {
            value = max > min
                ? (int)Math.Round((current - min) * 100.0 / (max - min))
                : (int)current;
            return true;
        }

        int error = Marshal.GetLastWin32Error();
        Program.Log($"DDC: GetMonitorBrightness 失败 (错误: {error}), 尝试 VCP 方式");

        // 回退到 VCP 方式
        return GetVcpValue(handle, VCP_BRIGHTNESS, out value);
    }

    /// <summary>
    /// 设置对比度值
    /// </summary>
    private static bool SetContrastValue(IntPtr handle, int value) {
        Program.Log($"DDC: SetContrastValue 入口 (handle: {handle}, value: {value})");

        // 优先使用 High Level API
        if (GetMonitorContrast(handle, out uint min, out _, out uint max)) {
            Program.Log($"DDC: 对比度范围 {min}-{max}");

            uint scaledValue = max > min
                ? (uint)Math.Round(min + (value / 100.0) * (max - min))
                : (uint)value;

            if (SetMonitorContrast(handle, scaledValue)) {
                return true;
            }

            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: SetMonitorContrast 失败 (错误: {error}), 尝试 VCP 方式");
        }

        // 回退到 VCP 方式
        return SetVcpValue(handle, VCP_CONTRAST, value);
    }

    /// <summary>
    /// 获取对比度值（已缩放到 0-100）
    /// </summary>
    private static bool GetContrastValue(IntPtr handle, out int value) {
        value = 0;

        // 优先使用 High Level API
        if (GetMonitorContrast(handle, out uint min, out uint current, out uint max)) {
            value = max > min
                ? (int)Math.Round((current - min) * 100.0 / (max - min))
                : (int)current;
            return true;
        }

        // 回退到 VCP 方式
        return GetVcpValue(handle, VCP_CONTRAST, out value);
    }

    #endregion

    #region VCP (Low Level API)

    /// <summary>
    /// 设置 VCP 功能值
    /// </summary>
    private static bool SetVcpValue(IntPtr handle, byte vcpCode, int value) {
        Program.Log($"DDC: SetVcpValue 入口 (handle: {handle}, VCP: 0x{vcpCode:X2}, value: {value})");

        if (!GetVCPFeatureAndVCPFeatureReply(handle, vcpCode, out _, out _, out uint maxValue)) {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: GetVCPFeature 失败 (VCP: 0x{vcpCode:X2}, 错误: {error})");
            return false;
        }

        Program.Log($"DDC: VCP 0x{vcpCode:X2} 最大值: {maxValue}");

        uint scaledValue = maxValue > 0
            ? (uint)Math.Round(value / 100.0 * maxValue)
            : (uint)value;

        if (!SetVCPFeature(handle, vcpCode, scaledValue)) {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: SetVCPFeature 失败 (VCP: 0x{vcpCode:X2}, 值: {scaledValue}, 错误: {error})");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取 VCP 功能值（已缩放到 0-100）
    /// </summary>
    private static bool GetVcpValue(IntPtr handle, byte vcpCode, out int value) {
        value = 0;

        if (!GetVCPFeatureAndVCPFeatureReply(handle, vcpCode, out _, out uint currentValue, out uint maxValue)) {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"DDC: GetVCPFeature 失败 (VCP: 0x{vcpCode:X2}, 错误: {error})");
            return false;
        }

        value = maxValue > 0
            ? (int)Math.Round(currentValue * 100.0 / maxValue)
            : (int)currentValue;

        return true;
    }

    #endregion

    #region WMI Fallback

    /// <summary>
    /// 通过 WMI 设置显示器亮度
    /// </summary>
    private static bool SetBrightnessViaWmi(int brightness) {
        try {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
            using var results = searcher.Get();

            foreach (ManagementObject obj in results) {
                obj.InvokeMethod("WmiSetBrightness", [(uint)1, (byte)brightness]);
                Program.Log($"WMI: 亮度已设置为 {brightness}");
                return true;
            }

            Program.Log("WMI: 未找到 WmiMonitorBrightnessMethods 实例");
            return false;
        } catch (Exception ex) {
            Program.Log($"WMI: 设置亮度失败 - {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 通过 WMI 获取显示器亮度
    /// </summary>
    private static bool GetBrightnessViaWmi(out int brightness) {
        brightness = 0;

        try {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
            using var results = searcher.Get();

            foreach (ManagementObject obj in results) {
                brightness = Convert.ToInt32(obj["CurrentBrightness"]);
                Program.Log($"WMI: 当前亮度 {brightness}");
                return true;
            }

            Program.Log("WMI: 未找到 WmiMonitorBrightness 实例");
            return false;
        } catch (Exception ex) {
            Program.Log($"WMI: 获取亮度失败 - {ex.Message}");
            return false;
        }
    }

    #endregion
}
