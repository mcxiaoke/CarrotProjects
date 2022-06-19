using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Carrot.UI.Controls.Font;

namespace Carrot.UI.Controls.Common {
    /// <summary>
    /// SimpleComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleComboBox : UserControl {

        #region Event Handlers

        private object oldItem;

        public static readonly RoutedEvent SelectionChangedEvent =
    EventManager.RegisterRoutedEvent(nameof(SelectionChanged),
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>),
        typeof(SimpleComboBox));


        public event RoutedPropertyChangedEventHandler<object> SelectionChanged {
            add { AddHandler(SelectionChangedEvent, value); }

            remove { RemoveHandler(SelectionChangedEvent, value); }
        }


        private static void OnSelectionChanged(DependencyObject dpobj,
            DependencyPropertyChangedEventArgs e) {
            if (dpobj is SimpleComboBox combo) {
                combo.oldItem = e.OldValue;
                Debug.WriteLine($"OnSelectedItemChanged {e.OldValue} => {e.NewValue}");
            }

        }

        #endregion

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        name: nameof(SelectedItem),
        propertyType: typeof(object),
        ownerType: typeof(SimpleComboBox),
        typeMetadata: new UIPropertyMetadata(null, OnSelectionChanged));


        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            nameof(SelectedIndex),
            typeof(int),
            typeof(SimpleComboBox),
            new UIPropertyMetadata(-1));

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
            nameof(ItemSource),
            typeof(IEnumerable),
            typeof(SimpleComboBox),
            new UIPropertyMetadata(null));


        public object SelectedItem {
            get => GetValue(SelectedItemProperty);
            set {
                SetValue(SelectedItemProperty, value);
            }
        }

        public int SelectedIndex {
            get => (int)GetValue(SelectedIndexProperty);
            set {
                SetValue(SelectedIndexProperty, value);
            }
        }

        public IEnumerable ItemSource {
            get => (IEnumerable)GetValue(ItemSourceProperty);
            set {
                SetValue(ItemSourceProperty, value);
            }
        }

        public IEnumerable ItemsSource => superCombo.ItemsSource;
        public ItemCollection ItemsControl => superCombo.Items;

        public SimpleComboBox() {
            InitializeComponent();
            Debug.WriteLine($"SimpleComboBox");
            superCombo.DataContext = this;
        }


        private void SimpleComboBox_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"SimpleComboBox_Loaded");
        }

        private void SuperCombo_DropDownClosed(object sender, EventArgs e) {
            var newItem = superCombo.SelectedItem;
            Debug.WriteLine($"SuperCombo_DropDownClosed selected={superCombo.SelectedIndex} {superCombo.SelectedItem}");
            if (newItem != oldItem) {
                var args = new RoutedPropertyChangedEventArgs<object>(oldItem, newItem);
                args.RoutedEvent = SelectionChangedEvent;
                RaiseEvent(args);
            }

        }

        private void SuperCombo_Loaded(object sender, RoutedEventArgs e) {
            oldItem = superCombo.SelectedItem;
            Debug.WriteLine($"SuperCombo_Loaded old={oldItem}");
        }

    }
}
