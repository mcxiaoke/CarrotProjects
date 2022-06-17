using System.Windows;
using System.Windows.Controls;

namespace GenshinNotifier {

    public class NoSizeDecorator : Decorator {

        protected override Size MeasureOverride(Size constraint) {
            // Ask for no space
            Child.Measure(new Size(0, 0));
            return new Size(0, 0);
        }
    }
}