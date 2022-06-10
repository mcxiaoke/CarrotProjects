using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using NotifierWidget.Properties;
using Newtonsoft.Json;

namespace NotifierWidget {
    class WidgetStyle {
        public static WidgetStyle Default = new WidgetStyle();


        public WidgetStyle() {
        }

        public string TextNormalColor { get; set; }
        public string TextHighlightColor { get; set; }
        public string BackgroundColor { get; set; }
        public int TextFontSize { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public void LoadStyles() {
            var normalColor = Settings.Default.WidgetTextNormalColor;
            var highlightColor = Settings.Default.WidgetTextHighlightColor;
            var backgroundColor = Settings.Default.WidgetBackgroundColor;
            var textFontSize = Settings.Default.WidgetTextFontSize;

            var res = Application.Current.Resources;
            var resNormalColor = (string)res["TextNormalColor"];
            var resHighlightColor = (string)res["TextHighlightColor"];
            var resBackgroundColor = (string)res["BackgroundColor"];
            var resTextFontSize = (double)res["TextFontSize"];

            TextNormalColor = resNormalColor;
            TextHighlightColor = resHighlightColor;
            BackgroundColor = resBackgroundColor;
            TextFontSize = Convert.ToInt32(resTextFontSize);

            Debug.WriteLine("WidgetStyle:" + this.ToString());


        }
    }
}
