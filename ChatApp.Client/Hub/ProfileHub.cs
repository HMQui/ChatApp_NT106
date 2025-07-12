using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class ProfileHub : IAsyncDisposable
    {
        private HubConnection _connection;
        private bool _isDisposed = false;
        private string _myEmail = string.Empty;

        public ProfileHub(string myEmail)
        {
            _myEmail = myEmail;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/profile") 
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<string> onAvatarUpdated)
        {
            if (_isDisposed) return;

            _connection.On<string>("AvatarUpdated", (avatarUrl) =>
            {
                onAvatarUpdated?.Invoke(avatarUrl);
            });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }

        public async Task UpdateAvatarAsync(byte[] imageData)
        {
            if (_isDisposed) return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }

            await _connection.InvokeAsync("UpdateAvatar", imageData, _myEmail);
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