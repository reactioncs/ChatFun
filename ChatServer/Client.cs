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

        private readonly PacketReader packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;

            packetReader = new(ClientSocket.GetStream());

            if (packetReader.ReadOpcede() != Opcode.EstablishConnection)
                throw new Exception("EstablishConnection failed.");

            string username = packetReader.ReadMessage();
            Guid uid = new(packetReader.ReadMessage());
            User = new(username, uid);

            IPEndPoint p = (IPEndPoint)client.Client.RemoteEndPoint!;
            Console.WriteLine($"[{DateTime.Now}][Connect]   : UserName: \"{User.UserName}\" From: {p.Address}:{p.Port}");

            Thread ProcessThread = new(Process);
            ProcessThread.Start();
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
                            Console.WriteLine($"[{DateTime.Now}][Message]   : {message}   ({User.UserName})");
                            MessageReceivedEvent?.Invoke(message);
                            break;
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine($"[{DateTime.Now}][Disconnect]: UserName: \"{User.UserName}\" ({User.UID})");
                    ClientSocket.Close();
                    DisconnectedEvent?.Invoke(User.UID);
                    break;
                }
            }
        }
    }
}
