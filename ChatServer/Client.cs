using System.Net;
using System.Net.Sockets;
using ChatCommon.IO;

namespace ChatServer
{
    public class Client
    {
        public string UserName { get; set; } = string.Empty;
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        public event Action<string>? MessageReceivedEvent;
        public event Action<Guid>? DisconnectedEvent;

        private PacketReader packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();

            packetReader = new(ClientSocket.GetStream());

            Opcode opcode = packetReader.ReadOpcede();
            UserName = packetReader.ReadMessage();

            IPEndPoint p = (IPEndPoint)client.Client.RemoteEndPoint!;
            Console.WriteLine($"[{DateTime.Now}]: Client connected: {UserName} From: {p.Address}:{p.Port}");

            Task.Run(Process);
        }

        private void Process()
        {
            while (true)
            {
                try
                {
                    Opcode opcode = packetReader.ReadOpcede();
                    switch (opcode)
                    {
                        case Opcode.Message:
                            string message = packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message: {message}");
                            MessageReceivedEvent?.Invoke(message);
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{DateTime.Now}]: {UID} Disconnected");
                    ClientSocket.Close();
                    DisconnectedEvent?.Invoke(UID);
                    break;
                }
            }
        }
    }
}
