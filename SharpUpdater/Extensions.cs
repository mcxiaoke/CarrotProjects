using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpUpdater {


    public static class RichTextBoxExtensions {

        public static void AppendColorText2(this RichTextBox box,
            string text, Color? color = null, bool addNewLine = false) {
            box.SuspendLayout();
            box.SelectionColor = color ?? box.ForeColor;
            box.AppendText(addNewLine
                ? $"{text}{Environment.NewLine}"
                : text);
            box.ScrollToCaret();
            box.ResumeLayout();
        }

        public static void AppendColorText(this RichTextBox box, string text, Color? color = null) {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color ?? box.ForeColor;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
