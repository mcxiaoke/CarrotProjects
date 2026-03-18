using System;
using System.Drawing;
using System.Windows.Forms;

namespace SharpUpdater {

    public static class RichTextBoxExtensions {

        public static void AppendColorText(this RichTextBox box, string text, Color? color = null) {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color ?? box.ForeColor;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}