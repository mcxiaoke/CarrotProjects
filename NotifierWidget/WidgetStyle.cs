using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using NotifierWidget.Properties;

namespace NotifierWidget {

    internal class WidgetStyle : INotifyPropertyChanged {
        public static WidgetStyle Default = new WidgetStyle();

        private static Color ERROR_COLOR = UI.ParseColor("#00000000");

        public WidgetStyle() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color TextNormalColor { get; set; }
        public Color TextHighlightColor { get; set; }
        public Color TextErrorColor { get; set; }
        public Color BackgroundColor { get; set; }
        public bool BackgroundTransparent { get; set; }
        public bool TextFontBold { get; set; }
        public double TextFontSize { get; set; } = double.NaN;

        [JsonIgnore]
        public double HeaderFontSize => TextFontSize + 3;

        [JsonIgnore]
        public double FooterFontSize => TextFontSize - 3;

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        private static bool IsValidFontSize(double fontSize) {
            return !double.IsNaN(fontSize) && fontSize >= 8 && fontSize <= 24;
        }

        private WidgetStyle LoadUserStyle() {
            try {
                var styleJson = Settings.Default.WidgetStyle;
                if (string.IsNullOrWhiteSpace(styleJson)) {
                    return new WidgetStyle();
                }
                return JsonConvert.DeserializeObject<WidgetStyle>(styleJson);
            } catch (Exception) {
                Settings.Default.WidgetStyle = null;
                Settings.Default.Save();
                return new WidgetStyle();
            }
        }

        public void Initialize() {
            // read user setting values
            var us = LoadUserStyle();
            Debug.WriteLine("LoadUserStyle=" + us);

            // read xaml resource values
            var res = Application.Current.Resources;
            UI.PrintResources(res);

            var rTextNormalColor = (Color)res["TextNormalColor"];
            var rTextHighlightColor = (Color)res["TextHighlightColor"];
            var rTextErrorColor = (Color)res["TextErrorColor"];
            var rBackgroundColor = (Color)res["BackgroundColor"];
            var rTextFontSize = (double)res["TextFontSize"];

            TextNormalColor = us.TextNormalColor == ERROR_COLOR ? rTextNormalColor : us.TextNormalColor;
            TextHighlightColor = us.TextHighlightColor == ERROR_COLOR ? rTextHighlightColor : us.TextHighlightColor;
            TextErrorColor = us.TextErrorColor == ERROR_COLOR ? rTextErrorColor : us.TextErrorColor;
            BackgroundColor = us.BackgroundColor == ERROR_COLOR ? rBackgroundColor : us.BackgroundColor;
            BackgroundTransparent = us.BackgroundTransparent;
            TextFontBold = us.TextFontBold;
            TextFontSize = IsValidFontSize(us.TextFontSize) ? us.TextFontSize : rTextFontSize;

            Debug.WriteLine("FinalStyles=" + this);
        }
    }
}