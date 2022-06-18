using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using Carrot.UI.Controls.Picker;

namespace Carrot.UI.Controls.Font {

    public partial class FontComboBox : UserControl {
        #region Event Handlers

        public static readonly RoutedEvent FontChangedEvent =
    EventManager.RegisterRoutedEvent(nameof(FontChanged),
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<FontExtraInfo>),
        typeof(FontComboBox));


        public event RoutedPropertyChangedEventHandler<FontExtraInfo> FontChanged {
            add { AddHandler(FontChangedEvent, value); }

            remove { RemoveHandler(FontChangedEvent, value); }
        }


        private static void OnSelectedFontChanged(DependencyObject dpobj,
            DependencyPropertyChangedEventArgs e) {
            var fcb = dpobj as FontComboBox;
            fcb.oldItem = e.OldValue as FontExtraInfo;
            Debug.WriteLine($"OnSelectedFontChanged {e.OldValue} => {e.NewValue}");
        }

        #endregion

        public static IEnumerable<FontExtraInfo> AllFonts => FontUtilities.LocalizedFonts();

        public static readonly DependencyProperty SelectedFontProperty = DependencyProperty.Register(
                name: nameof(SelectedFont),
                propertyType: typeof(FontExtraInfo),
                ownerType: typeof(FontComboBox),
                typeMetadata: new UIPropertyMetadata(null, OnSelectedFontChanged));


        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(FontComboBox),
            new UIPropertyMetadata(-1));

        public FontComboBox() {
            InitializeComponent();
            Debug.WriteLine(CultureInfo.CurrentCulture);
            cbFonts.DataContext = this;
            //this.DataContext = this;
        }


        public FontExtraInfo SelectedFont {
            get => (FontExtraInfo)GetValue(SelectedFontProperty);
            set {
                SetValue(SelectedFontProperty, value);
                Debug.WriteLine($"SelectedFont set to {value}");
            }
        }

        public int SelectedIndex {
            get => (int)GetValue(SelectedIndexProperty);
            set {
                SetValue(SelectedIndexProperty, value);
            }
        }

        private void FontComboBox_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"FontComboBox_Loaded {Name} init=[{SelectedFont}] index=[{SelectedIndex}]");
        }

        private void CBFonts_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"CBFonts_Loaded {Name} init=[{SelectedFont}] index=[{SelectedIndex}]");
            oldItem = (FontExtraInfo)cbFonts.SelectedItem;
        }

        private FontExtraInfo oldItem;
        private void CBFonts_DropDownClosed(object sender, EventArgs e) {
            var newItem = (FontExtraInfo)cbFonts.SelectedItem;
            Debug.WriteLine($"CBFonts_DropDownClosed new={newItem}");
            if (newItem != oldItem) {
                var args = new RoutedPropertyChangedEventArgs<FontExtraInfo>(oldItem, newItem);
                args.RoutedEvent = FontChangedEvent;
                RaiseEvent(args);
            }

        }
    }
}
