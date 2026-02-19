using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Carrot.Common;
using Carrot.Common.Native;

namespace Carrot.AutoLock;

/// <summary>
/// Represents a physical monitor.
/// 物理显示器信息。
/// </summary>
public class PhysicalMonitor {
    public IntPtr Handle; // 物理显示器句柄
    public string DeviceName = string.Empty; // 设备名称
}

/// <summary>
/// Represents a logical monitor (may correspond to multiple physical monitors).
/// 逻辑显示器信息 (可能对应多个物理显示器)。
/// </summary>
public class LogicalMonitor {
    public IntPtr Handle; // HMONITOR 句柄
    public RECT Rect;       // 显示区域
    public List<PhysicalMonitor> PhysicalMonitors = new(); // 关联的物理显示器列表
}

/// <summary>
/// 显示器管理器
/// 封装了 DDC/CI 协议相关的 Windows API，用于控制显示器硬件功能 (如亮度、电源)
/// </summary>
public class MonitorManager : IDisposable {
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PHYSICAL_MONITOR {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hPhysicalMonitor, byte bVCPCode, IntPtr pvct, out uint pdwCurrentValue, out uint pdwMaximumValue);

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool SetVCPFeature(IntPtr hPhysicalMonitor, byte bVCPCode, uint dwNewValue);

    private const byte VCP_BRIGHTNESS = 0x10; // 亮度 VCP 代码，范围 [0-100]

    private readonly List<LogicalMonitor> _monitors = new();

    /// <summary>
    /// Initializes the monitor manager by enumerating all monitors.
    /// 初始化显示器管理器。枚举所有显示器并获取其物理监视器句柄。
    /// </summary>
    public void Initialize() {
        Dispose(); // Clear existing

        UnsafeNative.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) => {
            var monitor = new LogicalMonitor {
                Handle = hMonitor,
                Rect = lprcMonitor
            };

            if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint count) && count > 0) {
                var physicalMonitors = new PHYSICAL_MONITOR[count];
                if (GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors)) {
                    monitor.PhysicalMonitors = physicalMonitors.Select(pm => new PhysicalMonitor {
                        Handle = pm.hPhysicalMonitor,
                        DeviceName = pm.szPhysicalMonitorDescription ?? string.Empty
                    }).ToList();
                }
            }

            _monitors.Add(monitor);
            return true;
        }, IntPtr.Zero);
    }

    /// <summary>
    /// Sets the brightness of all physical monitors.
    /// 设置所有显示器的亮度。
    /// </summary>
    /// <param name="brightness">Brightness level (0-100). 亮度值 (0-100)。</param>
    public void SetAllBrightness(uint brightness) {
        brightness = Math.Min(brightness, 100);
        foreach (var m in _monitors) {
            foreach (var pm in m.PhysicalMonitors) {
                // 暂时禁用亮度调整
                //SetVCPFeature(pm.Handle, VCP_BRIGHTNESS, brightness);
            }
        }
    }

    /// <summary>
    /// Disposes resources and closes physical monitor handles.
    /// 释放资源，关闭所有物理显示器句柄。
    /// </summary>
    public void Dispose() {
        foreach (var m in _monitors) {
            if (m.PhysicalMonitors.Count > 0) {
                // Reconstruct the array to destroy handles
                var pms = m.PhysicalMonitors.Select(pm => new PHYSICAL_MONITOR { 
                    hPhysicalMonitor = pm.Handle, 
                    szPhysicalMonitorDescription = pm.DeviceName 
                }).ToArray();
                
                DestroyPhysicalMonitors((uint)pms.Length, pms);
            }
        }
        _monitors.Clear();
        GC.SuppressFinalize(this);
    }
}
