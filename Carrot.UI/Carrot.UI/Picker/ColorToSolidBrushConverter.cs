﻿//////////////////////////////////////////////
// MIT  - 2019
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Carrot.UI.Controls.Picker {
    public class ColorToSolidBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            new SolidColorBrush((Color)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
            //return value.ToString();
        }
    }
}
