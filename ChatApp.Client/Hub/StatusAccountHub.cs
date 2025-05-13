using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class StatusAccountHub
    {
        private HubConnection _connection;
        private bool _isDisposed = false;

        public StatusAccountHub()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/status")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<string, string> onFriendStatusChanged)
        {
            if (_isDisposed) return;

            _connection.On<string, string>("FriendStatusChanged", (friendEmail, status) =>
            {
                if (!_isDisposed)
                    onFriendStatusChanged?.Invoke(friendEmail, status);
            });

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ConnectAsync failed: {ex.Message}");
            }
        }

        public async Task SetOnline(string email)
        {
            if (_isDisposed) return;

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
                await _connection.InvokeAsync("SetOnline", email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SetOnline failed: {ex.Message}");
            }
        }

        public async Task SetOffline(string email)
        {
            if (_isDisposed) return;

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
                await _connection.InvokeAsync("SetOffline", email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SetOffline failed: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_isDisposed) return;

            try
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DisconnectAsync failed: {ex.Message}");
            }
            finally
            {
                _isDisposed = true;
            }
        }
    }
}
