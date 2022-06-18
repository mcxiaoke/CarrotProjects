using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Carrot.UI.Controls.Picker {
    /// <summary>
    /// Separate control of color dialog
    /// </summary>
    public class ColorCanvasUI : Canvas {



        #region Translated color property
        /// <summary>
        /// Color is changed by user
        /// </summary>
        public event EventHandler<ColorEventArgs> ColorChanged;
        protected virtual void OnColorChanged(ColorEventArgs e) {
            ColorChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Get or set Hue value
        /// </summary>
        public double H { get { return _logic.H; } set { _logic.H = value; } }

        /// <summary>
        /// Get or set Saturation value
        /// </summary>
        public double S { get { return _logic.S; } set { _logic.S = value; } }

        /// <summary>
        /// Get or set Lightness value
        /// </summary>
        public double L { get { return _logic.L; } set { _logic.L = value; } }

        /// <summary>
        /// Get or set Alpha value
        /// </summary>
        public byte A { get { return _logic.A; } set { _logic.A = value; } }

        /// <summary>
        /// Get or set color value w/o specific Alpha
        /// </summary>
        public Color ColorRGB { get { return _logic.ColorRGB; } set { _logic.ColorRGB = value; } }

        /// <summary>
        /// Get or set color value with specific Alpha
        /// </summary>
        public Color ColorARGB { get { return _logic.ColorARGB; } set { _logic.ColorARGB = value; } }

        #endregion

        private readonly ColorCanvasLogic _logic;

        public ColorCanvasUI() {
            _logic = new ColorCanvasLogic(this);
            _logic.ColorChanged += (sender, args) => OnColorChanged(args);
            this.SizeChanged += ColorCanvasUI_SizeChanged;
        }

        private void ColorCanvasUI_SizeChanged(object sender, SizeChangedEventArgs e) {
            _logic.RefreshResize();
        }
    }
}
