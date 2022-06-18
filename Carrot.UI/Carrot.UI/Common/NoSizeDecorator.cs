using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Carrot.UI.Controls.Common {
    public class NoSizeDecorator : Decorator {

        protected override Size MeasureOverride(Size constraint) {
            // Ask for no space
            Child.Measure(new Size(0, 0));
            return new Size(0, 0);
        }
    }
}
