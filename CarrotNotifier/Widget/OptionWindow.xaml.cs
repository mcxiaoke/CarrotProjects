using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Newtonsoft.Json;
using Carrot.UI.Controls.Utils;
using Carrot.UI.Controls.Picker;
using Carrot.UI.Controls.Font;
using Carrot.UI.ColorPicker;
using CarrotCommon;

namespace GenshinNotifier {

    /// <summary>
    /// OptionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionWindow : Window {
        // https://stackoverflow.com/questions/30440634
        // https://wpf-tutorial.com/list-controls/combobox-control/

        private static ColorComboBoxItem CreatePair(string s, Color c) {
            return ColorComboBoxItem.Create(s, c);
        }

        public WidgetStyle UserStyle => WidgetStyle.User;

        public OptionWindow() {
            InitializeComponent();
            this.DataContext = UserStyle;
            //previewLayout.DataContext = UserStyle;
            //btnBackground.DataContext = UserStyle;
            //cbTextNormal.DataContext = UserStyle;
            //cbTextHightlight.DataContext = UserStyle;
            //cbFontFamily.DataContext = UserStyle;
            //cbFontSize.DataContext = UserStyle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Logger.Debug($"Window_Loaded");
            WidgetStyle.User.PropertyChanged += UserStyle_PropertyChanged;
        }

        private void UserStyle_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName ?? String.Empty;
            var value = sender?.GetType().GetProperty(name: key)?.GetValue(sender);
            Logger.Debug($"UserStyle_PropertyChanged {key} = {value}");
            if (key == nameof(WidgetStyle.BackgroundColor)) {
                btnBackground.Foreground = new SolidColorBrush(WidgetStyle.User.BackgroundColorOpposite);
                btnBackground.Background = new SolidColorBrush(WidgetStyle.User.BackgroundColor);
            }
        }

        private void PreviewLayout_Loaded(object sender, RoutedEventArgs e) {
            Logger.Debug($"PreviewLayout_Loaded");
            //ApplyStyle(previewLayout, WidgetStyle.ResDefault);

        }

        private void ApplyStyle(Panel container, WidgetStyle style) {
            if (style.BackgroundTransparent) {
                container.Background = Brushes.Transparent;
            } else {
                container.Background = new SolidColorBrush(style.BackgroundColor);
            }
            var labels = container.FindLogicalChildren<Label>();
            foreach (var label in labels) {
                Logger.Debug($"Update {label}");
                label.FontSize = style.TextFontSize;
                label.FontFamily = style.TextFontFamily ?? WidgetStyle.FONT_FAMILY_DEFAULT;
                label.FontWeight = style.TextFontWeight ?? FontWeights.Normal;
                label.FontStyle = style.TextFontStyle ?? FontStyles.Normal;
                if (label.Name.Contains("value")) {
                    label.Foreground = new SolidColorBrush(style.TextHighlightColor);
                } else {
                    label.Foreground = new SolidColorBrush(style.TextNormalColor);
                }
            }
        }

        private void CbBackground_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<ColorComboBoxItem> e) {
            Logger.Debug($"CbBackground_SelectedColorChanged ${e.OldValue} => {e.NewValue}");
        }

        private void CbTextNormal_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<ColorComboBoxItem> e) {
            Logger.Debug($"CbTextNormal_SelectedColorChanged ${e.OldValue} => {e.NewValue}");
            UserStyle.TextNormalColor = e.NewValue.Value;
        }

        private void CbTextHightlight_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<ColorComboBoxItem> e) {
            Logger.Debug($"CbTextHightlight_SelectedColorChanged ${e.OldValue} => {e.NewValue}");
            UserStyle.TextHighlightColor = e.NewValue.Value;
        }

        private void CbFontFamily_FontChanged(object sender, RoutedPropertyChangedEventArgs<FontExtraInfo> e) {
            Logger.Debug($"CbFontFamily_FontChanged ${e.OldValue} => {e.NewValue}");
            UserStyle.TextFontFamily = e.NewValue.Family;
        }

        private int oldFontSize = 0;
        private void CbFontSize_DropDownClosed(object sender, EventArgs e) {
            var newFontSize = Convert.ToInt32(cbFontSize.SelectedItem);
            Logger.Debug($"CbFontSize_DropDownClosed old={oldFontSize} new={newFontSize}");
            if (newFontSize != oldFontSize) {
                oldFontSize = newFontSize;
                UserStyle.TextFontSize = newFontSize;
            }

        }

        private void ChkFontBold_Changed(object sender, RoutedEventArgs e) {
            Logger.Debug($"ChkFontBold_Changed {chkFontBold.IsChecked}");
            UserStyle.TextFontWeight = chkFontBold.IsChecked == true ? FontWeights.Bold : FontWeights.Normal;
        }

        private void ChkFontItalic_Changed(object sender, RoutedEventArgs e) {
            Logger.Debug($"ChkFontItalic_Changed {chkFontItalic.IsChecked}");
            UserStyle.TextFontStyle = chkFontItalic.IsChecked == true ? FontStyles.Italic : FontStyles.Normal;
        }


        private void BtnBackground_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Logger.Debug("BtnBackground_Click");
            var dialog = new Carrot.UI.ColorPicker.ColorPickerDialog(UserStyle.BackgroundColor) {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "选择背景颜色",
                ButtonOKText = "确定",
                ButtonCancelText = "取消"
            };
            if (dialog.ShowDialog() == true) {
                Logger.Debug($"ColorPickerDialog result={dialog.PickedColor}");
                UserStyle.BackgroundColor = dialog.PickedColor;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            // save user style settings
            WidgetStyle.SaveUserStyle();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e) {
            // reset to default
            WidgetStyle.ResetUserStyle();
            if (WidgetStyle.User is WidgetStyle style) {
                //btnBackground.Background = new SolidColorBrush(style.BackgroundColor);
                //btnBackground.Foreground = new SolidColorBrush(style.BackgroundColorOpposite);
                cbTextNormal.SelectedIndex = 0;
                cbTextHightlight.SelectedIndex = 0;
                cbFontFamily.SelectedFont = style.TextFontExtraInfo;
                cbFontSize.SelectedItem = style.TextFontSize;
            }
        }
    }
}