using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Console.WriteLine(Encoding.UTF8.GetString(receivedResult.Buffer));
            }
        }

        public static void StopServer() {
            Closed = true;
            Client.Close();
            Client = null;
        }

        private static bool ListenUDPNext = true;
        public static void OnUDPReceived(IAsyncResult result) {
            // this is what had been passed into BeginReceive as the second parameter:
            UdpClient socket = result.AsyncState as UdpClient;
            // points towards whoever had sent the message:
            IPEndPoint source = new IPEndPoint(0, 0);
            // get the actual message and fill out the source:
            byte[] bytes = socket.EndReceive(result, ref source);
            string message = Encoding.UTF8.GetString(bytes);
            //Console.WriteLine("UDP Received: " + Encoding.UTF8.GetString(bytes));
            if (message == Storage.AppGuidStr) {
                // new instance, show front
                Handlers?.Invoke(message, new EventArgs());
            }

            if (ListenUDPNext) {
                // schedule the next receive operation once reading is done:
                socket.BeginReceive(new AsyncCallback(OnUDPReceived), socket);
            }
        }

        public static void StopUDP() {
            ListenUDPNext = false;
        }

        public static void BeginUDP(int listenPort = 45678) {
            UdpClient socket = new UdpClient(listenPort);
            socket.BeginReceive(new AsyncCallback(OnUDPReceived), socket);
        }

        public static void SendUDP(string message = null, int targetPort = 45678, string targetIP = "127.0.0.1") {
            UdpClient socket = new UdpClient();
            IPEndPoint target = new IPEndPoint(IPAddress.Parse(targetIP), targetPort);
            var bytes = Encoding.UTF8.GetBytes(message ?? Storage.AppGuidStr);
            socket.Send(bytes, bytes.Length, target);
        }

    }
}
