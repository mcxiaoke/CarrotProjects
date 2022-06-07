using System.IO;
using System.Text;

namespace Carrot.ProCom.Common {

    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString {
        private const string MAGIC = "@::@";

        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream) {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString() {
            int length = streamEncoding.GetByteCount(MAGIC);
            byte[] result = new byte[length];
            ioStream.Read(result, 0, result.Length);
            string rMagic = streamEncoding.GetString(result);
            //Debug.WriteLine($"ReadString magic={rMagic}");
            if (rMagic != MAGIC) {
                throw new IOException($"Invalid Magic Header {rMagic}");
            }

            int len = 0;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            //Debug.WriteLine($"ReadString len={len}");
            if (len < 0 || len > ushort.MaxValue) {
                throw new IOException($"Invalid Stream Length {len}");
            }
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString) {
            byte[] magicBuf = streamEncoding.GetBytes(MAGIC);
            ioStream.Write(magicBuf, 0, magicBuf.Length);
            //Debug.WriteLine($"WriteString magicBuf={magicBuf}");

            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue) {
                len = ushort.MaxValue;
            }
            //Debug.WriteLine($"WriteString len={len}");
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
            return outBuffer.Length + 2;
        }
    }
}