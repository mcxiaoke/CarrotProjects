using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GenshinNotifier {

    internal static class Program {
        // single instance impl
        // http://sanity-free.org/143/csharp_dotnet_single_instance_application.html
        // https://www.codeproject.com/Articles/32908/C-Single-Instance-App-With-the-Ability-To-Restore

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            bool onlyInstance = false;
            Mutex mutex = new Mutex(true, @"Global\" + Storage.AppGuidStr, out onlyInstance);
            if (!onlyInstance) {
                MessageBox.Show("检测到另一个实例正在运行，请勿重复开启！", Storage.AppName, MessageBoxButtons.OK);
                // bring prev instance to front
                UDPService.SendUDP(Storage.AppGuidStr);
                return;
            }

#if DEBUG
            NativeHelper.AllocConsole();
#endif
            if (args.Length > 0) {
                Console.WriteLine(string.Join(" ", args));
            }
            CheckSettingsUpgrade();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UDPService.BeginUDP();

            bool shouldHide = (args.Length == 1 && args[0] == "--autostart");
            Application.Run(new CustomApplicationContext(new MainForm(shouldHide), shouldHide));

            ToastNotificationManagerCompat.Uninstall();
            SchedulerController.Default.Stop();
            Logger.Close();
            NativeHelper.FreeConsole();
            GC.KeepAlive(mutex);
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
