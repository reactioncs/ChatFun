using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using ChatCommon.Model;
using System.Net;
using ChatCommon;
using System.Threading.Tasks;

namespace ChatFun
{
    public partial class MainWindowViewModel: ObservableObject
    {
        [ObservableProperty]
        private string address = "";
        [ObservableProperty]
        private string portStr = "";
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

        private readonly Server server = Server.Instance;

        public string ConnectButtonDisplay => IsConnected ? "Disconnect" : "Connect";

        [RelayCommand]
        private async Task ToggleConnectionAsync()
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
                try
                {
                    await server.ConnectToServerAsync(ipAddress, port, Username);
                    IsConnected = true;
                }
                catch
                {
                    Address = "";
                    PortStr = "";
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private void SendMessage()
        {
            server.SendMessageToServer(Message);
            //Message = string.Empty;
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
            if (ReadConfig.ReadAdress("config.txt", out IPAddress adress, out int port))
            {
                Address = adress.ToString();
                PortStr = port.ToString();
            }

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
