using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Carrot.UI.Controls.ColorUtils {


    /// <summary>
    /// Represents a HSV (=HSB) color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    internal sealed class HsbColor {
        public HsbColor(
            double hue,
            double saturation,
            double brightness,
            int alpha) {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseBrightness = brightness;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the hue. Values from 0 to 360.
        /// </summary>

        public double PreciseHue { get; }

        /// <summary>
        /// Gets or sets the saturation. Values from 0 to 100.
        /// </summary>

        public double PreciseSaturation { get; }

        /// <summary>
        /// Gets or sets the brightness. Values from 0 to 100.
        /// </summary>

        public double PreciseBrightness { get; }


        public int Hue => Convert.ToInt32(PreciseHue);


        public int Saturation => Convert.ToInt32(PreciseSaturation);


        public int Brightness => Convert.ToInt32(PreciseBrightness);

        /// <summary>
        /// Gets or sets the alpha. Values from 0 to 255.
        /// </summary>

        public int Alpha { get; }


        public static HsbColor FromColor(
            Color color) {
            return ColorConverting.ColorToRgb(color).ToHsbColor();
        }


        public static HsbColor FromRgbColor(
            RgbColor color) {
            return color.ToHsbColor();
        }


        public static HsbColor FromHsbColor(
            HsbColor color) {
            return new HsbColor(color.PreciseHue, color.PreciseSaturation, color.PreciseBrightness, color.Alpha);
        }


        public static HsbColor FromHslColor(
            HslColor color) {
            return FromRgbColor(color.ToRgbColor());
        }


        public override string ToString() {
            return $"Hue: {Hue}; saturation: {Saturation}; brightness: {Brightness}.";
        }


        public Color ToColor() {
            return ColorConverting.HsbToRgb(this).ToColor();
        }


        public RgbColor ToRgbColor() {
            return ColorConverting.HsbToRgb(this);
        }


        public HsbColor ToHsbColor() {
            return new HsbColor(PreciseHue, PreciseSaturation, PreciseBrightness, Alpha);
        }


        public HslColor ToHslColor() {
            return ColorConverting.RgbToHsl(ToRgbColor());
        }

        public override bool Equals(
            object obj) {
            var equal = false;

            if (obj is HsbColor color) {
                var hsb = color;

                if (Math.Abs(PreciseHue - hsb.PreciseHue) < 0.001 &&
                    Math.Abs(PreciseSaturation - hsb.PreciseSaturation) < 0.001 &&
                    Math.Abs(PreciseBrightness - hsb.PreciseBrightness) < 0.001) {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode() {
            return $"H:{Hue}-S:{Saturation}-B:{Brightness}-A:{Alpha}".GetHashCode();
        }
    }


    /// <summary>
    /// Represents a HSL color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    internal sealed class HslColor {
        public HslColor(
            double hue,
            double saturation,
            double light,
            int alpha) {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseLight = light;
            Alpha = alpha;
        }

        public HslColor(
            int hue,
            int saturation,
            int light,
            int alpha) {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseLight = light;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets the hue. Values from 0 to 360.
        /// </summary>

        public int Hue => Convert.ToInt32(PreciseHue);

        /// <summary>
        /// Gets the precise hue. Values from 0 to 360.
        /// </summary>

        public double PreciseHue { get; }

        /// <summary>
        /// Gets the saturation. Values from 0 to 100.
        /// </summary>

        public int Saturation => Convert.ToInt32(PreciseSaturation);

        /// <summary>
        /// Gets the precise saturation. Values from 0 to 100.
        /// </summary>
        public double PreciseSaturation { get; }

        /// <summary>
        /// Gets the light. Values from 0 to 100.
        /// </summary>

        public int Light => Convert.ToInt32(PreciseLight);

        /// <summary>
        /// Gets the precise light. Values from 0 to 100.
        /// </summary>
        public double PreciseLight { get; }

        /// <summary>
        /// Gets the alpha. Values from 0 to 255
        /// </summary>
        public int Alpha { get; }

        public static HslColor FromColor(
            Color color) {
            return ColorConverting.RgbToHsl(
                ColorConverting.ColorToRgb(color));
        }


        public static HslColor FromRgbColor(
            RgbColor color) {
            return color.ToHslColor();
        }


        public static HslColor FromHslColor(
            HslColor color) {
            return new HslColor(
                color.PreciseHue,
                color.PreciseSaturation,
                color.PreciseLight,
                color.Alpha);
        }


        public static HslColor FromHsbColor(
            HsbColor color) {
            return FromRgbColor(color.ToRgbColor());
        }

        public override string ToString() {
            return Alpha < 255
                ? $"hsla({Hue}, {Saturation}%, {Light}%, {Alpha / 255f})"
                : $"hsl({Hue}, {Saturation}%, {Light}%)";
        }

        public Color ToColor() {
            return ColorConverting.HslToRgb(this).ToColor();
        }

        public RgbColor ToRgbColor() {
            return ColorConverting.HslToRgb(this);
        }

        public HslColor ToHslColor() {
            return this;
        }


        public HsbColor ToHsbColor() {
            return ColorConverting.RgbToHsb(ToRgbColor());
        }

        public override bool Equals(
            object obj) {
            var equal = false;

            if (obj is HslColor color) {
                var hsb = color;

                if (Math.Abs(Hue - hsb.PreciseHue) < double.Epsilon &&
                    Math.Abs(Saturation - hsb.PreciseSaturation) < double.Epsilon &&
                    Math.Abs(Light - hsb.PreciseLight) < double.Epsilon) {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode() {
            return $"H:{PreciseHue}-S:{PreciseSaturation}-L:{PreciseLight}".GetHashCode();
        }
    }


    /// <summary>
    /// Represents a RGB color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    internal sealed class RgbColor {
        public RgbColor(
            int red,
            int green,
            int blue,
            int alpha) {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the red component. Values from 0 to 255.
        /// </summary>
        public int Red { get; }

        /// <summary>
        /// Gets or sets the green component. Values from 0 to 255.
        /// </summary>
        public int Green { get; }

        /// <summary>
        /// Gets or sets the blue component. Values from 0 to 255.
        /// </summary>
        public int Blue { get; }

        /// <summary>
        /// Gets or sets the alpha component. Values from 0 to 255.
        /// </summary>
        public int Alpha { get; }

        public static RgbColor FromColor(
            Color color) {
            return ColorConverting.ColorToRgb(color);
        }

        public static RgbColor FromRgbColor(
            RgbColor color) {
            return new RgbColor(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static RgbColor FromHsbColor(
            HsbColor color) {
            return color.ToRgbColor();
        }

        public static RgbColor FromHslColor(
            HslColor color) {
            return color.ToRgbColor();
        }

        public override string ToString() {
            return Alpha < 255 ? $"rgba({Red}, {Green}, {Blue}, {Alpha / 255d})" : $"rgb({Red}, {Green}, {Blue})";
        }

        public Color ToColor() {
            return ColorConverting.RgbToColor(this);
        }

        public RgbColor ToRgbColor() {
            return this;
        }

        public HsbColor ToHsbColor() {
            return ColorConverting.RgbToHsb(this);
        }

        public HslColor ToHslColor() {
            return ColorConverting.RgbToHsl(this);
        }

        public override bool Equals(
            object obj) {
            var equal = false;

            if (obj is RgbColor color) {
                var rgb = color;

                if (Red == rgb.Red && Blue == rgb.Blue && Green == rgb.Green) {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode() {
            return $"R:{Red}-G:{Green}-B:{Blue}-A:{Alpha}".GetHashCode();
        }
    }

}
