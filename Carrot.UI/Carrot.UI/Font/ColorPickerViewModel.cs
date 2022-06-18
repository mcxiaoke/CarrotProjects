using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    internal class ColorPickerViewModel : INotifyPropertyChanged {
        private FontColor selectedFontColor;

        public ColorPickerViewModel() {
            this.selectedFontColor = AvailableColors.GetFontColor(Colors.Black);
            this.FontColors = new ReadOnlyCollection<FontColor>(AvailableColors.AllColors);
        }

        public ReadOnlyCollection<FontColor> FontColors { get; }

        public FontColor SelectedFontColor {
            get {
                return this.selectedFontColor;
            }

            set {
                if (this.selectedFontColor == value)
                    return;

                this.selectedFontColor = value;
                OnPropertyChanged(nameof(SelectedFontColor));
            }
        }

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members
    }
}