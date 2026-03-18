using Carrot.Common;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Carrot.AutoLock;

/// <summary>
/// The main entry point for the Carrot.AutoLock application.
/// Carrot.AutoLock 应用程序入口点。
///
/// This application monitors a target device's online status and automatically
/// locks the workstation when the device goes offline and the user is inactive.
/// 该应用程序监控目标设备的在线状态，在设备离线且用户无活动时自动锁定工作站。
/// </summary>
internal static class Program {
    /// <summary>
    ///  The command to show the window via IPC.
    ///  通过 IPC 显示窗口的命令。
    /// </summary>
    public const string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";

    /// <summary>
    /// 内存日志目标实例，用于日志查看器
    /// Memory log target instance for log viewer
    /// </summary>
    public static MemoryLogTarget MemoryLog { get; private set; } = null!;

    /// <summary>
    ///  The main entry point for the application.
    ///  应用程序入口点。
    /// </summary>
    [STAThread]
    static void Main() {
        // 添加全局异常处理
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        try {
            // Use a global Mutex to ensure a single instance.
            // 使用全局 Mutex 确保单实例。
            using var mutex = new Mutex(true, @$"Global\{ProComConst.PIPE_MAIN}", out bool isNewInstance);

            if (!isNewInstance) {
                // Try to activate the existing instance via IPC.
                // 尝试通过 IPC 激活前一个实例。
                var (_, error) = PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
                if (error != null) {
                    Logger.Warning($"Failed to wake existing instance: {error.Message}");
                }
                return;
            }

            // Initialize application configuration.
            // 初始化应用程序配置。
            // See https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // 初始化内存日志目标并注册到 Logger
            // Initialize memory log target and register to Logger
            MemoryLog = new MemoryLogTarget(maxLines: 5000);
            Logger.AddSink(MemoryLog);

            // Log the current framework version for debugging.
            // 记录当前框架版本以便调试。
            Logger.Info($"Starting Carrot.AutoLock v{Application.ProductVersion}");
            Logger.Info($"Framework: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");
            Logger.Info($"App Data Path: {AppInfo.LocalAppDataPath}");
            Logger.Info($"OS Version: {Environment.OSVersion}");
            Logger.Info($"Runtime: {RuntimeInformation.FrameworkDescription}");

            var mainForm = new MainForm();
            PipeMessageHandler messageHandler = (_, message) => {
                if (!string.Equals(message, CmdShowWindow, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }

                if (mainForm.IsHandleCreated) {
                    if (mainForm.InvokeRequired) {
                        mainForm.BeginInvoke(mainForm.ShowWindow);
                    } else {
                        mainForm.ShowWindow();
                    }
                }

                return false;
            };
            PipeService.Default.MessageHandler += messageHandler;

            PipeService.Default.StartServer(ProComConst.PIPE_MAIN);
            try {
                Application.Run(mainForm);
            } finally {
                PipeService.Default.MessageHandler -= messageHandler;
                PipeService.Default.StopServer();
            }
        } catch (Exception ex) {
            Logger.Error("Application startup failed", ex);
            MessageBox.Show($"程序启动失败：\n\n{ex.Message}\n\n详细信息：\n{ex.StackTrace}",
                "启动错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
        Logger.Error("Unhandled thread exception", e.Exception);
        MessageBox.Show($"发生未处理的异常：\n\n{e.Exception.Message}\n\n详细信息：\n{e.Exception.StackTrace}",
            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
        if (e.ExceptionObject is Exception ex) {
            Logger.Error("Unhandled domain exception", ex);
            MessageBox.Show($"发生未处理的异常：\n\n{ex.Message}\n\n详细信息：\n{ex.StackTrace}",
                "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
