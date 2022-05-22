using System;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GenshinNotifier {

    internal static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
#if DEBUG
            NativeHelper.AllocConsole();
#endif
            if (args.Length > 0) {
                Console.WriteLine(string.Join(" ", args));
            }
            CheckSettingsUpgrade();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool shouldHide = (args.Length == 1 && args[0] == "--autostart");
            Application.Run(new CustomApplicationContext(new MainForm(shouldHide), shouldHide));
            ToastNotificationManagerCompat.Uninstall();
            SchedulerController.Default.Stop();
            Logger.Close();
            NativeHelper.FreeConsole();
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
