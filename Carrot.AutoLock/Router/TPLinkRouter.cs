
using System.Collections.Generic;
using System.Text.Json;

namespace Carrot.AutoLock.Router;

/// <summary>
/// TP-Link 路由器管理类
/// </summary>
public class TPLinkRouter {
    // TP-Link 固定加密盐值
    private const string StrB = "RDpbLfCPsJZ7fiv";
    private const string StrC = "yLwVl0zKqws7LgKPRQ84Mdt708T1qQ3Ha7xv3H7NyU84p21BriUWBU43odz3iP4rBL3cD02KZciXTysVXiV8ngg6vL48rPJyAUw0HurW20xqxv9aYb4M9wK1Ae0wlro510qXeU07kV57fQMc8L6aLgMLwygtc0F10a0Dg70TOoouyFhdysuRMO51yY5ZlOZZLEal1h0t9YQW0Ko7oBwmCAHoic4HYbUyVeU3sfQ1xtXcPcf1aT303wAQhv66qzW";

    private readonly string _ip;
    private readonly string _password;
    private readonly int _timeout;
    private readonly HttpClient _httpClient;
    private string? _stok;
    private List<HostInfo> _hostsCache = new List<HostInfo>();

    /// <summary>
    /// 路由器 IP 地址
    /// </summary>
    public const string DefaultRouterIp = "192.168.1.1";

    /// <summary>
    /// 默认超时时间（毫秒）
    /// </summary>
    public const int DefaultTimeoutMs = 5000;

    public TPLinkRouter(string password, string? ip = null, int timeoutMs = DefaultTimeoutMs) {
        _ip = ip ?? DefaultRouterIp;
        _password = password;
        _timeout = timeoutMs;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(_timeout) };
    }

    /// <summary>
    /// TP-Link 密码加密
    /// </summary>
    public static string SecurityEncode(string password) {
        var result = new System.Text.StringBuilder();
        var pwdLen = password.Length;
        var bLen = StrB.Length;
        var cLen = StrC.Length;
        var maxLen = Math.Max(pwdLen, bLen);

        for (var i = 0; i < maxLen; i++) {
            var k = i < pwdLen ? (int)password[i] : 187;
            var l = i < bLen ? (int)StrB[i] : 187;
            result.Append(StrC[(k ^ l) % cLen]);
        }

        return result.ToString();
    }

    /// <summary>
    /// 登录路由器
    /// </summary>
    public async Task<bool> LoginAsync() {
        var url = $"http://{_ip}/";
        var payload = new {
            method = "do",
            login = new { password = SecurityEncode(_password) }
        };

        try {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            if (data.TryGetProperty("error_code", out var errorCode) && errorCode.GetInt32() == 0) {
                _stok = data.GetProperty("stok").GetString();
                return true;
            }

            Console.WriteLine($"登录失败: {data}");
        } catch (Exception ex) {
            Console.WriteLine($"登录异常: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// 确保已登录
    /// </summary>
    private async Task<bool> EnsureLoginAsync() {
        return !string.IsNullOrEmpty(_stok) || await LoginAsync();
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    private async Task<JsonElement?> RequestAsync(object payload) {
        if (!await EnsureLoginAsync())
            return null;

        var url = $"http://{_ip}/stok={_stok}/ds";
        try {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            // 检查 token 是否过期
            if (data.ValueKind != JsonValueKind.Object)
                return data;

            if (data.TryGetProperty("error_code", out var errorCode) && errorCode.GetInt32() != 0) {
                _stok = null;
                return null;
            }

            return data;
        } catch (Exception ex) {
            Console.WriteLine($"请求异常: {ex.Message}");
            _stok = null;
            return null;
        }
    }

    /// <summary>
    /// 获取所有在线设备
    /// </summary>
    public async Task<List<HostInfo>> GetHostsAsync() {
        var payload = new {
            system = new { name = new[] { "sys" } },
            hosts_info = new { table = "host_info" },
            network = new { name = "iface_mac" },
            @function = new { name = "new_module_spec" },
            method = "get"
        };

        var data = await RequestAsync(payload);
        if (data == null)
            return new List<HostInfo>();

        var hosts = new List<HostInfo>();

        if (!data.Value.TryGetProperty("hosts_info", out var hostsInfo) ||
            !hostsInfo.TryGetProperty("host_info", out var hostList))
            return hosts;

        foreach (var item in hostList.EnumerateArray()) {
            foreach (var prop in item.EnumerateObject()) {
                try {
                    var hostData = JsonSerializer.Deserialize<Dictionary<string, object>>(prop.Value.GetRawText());
                    if (hostData != null) {
                        var host = HostInfo.FromApiData(hostData);
                        hosts.Add(host);
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"解析设备信息失败: {ex.Message}");
                }
            }
        }

        _hostsCache = hosts;
        return hosts;
    }

    /// <summary>
    /// 根据 MAC 地址查找设备
    /// </summary>
    public async Task<HostInfo?> FindHostByMacAsync(string mac) {
        mac = mac.ToUpperInvariant().Replace(":", "-");
        foreach (var host in await GetHostsAsync()) {
            if (host.Mac.ToUpperInvariant() == mac)
                return host;
        }
        return null;
    }

    /// <summary>
    /// 根据 IP 地址查找设备
    /// </summary>
    public async Task<HostInfo?> FindHostByIpAsync(string ip) {
        foreach (var host in await GetHostsAsync()) {
            if (host.Ip == ip)
                return host;
        }
        return null;
    }

    /// <summary>
    /// 根据设备名称模糊查找
    /// </summary>
    public async Task<List<HostInfo>> FindHostByNameAsync(string name) {
        var nameLower = name.ToLowerInvariant();
        var result = new List<HostInfo>();
        foreach (var host in await GetHostsAsync()) {
            if (host.HostnameDecoded.ToLowerInvariant().Contains(nameLower))
                result.Add(host);
        }
        return result;
    }

    /// <summary>
    /// 检查指定 MAC 设备是否在线
    /// </summary>
    public async Task<bool> IsOnlineAsync(string mac) {
        return await FindHostByMacAsync(mac) != null;
    }
}
