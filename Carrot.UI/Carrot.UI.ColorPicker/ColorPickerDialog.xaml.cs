using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Carrot.UI.ColorPicker {

    /// <summary>
    /// Window that contains a <see cref="ColorPicker"/> and a color palette.
    /// </summary>
    public partial class ColorPickerDialog : Window {

        public static IEnumerable<Color> AllColors =>
    typeof(Colors).GetProperties()
    .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
    .Select(prop => (Color)prop.GetValue(null));

        private static readonly Color[] PaletteColors = new Color[] {
            Colors.Black, Colors.Gray, Colors.White, Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Indigo, Colors.Magenta,
            Colors.Purple, Colors.DarkRed,Colors.DarkOrange, Colors.DarkGreen, Colors.DarkBlue, Colors.DarkMagenta, Colors.MediumPurple,
            Colors.LightGreen, Colors.LightBlue,Colors.Pink,Colors.DeepPink,Colors.BlueViolet
        };

        private static readonly ObservableCollection<Color> DefaultPalette = new ObservableCollection<Color>(PaletteColors);

        /// <summary>
        /// Creates an instance of the <see cref="ColorPickerDialog"/> class
        /// with a default color and default color palette.
        /// </summary>
        public ColorPickerDialog()
            : this(Colors.Red) {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ColorPickerDialog"/> class
        /// with an initial <see cref="PickedColor"/> and a default color palette.
        /// </summary>
        /// <param name="color">The initial color.</param>
        public ColorPickerDialog(Color color)
            : this(color, DefaultPalette) {
        }

        /// <summary>
        /// Creates an instanace of the <see cref="ColorPickerDialog"/> class
        /// with an initial <see cref="PickedColor"/> and a color palette.
        /// </summary>
        /// <param name="color">The initial color.</param>
        /// <param name="palette">The color palette.</param>
        public ColorPickerDialog(Color color, IEnumerable<Color> palette) {
            Palette = new ObservableCollection<Color>(palette ?? PaletteColors);
            InitializeComponent();
            colorPicker.PickedColor = color;
            DialogTitle = "Color Picker";
            ButtonOKText = "OK";
            ButtonCancelText = "Cancel";
        }

        /// <summary>
        /// Gets the selected color.
        /// </summary>
        public Color PickedColor { get; private set; }

        /// <summary>
        /// Gets the color palette.
        /// </summary>
        public ObservableCollection<Color> Palette { get; }

        public string DialogTitle {
            get => this.Title;
            set => this.Title = value;
        }

        public string ButtonOKText {
            get => (string)btnOk.Content;
            set => btnOk.Content = value;
        }

        public string ButtonCancelText {
            get => (string)btnCancel.Content;
            set => btnCancel.Content = value;
        }

        private void PaletteButtonOnClick(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            var color = ((SolidColorBrush)button.Background).Color;
            colorPicker.PickedColor = color;
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void OkButtonOnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            PickedColor = colorPicker.PickedColor;
            Close();
        }
    }
}