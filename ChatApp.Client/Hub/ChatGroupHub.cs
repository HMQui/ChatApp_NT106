using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class ChatGroupHub : IAsyncDisposable
    {
        private HubConnection _connection;
        private bool _isDisposed = false;
        private string _myEmail;

        public ChatGroupHub(string myEmail)
        {
            _myEmail = myEmail;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/chat-group")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<int, string, string, string, string, DateTime> onGroupMessageReceived)
        {
            if (_isDisposed) return;

            _connection.On<int, string, string, string, string, DateTime>(
                "ReceiveGroupMessage",
                (groupId, senderEmail, senderNickname, data, messageType, sendAt) =>
                {
                    onGroupMessageReceived?.Invoke(groupId, senderEmail, senderNickname, data, messageType, sendAt);
                });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }


        public async Task SendGroupMessageAsync(int groupId,
                                        byte[] data,
                                        string nickname,
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

            await _connection.InvokeAsync("SendGroupMessage",
                groupId,
                data,
                _myEmail,
                nickname,
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
    }
}
