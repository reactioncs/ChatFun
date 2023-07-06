using System.Net.Sockets;
using System.Text;

namespace IO
{
    public class PacketReader : BinaryReader
    {
        private readonly NetworkStream stream;

        public PacketReader(NetworkStream ns) : base(ns)
        {
            stream = ns;
        }

        public string ReadMessage()
        {
            int length = ReadInt32();
            if (length <= 0)
                return string.Empty;

            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);

            string message = Encoding.ASCII.GetString(buffer);
            return message;
        }
    }
}
