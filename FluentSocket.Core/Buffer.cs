using System.Text;

namespace FluentSocket
{
    public class Buffer
    {
        private byte[] _bytes;

        public Buffer (byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] GetBytes() => _bytes;

        public string ToString(Encoding encoding) => encoding.GetString(_bytes);

        public static Buffer FromBytes(byte[] bytes) => new Buffer(bytes);
        public static Buffer FromBytes(byte[] bytes, int offset, int count) => FromBytes(Copy(bytes, offset, count));
        public static Buffer FromString(string str, Encoding encoding) => FromBytes(encoding.GetBytes(str));
        public static Buffer Create(int size) => FromBytes(new byte[size]);

        private static byte[] Copy(byte[] src, int offset, int count)
        {
            var dst = new byte[count];
            System.Buffer.BlockCopy(src, offset, dst, 0, count);
            return dst;
        }
    }
}