using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Windows;

namespace NotifierWidget {

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private GlobalExceptionHandler _handler;


        public App() {
            _handler = new GlobalExceptionHandler();
            this.LoadCompleted += App_LoadCompleted;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            Debug.WriteLine("App_LoadCompleted");
            WidgetStyle.Default.LoadStyles();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            var error = e.Exception;
            Debug.WriteLine($"App_DispatcherUnhandledException: {error.Message} {error.StackTrace}");
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            Debug.WriteLine("OnStartup");
        }
    }
}