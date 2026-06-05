using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MonitorControlTray;

/// <summary>
/// 显示器亮度控制托盘应用程序
/// <para>支持游戏/日常模式切换、根据时间自动调整亮度对比度、全局快捷键、睡眠唤醒自动恢复</para>
/// </summary>
file static class Program {
    /// <summary>
    /// 日志文件所在目录
    /// </summary>
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// 日志文件完整路径
    /// </summary>
    private static string LogPath => Path.Combine(LogDirectory, "app.log");

    /// <summary>
    /// 单个日志文件最大大小（字节），超过时触发轮转
    /// </summary>
    private const long MaxLogSize = 2 * 1024 * 1024; // 2 MB

    /// <summary>
    /// 保留的历史日志文件数量
    /// </summary>
    private const int MaxLogFiles = 5;

    /// <summary>
    /// 应用程序主入口
    /// </summary>
    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        EnsureLogDirectory();
        RotateLogIfNeeded();
        Log("程序启动");

        try {
            Application.Run(new TrayApplicationContext());
        } catch (Exception ex) {
            Log($"程序异常退出: {ex}");
            MessageBox.Show($"程序异常退出: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 确保日志目录存在
    /// </summary>
    private static void EnsureLogDirectory() {
        if (!Directory.Exists(LogDirectory)) {
            Directory.CreateDirectory(LogDirectory);
        }
    }

    /// <summary>
    /// 检查日志文件大小，超过 <see cref="MaxLogSize"/> 时进行轮转
    /// <para>轮转策略：app.log → app.1.log → app.2.log → ... → 删除最旧的</para>
    /// </summary>
    private static void RotateLogIfNeeded() {
        try {
            if (!File.Exists(LogPath)) return;

            var info = new FileInfo(LogPath);
            if (info.Length < MaxLogSize) return;

            // 删除最旧的日志文件
            string oldestLog = Path.Combine(LogDirectory, $"app.{MaxLogFiles}.log");
            if (File.Exists(oldestLog))
                File.Delete(oldestLog);

            // 依次重命名 app.{n-1}.log → app.{n}.log
            for (int i = MaxLogFiles - 1; i >= 1; i--) {
                string src = Path.Combine(LogDirectory, $"app.{i}.log");
                string dst = Path.Combine(LogDirectory, $"app.{i + 1}.log");
                if (File.Exists(src))
                    File.Move(src, dst);
            }

            // 当前日志 → app.1.log
            File.Move(LogPath, Path.Combine(LogDirectory, "app.1.log"));

            Log("日志文件已轮转");
        } catch {
            // 轮转失败时静默处理
        }
    }

    /// <summary>
    /// 记录日志到文件，格式：<c>[yyyy-MM-dd HH:mm:ss] 消息</c>
    /// <para>每次写入后检查文件大小，必要时触发轮转</para>
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void Log(string message) {
        try {
            using var writer = new StreamWriter(LogPath, append: true);
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        } catch {
            // 日志写入失败时静默处理，不影响主程序
        }

        // 异步检查轮转，避免每次写入都检查
        if (DateTime.Now.Second == 0)
            RotateLogIfNeeded();
    }

    /// <summary>
    /// 显示错误消息框并记录日志
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="ex">可选的异常对象，提供时将附加详细信息</param>
    public static void ShowError(string message, Exception? ex = null) {
        Log($"错误: {message}{(ex is not null ? $" - {ex}" : "")}");
        var detail = ex is not null ? $"\n详细信息: {ex.Message}" : "";
        MessageBox.Show($"{message}{detail}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

/// <summary>
/// 时间段设置，定义某个时间点的亮度和对比度
/// </summary>
public class TimeSetting {
    /// <summary>
    /// 时间点（格式：HH:mm）
    /// </summary>
    public required string Time { get; set; }

    /// <summary>
    /// 亮度值（0-100）
    /// </summary>
    public required int Brightness { get; set; }

    /// <summary>
    /// 对比度值（0-100）
    /// </summary>
    public required int Contrast { get; set; }

    /// <summary>
    /// 将 Time 字段解析为 TimeSpan，解析失败返回 <see cref="TimeSpan.Zero"/>
    /// </summary>
    public TimeSpan ToTimeSpan() => TimeSpan.TryParse(Time, out var ts) ? ts : TimeSpan.Zero;
}

/// <summary>
/// 快捷键配置
/// </summary>
public class HotkeyConfig {
    /// <summary>切换到游戏模式的快捷键</summary>
    public string? SwitchToGameMode { get; set; }

    /// <summary>切换到日常模式的快捷键</summary>
    public string? SwitchToDailyMode { get; set; }

    /// <summary>手动刷新设置的快捷键</summary>
    public string? ManualRefresh { get; set; }

    /// <summary>增加亮度的快捷键</summary>
    public string? IncreaseBrightness { get; set; }

    /// <summary>降低亮度的快捷键</summary>
    public string? DecreaseBrightness { get; set; }
}

/// <summary>
/// 应用配置，包含 ddccli 路径、模式设置和快捷键
/// </summary>
public class AppConfig {
    /// <summary>
    /// ddccli.exe 路径（支持绝对路径和相对路径）
    /// </summary>
    public string? DdccliPath { get; set; }

    /// <summary>
    /// 模式配置（键：模式名称，值：时间段设置列表）
    /// </summary>
    public required Dictionary<string, List<TimeSetting>> Modes { get; set; }

    /// <summary>
    /// 快捷键配置
    /// </summary>
    public HotkeyConfig? Hotkeys { get; set; }
}

/// <summary>
/// 托盘应用上下文，管理系统托盘图标、菜单、定时器和快捷键
/// </summary>
public class TrayApplicationContext : ApplicationContext {
    private NotifyIcon? trayIcon;
    private AppConfig? config;
    private string currentMode = "Daily";
    private System.Windows.Forms.Timer? timer;
    private TimeSetting? lastAppliedSetting;
    private int refreshIntervalMinutes = 1;
    private bool isExecuting;

    /// <summary>亮度调整步长</summary>
    private const int BrightnessStep = 5;

    /// <summary>可选的刷新间隔（分钟）</summary>
    private static readonly int[] RefreshIntervals = [1, 2, 5, 10];

    /// <summary>托盘提示文本最大长度（NotifyIcon 限制 128 字符）</summary>
    private const int MaxTooltipLength = 127;

    #region Win32 快捷键 API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>Win32 修饰键标志</summary>
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    /// <summary>WM_HOTKEY 消息</summary>
    private const int WM_HOTKEY = 0x0312;

    /// <summary>热键 ID 计数器</summary>
    private int hotkeyIdCounter;

    /// <summary>热键 ID 到操作的映射表</summary>
    private readonly Dictionary<int, Action> hotkeyActions = [];

    /// <summary>已注册的热键 ID 列表</summary>
    private readonly List<int> registeredHotkeyIds = [];

    /// <summary>用于接收 WM_HOTKEY 消息的隐藏窗口</summary>
    private HotkeyWindow? hotkeyWindow;

    #endregion

    /// <summary>
    /// 初始化托盘应用：加载配置、创建托盘图标、注册快捷键、启动定时器
    /// </summary>
    public TrayApplicationContext() {
        LoadConfig();

        if (config is null) {
            Program.ShowError("配置加载失败");
            Environment.Exit(1);
        }

        trayIcon = new NotifyIcon {
            Icon = LoadTrayIcon(),
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true,
            Text = BuildTooltipText()
        };

        trayIcon.DoubleClick += (_, _) => ApplySettings(force: true);

        // 创建隐藏窗口用于接收 WM_HOTKEY 消息
        hotkeyWindow = new HotkeyWindow(hotkeyActions);

        BuildMenu();
        RegisterHotkeys();

        timer = new System.Windows.Forms.Timer {
            Interval = refreshIntervalMinutes * 60000
        };
        timer.Tick += (_, _) => ApplySettings();
        timer.Start();

        ApplySettings(force: true);
        Program.Log("托盘应用初始化完成");
    }

    #region 图标与路径

    /// <summary>
    /// 加载托盘图标，优先级：assets/icon.ico → assets/icon.png → 系统默认图标
    /// </summary>
    private static Icon LoadTrayIcon() {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        string icoPath = Path.Combine(baseDir, "assets", "icon.ico");
        if (File.Exists(icoPath)) {
            try { return new Icon(icoPath); } catch (Exception ex) { Program.Log($"加载 icon.ico 失败: {ex.Message}"); }
        }

        string pngPath = Path.Combine(baseDir, "assets", "icon.png");
        if (File.Exists(pngPath)) {
            try {
                using var bmp = new Bitmap(pngPath);
                return Icon.FromHandle(bmp.GetHicon());
            } catch (Exception ex) { Program.Log($"加载 icon.png 失败: {ex.Message}"); }
        }

        Program.Log("未找到自定义图标，使用系统默认图标");
        return SystemIcons.Application;
    }

    /// <summary>
    /// 查找 ddccli.exe，优先级：配置路径 → 当前目录 → PATH 环境变量
    /// </summary>
    /// <returns>ddccli.exe 的完整路径；未找到时返回 <c>null</c></returns>
    private string? FindDdccli() {
        if (config is null) return null;

        // 1. 配置路径
        if (!string.IsNullOrWhiteSpace(config.DdccliPath)) {
            string configuredPath = config.DdccliPath;
            string resolvedPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);

            if (File.Exists(resolvedPath)) {
                Program.Log($"使用配置路径: {resolvedPath}");
                return resolvedPath;
            }
        }

        // 2. 当前目录
        string localDdccli = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ddccli.exe");
        if (File.Exists(localDdccli)) {
            Program.Log($"在当前目录找到 ddccli.exe: {localDdccli}");
            return localDdccli;
        }

        // 3. PATH 环境变量
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv)) {
            foreach (string dir in pathEnv.Split(';')) {
                if (string.IsNullOrWhiteSpace(dir)) continue;
                string ddccliInPath = Path.Combine(dir.Trim(), "ddccli.exe");
                if (File.Exists(ddccliInPath)) {
                    Program.Log($"在 PATH 中找到 ddccli.exe: {ddccliInPath}");
                    return ddccliInPath;
                }
            }
        }

        return null;
    }

    #endregion

    #region 配置管理

    /// <summary>
    /// 从 config.json 加载配置，失败时退出程序
    /// </summary>
    private void LoadConfig() {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        if (!File.Exists(configPath)) {
            Program.ShowError("找不到 config.json 配置文件");
            Environment.Exit(1);
        }

        try {
            string jsonContent = File.ReadAllText(configPath);
            config = JsonSerializer.Deserialize<AppConfig>(jsonContent);

            if (!ValidateConfig(config)) {
                Environment.Exit(1);
            }

            Program.Log("配置文件加载成功");
        } catch (JsonException ex) {
            Program.ShowError("配置文件格式错误", ex);
            Environment.Exit(1);
        } catch (Exception ex) {
            Program.ShowError("加载配置文件失败", ex);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// 验证配置有效性：检查模式非空、时间格式、亮度/对比度范围
    /// </summary>
    /// <param name="cfg">待验证的配置对象</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateConfig(AppConfig? cfg) {
        if (cfg is null) {
            Program.ShowError("配置对象为空");
            return false;
        }

        List<string> errors = [];

        if (cfg.Modes is null or { Count: 0 }) {
            errors.Add("Modes 配置不能为空");
        } else {
            foreach (var (modeName, settings) in cfg.Modes) {
                if (string.IsNullOrWhiteSpace(modeName)) {
                    errors.Add("模式名称不能为空");
                    continue;
                }

                if (settings is null or { Count: 0 }) {
                    errors.Add($"模式 '{modeName}' 的时间设置不能为空");
                    continue;
                }

                foreach (var setting in settings) {
                    if (setting is null) {
                        errors.Add($"模式 '{modeName}' 中存在空的时间设置项");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(setting.Time)) {
                        errors.Add($"模式 '{modeName}' 中存在空的时间设置");
                    } else if (!TimeSpan.TryParse(setting.Time, out _)) {
                        errors.Add($"模式 '{modeName}' 中的时间格式错误: {setting.Time}");
                    }

                    if (setting.Brightness is < 0 or > 100) {
                        errors.Add($"模式 '{modeName}' 中的亮度值超出范围 (0-100): {setting.Brightness}");
                    }

                    if (setting.Contrast is < 0 or > 100) {
                        errors.Add($"模式 '{modeName}' 中的对比度值超出范围 (0-100): {setting.Contrast}");
                    }
                }
            }
        }

        if (errors.Count > 0) {
            Program.ShowError("配置验证失败:\n" + string.Join("\n", errors));
            return false;
        }

        return true;
    }

    /// <summary>
    /// 重载配置文件：重新读取并验证 config.json，更新模式、快捷键等
    /// </summary>
    private void ReloadConfig() {
        if (trayIcon is null) return;

        try {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(configPath)) {
                trayIcon.ShowBalloonTip(3000, "错误", "找不到 config.json", ToolTipIcon.Error);
                return;
            }

            string jsonContent = File.ReadAllText(configPath);
            var newConfig = JsonSerializer.Deserialize<AppConfig>(jsonContent);

            if (newConfig is null || newConfig.Modes is null) {
                Program.Log("配置文件无效");
                trayIcon.ShowBalloonTip(3000, "错误", "配置文件无效", ToolTipIcon.Error);
                return;
            }

            if (!ValidateConfig(newConfig)) return;

            // 注销旧热键 → 更新配置 → 注册新热键
            UnregisterAllHotkeys();
            config = newConfig;

            if (!newConfig.Modes.ContainsKey(currentMode)) {
                currentMode = newConfig.Modes.Keys.First();
            }

            lastAppliedSetting = null;
            ApplySettings(force: true);
            BuildMenu();
            RegisterHotkeys();
            UpdateTooltip();

            Program.Log("配置文件已重载");
            trayIcon.ShowBalloonTip(2000, "提示", "配置文件已重载", ToolTipIcon.Info);
        } catch (JsonException ex) {
            Program.Log($"配置文件格式错误: {ex.Message}");
            trayIcon.ShowBalloonTip(3000, "错误", "配置文件格式错误", ToolTipIcon.Error);
        } catch (Exception ex) {
            Program.Log($"重载配置失败: {ex.Message}");
            trayIcon.ShowBalloonTip(3000, "错误", "重载配置失败", ToolTipIcon.Error);
        }
    }

    #endregion

    #region 设置应用

    /// <summary>
    /// 根据当前时间获取指定模式的有效设置
    /// </summary>
    /// <param name="modeName">模式名称</param>
    /// <returns>当前时段对应的时间设置；未找到时返回 <c>null</c></returns>
    private TimeSetting? GetActiveSettingForMode(string modeName) {
        if (config?.Modes is null || !config.Modes.TryGetValue(modeName, out var settings))
            return null;

        var sorted = settings.OrderByDescending(s => s.ToTimeSpan()).ToList();
        if (sorted.Count == 0) return null;

        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        return sorted.FirstOrDefault(s => currentTime >= s.ToTimeSpan()) ?? sorted.Last();
    }

    /// <summary>
    /// 获取当前模式下下一个切换时间点
    /// </summary>
    /// <returns>下一个时间设置；无则返回 <c>null</c></returns>
    private TimeSetting? GetNextSetting() {
        if (config?.Modes is null || !config.Modes.TryGetValue(currentMode, out var settings))
            return null;

        var sorted = settings.OrderBy(s => s.ToTimeSpan()).ToList();
        if (sorted.Count == 0) return null;

        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        return sorted.FirstOrDefault(s => currentTime < s.ToTimeSpan()) ?? sorted.First();
    }

    /// <summary>
    /// 应用当前模式下的亮度/对比度设置
    /// </summary>
    /// <param name="force">为 <c>true</c> 时忽略缓存，强制重新应用</param>
    private void ApplySettings(bool force = false) {
        if (config?.Modes is null || !config.Modes.ContainsKey(currentMode)) {
            Program.Log($"当前模式 '{currentMode}' 不存在于配置中");
            return;
        }

        var activeSetting = GetActiveSettingForMode(currentMode);
        if (activeSetting is null) {
            Program.Log($"模式 '{currentMode}' 没有时间设置");
            return;
        }

        if (force || lastAppliedSetting is null ||
            lastAppliedSetting.Brightness != activeSetting.Brightness ||
            lastAppliedSetting.Contrast != activeSetting.Contrast) {
            lastAppliedSetting = activeSetting;
            ExecuteDdccliAsync(activeSetting.Brightness, activeSetting.Contrast);
            UpdateTooltip();
        }
    }

    /// <summary>
    /// 异步执行 ddccli 命令设置显示器亮度和对比度
    /// </summary>
    /// <param name="brightness">亮度值（0-100）</param>
    /// <param name="contrast">对比度值（0-100）</param>
    private async void ExecuteDdccliAsync(int brightness, int contrast) {
        if (trayIcon is null || isExecuting) return;
        isExecuting = true;

        string? ddccliPath = FindDdccli();
        if (string.IsNullOrEmpty(ddccliPath)) {
            const string errorMsg = "找不到 ddccli.exe（搜索了当前目录和 PATH）";
            Program.Log(errorMsg);
            trayIcon.ShowBalloonTip(3000, "错误", errorMsg, ToolTipIcon.Error);
            isExecuting = false;
            return;
        }

        try {
            using var process = Process.Start(new ProcessStartInfo {
                FileName = ddccliPath,
                Arguments = $"-b {brightness} -c {contrast}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process is null) {
                Program.Log("无法启动 ddccli 进程");
                trayIcon.ShowBalloonTip(3000, "错误", "无法启动 ddccli 进程", ToolTipIcon.Error);
                return;
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0) {
                string? errorOutput = await process.StandardError.ReadToEndAsync();
                string errorMsg = $"ddccli 执行失败 (ExitCode: {process.ExitCode})";
                if (!string.IsNullOrWhiteSpace(errorOutput))
                    errorMsg += $": {errorOutput}";

                Program.Log(errorMsg);
                trayIcon.ShowBalloonTip(3000, "错误", errorMsg, ToolTipIcon.Error);
            } else {
                Program.Log($"已应用设置 - 亮度: {brightness}, 对比度: {contrast}");
            }
        } catch (Exception ex) {
            Program.Log($"执行 ddccli 失败: {ex.Message}");
            trayIcon.ShowBalloonTip(3000, "错误", $"执行 ddccli 失败: {ex.Message}", ToolTipIcon.Error);
        } finally {
            isExecuting = false;
        }
    }

    /// <summary>
    /// 手动调整亮度（增减 <see cref="BrightnessStep"/>），值域自动钳制到 0-100
    /// </summary>
    /// <param name="delta">调整量（正数增加，负数减少）</param>
    private void AdjustBrightness(int delta) {
        if (trayIcon is null || config?.Modes is null || !config.Modes.ContainsKey(currentMode))
            return;

        var activeSetting = GetActiveSettingForMode(currentMode);
        if (activeSetting is null) return;

        int newBrightness = Math.Clamp(activeSetting.Brightness + delta, 0, 100);

        lastAppliedSetting = null;
        ExecuteDdccliAsync(newBrightness, activeSetting.Contrast);

        trayIcon.ShowBalloonTip(2000, "提示", $"亮度已调整为 {newBrightness}", ToolTipIcon.Info);
        Program.Log($"手动调整亮度: {activeSetting.Brightness} -> {newBrightness}");
    }

    #endregion

    #region 托盘菜单与提示

    /// <summary>
    /// 构建托盘右键菜单
    /// </summary>
    private void BuildMenu() {
        if (trayIcon?.ContextMenuStrip is null || config?.Modes is null) return;

        var menu = trayIcon.ContextMenuStrip;
        menu.Items.Clear();

        // 模式切换
        var modeMenuItem = new ToolStripMenuItem("切换模式");
        foreach (var mode in config.Modes.Keys) {
            var activeSetting = GetActiveSettingForMode(mode);
            string suffix = activeSetting is not null
                ? $" [{activeSetting.Brightness}/{activeSetting.Contrast}]"
                : "";

            modeMenuItem.DropDownItems.Add(new ToolStripMenuItem(
                $"{mode}{suffix}", null, (_, _) => SwitchMode(mode)) { Checked = mode == currentMode });
        }
        menu.Items.Add(modeMenuItem);
        menu.Items.Add(new ToolStripSeparator());

        // 操作
        menu.Items.Add("手动刷新", null, (_, _) => ApplySettings(force: true));
        menu.Items.Add("重载配置", null, (_, _) => ReloadConfig());

        // 刷新间隔
        var intervalMenuItem = new ToolStripMenuItem("刷新间隔");
        foreach (int interval in RefreshIntervals) {
            intervalMenuItem.DropDownItems.Add(new ToolStripMenuItem(
                $"{interval} 分钟", null, (_, _) => SetRefreshInterval(interval)) { Checked = refreshIntervalMinutes == interval });
        }
        menu.Items.Add(intervalMenuItem);
        menu.Items.Add(new ToolStripSeparator());

        // 开机自启
        var autoStartMenuItem = new ToolStripMenuItem("开机自启");
        bool isAutoStartEnabled = IsAutoStartEnabled();
        autoStartMenuItem.DropDownItems.Add(new ToolStripMenuItem("启用", null, (_, _) => SetAutoStart(true)) { Checked = isAutoStartEnabled });
        autoStartMenuItem.DropDownItems.Add(new ToolStripMenuItem("禁用", null, (_, _) => SetAutoStart(false)) { Checked = !isAutoStartEnabled });
        menu.Items.Add(autoStartMenuItem);

        menu.Items.Add("退出", null, (_, _) => Exit());
    }

    /// <summary>
    /// 构建托盘提示文本，包含当前模式、亮度、对比度和下次切换时间
    /// </summary>
    private string BuildTooltipText() {
        var activeSetting = GetActiveSettingForMode(currentMode);
        var nextSetting = GetNextSetting();

        string brightness = activeSetting is not null ? $"亮度:{activeSetting.Brightness}" : "";
        string contrast = activeSetting is not null ? $" 对比度:{activeSetting.Contrast}" : "";

        string nextSwitch = nextSetting is not null
            ? $" 下次切换:{nextSetting.Time}"
            : "";

        string text = $"{currentMode} {brightness}{contrast}{nextSwitch}";

        // NotifyIcon.Text 最大 128 字符
        return text.Length > MaxTooltipLength
            ? text[..MaxTooltipLength]
            : text;
    }

    /// <summary>
    /// 更新托盘图标提示文本
    /// </summary>
    private void UpdateTooltip() {
        if (trayIcon is not null)
            trayIcon.Text = BuildTooltipText();
    }

    /// <summary>
    /// 切换到指定模式
    /// </summary>
    /// <param name="newMode">目标模式名称</param>
    private void SwitchMode(string newMode) {
        if (currentMode == newMode) return;

        currentMode = newMode;
        BuildMenu();
        ApplySettings(force: true);
        UpdateTooltip();
        Program.Log($"切换到 {newMode} 模式");
    }

    /// <summary>
    /// 设置定时刷新间隔
    /// </summary>
    /// <param name="minutes">间隔分钟数</param>
    private void SetRefreshInterval(int minutes) {
        refreshIntervalMinutes = minutes;
        if (timer is not null)
            timer.Interval = minutes * 60000;

        BuildMenu();
        Program.Log($"刷新间隔已设置为 {minutes} 分钟");
        trayIcon?.ShowBalloonTip(2000, "提示", $"刷新间隔已设置为 {minutes} 分钟", ToolTipIcon.Info);
    }

    #endregion

    #region 开机自启

    /// <summary>
    /// 获取自启动快捷方式路径
    /// </summary>
    private static string GetStartupShortcutPath() {
        string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string appName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
        return Path.Combine(startupFolder, $"{appName}.lnk");
    }

    /// <summary>
    /// 检查是否已启用开机自启
    /// </summary>
    private static bool IsAutoStartEnabled() => File.Exists(GetStartupShortcutPath());

    /// <summary>
    /// 设置开机自启状态：启用时创建快捷方式，禁用时删除
    /// </summary>
    /// <param name="enable"><c>true</c> 启用，<c>false</c> 禁用</param>
    private void SetAutoStart(bool enable) {
        if (trayIcon is null) return;

        string shortcutPath = GetStartupShortcutPath();

        try {
            if (enable) {
                string exePath = Application.ExecutablePath;
                CreateShortcut(shortcutPath, exePath, "显示器亮度控制托盘程序");
                Program.Log("已启用开机自启");
            } else {
                if (File.Exists(shortcutPath)) {
                    File.Delete(shortcutPath);
                    Program.Log("已禁用开机自启");
                }
            }

            BuildMenu();
        } catch (Exception ex) {
            Program.Log($"设置自启动失败: {ex.Message}");
            trayIcon.ShowBalloonTip(3000, "错误", $"设置自启动失败: {ex.Message}", ToolTipIcon.Error);
        }
    }

    /// <summary>
    /// 创建 Windows 快捷方式（.lnk），优先使用 COM，失败时回退到 PowerShell
    /// </summary>
    /// <param name="shortcutPath">快捷方式保存路径</param>
    /// <param name="targetPath">目标程序路径</param>
    /// <param name="description">快捷方式描述</param>
    private static void CreateShortcut(string shortcutPath, string targetPath, string description) {
        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null) {
            CreateShortcutViaPowerShell(shortcutPath, targetPath, description);
            return;
        }

        object? shellObj = Activator.CreateInstance(shellType);
        if (shellObj is null) {
            CreateShortcutViaPowerShell(shortcutPath, targetPath, description);
            return;
        }

        try {
            object? shortcut = shellType.InvokeMember(
                "CreateShortcut", BindingFlags.InvokeMethod, null, shellObj, [shortcutPath]);

            if (shortcut is not null) {
                Type scType = shortcut.GetType();
                string? workingDir = Path.GetDirectoryName(targetPath);

                scType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, [targetPath]);
                scType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, [description]);
                scType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, [workingDir ?? string.Empty]);
                scType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
        } finally {
            Marshal.ReleaseComObject(shellObj);
        }
    }

    /// <summary>
    /// 通过 PowerShell 创建快捷方式（COM 方式的回退方案）
    /// </summary>
    private static void CreateShortcutViaPowerShell(string shortcutPath, string targetPath, string description) {
        string workingDir = Path.GetDirectoryName(targetPath) ?? string.Empty;
        string psScript = $"""
            $WshShell = New-Object -ComObject WScript.Shell
            $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
            $Shortcut.TargetPath = '{targetPath}'
            $Shortcut.Description = '{description}'
            $Shortcut.WorkingDirectory = '{workingDir}'
            $Shortcut.Save()
            """;

        var psi = new ProcessStartInfo {
            FileName = "powershell",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript.Replace('"', '\'').Replace("'", "''")}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        process?.WaitForExit();
    }

    #endregion

    #region 全局快捷键

    /// <summary>
    /// 注册所有配置的快捷键
    /// </summary>
    private void RegisterHotkeys() {
        if (config?.Hotkeys is null) return;

        RegisterHotkey(config.Hotkeys.SwitchToGameMode, () => SwitchMode("Game"));
        RegisterHotkey(config.Hotkeys.SwitchToDailyMode, () => SwitchMode("Daily"));
        RegisterHotkey(config.Hotkeys.ManualRefresh, () => ApplySettings(force: true));
        RegisterHotkey(config.Hotkeys.IncreaseBrightness, () => AdjustBrightness(BrightnessStep));
        RegisterHotkey(config.Hotkeys.DecreaseBrightness, () => AdjustBrightness(-BrightnessStep));
    }

    /// <summary>
    /// 注册单个快捷键：解析字符串并通过 RegisterHotKey API 注册
    /// </summary>
    /// <param name="hotkeyStr">快捷键字符串（如 "Ctrl+Shift+G"）</param>
    /// <param name="action">触发时执行的操作</param>
    private void RegisterHotkey(string? hotkeyStr, Action action) {
        if (string.IsNullOrWhiteSpace(hotkeyStr)) return;

        if (!TryParseHotkey(hotkeyStr, out uint modifiers, out uint vk)) {
            Program.Log($"快捷键格式无效: {hotkeyStr}");
            return;
        }

        int id = ++hotkeyIdCounter;
        IntPtr hWnd = hotkeyWindow?.Handle ?? IntPtr.Zero;
        if (hWnd == IntPtr.Zero) {
            Program.Log($"快捷键窗口未就绪，跳过注册: {hotkeyStr}");
            return;
        }

        if (RegisterHotKey(hWnd, id, modifiers, vk)) {
            hotkeyActions[id] = action;
            registeredHotkeyIds.Add(id);
            Program.Log($"已注册快捷键: {hotkeyStr} -> {action.Method.Name}");
        } else {
            int error = Marshal.GetLastWin32Error();
            Program.Log($"注册快捷键失败: {hotkeyStr} (Win32 Error: {error})");
        }
    }

    /// <summary>
    /// 解析快捷键字符串为 Win32 修饰键标志和虚拟键码
    /// </summary>
    /// <param name="hotkeyStr">快捷键字符串</param>
    /// <param name="modifiers">输出的修饰键标志</param>
    /// <param name="vk">输出的虚拟键码</param>
    /// <returns>解析是否成功</returns>
    private static bool TryParseHotkey(string hotkeyStr, out uint modifiers, out uint vk) {
        modifiers = 0;
        vk = 0;

        if (string.IsNullOrWhiteSpace(hotkeyStr)) return false;

        string[] parts = hotkeyStr.Split('+');
        Keys key = Keys.None;

        foreach (string part in parts) {
            string keyName = part.Trim().ToUpperInvariant();
            switch (keyName) {
                case "CTRL" or "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "ALT" or "MENU":
                    modifiers |= MOD_ALT;
                    break;
                case "WIN" or "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    try { key = (Keys)Enum.Parse(typeof(Keys), keyName, ignoreCase: true); } catch { return false; }
                    break;
            }
        }

        if (key == Keys.None) return false;
        vk = (uint)key;
        return true;
    }

    /// <summary>
    /// 注销所有已注册的快捷键
    /// </summary>
    private void UnregisterAllHotkeys() {
        IntPtr hWnd = hotkeyWindow?.Handle ?? IntPtr.Zero;
        foreach (int id in registeredHotkeyIds) {
            if (hWnd != IntPtr.Zero)
                UnregisterHotKey(hWnd, id);
        }
        registeredHotkeyIds.Clear();
        hotkeyActions.Clear();
    }

    #endregion

    #region 生命周期

    /// <summary>
    /// 退出应用程序：释放所有资源
    /// </summary>
    private void Exit() {
        UnregisterAllHotkeys();

        if (timer is not null) {
            timer.Stop();
            timer.Dispose();
        }

        if (trayIcon is not null) {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        Program.Log("程序退出");
        Application.Exit();
    }

    /// <summary>
    /// 释放托管和非托管资源
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            UnregisterAllHotkeys();
            timer?.Dispose();
            trayIcon?.Dispose();
            hotkeyWindow?.DestroyHandle();
        }
        base.Dispose(disposing);
    }

    #endregion

    /// <summary>
    /// 隐藏窗口，用于接收 WM_HOTKEY 消息并触发快捷键操作
    /// </summary>
    private class HotkeyWindow : NativeWindow {
        private readonly Dictionary<int, Action> hotkeyActions;

        /// <summary>
        /// 创建隐藏窗口并绑定快捷键映射表
        /// </summary>
        /// <param name="actions">热键 ID 到操作的映射表</param>
        public HotkeyWindow(Dictionary<int, Action> actions) {
            hotkeyActions = actions;
            CreateHandle(new CreateParams { Parent = new IntPtr(-3) }); // HWND_MESSAGE
        }

        /// <summary>
        /// 处理 Windows 消息，拦截 WM_HOTKEY 触发快捷键操作
        /// </summary>
        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                int id = m.WParam.ToInt32();
                if (hotkeyActions.TryGetValue(id, out Action? action)) {
                    try { action.Invoke(); } catch (Exception ex) { Program.Log($"快捷键执行失败: {ex.Message}"); }
                }
            }

            base.WndProc(ref m);
        }
    }
}
