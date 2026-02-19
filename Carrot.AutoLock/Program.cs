using Carrot.Common;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;



namespace Carrot.AutoLock {
    /// <summary>
    /// 应用程序入口类
    /// </summary>
    internal static class Program {
        /// <summary>
        /// 用于显示窗口的 IPC 命令
        /// </summary>
        public static string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";

        /// <summary>
        ///  应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            // 使用全局 Mutex 确保单实例运行
            Mutex mutex = new Mutex(true, @"Global\" + ProComConst.PIPE_MAIN, out bool onlyInstance);
            if (!onlyInstance) {
                // 如果已有实例在运行，提示用户并尝试唤醒前一个实例
                MessageBox.Show("检测到另一个实例正在运行，请勿重复开启！", Application.ProductName, MessageBoxButtons.OK);
                // 通过命名管道发送命令唤醒前一个实例
                //UDPService.SendUDP(Storage.AppGuidStr);
                PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
                return;
            }
            // 自定义应用程序配置，例如设置高 DPI 设置或默认字体
            // 详见 https://aka.ms/applicationconfiguration.

#if (NET6_0_OR_GREATER)
            //Console.WriteLine("Using .NET 6+ or .NET Standard 2+ code.");
            ApplicationConfiguration.Initialize();
#else
            //Console.WriteLine("Using older code that doesn't support the above .NET versions.");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName);
            Application.Run(new MainForm());
        }
    }
}