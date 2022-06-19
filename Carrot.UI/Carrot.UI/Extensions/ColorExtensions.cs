using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Carrot.UI.Controls {
    public static class ColorExtensions {

        // https://stackoverflow.com/questions/3942878
        public static int PerceivedBrightness(this Color c) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
        }

        public static bool IsDarkColor(this Color c, int threhold = 150) {
            // if (red*0.299 + green*0.587 + blue*0.114) > 186 use #000000 else use #ffffff
            return c.PerceivedBrightness() > threhold;
        }

        public static Color ContrastColor(this Color iColor, Color darkColor, Color lightColor) {
            //  Counting the perceptive luminance (aka luma) - human eye favors green color... 
            double luma = (iColor.PerceivedBrightness() / 255);

            // Return black for bright colors, white for dark colors
            return luma > 0.5 ? darkColor : lightColor;
        }
        public static Color ContrastColor(this Color iColor) => iColor.ContrastColor(Colors.Black);
        public static Color ContrastColor(this Color iColor, Color darkColor) => iColor.ContrastColor(darkColor, Colors.White);
        // Converts a given Color to gray
        public static Color ToGray(this Color input) {
            byte g = (byte)(input.R * .299 + input.G * .587 + input.B * .114);
            return Color.FromArgb(input.A, g, g, g);
        }

        // Y = 0.2126 R + 0.7152 G + 0.0722 B
        public static bool IsDark(this Color c) => (c.ScR * 0.2126) + (c.ScG * 0.7152) + (c.ScB * 0.0722) < 0.5;

        // https://web.mst.edu/~rhall/web_design/color_readability.html
        public static int Brightness(this Color c) => ((c.R * 299) + (c.G * 587) + (c.B * 114)) / 1000;

        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color) {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }


        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ColorFromHSV(double hue, double saturation, double value, byte alpha = 255) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - (f * saturation)));
            int t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

            return hi switch {
                0 => Color.FromArgb(alpha, (byte)v, (byte)t, (byte)p),
                1 => Color.FromArgb(alpha, (byte)q, (byte)v, (byte)p),
                2 => Color.FromArgb(alpha, (byte)p, (byte)v, (byte)t),
                3 => Color.FromArgb(alpha, (byte)p, (byte)q, (byte)v),
                4 => Color.FromArgb(alpha, (byte)t, (byte)p, (byte)v),
                _ => Color.FromArgb(alpha, (byte)v, (byte)p, (byte)q),
            };
        }

        public static void GetHSV(this Color color, out double hue, out double saturation, out double value, out byte alpha) {
            var dc = color.ToDrawingColor();
            int max = Math.Max(dc.R, Math.Max(dc.G, dc.B));
            int min = Math.Min(dc.R, Math.Min(dc.G, dc.B));

            hue = dc.GetHue();
            //saturation = c.GetSaturation();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
            alpha = dc.A;
        }

        public static Color Opposite(this Color c) {
            c.GetHSV(out var h, out var s, out var v, out var a);
            h = (h + 180) % 360;
            return ColorFromHSV(h, s, v, a);
        }

        // https://stackoverflow.com/questions/1165107/how-do-i-invert-a-colour
        // https://stackoverflow.com/questions/359612/how-to-convert-rgb-color-to-hsv
        public static Color SmartInvert(this Color c) {
            var dc = System.Drawing.Color.FromArgb(c.ToDrawingColor().ToArgb() ^ 0xffffff);
            if (dc.R > 108 && dc.R < 148 &&
                dc.G > 108 && dc.G < 148 &&
                dc.B > 108 && dc.B < 148) {
                int avg = (dc.R + dc.G + dc.B) / 3;
                avg = avg > 128 ? 188 : 68;
                dc = System.Drawing.Color.FromArgb(255, avg, avg, avg);
            }
            return dc.ToMediaColor();
        }

        public static Color Invert(this Color c) => Color.FromArgb(c.A, c.R.Invert(), c.G.Invert(), c.B.Invert());


        private static byte Invert(this byte b) {
            unchecked {
                return (byte)(b + 128);
            }
        }

        /// <summary>
        /// Mixes 2 colors equally
        /// </summary>
        public static Color Mix(this Color color1, Color color2) {
            return Mix(color1, 0.5, color2);
        }

        /// <summary>
        /// Mixes factor*color1 with (1-factor)*color2.
        /// </summary>
        public static Color Mix(this Color color1, double factor, Color color2) {
            if (factor < 0)
                throw new Exception($"Factor {factor} must be greater equal 0.");
            if (factor > 1)
                throw new Exception($"Factor {factor} must be smaller equal 1.");

            if (factor == 0)
                return color2;
            if (factor == 1)
                return color1;

            var factor1 = 1 - factor;
            return Color.FromArgb(
              (byte)(color1.A * factor + color2.A * factor1),
              (byte)(color1.R * factor + color2.R * factor1),
              (byte)(color1.G * factor + color2.G * factor1),
              (byte)(color1.B * factor + color2.B * factor1));
        }

        /// <summary>
        /// Returns the hue, saturation and brightness of color
        /// </summary>
        public static (int Hue, double Saturation, double Brightness) GetHSB(this Color color) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int hue = 0;//for black, gray or white, hue could be actually any number, but usually 0 is 
                        //assign, which means red
            if (max - min != 0) {
                //not black, gray or white
                int maxMinDif = max - min;
                if (max == color.R) {
                    if (color.G >= color.B) {
                        hue = 60 * (color.G - color.B) / maxMinDif;
                    } else {
                        hue = 60 * (color.G - color.B) / maxMinDif + 360;
                    }
                } else if (max == color.G) {
                    hue = 60 * (color.B - color.R) / maxMinDif + 120;
                } else if (max == color.B) {
                    hue = 60 * (color.R - color.G) / maxMinDif + 240;
                }
            }

            double saturation = (max == 0) ? 0.0 : (1.0 - ((double)min / (double)max));

            return (hue, saturation, (double)max / 0xFF);
        }

        /// <summary>
        /// Makes the color lighter if factor>0 and darker if factor<0. 1 returns white, -1 returns 
        /// black.
        /// https://www.codeproject.com/Articles/5296124/Definitive-Guide-to-WPF-Colors-Color-Spaces-Color
        /// </summary>
        public static Color GetBrighterOrDarker(this Color color, double factor) {
            if (factor < -1)
                throw new Exception($"Factor {factor} must be greater equal -1.");
            if (factor > 1)
                throw new Exception($"Factor {factor} must be smaller equal 1.");

            if (factor == 0)
                return color;

            if (factor < 0) {
                //make color darker, changer brightness
                factor++;
                return Color.FromArgb(
                  color.A,
                  (byte)(color.R * factor),
                  (byte)(color.G * factor),
                  (byte)(color.B * factor));
            } else {
                //make color lighter, change saturation
                return Color.FromArgb(
                  color.A,
                  (byte)(color.R + (255 - color.R) * factor),
                  (byte)(color.G + (255 - color.G) * factor),
                  (byte)(color.B + (255 - color.B) * factor));
            }
        }

        /// <summary>
        /// Returns a color with the same hue, but brightness and saturation increased to 100%.
        /// </summary>
        public static Color ToFullColor(this Color color) {
            //step 1: increase brightness to 100%
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));
            if (max == min) {
                //for black, gray or white return white
                return Color.FromArgb(color.A, 0xFF, 0xFF, 0xFF);
            }

            double rBright = (double)color.R * 255 / max;
            double gBright = (double)color.G * 255 / max;
            double bBright = (double)color.B * 255 / max;

            //step2: increase saturation to 100%
            //lower smallest R, G, B component to zero and adjust second smallest color accordingly
            //p = (smallest R, G, B component) / 255
            //(255-FullColor.SecondComponent) * p + FullColor.SecondComponent = color.SecondComponent
            //FullColor.SecondComponent = (color.SecondComponent-255p)/(1-p)
            if (color.R == max) {
                if (color.G == min) {
                    double p = gBright / 255;
                    return Color.FromArgb(color.A, 0xFF, 0, (byte)((bBright - gBright) / (1 - p)));
                } else {
                    double p = bBright / 255;
                    return Color.FromArgb(color.A, 0xFF, (byte)((gBright - bBright) / (1 - p)), 0);
                }
            } else if (color.G == max) {
                if (color.R == min) {
                    double p = rBright / 255;
                    return Color.FromArgb(color.A, 0, 0xFF, (byte)((bBright - rBright) / (1 - p)));
                } else {
                    double p = bBright / 255;
                    return Color.FromArgb(color.A, (byte)((rBright - bBright) / (1 - p)), 0xFF, 0);
                }
            } else {
                if (color.R == min) {
                    double p = rBright / 255;
                    return Color.FromArgb(color.A, 0, (byte)((gBright - rBright) / (1 - p)), 0xFF);
                } else {
                    double p = bBright / 255;
                    return Color.FromArgb(color.A, (byte)((rBright - bBright) / (1 - p)), 0, 0xFF);
                }
            }
        }

        // https://sharpsnippets.wordpress.com/2014/03/11/c-extension-complementary-color/
        public static Color GetContrast(this Color Source, bool PreserveOpacity) {
            Color inputColor = Source;
            //if RGB values are close to each other by a diff less than 10%, then if RGB values are lighter side, decrease the blue by 50% (eventually it will increase in conversion below), if RBB values are on darker side, decrease yellow by about 50% (it will increase in conversion)
            byte avgColorValue = (byte)((Source.R + Source.G + Source.B) / 3);
            int diff_r = Math.Abs(Source.R - avgColorValue);
            int diff_g = Math.Abs(Source.G - avgColorValue);
            int diff_b = Math.Abs(Source.B - avgColorValue);
            if (diff_r < 20 && diff_g < 20 && diff_b < 20) //The color is a shade of gray
            {
                if (avgColorValue < 123) //color is dark
                {
                    inputColor = Color.FromArgb(Source.A, 220, 230, 50);
                } else {
                    inputColor = Color.FromArgb(Source.A, 255, 255, 50);
                }
            }
            byte sourceAlphaValue = Source.A;
            if (!PreserveOpacity) {
                sourceAlphaValue = Math.Max(Source.A, (byte)127); //We don't want contrast color to be more than 50% transparent ever.
            }
            RGB rgb = new RGB { R = inputColor.R, G = inputColor.G, B = inputColor.B };
            HSB hsb = rgb.ToHSB();
            hsb.H = hsb.H < 180 ? hsb.H + 180 : hsb.H - 180;
            //_hsb.B = _isColorDark ? 240 : 50; //Added to create dark on light, and light on dark
            rgb = hsb.ToRGB();
            return Color.FromArgb((byte)sourceAlphaValue, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
        }

        #region Code from MSDN
        internal static RGB ToRGB(this HSB hsb) {
            // Following code is taken as it is from MSDN. See link below.
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double chroma = hsb.S * hsb.B;
            double hue2 = hsb.H / 60;
            double x = chroma * (1 - Math.Abs((hue2 % 2) - 1));
            double r1 = 0d;
            double g1 = 0d;
            double b1 = 0d;
            if (hue2 >= 0 && hue2 < 1) {
                r1 = chroma;
                g1 = x;
            } else if (hue2 >= 1 && hue2 < 2) {
                r1 = x;
                g1 = chroma;
            } else if (hue2 >= 2 && hue2 < 3) {
                g1 = chroma;
                b1 = x;
            } else if (hue2 >= 3 && hue2 < 4) {
                g1 = x;
                b1 = chroma;
            } else if (hue2 >= 4 && hue2 < 5) {
                r1 = x;
                b1 = chroma;
            } else if (hue2 >= 5 && hue2 <= 6) {
                r1 = chroma;
                b1 = x;
            }
            double m = hsb.B - chroma;
            return new RGB() {
                R = r1 + m,
                G = g1 + m,
                B = b1 + m
            };
        }
        internal static HSB ToHSB(this RGB rgb) {
            // Following code is taken as it is from MSDN. See link below.
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double r = rgb.R;
            double g = rgb.G;
            double b = rgb.B;

            double max = Max(r, g, b);
            double min = Min(r, g, b);
            double chroma = max - min;
            double hue2 = 0d;
            if (chroma != 0) {
                if (max == r) {
                    hue2 = (g - b) / chroma;
                } else if (max == g) {
                    hue2 = ((b - r) / chroma) + 2;
                } else {
                    hue2 = ((r - g) / chroma) + 4;
                }
            }
            double hue = hue2 * 60;
            if (hue < 0) {
                hue += 360;
            }
            double brightness = max;
            double saturation = 0;
            if (chroma != 0) {
                saturation = chroma / brightness;
            }
            return new HSB() {
                H = hue,
                S = saturation,
                B = brightness
            };
        }
        private static double Max(double d1, double d2, double d3) {
            if (d1 > d2) {
                return Math.Max(d1, d3);
            }
            return Math.Max(d2, d3);
        }
        private static double Min(double d1, double d2, double d3) {
            if (d1 < d2) {
                return Math.Min(d1, d3);
            }
            return Math.Min(d2, d3);
        }
        internal struct RGB {
            internal double R;
            internal double G;
            internal double B;
        }
        internal struct HSB {
            internal double H;
            internal double S;
            internal double B;
        }
        #endregion //Code from MSDN
    }
}
