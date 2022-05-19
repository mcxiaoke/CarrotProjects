using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinNotifier {

    public class SimpleEventArgs : EventArgs {
        public int intValue { get; set; }
        public string Value { get; set; }
        public object ObjValue { get; set; }

        public SimpleEventArgs(string value) {
            Value = value;
        }

    }
}
