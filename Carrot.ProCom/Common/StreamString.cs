using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Carrot.ProCom.Common {

    /// <summary>
    /// \class StreamString
    /// 定义用于在流上读写字符串的数据协议
    /// </summary>
    public class StreamString {
        private const string MAGIC = "@::@";

        private readonly Stream ioStream;
        private readonly UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream) {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public async Task<string> ReadStringAsync() {
            int length = streamEncoding.GetByteCount(MAGIC);
            byte[] result = new byte[length];
            await ioStream.ReadExactlyAsync(result, 0, result.Length).ConfigureAwait(false);

            string rMagic = streamEncoding.GetString(result);
            if (rMagic != MAGIC) {
                throw new IOException($"Invalid Magic Header {rMagic}");
            }

            int len = 0;
            var lenBuffer = new byte[2];
            await ioStream.ReadExactlyAsync(lenBuffer, 0, 2).ConfigureAwait(false);
            len = lenBuffer[0] * 256 + lenBuffer[1];

            if (len < 0 || len > ushort.MaxValue) {
                throw new IOException($"Invalid Stream Length {len}");
            }
            byte[] inBuffer = new byte[len];
            await ioStream.ReadExactlyAsync(inBuffer, 0, len).ConfigureAwait(false);

            return streamEncoding.GetString(inBuffer);
        }

        public async Task<int> WriteStringAsync(string outString) {
            byte[] magicBuf = streamEncoding.GetBytes(MAGIC);
            await ioStream.WriteAsync(magicBuf, 0, magicBuf.Length).ConfigureAwait(false);

            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue) {
                len = ushort.MaxValue;
            }

            var lenBuffer = new byte[] { (byte)(len / 256), (byte)(len & 255) };
            await ioStream.WriteAsync(lenBuffer, 0, 2).ConfigureAwait(false);
            await ioStream.WriteAsync(outBuffer, 0, len).ConfigureAwait(false);
            await ioStream.FlushAsync().ConfigureAwait(false);
            return outBuffer.Length + 2;
        }

        // Synchronous methods for backward compatibility
        public string ReadString() {
            return Task.Run(() => ReadStringAsync()).GetAwaiter().GetResult();
        }

        public int WriteString(string outString) {
            return Task.Run(() => WriteStringAsync(outString)).GetAwaiter().GetResult();
        }
    }
}