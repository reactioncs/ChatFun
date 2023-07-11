using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ChatCommon.IO;
using ChatCommon.Model;

namespace ChatFun
{
    public class Server
    {
        public UserModel User { get; set; }

        public event Action<UserModel>? UserConnectedEvent;
        public event Action<string>? MessageReceivedEvent;
        public event Action<Guid>? UserDisconnectedEvent;

        private TcpClient client;

        private PacketReader? PacketReader;

        private Server()
        {
            User = new();
            client = new();
        }

        private static Server? mInstance = null;
        public static Server Instance
        {
            get
            {
                mInstance ??= new Server();
                return mInstance;
            }
        }

        public async Task ConnectToServerAsync(IPAddress address, int port, string username)
        {
            if (client.Connected)
                return;

            await client.ConnectAsync(address, port);

            PacketReader = new(client.GetStream());

            User.UserName = username;

            PacketBuilder packet = new();
            packet.WriteOpCode(Opcode.EstablishConnection);
            packet.WriteMessage(User.UserName);
            packet.WriteMessage(User.UID.ToString());
            client.Client.Send(packet.GetPackedBytes());

            Thread ProcessThread = new(Process);
            ProcessThread.Start();
        }

        public void DisConnect()
        {
            client.Close();
            client = new();
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
                try
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
                catch (IOException)
                {
                    break;
                }
            }
        }
    }
}
