using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Carrot.Common;

namespace Carrot.AutoLock;

/// <summary>
/// 物理显示器信息
/// </summary>
public class PhysicalMonitor {
    public IntPtr hPhysicalMonitor; // 物理显示器句柄
    public string DeviceName = string.Empty; // 设备名称
    public bool IsEnabled;
    public bool IsPoweredOn;
    public uint BrightnessLevel;
}

/// <summary>
/// 逻辑显示器信息 (可能对应多个物理显示器)
/// </summary>
public class Monitor {
    public IntPtr hMonitor; // HMONITOR 句柄
    public Rect rect;       // 显示区域
    public List<PhysicalMonitor> physicalMonitors = new(); // 关联的物理显示器列表
}

/// <summary>
/// 物理显示器结构体 (WinAPI)
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct PHYSICAL_MONITOR {
    public IntPtr hPhysicalMonitor;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szPhysicalMonitorDescription;
}

/// <summary>
/// 矩形区域结构体 (WinAPI)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Rect {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

/// <summary>
/// 显示器管理器
/// 封装了 DDC/CI 协议相关的 Windows API，用于控制显示器硬件功能 (如亮度、电源)
/// </summary>
public class MonitorManager : IDisposable {
    #region [Windows API]
    // 获取指定窗口所在的显示器句柄
    [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
    private static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

    // 销毁物理显示器句柄数组
    [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, ref PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    // 获取与 HMONITOR 关联的物理显示器数量
    [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

    // 获取物理显示器句柄列表
    [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize,
        [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    // 获取 VCP (Virtual Control Panel) 特征值
    [DllImport("dxva2.dll", EntryPoint = "GetVCPFeatureAndVCPFeatureReply", SetLastError = true)]
    private static extern bool GetVCPFeatureAndVCPFeatureReply([In] IntPtr hPhisicalMonitor, [In] byte bVCPCode,
        IntPtr pvct, ref uint pdwCurrentValue, ref uint pdwMaximumValue);

    private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

    // 枚举显示器
    [DllImport("user32")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

    // 设置 VCP 特征值 (例如亮度)
    [DllImport("dxva2.dll", EntryPoint = "SetVCPFeature", SetLastError = true)]
    private static extern bool SetVCPFeature([In] IntPtr hPhisicalMonitor, byte bVCPCode, uint dwNewValue);
    #endregion

    private const byte SVC_FEATURE__POWER_MODE = 0xD6; // 电源模式 VCP 代码
    private const byte SVC_FEATURE__BRIGHTNESS = 0x10; // 亮度 VCP 代码，范围 [0-100]
    private const byte SVC_FEATURE__CONTRAST = 0x12;   // 对比度 VCP 代码，范围 [0-100]

    public enum PowerModeEnum : uint {
        PowerOn = 0x01,
        PowerStandby = 0x02,
        PowerSuspend = 0x03,
        PowerOff = 0x04,
        PowerOffButton = 0x05 // Readonly
    }

    private List<Monitor> monitors = new();

    /// <summary>
    /// 初始化显示器管理器
    /// 枚举所有显示器并获取其物理监视器句柄
    /// </summary>
    public void Initialize() {
        monitors.Clear();

        bool callback(IntPtr hMonitor, IntPtr hdc, ref Rect prect, int d) {
            monitors.Add(new Monitor {
                hMonitor = hMonitor,
                rect = prect,
            });
            return true;
        }

        // 枚举所有显示器
        if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0)) {
            foreach (var m in monitors) {
                uint mcount = 0;
                // 获取物理显示器数量
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(m.hMonitor, ref mcount)) {
                    Logger.Info($"Cannot get monitor count for {m.hMonitor}");
                    continue;
                }
                PHYSICAL_MONITOR[] physicalMonitors = new PHYSICAL_MONITOR[mcount];

                // 获取物理显示器句柄
                if (!GetPhysicalMonitorsFromHMONITOR(m.hMonitor, mcount, physicalMonitors)) {
                    Logger.Info($"Cannot get physical monitor handle for {m.hMonitor}");
                    continue;
                }

                m.physicalMonitors = physicalMonitors.Select(a => new PhysicalMonitor {
                    DeviceName = a.szPhysicalMonitorDescription,
                    hPhysicalMonitor = a.hPhysicalMonitor
                }).ToList();
            }
        }
    }

    /// <summary>
    /// 设置所有显示器的亮度
    /// </summary>
    /// <param name="brightness">亮度值 (0-100)</param>
    public void SetAllBrightness(uint brightness) {
        if (brightness > 100) brightness = 100;
        foreach (var m in monitors) {
            foreach (var pm in m.physicalMonitors) {
                SetFeatureValue(pm.hPhysicalMonitor, SVC_FEATURE__BRIGHTNESS, brightness);
            }
        }
    }

    /// <summary>
    /// 获取所有显示器的平均亮度
    /// </summary>
    /// <returns>平均亮度值</returns>
    public uint GetAverageBrightness() {
        uint total = 0;
        int count = 0;
        foreach (var m in monitors) {
            foreach (var pm in m.physicalMonitors) {
                uint current = 0;
                if (GetFeatureValue(pm.hPhysicalMonitor, SVC_FEATURE__BRIGHTNESS, ref current)) {
                    total += current;
                    count++;
                }
            }
        }
        return count > 0 ? total / (uint)count : 50;
    }

    /// <summary>
    /// 获取指定的 VCP 特征值
    /// </summary>
    private bool GetFeatureValue(IntPtr hPhysicalMonitor, byte svc_feature, ref uint currentValue) {
        uint mv = 0;
        return GetVCPFeatureAndVCPFeatureReply(hPhysicalMonitor, svc_feature, IntPtr.Zero, ref currentValue, ref mv);
    }

    /// <summary>
    /// 设置指定的 VCP 特征值
    /// </summary>
    private bool SetFeatureValue(IntPtr hPhysicalMonitor, byte svc_feature, uint newValue) {
        return SetVCPFeature(hPhysicalMonitor, svc_feature, newValue);
    }

    /// <summary>
    /// 释放资源，关闭所有物理显示器句柄
    /// </summary>
    public void Dispose() {
        foreach (var m in monitors) {
            if (m.physicalMonitors.Count > 0) {
                var pms = m.physicalMonitors.Select(pm => new PHYSICAL_MONITOR { hPhysicalMonitor = pm.hPhysicalMonitor, szPhysicalMonitorDescription = pm.DeviceName }).ToArray();
                DestroyPhysicalMonitors((uint)pms.Length, ref pms);
            }
        }
        monitors.Clear();
        GC.SuppressFinalize(this);
    }
}
