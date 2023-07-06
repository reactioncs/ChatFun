using ChatServer;
using IO;
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
                broadCastPacket.WriteOpCode(1);
                broadCastPacket.WriteMessage(client.UserName);
                broadCastPacket.WriteMessage(client.UID.ToString());
                client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
            }
        }
    }

    private void BroadCastMessage(string message)
    {
        PacketBuilder broadCastPacket = new();
        broadCastPacket.WriteOpCode(5);
        broadCastPacket.WriteMessage(message);

        foreach (Client client_to_send in clients)
        {
            client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
        }
    }

    private void BroadCastDisconnected(Guid uid)
    {
        Client disconnectedClient = clients.Where(c => c.UID == uid).FirstOrDefault()!;
        clients.Remove(disconnectedClient);

        PacketBuilder broadCastPacket = new();
        broadCastPacket.WriteOpCode(10);
        broadCastPacket.WriteMessage(uid.ToString());

        foreach (Client client_to_send in clients)
        {
            client_to_send.ClientSocket.Client.Send(broadCastPacket.GetPackedBytes());
        }
    }
}

//namespace ChatServer
//{
//    public class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Hello World!");
//        }
//    }
//}