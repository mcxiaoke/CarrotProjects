using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carrot.UI.Controls.Utils;
using System.Windows.Media;

namespace Carrot.UI.Controls.Picker {
    public class NamedColor {

        public static readonly NamedColor INVALID = NamedColor.Create("Invalid", UIHelper.ParseColor("#00000000"));

        public static NamedColor Create(Color value) => new NamedColor(Convert.ToString(value), value);
        public static NamedColor Create(string key, Color value) => new NamedColor(key, value);
        public static NamedColor Create(string key, string hex) => new NamedColor(key, UIHelper.ParseColor(hex));

        public string Key { get; set; }
        public Color Value { get; set; }

        internal NamedColor(string key, Color value) {
            Key = key ?? value.ToString();
            Value = value;
        }

        public override string ToString() {
            return $"{Key} {Value}";
        }

        public override bool Equals(object? obj) {
            return obj is NamedColor item && Value.Equals(item.Value);
        }

        public override int GetHashCode() {
            return 206514262 + Value.GetHashCode();
        }

        public static bool operator ==(NamedColor? left, NamedColor? right) {
            return EqualityComparer<NamedColor>.Default.Equals(left, right);
        }

        public static bool operator !=(NamedColor? left, NamedColor? right) {
            return !(left == right);
        }
    }
}
