using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Carrot.UI.Controls.Picker {
    public class ColorEventArgs : EventArgs {
        /// <summary>
        /// Evented color w/o Alpha component
        /// </summary>
        public Color ColorRGB { get; set; }

        /// <summary>
        /// Evented color with Alpha component
        /// </summary>
        public Color ColorARGB { get; set; }
    }
}
