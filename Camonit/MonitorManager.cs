using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;

namespace Camonit {
    public class PhysicalMonitor {
        public IntPtr hPhysicalMonitor;
        public string DeviceName = string.Empty;
        public bool IsEnabled;
        public bool IsPoweredOn;
        public uint BrightnessLevel;
    }
    public class Monitor {
        public IntPtr hMonitor;
        public Rect rect;
        public List<PhysicalMonitor> physicalMonitors = new();
    }


    internal class MonitorManager : IDisposable {
        #region [Windows API]
        [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
        private static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, ref PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize,
            [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetVCPFeatureAndVCPFeatureReply", SetLastError = true)]
        private static extern Boolean GetVCPFeatureAndVCPFeatureReply([In] IntPtr hPhisicalMonitor, [In] byte bVCPCode,
            IntPtr pvct, ref uint pdwCurrentValue, ref uint pdwMaximumValue);


        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        [DllImport("dxva2.dll", EntryPoint = "SetVCPFeature", SetLastError = true)]
        private static extern bool SetVCPFeature([In] IntPtr hPhisicalMonitor, byte bVCPCode, uint dwNewValue);
        #endregion

        private const byte SVC_FEATURE__POWER_MODE = 0xD6; // values use PowerModeEnum
        private const byte SVC_FEATURE__BRIGHTNESS = 0x10; // value range is [0-100]
        private const byte SVC_FEATURE__CONTRAST = 0x12; // value range is [0-100]

        public enum PowerModeEnum : uint {
            PowerOn = 0x01,
            PowerStandby = 0x02,
            PowerSuspend = 0x03,
            PowerOff = 0x04,
            PowerOffButton = 0x05 // Readonly
        }

        List<Monitor> monitors = new();
        List<PHYSICAL_MONITOR> physicalMonitors = new();

        public IReadOnlyList<Monitor> Monitors {
            get {
                return monitors.AsReadOnly();
            }
        }
        public void Initialize() {
            monitors = new List<Monitor>();
            physicalMonitors = new List<PHYSICAL_MONITOR>();


            List<(IntPtr monitor, Rect rect)> hMonitors = new List<(IntPtr, Rect)>();

            bool callback(IntPtr hMonitor, IntPtr hdc, ref Rect prect, int d) {
                monitors.Add(new Monitor {
                    hMonitor = hMonitor,
                    rect = prect,
                });
                return true;
            }


            if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0)) {
                foreach (var m in monitors) {
                    uint mcount = 0;
                    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(m.hMonitor, ref mcount)) {
                        throw new Exception("Cannot get monitor count!");
                    }
                    PHYSICAL_MONITOR[] physicalMonitors = new PHYSICAL_MONITOR[mcount];

                    if (!GetPhysicalMonitorsFromHMONITOR(m.hMonitor, mcount, physicalMonitors)) {
                        throw new Exception("Cannot get phisical monitor handle!");
                    }

                    Debug.WriteLine($"PM:{physicalMonitors.Length}) RECT: T:{m.rect.Top}/L:{m.rect.Left}/R:{m.rect.Right}/B:{m.rect.Bottom}");

                    this.physicalMonitors.AddRange(physicalMonitors);

                    m.physicalMonitors = physicalMonitors.Select(a => new PhysicalMonitor {
                        DeviceName = a.szPhysicalMonitorDescription,
                        hPhysicalMonitor = a.hPhysicalMonitor
                    }).ToList();

                }

                foreach (var p in monitors.SelectMany(a => a.physicalMonitors)) {
                    uint cv = 0;

                    // power
                    if (GetFeatureValue(p.hPhysicalMonitor, SVC_FEATURE__POWER_MODE, ref cv)) {
                        p.IsPoweredOn = (cv == (uint)PowerModeEnum.PowerOn);
                        Debug.WriteLine($"{p.hPhysicalMonitor} + {p.DeviceName} + POWER={cv}");
                        p.IsEnabled = true;
                    } else {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                        Debug.WriteLine($"ERROR for {p.DeviceName}: `{errorMessage}`");
                    }

                    // BRIG
                    if (GetFeatureValue(p.hPhysicalMonitor, SVC_FEATURE__BRIGHTNESS, ref cv)) {
                        p.BrightnessLevel = cv;
                        Debug.WriteLine($"{p.hPhysicalMonitor} + {p.DeviceName} + BRIGHTNESS={cv}");
                    } else {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                        Debug.WriteLine($"ERROR for {p.DeviceName}: `{errorMessage}`");
                    }
                }
            }
        }


