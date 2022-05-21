using System;
using System.Windows.Forms;

namespace GenshinNotifier {

    internal static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
#if DEBUG
            NativeHelper.AllocConsole();
#endif
            CheckSettings();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            Logger.Close();
        }

        static void CheckSettings() {
            if (Properties.Settings.Default.UpgradeRequired) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
