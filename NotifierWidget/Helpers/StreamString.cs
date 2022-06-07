using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NotifierWidget {

    public class StreamString {
        private static byte[] MAGIC = Encoding.UTF8.GetBytes("@::@");

        private static byte[] ByteArrayRightPad(byte[] input, byte padValue, int len) {
            var temp = Enumerable.Repeat(padValue, len).ToArray();
            for (var i = 0; i < input.Length; i++)
                temp[len - i - 1] = input[i];
            return temp;
        }

        private static byte[] ByteArrayLeftPad(byte[] input, byte padValue, int len) {
            var temp = Enumerable.Repeat(padValue, len).ToArray();
            for (var i = 0; i < input.Length; i++)
                temp[i] = input[i];
            return temp;
        }

        private Stream stream;
        private Encoding encoding;

        public StreamString(Stream ioStream) {
            this.stream = ioStream;
            this.encoding = new UnicodeEncoding();
        }

        public string ReadString() {
            // first 4b = magic str
            // next 4b = content length int
            // next 0-length content str
            byte[] magicBuf = new byte[MAGIC.Length];
            stream.Read(magicBuf, 0, MAGIC.Length);
            if (magicBuf != MAGIC) {
                return string.Empty;
            }

            byte[] lenBuf = new byte[4];
            stream.Read(lenBuf, 0, 4);
            int len = BitConverter.ToInt32(lenBuf, 0);
            byte[] inBuffer = new byte[len];
            stream.Read(inBuffer, 0, len);

            return encoding.GetString(inBuffer);
        }

        public int WriteString(string outString) {
            byte[] outBuffer = encoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue) {
                len = (int)UInt16.MaxValue;
            }
            byte[] lenBuf = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(lenBuf);
            }
            stream.Write(MAGIC, 0, MAGIC.Length);
            stream.Write(lenBuf, 0, lenBuf.Length);
            stream.Write(outBuffer, 0, len);
            stream.Flush();

            return outBuffer.Length + 2;
        }
    }
}