using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrot.UI.Samples {
    public class BindingErrorListener : TraceListener {
        private readonly Action<string> _errorHandler;

        public BindingErrorListener(Action<string> errorHandler) {
            _errorHandler = errorHandler;
            TraceSource bindingTrace = PresentationTraceSources
                .DataBindingSource;

            bindingTrace.Listeners.Add(this);
            bindingTrace.Switch.Level = SourceLevels.Information;
        }

        public override void WriteLine(string message) {
            _errorHandler?.Invoke(message);
        }

        public override void Write(string message) {
        }
    }
}
