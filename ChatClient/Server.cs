using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatCommon.IO;
using ChatCommon.Model;

namespace ChatFun
{
    public class Server
    {
        private readonly TcpClient client;

        public PacketReader? PacketReader { get; private set; }

        public event Action<UserModel>? UserConnectedEvent;
        public event Action<string>? MessageReceivedEvent;
        public event Action<Guid>? UserDisconnectedEvent;

        public Server()
        {
            client = new();
        }

        public void ConnectToServer(string address, int port, string username)
        {
            if (client.Connected)
                return;

            client.Connect(address, port);

            PacketReader = new(client.GetStream());

            PacketBuilder packet = new();
            packet.WriteOpCode(Opcode.EstablishConnection);
            packet.WriteMessage(username);
            client.Client.Send(packet.GetPackedBytes());

            Task.Run(Process);
        }

        public void SendMessageToServer(string message)
        {
            PacketBuilder packet = new();
            packet.WriteOpCode(Opcode.Message);
            packet.WriteMessage(message);
            client.Client.Send(packet.GetPackedBytes());
        }

        private void Process()
        {
            while (true)
            {
                Opcode opcode = PacketReader!.ReadOpcede();

                switch (opcode)
                {
                    case Opcode.UserConnect:
                        UserModel user = new(PacketReader!.ReadMessage(), new(PacketReader!.ReadMessage()));
                        UserConnectedEvent?.Invoke(user);
                        break;
                    case Opcode.Message:
                        string message = PacketReader!.ReadMessage();
                        MessageReceivedEvent?.Invoke(message);
                        break;
                    case Opcode.UserDisconnect:
                        Guid uid = new(PacketReader!.ReadMessage());
                        UserDisconnectedEvent?.Invoke(uid);
                        break;
                }
            }
        }
    }
}
