namespace RemotePCControl.Models;

public class AppSettings
{
    public WebSettings Web { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public PowerSettings Power { get; set; } = new();
    public BotSettings Bots { get; set; } = new();
    public string MachineName { get; set; } = Environment.MachineName;
}

public class WebSettings
{
    public bool Enabled { get; set; } = true;
    public int Port { get; set; } = 5000;
    public bool OpenUrlOnStart { get; set; } = true;
    public string BindIp { get; set; } = "0.0.0.0";
    public string GetBaseUrl() => $"http://{BindIp}:{Port}";
}

public class SecuritySettings
{
    public bool RequireToken { get; set; } = true;
    public string AccessToken { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 16);
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> AllowedIpPrefixes { get; set; } = new() { "192.168.", "10.", "127." };
}

public class PowerSettings
{
    public bool AutoSleepEnabled { get; set; } = true;
    public int MonitorTimeoutMinutes { get; set; } = 15;
    public int SleepTimeoutMinutes { get; set; } = 30;
}

public class BotSettings
{
    public WeWorkBot WeWork { get; set; } = new();
    public TelegramBotConfig Telegram { get; set; } = new();
    public WeChatBot WeChat { get; set; } = new();
    public bool NotifyOnOperation { get; set; } = true;
}

public class WeWorkBot
{
    public bool Enabled { get; set; } = false;
    public string WebhookUrl { get; set; } = string.Empty;
}

public class TelegramBotConfig
{
    public bool Enabled { get; set; } = false;
    public string BotToken { get; set; } = string.Empty;
    public string AllowedChatIds { get; set; } = string.Empty;
}

public class WeChatBot
{
    public bool Enabled { get; set; } = false;
    public string WebhookUrl { get; set; } = string.Empty;
}

public record CommandRequest(string Command, string? Token = null);
public record CommandResponse(bool Success, string Message, string Command, DateTime Timestamp);
public record StatusResponse(
    string MachineName,
    string OsVersion,
    string OsName,
    double CpuUsagePercent,
    long MemoryUsedMb,
    long MemoryTotalMb,
    DateTime Uptime,
    bool AutoSleepEnabled,
    string MonitorTimeout,
    string SleepTimeout,
    string LocalIp,
    string AppVersion);
