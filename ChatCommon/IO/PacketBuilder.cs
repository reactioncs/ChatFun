using System.Text;

namespace ChatCommon.IO
{
    public class PacketBuilder
    {
        private readonly MemoryStream stream;

        public PacketBuilder()
        {
            stream = new();    
        }

        public void WriteOpCode(Opcode opcode)
        {
            stream.WriteByte((byte)opcode);
        }

        public void WriteMessage(string message)
        {
            int length = message.Length;
            stream.Write(BitConverter.GetBytes(length));
            stream.Write(Encoding.ASCII.GetBytes(message));
        }

        public byte[] GetPackedBytes()
        {
            return stream.ToArray();
        }
    }
}
