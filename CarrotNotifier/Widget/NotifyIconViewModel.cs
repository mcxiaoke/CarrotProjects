using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.NotifyIcon;
using System.Diagnostics;
using Carrot.Common;
using GenshinNotifier.Properties;
using Carrot.UI.Controls.Dialog;

namespace GenshinNotifier.Widget {
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel {

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand => new SimpleCommand {
            CanExecuteFunc = () => Application.Current.MainWindow?.Visibility != Visibility.Visible,
            CommandAction = () => Application.Current.MainWindow.Show(),
        };

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand => new SimpleCommand {
            CommandAction = () => Application.Current.MainWindow.Hide(),
            CanExecuteFunc = () => Application.Current.MainWindow?.Visibility == Visibility.Visible,
        };

        /// <summary>
        /// Toggle main window visibility
        /// </summary>
        public ICommand ToggleWindowCommand => new SimpleCommand {
            CommandAction = () => {
                bool visible = Application.Current.MainWindow?.Visibility == Visibility.Visible;
                if (visible) {
                    Application.Current.MainWindow?.Hide();
                } else {
                    Application.Current.MainWindow?.Show();
                }
            },
        };

        /// <summary>
        ///  Restart Application
        /// </summary>
        public ICommand RestartApplicationCommand => new SimpleCommand {
            CommandAction = () => MiscUtils.RestartApplication()
        };


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand => new SimpleCommand {
            CommandAction = () => {
                var resultOk = MessageDialog.Show(Application.Current.MainWindow,
            "程序关闭后将无法在桌面展示小组件，也不能提供系统通知提醒，确定退出吗？", "退出确认");
                if (resultOk) {
                    Application.Current.Shutdown();
                }

            }
        };

        /// <summary>
        ///  Check Program update
        /// </summary>
        public ICommand CheckUpdateCommand => new SimpleCommand {
            CommandAction = () => AutoUpdater.ShowUpdater()
        };

        /// <summary>
        ///  Open project page in browser
        /// </summary>
        public ICommand GoToProjectCommmand => new SimpleCommand {
            CommandAction = () => Process.Start(AutoUpdater.ProjectUrl)
        };
    }
}
