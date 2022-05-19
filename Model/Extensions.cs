using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinNotifier {
    public static class Extensions {

        public static StringBuilder AppendIf<T>(this StringBuilder @this, Func<T, bool> predicate, params T[] values) {
            foreach (var value in values) {
                if (predicate(value)) {
                    @this.Append(value);
                }
            }

            return @this;
        }


        public static StringBuilder AppendIf(this StringBuilder @this, bool condition, string value) {
            if (condition)
                @this.Append(value);
            return @this;
        }
    }
}
