using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GenshinNotifier {


    internal static class UDPService {

        public static EventHandler Handlers;
        public static int Port = 56789;
        public static bool Closed = false;
        private static UdpClient Client;

        public static async Task StartServer() {
            Client = new UdpClient(Port);
            while (!Closed) {
                var receivedResult = await Client.ReceiveAsync();
                Logger.Debug(Encoding.UTF8.GetString(receivedResult.Buffer));
            }
        }

        public static void StopServer() {
            Closed = true;
            Client.Close();
            Client = null;
        }

        private static bool ListenUDPNext = true;
        private const string DefaultIP = "127.0.0.1";
        private const int DefaultPort = 45678;
        public static void OnUDPReceived(IAsyncResult result) {
            // this is what had been passed into BeginReceive as the second parameter:
            UdpClient server = result.AsyncState as UdpClient;
            if (!ListenUDPNext) {
                server.Close();
                return;
            }
            // points towards whoever had sent the message:
            IPEndPoint source = new IPEndPoint(0, 0);
            // get the actual message and fill out the source:
            byte[] bytes = server.EndReceive(result, ref source);
            string message = Encoding.UTF8.GetString(bytes);
            //Console.WriteLine("UDP Received: " + Encoding.UTF8.GetString(bytes));
            if (message == Storage.AppGuidStr) {
                // new instance, show front
                Handlers?.Invoke(message, new EventArgs());
            }
            // schedule the next receive operation once reading is done:
            server.BeginReceive(new AsyncCallback(OnUDPReceived), server);
        }

        public static void StopUDP() {
            ListenUDPNext = false;
        }

        // another way is using IpcChannel
        public static void BeginUDP(int listenPort = DefaultPort) {
            var ip = new IPEndPoint(IPAddress.Parse(DefaultIP), listenPort);
            UdpClient server = new UdpClient(ip);
            server.BeginReceive(new AsyncCallback(OnUDPReceived), server);
        }

        public static void SendUDP(string message = null, int targetPort = DefaultPort, string targetIP = DefaultIP) {
            UdpClient client = new UdpClient();
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
            var bytes = Encoding.UTF8.GetBytes(message ?? Storage.AppGuidStr);
            client.Send(bytes, bytes.Length, target);
        }

    }
}
