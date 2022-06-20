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
using Carrot.Common;
using System.Xml.Linq;
using System.Windows.Input;
using Carrot.UI.Controls;
using System.Text;
using System.Collections.ObjectModel;

namespace GenshinNotifier {

    public readonly record struct WidgetColor(
    [JsonProperty("name")] string Name,
    [JsonProperty("background")] Color Background,
    [JsonProperty("textNormal")] Color TextNormal,
    [JsonProperty("textHighlight")] Color TextHighlight);

    public class WidgetStyle : INotifyPropertyChanged {
        public static WidgetStyle Empty = new WidgetStyle();
        public static WidgetStyle User { get; private set; } = Empty;
        public static WidgetStyle ResDefault { get; private set; } = Empty;

        public static FontFamily FONT_FAMILY_DEFAULT = new FontFamily("NSimsun");
        public static FontWeight FONT_WEIGHT_DEFAULT = FontWeights.Normal;
        public static FontStyle FONT_STYLE_DEFAULT = FontStyles.Normal;
        public static Color ERROR_COLOR = UIHelper.ParseColor("#00000000");

        public static ObservableCollection<WidgetColor> ThemeColors = new();


        private static int WidgetColorCompare(WidgetColor a, WidgetColor b) {
            var acv = a.Background.PerceivedBrightness();
            var bcv = b.Background.PerceivedBrightness();
            if (acv > bcv) { return 1; } else if (acv < bcv) { return -1; } else { return 0; }
        }

