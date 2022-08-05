using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDPanel {
    // from https://stackoverflow.com/questions/4501511
    /*
standardOutput = new AsyncStreamReader(process.StandardOutput);
standardError = new AsyncStreamReader(process.StandardError);

standardOutput.DataReceived += (sender, data) =>
{
    //Code here
};

standardError.DataReceived += (sender, data) =>
{
    //Code here
};

StandardOutput.Start();
StandardError.Start();
     */
    public class AsyncStreamReader {

        public event EventHandler<string>? DataReceived;

        protected readonly byte[] buffer = new byte[4096];
        protected readonly StreamReader reader;


        /// <summary>
        ///  If AsyncStreamReader is active
        /// </summary>
        public bool Active { get; private set; }

        public void Start() {
            if (!Active) {
                Active = true;
                BeginReadAsync();
            }
        }

        public void Stop() {
            Active = false;
        }

        public AsyncStreamReader(StreamReader readerToBypass) {
            this.reader = readerToBypass;
            this.Active = false;
        }

        protected void BeginReadAsync() {
            if (this.Active) {
                reader.BaseStream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(ReadCallback), null);
            }
        }

        private void ReadCallback(IAsyncResult asyncResult) {
            var bytesRead = reader.BaseStream.EndRead(asyncResult);

            //Terminate async processing if callback has no bytes
            if (bytesRead > 0) {
                string data = reader.CurrentEncoding.GetString(this.buffer, 0, bytesRead);
                //Send data to event subscriber - null if no longer active
                this.DataReceived?.Invoke(this, data);
            } else {
                //callback without data - stop async
                this.Active = false;
            }

            //Wait for more data from stream
            this.BeginReadAsync();
        }

    }
}
