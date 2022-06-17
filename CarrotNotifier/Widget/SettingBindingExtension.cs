using System.Windows.Data;
using GenshinNotifier.Properties;

namespace GenshinNotifier {

    internal class SettingBindingExtension : Binding {

        public SettingBindingExtension() {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path) {
            Initialize();
        }

        private void Initialize() {
            this.Source = Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}