        private static void LoadAppColors() {
            var resourceUri = new Uri("Resources/WidgetColors.json", UriKind.RelativeOrAbsolute);
            string userResourceFile = Path.Combine(AppInfo.StartupPath, "WidgetColors.json");
            var colorsJson = "[]";
            try {
                if (File.Exists(userResourceFile)) {
                    Debug.WriteLine($"WidgetStyle.LoadAppColors loaded user colors");
                    colorsJson = File.ReadAllText(userResourceFile, Encoding.UTF8);
                } else if (Application.GetResourceStream(resourceUri)?.Stream is Stream stream) {
                    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    Debug.WriteLine($"WidgetStyle.LoadAppColors loaded app colors");
                    colorsJson = reader.ReadToEnd();
                }
                var colors = JsonConvert.DeserializeObject<List<WidgetColor>>(colorsJson);
                if (colors?.Count > 0) {
                    Debug.WriteLine($"WidgetStyle.LoadAppColors loaded colors.Count={colors.Count}");
                    ThemeColors = new ObservableCollection<WidgetColor>(colors);
                } else {
                    Debug.WriteLine($"WidgetStyle.LoadAppColors loaded no colors");
                }
            } catch (Exception ex) {
                Logger.Debug($"WidgetStyle.LoadAppColors loaded failed {ex}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private static void GenerateColors() {
            var sysColors = ColorComboBox.AllSystemColors;
            var colors = new List<WidgetColor>();
            foreach (var color in sysColors) {
                var name = color.Key;
                var bg = color.Value;
                if (bg.PerceivedBrightness() > 120) {
                    bg = bg.GetBrighterOrDarker(-0.5);
                    if (name.Contains("Light")) {
                        name = name.Replace("Light", "");
                    } else if (name.Contains("Medium")) {
                        name = name.Replace("Medium", "Dark");
                    } else {
                        name = "Dark" + name;
                    }
                }
                var normal = Colors.White;
                var highlight = Colors.GreenYellow.Mix(Colors.Yellow).GetBrighterOrDarker(0.3);
                var wc = new WidgetColor(name, bg, normal, highlight);
                colors.Add(wc);
            }
            ThemeColors = new ObservableCollection<WidgetColor>(colors);
            Debug.WriteLine($"WidgetStyle.GenerateColors colors={ThemeColors.Count}");
            return;

        }

        public WidgetStyle() {
        }

        public WidgetStyle(ResourceDictionary res) {
            TextNormalColor = (Color)res["TextNormalColor"];
            TextHighlightColor = (Color)res["TextHighlightColor"];
            TextErrorColor = (Color)res["TextErrorColor"];
            BackgroundColor = (Color)res["BackgroundColor"];
            TextFontSize = (double)res["TextFontSize"];
            TextFontFamily = (FontFamily)res["TextFontFamily"];
            TextFontWeight = (FontWeight)res["TextFontWeight"];
            TextFontStyle = (FontStyle)res["TextFontStyle"];
        }

        public WidgetStyle(WidgetStyle other) {
            TextNormalColor = other.TextNormalColor;
            TextHighlightColor = other.TextHighlightColor;
            TextErrorColor = other.TextErrorColor;
            BackgroundColor = other.BackgroundColor;
            TextFontSize = other.TextFontSize;
            TextFontFamily = other.TextFontFamily;
            TextFontWeight = other.TextFontWeight;
            TextFontStyle = other.TextFontStyle;
        }

        public void MergeValues(WidgetStyle other) {
            if (other.TextNormalColor != ERROR_COLOR) {
                TextNormalColor = other.TextNormalColor;
            }
            if (other.TextHighlightColor != ERROR_COLOR) {
                TextHighlightColor = other.TextHighlightColor;
            }
            if (other.TextErrorColor != ERROR_COLOR) {
                TextErrorColor = other.TextErrorColor;
            }
            if (other.BackgroundColor != ERROR_COLOR) {
                BackgroundColor = other.BackgroundColor;
            }
            if (other.TextFontSize != 0) {
                TextFontSize = other.TextFontSize;
            }
            if (other.TextFontFamily != null) {
                TextFontFamily = other.TextFontFamily;
            }
            if (other.TextFontWeight != null) {
                TextFontWeight = other.TextFontWeight;
            }
            if (other.TextFontStyle != null) {
                TextFontStyle = other.TextFontStyle;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #region normal properties

        public Color TextNormalColor { get; set; } = ERROR_COLOR;
        public Color TextHighlightColor { get; set; } = ERROR_COLOR;
        public Color TextErrorColor { get; set; } = ERROR_COLOR;
        public Color BackgroundColor { get; set; } = ERROR_COLOR;
        public bool BackgroundTransparent { get; set; } = false;
        public double TextFontSize { get; set; } = 0;
        public FontFamily? TextFontFamily { get; set; }
        public FontWeight? TextFontWeight { get; set; }
        public FontStyle? TextFontStyle { get; set; }

        public int ThemeIndex { get; set; } = -1;

        #endregion

        #region calculated properties
        [JsonIgnore]
        public Color CurrentBackgroundColor => ThemeStyle?.Background ?? BackgroundColor;
        [JsonIgnore]
        public Color CurrentTextNormalColor => ThemeStyle?.TextNormal ?? TextNormalColor;
        [JsonIgnore]
        public Color CurrentTextHighlightColor => ThemeStyle?.TextHighlight ?? TextHighlightColor;
        [JsonIgnore]
        public WidgetColor? ThemeStyle => ThemeIndex >= 0 && ThemeIndex <= ThemeColors.Count ? ThemeColors[ThemeIndex] : null;
        [JsonIgnore]
        public IEnumerable<WidgetColor> ThemeStyles => ThemeColors;

        [JsonIgnore]
        [DoNotNotify]
        public List<NamedColor>? AppendBgColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<NamedColor>? AppendTextNColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<NamedColor>? AppendTextHColors { get; private set; }
        [JsonIgnore]
        [DoNotNotify]
        public List<double> FontSizeRange => Enumerable.Range(12, 9).Select(it => Convert.ToDouble(it)).ToList();


        [JsonIgnore]
        public Color BackgroundColorOpposite => CurrentBackgroundColor.IsDark() ? Colors.White : Colors.Black;

        [JsonIgnore]
        public FontExtraInfo TextFontExtraInfo => FontUtilities.GetFontExtraInfo(TextFontFamily ?? FONT_FAMILY_DEFAULT);

        [JsonIgnore]
        public int TextFontSizeIndex => FontSizeRange.IndexOf(TextFontSize);

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
            WidgetStyle.User.ThemeIndex = -1;
            DeleteUserStyle();
            WidgetStyle.User.MergeValues(WidgetStyle.ResDefault);

        }

        // must call on app start
        public static void Initialize() {

            // read xaml resource values
            //UI.PrintResources(res);
            var res = Application.Current.Resources;
            var resourceStyle = new WidgetStyle(res);
            var userStyle = new WidgetStyle(resourceStyle);
            // read user setting values
            if (LoadUserStyle() is WidgetStyle settingStyle) {
                Logger.Debug("WidgetStyle settings=" + settingStyle);
                userStyle.MergeValues(settingStyle);
                userStyle.AppendBgColors = new List<NamedColor>() {
            NamedColor.Create("当前", userStyle.BackgroundColor),
            NamedColor.Create("默认", resourceStyle.BackgroundColor)};

                userStyle.AppendTextNColors = new List<NamedColor>() {
            NamedColor.Create("当前", userStyle.TextNormalColor),
            NamedColor.Create("默认", resourceStyle.TextNormalColor)};

                userStyle.AppendTextHColors = new List<NamedColor>() {
            NamedColor.Create("当前", userStyle.TextHighlightColor),
            NamedColor.Create("默认", resourceStyle.TextHighlightColor) };
            }


            WidgetStyle.ResDefault = resourceStyle;
            WidgetStyle.User = userStyle;
            Logger.Debug("WidgetStyle res=" + ResDefault);
            Logger.Debug("WidgetStyle user=" + User);

            WidgetStyle.LoadAppColors();
            Logger.Debug("WidgetStyle colors=" + ThemeColors.Count);

            //Settings.Default.WidgetStyle = User.ToString();
            //Settings.Default.Save();

        }
    }
}