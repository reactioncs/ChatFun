using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IO;

namespace ChatFun
{
    public class Server
    {
        private readonly TcpClient client;

        public PacketReader? PacketReader { get; private set; }

        public event Action? ConnectedEvent;
        public event Action? MessageReceivedEvent;
        public event Action? DisconnectedEvent;

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
            packet.WriteOpCode(0);
            packet.WriteMessage(username);
            client.Client.Send(packet.GetPackedBytes());

            Task.Run(Process);
        }

        public void SendMessageToServer(string message)
        {
            PacketBuilder packet = new();
            packet.WriteOpCode(5);
            packet.WriteMessage(message);
            client.Client.Send(packet.GetPackedBytes());
        }

        private void Process()
        {
            while (true)
            {
                byte opcode = PacketReader!.ReadByte();

                switch(opcode)
                {
                    case 1:
                        ConnectedEvent?.Invoke();
                        break;
                    case 5:
                        MessageReceivedEvent?.Invoke();
                        break;
                    case 10:
                        DisconnectedEvent?.Invoke();
                        break;
                }
            }
        }
    }
}
