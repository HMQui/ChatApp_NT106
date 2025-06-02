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

        public async Task ConnectAsync(Action<string, string, string, DateTime> onMessageReceived)
        {
            if (_isDisposed) return;

            _connection.On<string, string, string, DateTime>("ReceiveMessage", (senderId, data, messageType, sendAt) =>
            {
                onMessageReceived?.Invoke(senderId, data, messageType, sendAt);
            });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }


        public async Task SendMessageAsync(string receiverEmail,
                                   byte[] data,
                                   string messageType,
                                   DateTime sendAt,
                                   string originalFileName = "")
        {
            if (_isDisposed) return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }


            await _connection.InvokeAsync("SendMessage",
                                          receiverEmail,
                                          data,
                                          _myEmail,
                                          messageType,
                                          sendAt,
                                          originalFileName);
        }



        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                await _connection.DisposeAsync();
                _isDisposed = true;
            }
        }

        internal async Task SendMessageAsync(string toEmail, byte[] data, object value, string v)
        {
            throw new NotImplementedException();
        }
    }
}
