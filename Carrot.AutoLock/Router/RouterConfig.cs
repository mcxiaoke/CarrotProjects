namespace Carrot.AutoLock.Router;

/// <summary>
/// 路由器配置
/// </summary>
public class RouterConfig {
    /// <summary>
    /// 路由器 IP 地址
    /// </summary>
    public string RouterIp { get; set; } = "192.168.1.1";

    /// <summary>
    /// 路由器管理密码
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// HTTP 请求超时（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;
}
