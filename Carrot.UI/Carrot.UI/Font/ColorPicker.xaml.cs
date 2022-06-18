using System;
using System.Windows;
using System.Windows.Controls;

namespace Carrot.UI.Controls.Font {

    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl {
        private ColorPickerViewModel viewModel;

        public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ColorChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(FontColor), typeof(ColorPicker), new UIPropertyMetadata(null));

        public ColorPicker() {
            InitializeComponent();
            this.viewModel = new ColorPickerViewModel();
            this.DataContext = this.viewModel;
        }

        public event EventHandler<RoutedEventArgs> ColorChanged {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        public FontColor SelectedColor {
            get => (FontColor)this.GetValue(SelectedColorProperty)
                    ?? AvailableColors.GetFontColor("Black");
            set {
                this.viewModel.SelectedFontColor = value;
                SetValue(SelectedColorProperty, value);
            }
        }

        private void RaiseColorChangedEvent() {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ColorPicker.ColorChangedEvent);
            RaiseEvent(newEventArgs);
        }

        private void SuperCombo_DropDownClosed(object sender, EventArgs e) {
            this.SetValue(SelectedColorProperty, this.viewModel.SelectedFontColor);
            this.RaiseColorChangedEvent();
        }

        private void SuperCombo_Loaded(object sender, RoutedEventArgs e) {
            this.SetValue(SelectedColorProperty, this.viewModel.SelectedFontColor);
        }
    }
}