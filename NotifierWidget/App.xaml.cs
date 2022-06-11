using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.Windows;
using NotifierWidget.Properties;

namespace NotifierWidget {

    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private GlobalExceptionHandler _handler;


        public App() {
            _handler = new GlobalExceptionHandler();
            this.Activated += App_Activated;
            this.Deactivated += App_Deactivated;
            this.Startup += App_Startup;
            this.Exit += App_Exit;
            this.LoadCompleted += App_LoadCompleted;
        }

        private void App_Exit(object sender, ExitEventArgs e) {
            Debug.WriteLine("App_Exit");
            Settings.Default.Save();
        }

        private void App_Startup(object sender, StartupEventArgs e) {
            Debug.WriteLine("App_Startup");
            WidgetStyle.Default.Initialize();
        }

        private void App_Deactivated(object sender, EventArgs e) {
            Debug.WriteLine("App_Deactivated");
        }

        private void App_Activated(object sender, EventArgs e) {
            Debug.WriteLine("App_Activated");
        }

        private void App_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            Debug.WriteLine("App_LoadCompleted");
        }

    }
}