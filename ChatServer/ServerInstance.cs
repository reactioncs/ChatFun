﻿using ChatCommon.IO;
using System.Net;
using System.Net.Sockets;
using ChatCommon;

namespace ChatServer
{
    class ServerInstance
    {
        private readonly List<Client> clients;
        private readonly TcpListener? listener;

        public ServerInstance()
        {
            clients = [];

            if (!ReadConfig.ReadAddress("config.txt", out IPAddress address, out int port))
                return;
            listener = new(address, port);

            Console.WriteLine($"[{DateTime.Now}][Startup]   : Start Server on {address}:{port}");
        }

        public void Run()
        {
            listener!.Start(100);

            while (true)
            {
                Client client = new(listener.AcceptTcpClient());

                client.MessageReceivedEvent += BroadCastMessage;
                client.DisconnectedEvent += BroadCastDisconnected;
                clients.Add(client);

                BroadCastConnection();
            }
        }

        private void BroadCastConnection()
        {
            foreach (Client client_to_send in clients)
            {
                foreach (Client client in clients)
                {
                    PacketBuilder broadCastPacket = new();
                    broadCastPacket.WriteOpCode(Opcode.UserConnect);
                    broadCastPacket.WriteMessage(client.User.UserName);
                    broadCastPacket.WriteMessage(client.User.UID.ToString());
                    client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
                }
            }
        }

        private void BroadCastMessage(string message)
        {
            PacketBuilder broadCastPacket = new();
            broadCastPacket.WriteOpCode(Opcode.Message);
            broadCastPacket.WriteMessage(message);

            foreach (Client client_to_send in clients)
            {
                client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
            }
        }

        private void BroadCastDisconnected(Guid uid)
        {
            Client disconnectedClient = clients.Where(c => c.User.UID == uid).FirstOrDefault()!;
            clients.Remove(disconnectedClient);

            PacketBuilder broadCastPacket = new();
            broadCastPacket.WriteOpCode(Opcode.UserDisconnect);
            broadCastPacket.WriteMessage(uid.ToString());

            foreach (Client client_to_send in clients)
            {
                client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
            }
        }
    }
}
