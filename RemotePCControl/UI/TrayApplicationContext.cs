#if WINDOWS
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace RemotePCControl.UI;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly string _configPath;
    private readonly string _webUrl;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;
    private readonly ToolStripMenuItem _statusItem;

    public TrayApplicationContext(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        _services = services;
        _logger = logger;
        _configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var web = config.GetSection("Web").Get<WebSettings>() ?? new WebSettings();
        _webUrl = $"http://localhost:{web.Port}";

        var icon = CreateAppIcon();

        var menu = new ContextMenuStrip();
        _statusItem = new ToolStripMenuItem($"远程控制服务运行中 · 端口 {web.Port}") { Enabled = false, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("🌐 打开控制面板", null, (_, _) => OpenBrowser(_webUrl));
        menu.Items.Add("📋 复制访问地址", null, (_, _) => { Clipboard.SetText(_webUrl); ShowBalloon("已复制", "访问地址已复制到剪贴板"); });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("⚡ 立即锁定", null, (_, _) => ExecuteLocal("lock"));
        menu.Items.Add("😴 立即睡眠", null, (_, _) => ExecuteLocal("sleep"));
        menu.Items.Add("💤 立即休眠", null, (_, _) => ExecuteLocal("hibernate"));
        menu.Items.Add("⏻ 延迟关机 (60s)", null, (_, _) => ExecuteLocal("shutdown"));
        menu.Items.Add("✋ 取消已计划关机", null, (_, _) => ExecuteLocal("cancel"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("⚙️ 修改配置文件", null, (_, _) => OpenConfigEditor());
        menu.Items.Add("📂 打开程序目录", null, (_, _) => Process.Start(new ProcessStartInfo(AppContext.BaseDirectory) { UseShellExecute = true }));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("❌ 退出", null, (_, _) => ExitApplication());

        _notifyIcon = new NotifyIcon
        {
            Icon = icon,
            Visible = true,
            Text = $"远程电脑控制 ({Environment.MachineName})",
            ContextMenuStrip = menu
        };
        _notifyIcon.DoubleClick += (_, _) => OpenBrowser(_webUrl);

        ShowBalloon("服务已启动", $"访问 {_webUrl} 控制电脑");
    }

    private static Icon CreateAppIcon()
    {
        using var bmp = new Bitmap(64, 64);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(Color.FromArgb(59, 130, 246));
        g.FillEllipse(brush, 4, 4, 56, 56);
        using var white = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", 28, FontStyle.Bold);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("PC", font, white, new RectangleF(0, 0, 64, 64), sf);
        var hicon = bmp.GetHicon();
        return Icon.FromHandle(hicon);
    }

    private void ShowBalloon(string title, string message)
    {
        try { _notifyIcon.ShowBalloonTip(2000, title, message, ToolTipIcon.Info); }
        catch { }
    }

    private void OpenBrowser(string url)
    {
        try
        {
            var token = ReadTokenFromConfig();
            var full = url + (string.IsNullOrEmpty(token) ? "" : $"?token={Uri.EscapeDataString(token)}");
            Process.Start(new ProcessStartInfo(full) { UseShellExecute = true });
        }
        catch (Exception ex) { _logger.LogError(ex, "Open browser failed"); }
    }

    private string ReadTokenFromConfig()
    {
        try
        {
            var json = File.ReadAllText(_configPath);
            var node = JsonNode.Parse(json);
            return node?["Security"]?["AccessToken"]?.GetValue<string>() ?? string.Empty;
        }
        catch { return string.Empty; }
    }

    private void OpenConfigEditor()
    {
        try { Process.Start(new ProcessStartInfo("notepad.exe", _configPath) { UseShellExecute = false }); }
        catch { }
    }

    private void ExecuteLocal(string command)
    {
        try
        {
            var router = _services.GetRequiredService<ICommandRouter>();
            var result = router.Execute(command);
            ShowBalloon(result.Success ? "执行成功" : "执行失败", result.Message);
        }
        catch (Exception ex) { _logger.LogError(ex, "Local execution failed"); }
    }

    private void ExitApplication()
    {
        try { _notifyIcon.Visible = false; _notifyIcon.Dispose(); }
        catch { }
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { try { _notifyIcon.Dispose(); } catch { } }
        base.Dispose(disposing);
    }
}
#endif
