using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carrot.ProCom.Common;

namespace Carrot.ProCom.Pipe {

    public delegate bool PipeMessageHandler(NamedPipeServerStream server, string message);

    public class PipeServiceEventArgs : EventArgs {
        public string Message { get; set; }
        public Exception Error { get; set; }
        public bool Failed { get; set; }

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

        public static PipeService Default {
            get { return lazy.Value; }
        }
        private static readonly Lazy<PipeService> lazy =
       new Lazy<PipeService>(() => new PipeService());


        public EventHandler Handlers;
        public PipeMessageHandler MessageHandler;

        private bool serverReady;
        private NamedPipeServerStream pipeServer;
        public string PipeName { get; set; }


        public PipeService() {
            Debug.WriteLine("PipeService");
        }

        public void StartServer(string pipeName = null) {
            this.PipeName = pipeName ?? Const.AppGuidStr;
            Debug.WriteLine($"PipeService.StartServer {PipeName}");
            pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            pipeServer.BeginWaitForConnection(new AsyncCallback(OnPipeReceived), pipeServer);
            serverReady = true;
        }

        public void StopServer() {
            serverReady = false;
            try {
                MessageHandler = null;
                if (pipeServer != null) {
                    Debug.WriteLine($"PipeService.StopServer {PipeName}");
                    pipeServer.Close();
                    pipeServer.Dispose();
                }
            } catch (Exception ex) {
                Debug.WriteLine($"PipeService.StopServer error={ex.Message}");
            } finally {
                pipeServer = null;
            }
        }

        private void OnPipeReceived(IAsyncResult asyncResult) {
            if (serverReady) {
                NamedPipeServerStream server = asyncResult.AsyncState as NamedPipeServerStream;
                Debug.WriteLine($"PipeService.OnPipeReceived {PipeName}");
                server.EndWaitForConnection(asyncResult);
                HandleMessageAsync(server);
            }
        }

        private void HandleMessageAsync(NamedPipeServerStream server) {
            Task.Run(() => {
                var stream = new StreamString(server);
                try {
                    var message = stream.ReadString();
                    bool handled = MessageHandler?.Invoke(server, message) ?? false;
                    if (!handled) {
                        stream.WriteString(Const.RES_OK + DateTime.Now.ToString("s"));
                    }
                    Debug.WriteLine($"PipeService.HandleMessage {message} (handled={handled})");
                    Handlers?.Invoke(server, new PipeServiceEventArgs(false, message));
                } catch (Exception ex) {
                    stream.WriteString(Const.RES_ERR + ex.Message);
                    Handlers?.Invoke(server, new PipeServiceEventArgs(true, ex));
                    Debug.WriteLine($"PipeService.HandleMessage {ex.GetType().Name} {ex.Message}");
                } finally {
                    Debug.WriteLine("PipeService.HandleMessage waiting next...");
                    server.Disconnect();
                    server.BeginWaitForConnection(new AsyncCallback(OnPipeReceived), server);
                }
            });
        }

        public static void SendAndReceiveAsync(string pipe, string message,
            Action<string, Exception> callback) {
            Task.Run(() => {
                var (response, error) = SendAndReceive(pipe, message);
                callback?.Invoke(response, error);
            });

        }

        public static (string, Exception) SendAndReceive(string pName, string message) {
            Debug.WriteLine($"PipeService.SendAndReceive {pName} create");
            try {
                var pipe = new NamedPipeClientStream(".", pName, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect();
                pipe.ReadMode = PipeTransmissionMode.Message;
                var stream = new StreamString(pipe);
                stream.WriteString(message);
                var res = stream.ReadString();
                Debug.WriteLine($"PipeService.SendAndReceive {pName} res={res}");
                pipe.Flush();
                pipe.Close();
                return (res, null);
            } catch (Exception ex) {
                Debug.WriteLine($"PipeService.SendAndReceive {pName} error={ex.Message}");
                return (null, ex);
            }

        }



    }
}
