using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using ChatCommon.Model;
using System.Net;

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
        [NotifyPropertyChangedFor(nameof(ConnectButtonDisplay))]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private bool isConnected = false;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string message = string.Empty;

        public ObservableCollection<UserModel> Users { get; set; } = new();
        public ObservableCollection<string> Messages { get; set; } = new();

        private Server server = new();

        public string ConnectButtonDisplay => IsConnected ? "Disconnect" : "Connect";

        [RelayCommand]
        private void ToggleConnection()
        {
            if (IsConnected)
            {
                server.DisConnect();

                Users.Clear();
                Messages.Clear();

                IsConnected = false;
            }
            else
            {
                if (string.IsNullOrEmpty(Username))
                    return;
                if (!IPAddress.TryParse(Address, out IPAddress? ipAddress))
                    return;
                if (!int.TryParse(PortStr, out int port))
                    return;

                server.ConnectToServer(ipAddress, port, Username);
                IsConnected = true;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private void SendMessage()
        {
            server.SendMessageToServer(Message);
            Message = string.Empty;
        }
        private bool CanSendMessage()
        {
            if (string.IsNullOrEmpty(Message))
                return false;
            if (!IsConnected)
                return false;

            return true;
        }


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
