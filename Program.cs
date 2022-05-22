using System;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

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
            CheckSettingsUpgrade();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
            // hide on start
            Application.Run(new CustomApplicationContext(new MainForm()));
            ToastNotificationManagerCompat.Uninstall();
            SchedulerController.Default.Stop();
            Logger.Close();
        }

        static void CheckSettingsUpgrade() {
            if (Properties.Settings.Default.UpgradeRequired) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
