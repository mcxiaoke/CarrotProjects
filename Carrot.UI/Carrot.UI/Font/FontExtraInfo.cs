using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {
    public class FontExtraInfo {
        public string Source { get; }
        public string Name => LocalizedName ?? EnglishName;
        public string EnglishName { get; set; }
        public string LocalizedName { get; set; }
        public FontFamily Family { get; set; }



        public FontExtraInfo(string englishName, string localizedName, FontFamily font) {
            EnglishName = englishName;
            LocalizedName = localizedName;
            this.Family = font;
            this.Source = font.Source;
        }



        public override string ToString() {
            return $"{{Font({LocalizedName}, {EnglishName})}}";
        }

        // needed for set selectedItem
        public override bool Equals(object obj) {
            return obj is FontExtraInfo family &&
                   EqualityComparer<FontFamily>.Default.Equals(Family, family.Family);
        }

        public override int GetHashCode() {
            return 548286385 + EqualityComparer<FontFamily>.Default.GetHashCode(Family);
        }
    }
}
