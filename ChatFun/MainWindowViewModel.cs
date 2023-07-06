using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChatFun
{
    public partial class MainWindowViewModel: ObservableObject
    {
        [ObservableProperty]
        private string address = "192.168.110.81";
        [ObservableProperty]
        private string portStr = "8631";
        [ObservableProperty]
        private string username = "Alice";
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string message = string.Empty;

        public ObservableCollection<UserModel> Users { get; set; } = new() { new("David", new()), new("Bob", new()) };
        public ObservableCollection<string> Messages { get; set; } = new();

        private Server server = new();

        [RelayCommand]
        private void Connect()
        {
            if (!int.TryParse(PortStr, out int port))
                return;

            server.ConnectToServer(Address, port, Username);
        }

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private void SendMessage()
        {
            server.SendMessageToServer(Message);
            Message = string.Empty;
        }
        private bool CanSendMessage() => !string.IsNullOrEmpty(Message);


        public MainWindowViewModel()
        {
            server.ConnectedEvent += UserConnected;
            server.MessageReceivedEvent += MessageReceived;
            server.DisconnectedEvent += Disconnected;
        }

        private void UserConnected()
        {
            UserModel user = new(server.PacketReader!.ReadMessage(), new(server.PacketReader!.ReadMessage()));

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void MessageReceived()
        {
            string message = server.PacketReader!.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private void Disconnected()
        {
            Guid uid = new(server.PacketReader!.ReadMessage());
            UserModel? user = Users.Where(u => u.UID == uid).FirstOrDefault();
            if (user == null)
                return;

            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }
    }

    public record UserModel(string UserName, Guid UID);
}
