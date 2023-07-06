using System.Text;

namespace IO
{
    public class PacketBuilder
    {
        private readonly MemoryStream stream;

        public PacketBuilder()
        {
            stream = new();    
        }

        public void WriteOpCode(byte opcode)
        {
            stream.WriteByte(opcode);
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
