using ChatServer;
using ChatCommon.IO;
using System.Net;
using System.Net.Sockets;

ClientInstance clientInstance = new();
clientInstance.Run();

class ClientInstance
{
    private List<Client> clients = new();

    private TcpListener listener = new(IPAddress.Parse("192.168.110.81"), 8631);

    public void Run()
    {
        listener.Start();

        while (true)
        {
            Client client = new(listener.AcceptTcpClient());

            client.MessageReceivedEvent += BroadCastMessage;
            client.DisconnectedEvent += BroadCastDisconnected;
            clients.Add(client);

            BroadCastConnention();
        }
    }

    private void BroadCastConnention()
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