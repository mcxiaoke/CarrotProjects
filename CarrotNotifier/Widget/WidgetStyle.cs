using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Carrot.UI.Controls.Font;
using Newtonsoft.Json;
using Carrot.UI.Controls.Utils;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Carrot.UI.Controls.Picker;
using PropertyChanged;
using CarrotCommon;
using System.Xml.Linq;
using System.Windows.Input;
using Carrot.UI.Controls;

namespace GenshinNotifier {

    public class WidgetStyle : INotifyPropertyChanged {
        public static WidgetStyle Empty = new WidgetStyle();
        public static WidgetStyle User { get; private set; } = Empty;
        public static WidgetStyle ResDefault { get; private set; } = Empty;

        public static WidgetStyle FromResource(ResourceDictionary res) {
            var rTextNormalColor = (Color)res["TextNormalColor"];
            var rTextHighlightColor = (Color)res["TextHighlightColor"];
            var rTextErrorColor = (Color)res["TextErrorColor"];
            var rBackgroundColor = (Color)res["BackgroundColor"];
            var rTextFontSize = (double)res["TextFontSize"];
            var rFontFamily = (FontFamily)res["TextFontFamily"];
            var rFontWeight = (FontWeight)res["TextFontWeight"];
            var rFontStyle = (FontStyle)res["TextFontStyle"];
            return new WidgetStyle(rTextNormalColor, rTextHighlightColor, rTextErrorColor, rBackgroundColor, rTextFontSize, rFontFamily, rFontWeight, rFontStyle);
        }

        public static Color ERROR_COLOR = UIHelper.ParseColor("#00000000");

        private static ColorComboBoxItem CreateColorPair(string s, Color c) {
            return ColorComboBoxItem.Create(s, c);
        }

        public WidgetStyle() {
        }

        public WidgetStyle(Color textNormalColor,
            Color textHighlightColor,
            Color textErrorColor,
            Color backgroundColor,
            double textFontSize,
            FontFamily fontFamily,
            FontWeight fontWeight,
            FontStyle fontStyle) {
            TextNormalColor = textNormalColor;
            TextHighlightColor = textHighlightColor;
            TextErrorColor = textErrorColor;
            BackgroundColor = backgroundColor;
            TextFontSize = textFontSize;
            TextFontFamily = fontFamily;
            TextFontWeight = fontWeight;
            TextFontStyle = fontStyle;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #region normal properties

        public Color TextNormalColor { get; set; } = ERROR_COLOR;
        public Color TextHighlightColor { get; set; } = ERROR_COLOR;
        public Color TextErrorColor { get; set; } = ERROR_COLOR;
        public Color BackgroundColor { get; set; } = ERROR_COLOR;
        public bool BackgroundTransparent { get; set; } = false;
        public double TextFontSize { get; set; } = double.NaN;
        public FontFamily? TextFontFamily { get; set; }
        public FontWeight? TextFontWeight { get; set; }
        public FontStyle? TextFontStyle { get; set; }

        #endregion

        #region calculated properties
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem>? AppendBgColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem>? AppendTextNColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<ColorComboBoxItem>? AppendTextHColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<double> FontSizeRange => Enumerable.Range(12, 9).Select(it => Convert.ToDouble(it)).ToList();


        [JsonIgnore]
        public Color BackgroundColorOpposite => BackgroundColor.IsDark() ? Colors.White : Colors.Black;

        [JsonIgnore]
        public FontExtraInfo TextFontExtraInfo => FontUtilities.GetLocalizedFontFamily(TextFontFamily);

        [JsonIgnore]
        public bool TextFontBold {
            get => TextFontWeight == FontWeights.Bold;
            set => TextFontWeight = value ? FontWeights.Bold : FontWeights.Normal;
        }
        [JsonIgnore]
        public bool TextFontItalic {
            get => TextFontStyle == FontStyles.Italic;
            set => TextFontStyle = value ? FontStyles.Italic : FontStyles.Normal;
        }

        [JsonIgnore]
        public double HeaderFontSize => MiscUtils.Clamp(TextFontSize + 3, ResDefault!.TextFontSize, 24);

        [JsonIgnore]
        public double FooterFontSize => MiscUtils.Clamp(TextFontSize - 3, 12, ResDefault!.TextFontSize);

        #endregion

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        private static bool IsValidFontSize(double? fontSize) {
            return fontSize >= 8 && fontSize <= 24;
        }

        private static string GetUserStyleFilePath() {
            var root = AppInfo.LocalAppDataPath;
            var dir = Path.Combine(root, "styles");
            Storage.CheckOrCreateDir(dir);
            return Path.Combine(dir, $"widget_style.json");
        }

        private static void DeleteUserStyle() {
            var fileName = GetUserStyleFilePath();
            if (!File.Exists(fileName)) {
                return;
            }
            try {
                Logger.Debug("DeleteUserStyle ok");
                File.Delete(fileName);
            } catch (Exception ex) {
                Logger.Debug($"DeleteUserStyle failed {ex}");
            }
        }

        private static WidgetStyle? LoadUserStyle() {
            var fileName = GetUserStyleFilePath();
            if (!File.Exists(fileName)) {
                return WidgetStyle.Empty;
            }
            try {
                var styleJson = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(styleJson)) {
                    return WidgetStyle.Empty;
                }
                Logger.Debug("LoadUserStyle ok");
                return JsonConvert.DeserializeObject<WidgetStyle>(styleJson);
            } catch (Exception ex) {
                Logger.Debug($"LoadUserStyle failed {ex}");
                return WidgetStyle.Empty;
            }
        }

