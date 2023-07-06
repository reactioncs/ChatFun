using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using ChatCommon.Model;

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

        public ObservableCollection<UserModel> Users { get; set; } = new();
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
            server.UserConnectedEvent += UserConnected;
            server.MessageReceivedEvent += MessageReceived;
            server.UserDisconnectedEvent += UserDisconnected;
        }

        private void UserConnected(UserModel user)
        {
            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void MessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private void UserDisconnected(Guid uid)
        {
            UserModel? user = Users.Where(u => u.UID == uid).FirstOrDefault();
            if (user == null)
                return;

            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }
    }
}
