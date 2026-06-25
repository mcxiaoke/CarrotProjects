using System.Globalization;
using RemotePCControl.Models;

namespace RemotePCControl.Services;

public interface ICommandRouter
{
    CommandResponse Execute(string commandText);
    string GetHelp();
    StatusResponse GetStatus();
}

public class CommandRouter : ICommandRouter
{
    private readonly ISystemControlService _systemControl;
    private readonly PowerSettings _power;
    private readonly ILogger<CommandRouter> _logger;
    private readonly DateTime _startTime = DateTime.Now;
    private readonly string _version;

    public CommandRouter(ISystemControlService systemControl, IConfiguration config, ILogger<CommandRouter> logger)
    {
        _systemControl = systemControl;
        _power = config.GetSection("Power").Get<PowerSettings>() ?? new PowerSettings();
        _logger = logger;
        _version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }

    public CommandResponse Execute(string commandText)
    {
        var cmd = (commandText ?? string.Empty).Trim().ToLowerInvariant();
        var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        _logger.LogInformation("Command received: {cmd}", commandText);

        var now = DateTime.Now;
        try
        {
            return (parts[0]) switch
            {
                "shutdown" or "off" => Result(_systemControl.Shutdown(), "已请求关机 (60s 倒计时，可 cancel 取消)", commandText, now),
                "restart" or "reboot" => Result(_systemControl.Restart(), "已请求重启 (60s 倒计时)", commandText, now),
                "shutdown-now" or "off-now" => Result(_systemControl.Shutdown(0, true), "立即关机", commandText, now),
                "sleep" => Result(_systemControl.Sleep(), "已请求进入睡眠", commandText, now),
                "hibernate" => Result(_systemControl.Hibernate(), "已请求进入休眠", commandText, now),
                "lock" => Result(_systemControl.LockScreen(), "已锁定屏幕", commandText, now),
                "logout" or "logoff" => Result(_systemControl.Logoff(), "已请求注销用户", commandText, now),
                "cancel" or "abort" => Result(_systemControl.CancelShutdown(), "已取消关机计划", commandText, now),
                "autosleep" => HandleAutoSleep(parts, commandText, now),
                "monitor" => HandleMonitorTimeout(parts, commandText, now),
                "sleeptime" or "sleep-time" => HandleSleepTimeout(parts, commandText, now),
                "message" or "msg" => HandleMessage(parts, commandText, now),
                "status" => Result(true, FormatStatus(), commandText, now),
                "help" or "?" => Result(true, GetHelp(), commandText, now),
                _ => Result(false, $"未知命令: {commandText}\n{GetHelp()}", commandText, now),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed: {cmd}", commandText);
            return new CommandResponse(false, $"执行出错: {ex.Message}", commandText, now);
        }
    }

    private CommandResponse HandleAutoSleep(string[] parts, string raw, DateTime now)
    {
        if (parts.Length < 2) return Result(false, "用法: autosleep on|off", raw, now);
        var enabled = parts[1] == "on" || parts[1] == "1" || parts[1] == "true";
        var ok = _systemControl.SetAutoSleep(enabled);
        return Result(ok, enabled ? "已启用自动睡眠" : "已禁用自动睡眠", raw, now);
    }

    private CommandResponse HandleMonitorTimeout(string[] parts, string raw, DateTime now)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out var min) || min < 0)
            return Result(false, "用法: monitor <分钟数> (0 表示永不关闭)", raw, now);
        return Result(_systemControl.SetMonitorTimeout(min), $"显示器超时设为 {min} 分钟", raw, now);
    }

    private CommandResponse HandleSleepTimeout(string[] parts, string raw, DateTime now)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out var min) || min < 0)
            return Result(false, "用法: sleeptime <分钟数> (0 表示永不睡眠)", raw, now);
        return Result(_systemControl.SetSleepTimeout(min), $"系统睡眠超时设为 {min} 分钟", raw, now);
    }

    private CommandResponse HandleMessage(string[] parts, string raw, DateTime now)
    {
        var content = raw.Substring(raw.IndexOf(' ') + 1);
        return Result(_systemControl.DisplayMessageBox("远程控制通知", content), $"桌面消息已发送: {content}", raw, now);
    }

    public StatusResponse GetStatus()
    {
        var memoryTotal = 0L;
        var memoryUsed = 0L;
        try
        {
            memoryTotal = (long)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576);
            memoryUsed = memoryTotal - (long)(GC.GetGCMemoryInfo().HeapSizeBytes / 1048576);
        }
        catch { /* ignore */ }

        var localIp = "n/a";
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "n/a";
        }
        catch { /* ignore */ }

        return new StatusResponse(
            MachineName: Environment.MachineName,
            OsVersion: Environment.OSVersion.ToString(),
            OsName: OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsLinux() ? "Linux" : "macOS",
            CpuUsagePercent: 0,
            MemoryUsedMb: memoryUsed,
            MemoryTotalMb: memoryTotal,
            Uptime: _startTime,
            AutoSleepEnabled: _power.AutoSleepEnabled,
            MonitorTimeout: $"{_power.MonitorTimeoutMinutes} 分钟",
            SleepTimeout: $"{_power.SleepTimeoutMinutes} 分钟",
            LocalIp: localIp,
            AppVersion: _version);
    }

    private string FormatStatus()
    {
        var s = GetStatus();
        return $"【状态】\n主机: {s.MachineName}\n系统: {s.OsName} {s.OsVersion}\nIP: {s.LocalIp}\n内存: {s.MemoryUsedMb:N0}MB / {s.MemoryTotalMb:N0}MB\n运行时间: {(DateTime.Now - s.Uptime).TotalHours:F1}h\n自动睡眠: {s.AutoSleepEnabled}\n显示器超时: {s.MonitorTimeout}\n系统超时: {s.SleepTimeout}\n版本: {s.AppVersion}";
    }

    public string GetHelp() => @"【可用命令】
  shutdown          — 延迟 60s 关机
  restart           — 延迟 60s 重启
  shutdown-now      — 立即关机
  sleep             — 立即进入睡眠
  hibernate         — 立即休眠
  lock              — 锁定屏幕
  logout            — 注销用户
  cancel            — 取消已计划的关机
  autosleep on|off  — 启用/禁用自动睡眠
  monitor <分钟>    — 设置显示器自动关闭时间
  sleeptime <分钟>  — 设置系统自动睡眠时间
  message <文本>    — 在桌面弹出消息
  status            — 获取系统状态
  help              — 显示此帮助

示例：'sleep' 或 'monitor 30' 或 'message 记得保存工作'";

    private static CommandResponse Result(bool ok, string msg, string cmd, DateTime t)
        => new(ok, msg, cmd, t);
}
