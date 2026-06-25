using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RemotePCControl.Services;

public interface ISystemControlService
{
    bool Shutdown(int delaySeconds = 60, bool force = true);
    bool Restart(int delaySeconds = 60, bool force = true);
    bool Sleep();
    bool Hibernate();
    bool LockScreen();
    bool Logoff();
    bool CancelShutdown();
    bool SetAutoSleep(bool enabled);
    bool SetMonitorTimeout(int minutes);
    bool SetSleepTimeout(int minutes);
    bool DisplayMessageBox(string title, string message);
}

public class SystemControlService : ISystemControlService
{
    private readonly ILogger<SystemControlService> _logger;

    public SystemControlService(ILogger<SystemControlService> logger)
    {
        _logger = logger;
    }

#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    [DllImport("user32.dll")]
    private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    [DllImport("powrprof.dll", CharSet = CharSet.Auto)]
    private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

    private static void EnableShutdownPrivilege()
    {
        // The privilege is automatically enabled for modern Windows shutdown APIs; kept for safety
    }
#else
    private static bool LockWorkStation() => false;
    private static bool SetSuspendState(bool h, bool f, bool d) => false;
#endif

    public bool Shutdown(int delaySeconds = 60, bool force = true)
    {
        try
        {
            _logger.LogInformation("Shutdown requested: {delay}s", delaySeconds);
            string cmd, args;
            if (OperatingSystem.IsWindows())
            {
                cmd = "shutdown.exe";
                args = force ? $"/s /t {delaySeconds} /f" : $"/s /t {delaySeconds}";
            }
            else if (OperatingSystem.IsLinux())
            {
                cmd = "shutdown";
                args = force ? $"-h {delaySeconds}" : $"-h {delaySeconds}";
            }
            else
            {
                cmd = "osascript";
                args = $"-e 'tell application \"System Events\" to shut down'";
            }
            Process.Start(new ProcessStartInfo(cmd, args) { CreateNoWindow = true, UseShellExecute = false });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shutdown failed");
            return false;
        }
    }

    public bool Restart(int delaySeconds = 60, bool force = true)
    {
        try
        {
            _logger.LogInformation("Restart requested: {delay}s", delaySeconds);
            string cmd, args;
            if (OperatingSystem.IsWindows())
            {
                cmd = "shutdown.exe";
                args = force ? $"/r /t {delaySeconds} /f" : $"/r /t {delaySeconds}";
            }
            else if (OperatingSystem.IsLinux())
            {
                cmd = "shutdown";
                args = $"-r {delaySeconds}";
            }
            else
            {
                cmd = "osascript";
                args = "-e 'tell application \"System Events\" to restart'";
            }
            Process.Start(new ProcessStartInfo(cmd, args) { CreateNoWindow = true, UseShellExecute = false });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restart failed");
            return false;
        }
    }

    public bool Sleep()
    {
        try
        {
            _logger.LogInformation("Sleep requested");
            if (OperatingSystem.IsWindows())
            {
                return SetSuspendState(false, false, false);
            }
            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("systemctl", "suspend") { CreateNoWindow = true, UseShellExecute = false });
                return true;
            }
            Process.Start(new ProcessStartInfo("osascript", "-e 'tell application \"System Events\" to sleep'") { CreateNoWindow = true, UseShellExecute = false });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sleep failed");
            return false;
        }
    }

    public bool Hibernate()
    {
        try
        {
            _logger.LogInformation("Hibernate requested");
            if (OperatingSystem.IsWindows())
            {
                return SetSuspendState(true, false, false);
            }
            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("systemctl", "hibernate") { CreateNoWindow = true, UseShellExecute = false });
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hibernate failed");
            return false;
        }
    }

    public bool LockScreen()
    {
        try
        {
            _logger.LogInformation("Lock screen requested");
            if (OperatingSystem.IsWindows())
            {
                return LockWorkStation();
            }
            if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("xdg-screensaver", "lock") { CreateNoWindow = true, UseShellExecute = false });
                return true;
            }
            Process.Start(new ProcessStartInfo("osascript", "-e 'tell application \"System Events\" to keystroke \"q\" using {command down,control down}'") { CreateNoWindow = true, UseShellExecute = false });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lock screen failed");
            return false;
        }
    }

    public bool Logoff()
    {
        try
        {
            _logger.LogInformation("Logoff requested");
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/l") { CreateNoWindow = true, UseShellExecute = false });
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logoff failed");
            return false;
        }
    }

    public bool CancelShutdown()
    {
        try
        {
            _logger.LogInformation("Cancel shutdown requested");
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo("shutdown.exe", "/a") { CreateNoWindow = true, UseShellExecute = false });
            else if (OperatingSystem.IsLinux())
                Process.Start(new ProcessStartInfo("shutdown", "-c") { CreateNoWindow = true, UseShellExecute = false });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel shutdown failed");
            return false;
        }
    }

    public bool SetAutoSleep(bool enabled)
    {
        try
        {
            _logger.LogInformation("Set auto sleep: {enabled}", enabled);
            if (OperatingSystem.IsWindows())
            {
                var minutes = enabled ? 30 : 0;
                Process.Start(new ProcessStartInfo("powercfg", $"/change /standby-timeout-ac {minutes}") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit(2000);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set auto sleep failed");
            return false;
        }
    }

    public bool SetMonitorTimeout(int minutes)
    {
        try
        {
            _logger.LogInformation("Set monitor timeout: {min}min", minutes);
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("powercfg", $"/change /monitor-timeout-ac {minutes}") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit(2000);
                Process.Start(new ProcessStartInfo("powercfg", $"/change /monitor-timeout-dc {minutes}") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit(2000);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set monitor timeout failed");
            return false;
        }
    }

    public bool SetSleepTimeout(int minutes)
    {
        try
        {
            _logger.LogInformation("Set sleep timeout: {min}min", minutes);
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("powercfg", $"/change /standby-timeout-ac {minutes}") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit(2000);
                Process.Start(new ProcessStartInfo("powercfg", $"/change /standby-timeout-dc {minutes}") { CreateNoWindow = true, UseShellExecute = false })?.WaitForExit(2000);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set sleep timeout failed");
            return false;
        }
    }

    public bool DisplayMessageBox(string title, string message)
    {
        try
        {
            _logger.LogInformation("Display message: {title}", title);
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("msg", $"* \"{message}\"") { CreateNoWindow = true, UseShellExecute = false });
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Display message failed");
            return false;
        }
    }
}