        private bool GetFeatureValue(IntPtr hPhysicalMonitor, byte svc_feature, ref uint currentValue) {
            uint mv = 0;
            return GetVCPFeatureAndVCPFeatureReply(hPhysicalMonitor, svc_feature, IntPtr.Zero, ref currentValue, ref mv);
            //string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }

        private bool SetFeatureValue(IntPtr hPhysicalMonitor, byte svc_feature, uint newVurrent) {
            return SetVCPFeature(hPhysicalMonitor, svc_feature, newVurrent);
            //string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }

        public void ChangeBrightness(IntPtr hPhysicalMonitor, uint brightness) {
            SetFeatureValue(hPhysicalMonitor, SVC_FEATURE__BRIGHTNESS, brightness);
            // TODO:  use HighLevel API to set brightness on non VESA monitors
            // https://docs.microsoft.com/en-us/windows/win32/api/highlevelmonitorconfigurationapi/nf-highlevelmonitorconfigurationapi-setmonitorbrightness
        }

        public void ChangeContrast(IntPtr hPhysicalMonitor, uint contrast) {
            SetFeatureValue(hPhysicalMonitor, SVC_FEATURE__CONTRAST, contrast);
        }

        public void SetBrightnessAndContrast(IntPtr hMon, uint b, uint c) {
            SetFeatureValue(hMon, SVC_FEATURE__BRIGHTNESS, b);
            SetFeatureValue(hMon, SVC_FEATURE__CONTRAST, c);
        }

        public void ChangePower(IntPtr hPhysicalMonitor, bool PowerOn) {
            SetFeatureValue(hPhysicalMonitor, SVC_FEATURE__POWER_MODE, PowerOn ? (uint)PowerModeEnum.PowerOn : (uint)PowerModeEnum.PowerOff);
        }


        public void Dispose() {
            GC.SuppressFinalize(this);

            PHYSICAL_MONITOR[] toDestroy = physicalMonitors.ToArray();
            DestroyPhysicalMonitors((uint)toDestroy.Length, ref toDestroy);
        }


    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }




}


/**
 * 
 * 
VCP code 0x02 (New control value             ): One or more new control values have been saved (0x02)
VCP code 0x0b (Color temperature increment   ): Invalid value: 0
VCP code 0x0c (Color temperature request     ): 3000 + 2 * (feature 0B color temp increment) degree(s) Kelvin
VCP code 0x0e (Clock                         ): current value =    50, max value =   100
VCP code 0x10 (Brightness                    ): current value =    57, max value =   100
VCP code 0x12 (Contrast                      ): current value =    56, max value =   100
VCP code 0x14 (Select color preset           ): sRGB (sl=0x01)
VCP code 0x16 (Video gain: Red               ): current value =   100, max value =   100
VCP code 0x18 (Video gain: Green             ): current value =   100, max value =   100
VCP code 0x1a (Video gain: Blue              ): current value =   100, max value =   100
VCP code 0x1e (Auto setup                    ): Auto setup not active (sl=0x00)
VCP code 0x20 (Horizontal Position (Phase)   ): current value =    50, max value =   100
VCP code 0x30 (Vertical Position (Phase)     ): current value =    50, max value =   100
VCP code 0x3e (Clock phase                   ): current value =    31, max value =   100
VCP code 0x52 (Active control                ): Value: 0x14
VCP code 0x59 (6 axis saturation: Red        ): current value =    50, max value =   100
VCP code 0x5a (6 axis saturation: Yellow     ): current value =    50, max value =   100
VCP code 0x5b (6 axis saturation: Green      ): current value =    50, max value =   100
VCP code 0x5c (6 axis saturation: Cyan       ): current value =    50, max value =   100
VCP code 0x5d (6 axis saturation: Blue       ): current value =    50, max value =   100
VCP code 0x5e (6 axis saturation: Magenta    ): current value =    50, max value =   100
VCP code 0x60 (Input Source                  ): DisplayPort-1 (sl=0x0f)
VCP code 0x6c (Video black level: Red        ): current value =    50, max value =   100
VCP code 0x6e (Video black level: Green      ): current value =    50, max value =   100
VCP code 0x70 (Video black level: Blue       ): current value =    50, max value =   100
VCP code 0x87 (Sharpness                     ): current value =     8, max value =   100
VCP code 0x8a (Color Saturation              ): current value =    50, max value =   100
VCP code 0x90 (Hue                           ): current value =    50, max value =   100
VCP code 0x9b (6 axis hue control: Red       ): current value =    50, max value =   100
VCP code 0x9c (6 axis hue control: Yellow    ): current value =    50, max value =   100
VCP code 0x9d (6 axis hue control: Green     ): current value =    50, max value =   100
VCP code 0x9e (6 axis hue control: Cyan      ): current value =    50, max value =   100
VCP code 0x9f (6 axis hue control: Blue      ): current value =    50, max value =   100
VCP code 0xa0 (6 axis hue control: Magenta   ): current value =    50, max value =   100
VCP code 0xac (Horizontal frequency          ): 32864 hz
VCP code 0xae (Vertical frequency            ): 59.80 hz
VCP code 0xb2 (Flat panel sub-pixel layout   ): Red/Green/Blue vertical stripe (sl=0x01)
VCP code 0xb4 (Source Timing Mode            ): mh=0x00, ml=0x02, sh=0x00, sl=0x01
VCP code 0xb6 (Display technology type       ): LCD (active matrix) (sl=0x03)
VCP code 0xc0 (Display usage time            ): Usage time (hours) = 5199 (0x00144f) mh=0x00, ml=0x00, sh=0x14, sl=0x4f
VCP code 0xc6 (Application enable key        ): 0x45cc
VCP code 0xc8 (Display controller type       ): Mfg: Mstar (sl=0x05), controller number: mh=0x00, ml=0x94, sh=0x85
VCP code 0xc9 (Display firmware level        ): 1.5
VCP code 0xca (OSD                           ): OSD Enabled (sl=0x02)
VCP code 0xcc (OSD Language                  ): English (sl=0x02)
VCP code 0xd6 (Power mode                    ): DPM: On,  DPMS: Off (sl=0x01)
VCP code 0xdc (Display Mode                  ): Standard/Default mode (sl=0x00)
VCP code 0xdf (VCP Version                   ): 2.1
 * 
 */
