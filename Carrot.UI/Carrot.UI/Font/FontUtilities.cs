using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using System.Diagnostics;

namespace Carrot.UI.Controls.Font {
    public static class FontUtilities {

        public static readonly string[] ZhLocales = { "zh-CN", "zh-HK", "zh-TW" };
        public static FontExtraInfo GetLocalizedFontFamily(FontFamily fontfamily) {

            var lsd = fontfamily.FamilyNames;
            lsd.TryGetValue(XmlLanguage.GetLanguage("en-US"), out string englishName);
            string localizedName = null;
            //var locale = CultureInfo.CurrentCulture.Name;
            foreach (var loc in ZhLocales) {
                if (!string.IsNullOrEmpty(localizedName)) { break; }
                lsd.TryGetValue(XmlLanguage.GetLanguage(loc), out localizedName);
            }
            //if (lsd.ContainsKey(XmlLanguage.GetLanguage(locale))) {
            //    lsd.TryGetValue(XmlLanguage.GetLanguage(locale), out localizedName);
            //}
            return new FontExtraInfo(englishName ?? lsd.FirstOrDefault().Value, localizedName, fontfamily);
        }

        public static ICollection<FontExtraInfo> AllFonts => LocalizedFonts();

        public static ICollection<FontExtraInfo> LocalizedFonts() {
            var cnlist = new List<FontExtraInfo>();
            var enlist = new List<FontExtraInfo>();
            foreach (var font in Fonts.SystemFontFamilies) {
                var names = string.Join(", ", font.FamilyNames.Select(it => $"{it.Value}({it.Key})"));
                //Debug.WriteLine($"Add Font {font.Source} {names}");
                var localizedFont = GetLocalizedFontFamily(font);
                if (string.IsNullOrEmpty(localizedFont.LocalizedName)) {
                    enlist.Add(localizedFont);
                } else {
                    cnlist.Add(localizedFont);
                }
            }
            //Debug.WriteLine($"{cl.Count} {el.Count}");
            var result = cnlist.OrderBy(it => it.Name).ToList();
            result.AddRange(enlist.OrderBy(it => it.Name));
            return result;
        }
    }
}
