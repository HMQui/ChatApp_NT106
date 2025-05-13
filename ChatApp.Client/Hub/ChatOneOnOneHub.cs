using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class ChatOneOnOneHub : IAsyncDisposable
    {
        private HubConnection _connection;
        private bool _isDisposed = false;
        private string _myEmail = string.Empty;

        public ChatOneOnOneHub(string myEmail)
        {
            _myEmail = myEmail;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/chat-single")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<string, string, string> onMessageReceived)
        {
            if (_isDisposed) return;

            _connection.On<string, string, string>("ReceiveMessage", (senderId, data, messageType) =>
            {
                onMessageReceived?.Invoke(senderId, data, messageType);
            });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }


        public async Task SendMessageAsync(string receiverEmail, byte[] data, string messageType, string originalFileName = "")
        {
            if (_isDisposed) return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }

            await _connection.InvokeAsync("SendMessage", receiverEmail, data, _myEmail, messageType, originalFileName);
        }


        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                await _connection.DisposeAsync();
                _isDisposed = true;
            }
        }
    }
}
