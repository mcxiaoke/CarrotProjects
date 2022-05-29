using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using CarrotCommon;

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
            Mutex mutex = new Mutex(true, @"Global\" + Storage.AppGuidStr, out bool onlyInstance);
            if (!onlyInstance) {
                MessageBox.Show("检测到另一个实例正在运行，请勿重复开启！", Application.ProductName, MessageBoxButtons.OK);
                // bring prev instance to front
                UDPService.SendUDP(Storage.AppGuidStr);
                return;
            }
            CheckSettingsUpgrade();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // add exception handler
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            UDPService.BeginUDP();

            bool shouldHide = (args.Length == 1 && args[0] == "--autostart");
            Application.Run(new CustomApplicationContext(new MainForm(shouldHide), shouldHide));

            ToastNotificationManagerCompat.Uninstall();
            SchedulerController.Default.Stop();
            Logger.Close();
            UDPService.StopUDP();
            //NativeHelper.FreeConsole();
            GC.KeepAlive(mutex);
        }

        private static void CheckSettingsUpgrade() {
            if (Properties.Settings.Default.UpgradeRequired) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }

        // Handle the UI exceptions by showing a dialog box, and asking the user whether
        // or not they wish to abort execution.
        // NOTE: This exception cannot be kept from terminating the application - it can only
        // log the event, and inform the user about it.
        // https://makolyte.com/csharp-global-exception-event-handlers/
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            try {
                Exception ex = (Exception)e.ExceptionObject;
                Logger.Error("UnhandledException", ex);
                string errorMsg = "An application error occurred. Please contact the owner " +
                    "with the following information:\n\n";

                using (EventLog eventLog = new EventLog("Application")) {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry(errorMsg + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace, EventLogEntryType.Warning, 101, 1);
                }
#if DEBUG
                MessageBox.Show("Fatal Non-UI Error",
    "Fatal Non-UI Error. Could not write the error to the event log. Reason: "
    + ex.StackTrace, MessageBoxButtons.OK, MessageBoxIcon.Stop);
#endif
            } catch (Exception exc) {
#if DEBUG
                try {
                    MessageBox.Show("Fatal Non-UI Error",
                        "Fatal Non-UI Error. Could not write the error to the event log. Reason: "
                        + exc.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                } finally {
                    Application.Exit();
                }
#endif
            }
        }

        // Creates the error message and displays it.
        private static DialogResult ShowThreadExceptionDialog(string title, Exception e) {
            string errorMsg = "An application error occurred. Please contact the adminstrator " +
                "with the following information:\n\n";
            errorMsg = errorMsg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            return MessageBox.Show(errorMsg, title, MessageBoxButtons.OK,
                MessageBoxIcon.Stop);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) {
            DialogResult result = DialogResult.Cancel;
            try {
                result = ShowThreadExceptionDialog("Windows Forms Error", e.Exception);
            } catch {
                try {
                    MessageBox.Show("Fatal Windows Forms Error",
                        "Fatal Windows Forms Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                } finally {
                    Application.Exit();
                }
            }
            // Exits the program when the user clicks Abort.
            if (result == DialogResult.OK)
                Application.Exit();
        }
    }
}
