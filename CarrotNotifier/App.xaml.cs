using System;
using System.Diagnostics;
using System.Windows;
using GenshinNotifier.Properties;
using GenshinNotifier;
using System.Threading;
using System.Windows.Controls;
using CarrotCommon;
using Carrot.ProCom.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using H.NotifyIcon;

namespace GenshinNotifier {

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private const string GUID_STR = "{82761839-E200-402E-8C1D-2FDE9571239C}";

        private GlobalExceptionHandler? _handler;
        public TaskbarIcon? TrayIcon;
        private Mutex? mutex;

        public App() {
            _handler = new GlobalExceptionHandler();
            this.Startup += App_Startup;
            this.Exit += App_Exit;
        }

        private void App_Startup(object sender, StartupEventArgs e) {
            // https://stackoverflow.com/questions/14506406/wpf-single-instance-best-practices
            // https://github.com/it3xl/WPF-app-Single-Instance-in-one-line-of-code
            // https://weblog.west-wind.com/posts/2016/may/13/creating-single-instance-wpf-applications-that-open-multiple-files
            mutex = new Mutex(true, @"Global\" + GUID_STR, out bool onlyInstance);
            if (!onlyInstance) {
                Debug.WriteLine($"=== Warning: {AppInfo.ProductName} is already running, exit now! ===");
                MessageBox.Show("检测到另一个实例正在运行，请勿重复开启！", AppInfo.ProductName, MessageBoxButton.OK);
                // bring prev instance to front
                // AppService.SendCmdShowWindow();
                Environment.Exit(0);
                return;
            }
            Logger.Debug("=======================================");
            Logger.Debug("App_Startup");
            OnAppStart();
        }

        private void App_Exit(object sender, ExitEventArgs e) { OnAppStop(); }

        private void OnAppStart() {
#if DEBUG
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            //Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
#endif
            // init
            CheckSettingsUpgrade();
            if (Settings.Default.FirstLaunch) {
                Settings.Default.FirstLaunch = false;
                OnFirstLaunch();
            }
            var preload = DataController.Default;
            SchedulerController.Default.Initialize();
            WidgetStyle.Initialize();
            AppService.Start();
            ToastNotificationManagerCompat.OnActivated += OnNotificationActivated;
            // tray icon
            TrayIcon = (TaskbarIcon)FindResource("SysTrayIcon");
            TrayIcon.ForceCreate();
        }

        private void OnAppStop() {
            Settings.Default.Save();
            AppService.Stop();
            ToastNotificationManagerCompat.Uninstall();
            SchedulerController.Default.Stop();
            Logger.Close();
            TrayIcon?.Dispose();
            GC.KeepAlive(mutex);
        }


        private async void OnFirstLaunch() {
            Logger.Info("OnFirstLaunch");
            await AppUtils.CheckLocalAssets();
        }

        private void OnNotificationActivated(ToastNotificationActivatedEventArgsCompat toastArgs) {
            Logger.Debug($"OnNotificationActivated {toastArgs.Argument}");
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            var action = args.Get("action");
            // Need to dispatch to UI thread if performing UI operations
            if (action == "mute") {
                // mute current day
                SchedulerController.Default.MuteToday();
            } else if (action == "update") {
                AutoUpdater.ShowUpdater();
            }
        }

        private static void CheckSettingsUpgrade() {
            if (Settings.Default.UpgradeRequired) {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
        }
    }
}