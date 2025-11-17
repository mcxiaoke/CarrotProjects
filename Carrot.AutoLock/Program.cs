using Carrot.Common;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;



namespace Carrot.AutoLock {
    internal static class Program {
        public static string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Mutex mutex = new Mutex(true, @"Global\" + ProComConst.PIPE_MAIN, out bool onlyInstance);
            if (!onlyInstance) {
                MessageBox.Show("检测到另一个实例正在运行，请勿重复开启！", Application.ProductName, MessageBoxButtons.OK);
                // bring prev instance to front
                //UDPService.SendUDP(Storage.AppGuidStr);
                PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
                return;
            }
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

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