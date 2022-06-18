using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Carrot.UI.ColorPicker.Converters {

    internal class ColorToSolidConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var c = (Color)value;
            c.A = 255;
            return c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
}