        public static bool SaveUserStyle() {
            Logger.Debug("SaveUserStyle");
            try {
                var styleJson = JsonConvert.SerializeObject(WidgetStyle.User);
                if (string.IsNullOrWhiteSpace(styleJson)) {
                    return false;
                }
                var fileName = GetUserStyleFilePath();
                File.WriteAllText(fileName, styleJson, System.Text.Encoding.UTF8);
                Logger.Debug("SaveUserStyle ok");
                return true;
            } catch (Exception ex) {
                Logger.Debug($"SaveUserStyle failed {ex}");
                return false;
            }
        }

        public static void ResetUserStyle() {
            Logger.Debug("ResetUserStyle");
            DeleteUserStyle();
            var res = Application.Current.Resources;
            var rs = WidgetStyle.FromResource(res);
            var us = WidgetStyle.User!;
            us.TextNormalColor = rs.TextNormalColor;
            us.TextHighlightColor = rs.TextHighlightColor;
            us.TextErrorColor = rs.TextErrorColor;
            us.BackgroundColor = rs.BackgroundColor;
            us.BackgroundTransparent = rs.BackgroundColor.A == 0;
            us.TextFontSize = rs.TextFontSize;
            us.TextFontFamily = rs.TextFontFamily;
            us.TextFontWeight = rs.TextFontWeight;
            us.TextFontStyle = rs.TextFontStyle;

        }

        // must call on app start
        public static void Initialize() {
            // read user setting values
            var settingStyle = LoadUserStyle();
            Logger.Debug("WidgetStyle settings=" + settingStyle);

            // read xaml resource values
            var res = Application.Current.Resources;
            //UI.PrintResources(res);
            var resourceStyle = WidgetStyle.FromResource(res);

            var userStyle = new WidgetStyle();
            userStyle.TextNormalColor = settingStyle?.TextNormalColor ?? resourceStyle.TextNormalColor;
            userStyle.TextHighlightColor = settingStyle?.TextHighlightColor ?? resourceStyle.TextHighlightColor;
            userStyle.TextErrorColor = settingStyle?.TextErrorColor ?? resourceStyle.TextErrorColor;
            userStyle.BackgroundColor = settingStyle?.BackgroundColor ?? resourceStyle.BackgroundColor;
            userStyle.BackgroundTransparent = settingStyle?.BackgroundTransparent ?? false;
            userStyle.TextFontSize = settingStyle?.TextFontSize ?? resourceStyle.TextFontSize;
            userStyle.TextFontFamily = settingStyle?.TextFontFamily ?? resourceStyle.TextFontFamily;
            userStyle.TextFontWeight = settingStyle?.TextFontWeight ?? resourceStyle.TextFontWeight;
            userStyle.TextFontStyle = settingStyle?.TextFontStyle ?? resourceStyle.TextFontStyle;

            userStyle.AppendBgColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.BackgroundColor),
            CreateColorPair("默认", resourceStyle.BackgroundColor)
            };

            userStyle.AppendTextNColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.TextNormalColor),
            CreateColorPair("默认", resourceStyle.TextNormalColor)
        };

            userStyle.AppendTextHColors = new List<ColorComboBoxItem>() {
            CreateColorPair("当前", userStyle.TextHighlightColor),
            CreateColorPair("默认", resourceStyle.TextHighlightColor)
        };

            WidgetStyle.User = userStyle;
            WidgetStyle.ResDefault = resourceStyle;
            Logger.Debug("WidgetStyle res=" + ResDefault);
            Logger.Debug("WidgetStyle user=" + User);

            //Settings.Default.WidgetStyle = User.ToString();
            //Settings.Default.Save();

        }
    }
}