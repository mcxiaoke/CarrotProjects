using System;
using System.Threading;
using System.Windows.Forms;
using Carrot.Common;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
// using Carrot.AutoLock.ProCom; // Check if this namespace exists or needs adjustment

namespace Carrot.AutoLock;

/// <summary>
/// The main entry point for the application.
/// 应用程序的主入口点。
/// </summary>
internal static class Program {
    /// <summary>
    ///  The command to show the window via IPC.
    ///  用于通过 IPC 显示窗口的命令。
    /// </summary>
    public const string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";

    /// <summary>
    ///  The main entry point for the application.
    ///  应用程序的主入口点。
    /// </summary>
    [STAThread]
    static void Main() {
        // Use a global Mutex to ensure a single instance.
        // 使用全局 Mutex 确保单实例运行。
        using var mutex = new Mutex(true, @$"Global\{ProComConst.PIPE_MAIN}", out bool isNewInstance);
        
        if (!isNewInstance) {
            // Activate the existing instance if possible.
            // 如果程序已在运行，提示用户或激活前一个实例。
            MessageBox.Show("Another instance is already running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Try to activate the existing instance via IPC.
            // 尝试通过 IPC 唤醒前一个实例。
            PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
            return;
        }

        // Initialize application configuration.
        // 初始化应用程序配置。
        // See https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Log the current framework version for debugging.
        // 记录当前框架版本以便调试。
        // Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName);
        
        Application.Run(new MainForm());
    }
}