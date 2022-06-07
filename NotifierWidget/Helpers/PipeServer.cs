using System;
using System.IO;
using System.IO.Pipes;

namespace NotifierWidget {

    internal static class StringExtension {

        public static string[] SplitEx(this string str, string splitter) {
            return str.Split(new[] { splitter }, StringSplitOptions.None);
        }
    }

    internal class PipeServer {

        public event EventHandler<string[]> MessageReceived;

        public static string MsgDelimiter { get; } = "^_^";

        public PipeServer(string channelName)//, bool biDirection = false)
        {
            CreateRemoteService(channelName);
        }

        private async void CreateRemoteService(string channelName) {
            using (var pipeServer = new NamedPipeServerStream(channelName, PipeDirection.In)) {
                while (true) {
                    await pipeServer.WaitForConnectionAsync().ConfigureAwait(false);
                    var reader = new StreamReader(pipeServer);
                    var rawArgs = await reader.ReadToEndAsync();
                    MessageReceived?.Invoke(this, rawArgs.SplitEx(MsgDelimiter));
                    pipeServer.Disconnect();
                }
            }
        }
    }
}