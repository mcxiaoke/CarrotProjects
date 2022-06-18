using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NotifierWidget;

namespace Carrot.UI.Samples {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private GlobalExceptionHandler _handler;

        public App() {
            _handler = new GlobalExceptionHandler();
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
        }
    }
}
