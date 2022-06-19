using System.Collections.Generic;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    public class FontColor {
        public string Name { get; set; }
        public SolidColorBrush Brush { get; set; }

        public FontColor(string name, SolidColorBrush brush) {
            Name = name;
            Brush = brush;
        }

        public override string ToString() {
            return "FontColor [Color=" + this.Name + ", " + this.Brush.ToString() + "]";
        }

        public override bool Equals(object? obj) {
            return obj is FontColor color &&
                   Name == color.Name &&
                   EqualityComparer<SolidColorBrush>.Default.Equals(Brush, color.Brush);
        }

        public override int GetHashCode() {
            int hashCode = -320841079;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<SolidColorBrush>.Default.GetHashCode(Brush);
            return hashCode;
        }
    }
}