using System.Net;
using System.Net.Sockets;
using ChatCommon.IO;
using ChatCommon.Model;

namespace ChatServer
{
    public class Client
    {
        public UserModel User { get; set; }
        public TcpClient ClientSocket { get; set; }

        public event Action<string>? MessageReceivedEvent;
        public event Action<Guid>? DisconnectedEvent;

        private PacketReader packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;

            packetReader = new(ClientSocket.GetStream());

            Opcode opcode = packetReader.ReadOpcede();
            string username = packetReader.ReadMessage();
            User = new(username);

            IPEndPoint p = (IPEndPoint)client.Client.RemoteEndPoint!;
            Console.WriteLine($"[{DateTime.Now}][Connect]   : UserName: {User.UserName} From: {p.Address}:{p.Port}");

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
                            Console.WriteLine($"[{DateTime.Now}][Message]   : \"{message}\"({User.UserName})");
                            MessageReceivedEvent?.Invoke(message);
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{DateTime.Now}][Disconnect]: UserName: {User.UserName} ({User.UID})");
                    ClientSocket.Close();
                    DisconnectedEvent?.Invoke(User.UID);
                    break;
                }
            }
        }
    }
}
