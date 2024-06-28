using System.Text;

namespace ChatCommon.IO
{
    public class PacketReader : BinaryReader
    {
        private readonly Stream stream;

        public PacketReader(Stream ns) : base(ns)
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

        public Opcode ReadOpcode()
        {
            return (Opcode)ReadByte();
        }
    }
}
