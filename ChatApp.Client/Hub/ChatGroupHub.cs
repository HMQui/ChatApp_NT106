using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ChatApp.Client.Hub
{
    public class ChatGroupHub
    {
        private HubConnection _hubConnection;

        public event Action<int, string, string, string, DateTime> MessageReceived;
        public event Action ConnectionClosed;

        public ChatGroupHub()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7001/chatgrouphub")
                .Build();

            _hubConnection.On<int, string, string, string, DateTime>("ReceiveGroupMessage", 
                (groupId, senderEmail, message, messageType, sendAt) =>
                {
                    MessageReceived?.Invoke(groupId, senderEmail, message, messageType, sendAt);
                });

            _hubConnection.Closed += async (error) =>
            {
                ConnectionClosed?.Invoke();
            };
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
            }
        }

        public async Task Register(string email)
        {
            await _hubConnection.InvokeAsync("Register", email);
        }

        public async Task JoinGroup(int groupId)
        {
            await _hubConnection.InvokeAsync("JoinGroup", groupId);
        }

        public async Task LeaveGroup(int groupId)
        {
            await _hubConnection.InvokeAsync("LeaveGroup", groupId);
        }

        public async Task SendGroupMessage(int groupId, byte[] data, string senderEmail, string messageType, DateTime sendAt, string originalFileName = "")
        {
            await _hubConnection.InvokeAsync("SendGroupMessage", groupId, data, senderEmail, messageType, sendAt, originalFileName);
        }
    }
} 