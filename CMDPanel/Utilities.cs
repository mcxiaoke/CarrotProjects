using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMDPanel {
    public static class Utilities {

        // https://stackoverflow.com/questions/470256
        public static async Task WaitForExitAsyncSimple(this Process process, CancellationToken cancellationToken) {
            while (!process.HasExited) {
                await Task.Delay(100, cancellationToken);
            }
        }

        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
        /// immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsyncCustom(this Process process,
            CancellationToken cancellationToken = default) {
            if (process.HasExited)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => { process.Kill(); tcs.SetCanceled(); });

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            if (cancellationToken != default)
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
