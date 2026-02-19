using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Carrot.ProCom.Common;

namespace Carrot.ProCom.Pipe {

    public delegate bool PipeMessageHandler(NamedPipeServerStream server, string message);

    public class PipeServiceEventArgs : EventArgs {
        public string? Message { get; set; }
        public Exception? Error { get; set; }
        public bool Failed { get; set; } = false;

        public PipeServiceEventArgs(bool failed, string message) {
            this.Failed = failed;
            this.Message = message;
        }

        public PipeServiceEventArgs(bool failed, Exception error) {
            this.Failed = failed;
            this.Error = error;
        }
    }

    public class PipeService {

        public static PipeService Default => lazy.Value;

        private static readonly Lazy<PipeService> lazy =
            new Lazy<PipeService>(() => new PipeService());

        public event EventHandler<PipeServiceEventArgs>? Handlers;
        public event PipeMessageHandler? MessageHandler;

        private CancellationTokenSource? _cts;
        private NamedPipeServerStream? _pipeServer;
        public string? PipeName { get; private set; }

        private PipeService() {
            Debug.WriteLine("PipeService initialized");
        }

        public void StartServer(string pName) {
            if (_cts != null) return; // Already running

            this.PipeName = pName;
            _cts = new CancellationTokenSource();
            Debug.WriteLine($"PipeService.StartServer name={pName}");

            // Start the server loop in a background task
            Task.Run(() => ServerLoop(_cts.Token));
        }

        private async Task ServerLoop(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                try {
                    // Create a new instance for each connection cycle
                    // Note: NamedPipeServerStream instances are one-shot or must be disconnected.
                    // To be robust against disposal races, we create/dispose inside the loop or manage carefully.
                    // Here we reuse if possible but safest is to recreate or manage state.
                    // The original code reused the instance. Let's try to reuse it but handle disposal.

                    if (_pipeServer == null) {
                        _pipeServer = new NamedPipeServerStream(PipeName!, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    }

                    Debug.WriteLine($"PipeService.ServerLoop Waiting for connection on {PipeName}...");
                    await _pipeServer.WaitForConnectionAsync(token);

                    if (token.IsCancellationRequested) break;

                    Debug.WriteLine($"PipeService.ServerLoop Connected");
                    await HandleMessageAsync(_pipeServer);

                    // Disconnect to be ready for next client
                    _pipeServer.Disconnect();

                } catch (OperationCanceledException) {
                    break;
                } catch (Exception ex) {
                    Debug.WriteLine($"PipeService.ServerLoop Error: {ex.Message}");
                    // Backoff slightly on error
                    await Task.Delay(1000, token);
                    // Recreate server if needed
                    _pipeServer?.Dispose();
                    _pipeServer = null;
                }
            }
            _pipeServer?.Dispose();
            _pipeServer = null;
        }

        public void StopServer() {
            if (_cts == null) return;

            Debug.WriteLine($"PipeService.StopServer {PipeName}");
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            // Force close the stream to break any pending wait
            try {
                _pipeServer?.Close(); // This might throw if accessed concurrently but we are stopping.
                _pipeServer?.Dispose();
            } catch (Exception ex) {
                Debug.WriteLine($"PipeService.StopServer cleanup error: {ex.Message}");
            } finally {
                _pipeServer = null;
            }
        }

        private async Task HandleMessageAsync(NamedPipeServerStream server) {
            var stream = new StreamString(server);
            try {
                var message = await stream.ReadStringAsync();
                bool handled = MessageHandler?.Invoke(server, message) ?? false;
                if (!handled) {
                    await stream.WriteStringAsync(ProComConst.RES_OK + DateTime.Now.ToString("s"));
                }
                Debug.WriteLine($"PipeService.HandleMessage {message} (handled={handled})");
                Handlers?.Invoke(this, new PipeServiceEventArgs(false, message));
            } catch (Exception ex) {
                try {
                    if (server.IsConnected) {
                        await stream.WriteStringAsync(ProComConst.RES_ERR + ex.Message);
                    }
                } catch { /* Ignore write error during exception */ }

                Handlers?.Invoke(this, new PipeServiceEventArgs(true, ex));
                Debug.WriteLine($"PipeService.HandleMessage Error: {ex.GetType().Name} {ex.Message}");
            }
        }

        public static void SendAndReceiveCallback(string pipe, string message,
            Action<string?, Exception?> callback) {
            Task.Run(async () => {
                var (response, error) = await SendAndReceiveAsync(pipe, message).ConfigureAwait(false);
                callback?.Invoke(response, error);
            });
        }

        public static (string?, Exception?) SendAndReceive(string pName, string message) {
            return Task.Run(() => SendAndReceiveAsync(pName, message)).GetAwaiter().GetResult();
        }

        public static async Task<(string?, Exception?)> SendAndReceiveAsync(string pName, string message) {
            Debug.WriteLine($"PipeService.SendAndReceive {pName} create");
            try {
                using var pipe = new NamedPipeClientStream(".", pName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await pipe.ConnectAsync(5000).ConfigureAwait(false); // 5s timeout
                pipe.ReadMode = PipeTransmissionMode.Message;

                var stream = new StreamString(pipe);
                await stream.WriteStringAsync(message).ConfigureAwait(false);
                var res = await stream.ReadStringAsync().ConfigureAwait(false);

                Debug.WriteLine($"PipeService.SendAndReceive {pName} res={res}");
                // pipe.Close() handled by using
                return (res, null);
            } catch (Exception ex) {
                Debug.WriteLine($"PipeService.SendAndReceive {pName} error={ex.Message}");
                return (null, ex);
            }
        }
    }
}