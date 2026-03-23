using System.Text.Json.Serialization;

namespace Carrot.AutoLock.Router;

/// <summary>
/// 设备信息
/// </summary>
public class HostInfo {
    private static readonly Dictionary<int, string> WifiModeMap = new Dictionary<int, string>() {
        [0] = "有线",
        [1] = "无线"
    };

    private static readonly Dictionary<int, string> PhyModeMap = new Dictionary<int, string>() {
        [0] = "未知",
        [4] = "2.4G",
        [5] = "5G",
        [6] = "WiFi6"
    };

    public string Mac { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string Ipv6 { get; set; } = string.Empty;
    public int UpSpeed { get; set; }
    public int DownSpeed { get; set; }
    public int ConnectTime { get; set; }
    public int WifiMode { get; set; }
    public int PhyMode { get; set; }
    public bool IsCurHost { get; set; }
    public bool Blocked { get; set; }

    /// <summary>
    /// WiFi 连接类型
    /// </summary>
    [JsonIgnore]
    public string WifiType => WifiModeMap.GetValueOrDefault(WifiMode, "未知");

    /// <summary>
    /// 物理连接类型
    /// </summary>
    [JsonIgnore]
    public string PhyType => PhyModeMap.GetValueOrDefault(PhyMode, "未知");

    /// <summary>
    /// 解码后的设备名称
    /// </summary>
    [JsonIgnore]
    public string HostnameDecoded => string.IsNullOrEmpty(Hostname)
        ? "未知设备"
        : Uri.UnescapeDataString(Hostname);

    /// <summary>
    /// 格式化的连接时间
    /// </summary>
    [JsonIgnore]
    public string ConnectTimeStr => FormatConnectTime(ConnectTime);

    /// <summary>
    /// 格式化的上行速度
    /// </summary>
    [JsonIgnore]
    public string UpSpeedStr => FormatSpeed(UpSpeed);

    /// <summary>
    /// 格式化的下行速度
    /// </summary>
    [JsonIgnore]
    public string DownSpeedStr => FormatSpeed(DownSpeed);

    private static string FormatSpeed(int speed) {
        if (speed < 1024)
            return $"{speed} B/s";
        if (speed < 1024 * 1024)
            return $"{speed / 1024.0:F1} KB/s";
        return $"{speed / 1024.0 / 1024.0:F2} MB/s";
    }

    private static string FormatConnectTime(int seconds) {
        if (seconds < 60)
            return $"{seconds}秒";
        if (seconds < 3600)
            return $"{seconds / 60}分钟";
        if (seconds < 86400)
            return $"{seconds / 3600}小时{seconds % 3600 / 60}分钟";
        return $"{seconds / 86400}天{seconds % 86400 / 3600}小时";
    }

    /// <summary>
    /// 从 API 数据创建 HostInfo
    /// </summary>
    public static HostInfo FromApiData(Dictionary<string, object> data) {
        return new HostInfo {
            Mac = data.GetValueOrDefault("mac")?.ToString() ?? string.Empty,
            Ip = data.GetValueOrDefault("ip")?.ToString() ?? string.Empty,
            Hostname = data.GetValueOrDefault("hostname")?.ToString() ?? string.Empty,
            Ipv6 = data.GetValueOrDefault("ipv6")?.ToString() ?? string.Empty,
            UpSpeed = int.TryParse(data.GetValueOrDefault("up_speed")?.ToString(), out var up) ? up : 0,
            DownSpeed = int.TryParse(data.GetValueOrDefault("down_speed")?.ToString(), out var down) ? down : 0,
            ConnectTime = int.TryParse(data.GetValueOrDefault("connect_time")?.ToString(), out var time) ? time : 0,
            WifiMode = int.TryParse(data.GetValueOrDefault("wifi_mode")?.ToString(), out var wifi) ? wifi : 0,
            PhyMode = int.TryParse(data.GetValueOrDefault("phy_mode")?.ToString(), out var phy) ? phy : 0,
            IsCurHost = data.GetValueOrDefault("is_cur_host")?.ToString() == "1",
            Blocked = data.GetValueOrDefault("blocked")?.ToString() == "1"
        };
    }
}
