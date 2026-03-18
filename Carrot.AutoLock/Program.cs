using System;
using System.Threading;
using System.Windows.Forms;
using Carrot.Common;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
// using Carrot.AutoLock.ProCom; // Check if this namespace exists or needs adjustment

namespace Carrot.AutoLock;

/// <summary>
/// The main entry point for the Carrot.AutoLock application.
/// Carrot.AutoLock Ӧ�ó��������ڵ㡣
///
/// This application monitors a target device's online status and automatically
/// locks the workstation when the device goes offline and the user is inactive.
/// ���Ӧ�ó���������Ŀ���豸������״̬�������豸�����߲����û��ʱ�Զ����乤��վ��
/// </summary>
internal static class Program {
    /// <summary>
    ///  The command to show the window via IPC.
    ///  ����ͨ�� IPC ��ʾ���ڵ����
    /// </summary>
    public const string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";

    /// <summary>
    ///  The main entry point for the application.
    ///  Ӧ�ó��������ڵ㡣
    /// </summary>
    [STAThread]
    static void Main() {
        // Use a global Mutex to ensure a single instance.
        // ʹ��ȫ�� Mutex ȷ����ʵ�����С�
        using var mutex = new Mutex(true, @$"Global\{ProComConst.PIPE_MAIN}", out bool isNewInstance);
        
        if (!isNewInstance) {
            // Try to activate the existing instance via IPC.
            // ����ͨ�� IPC ����ǰһ��ʵ����
            var (_, error) = PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
            if (error != null) {
                Logger.Warning($"Failed to wake existing instance: {error.Message}");
            }
            return;
        }

        // Initialize application configuration.
        // ��ʼ��Ӧ�ó������á�
        // See https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Log the current framework version for debugging.
        // ��¼��ǰ��ܰ汾�Ա���ԡ�
        Logger.Info($"Starting Carrot.AutoLock v{Application.ProductVersion}");
        Logger.Info($"Framework: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");
        // Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName);
        
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
    }
}