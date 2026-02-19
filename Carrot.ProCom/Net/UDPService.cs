using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Carrot.ProCom.Common;

namespace Carrot.ProCom.Net {

    public static class UDPService {
        private static string AppGuidStr => ProComConst.PIPE_MAIN;

        // Message is passed as sender (object)
        public static event EventHandler? Handlers;

        private const int DefaultPort = 45678;
        private const string DefaultIP = "127.0.0.1";

        private static CancellationTokenSource? _cts;
        private static UdpClient? _listener;

        public static void Start(int listenPort = DefaultPort) {
            if (_cts != null) return;

            Debug.WriteLine($"UDPService.Start {AppGuidStr} Port={listenPort}");
            _cts = new CancellationTokenSource();

            Task.Run(() => ListenerLoop(listenPort, _cts.Token));
        }

        private static async Task ListenerLoop(int port, CancellationToken token) {
            try {
                var localEp = new IPEndPoint(IPAddress.Parse(DefaultIP), port);
                _listener = new UdpClient(localEp);

                while (!token.IsCancellationRequested) {
                    try {
                        var result = await _listener.ReceiveAsync(token);
                        var message = Encoding.UTF8.GetString(result.Buffer);

                        if (message == AppGuidStr) {
                             Handlers?.Invoke(message, EventArgs.Empty);
                        }

                        Debug.WriteLine($"UDPService Received: {message}");
                    } catch (OperationCanceledException) {
                        break;
                    } catch (Exception ex) {
                         if (token.IsCancellationRequested) break;
                        Debug.WriteLine($"UDPService.ListenerLoop Error: {ex.Message}");
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine($"UDPService.ListenerLoop Setup Error: {ex.Message}");
            } finally {
                _listener?.Close();
                _listener?.Dispose();
                _listener = null;
            }
        }

        public static void Stop() {
            Debug.WriteLine($"UDPService.Stop {AppGuidStr}");
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _listener?.Close(); // Force break blocking calls if any non-cancellable
        }

        public static void SendUDP(string? message = null, int targetPort = DefaultPort, string targetIP = DefaultIP) {
            Debug.WriteLine($"UDPService.SendUDP {AppGuidStr}");
            try {
                using var client = new UdpClient();
                var target = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
                var bytes = Encoding.UTF8.GetBytes(message ?? AppGuidStr);
                client.Send(bytes, bytes.Length, target);
            } catch (Exception ex) {
                Debug.WriteLine($"UDPService.SendUDP Error: {ex.Message}");
            }
        }
    }
}