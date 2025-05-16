using Microsoft.AspNetCore.SignalR.Client;


namespace ChatApp.Client.Hub
{
    public class NotificationHub
    {
        private HubConnection _connection;
        private bool _isDisposed = false;
        private string myEmail = string.Empty;
        public NotificationHub(string myEmail)
        {
            this.myEmail = myEmail;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/notification")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<int, string, string, string> onNotification)
        {
            if (_isDisposed) return;

            _connection.On<int, string, string, string>("HaveANotification", (id, senderEmail, message, notificationType) =>
            {
                if (!_isDisposed)
                    onNotification?.Invoke(id, senderEmail, message, notificationType);
            });

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }

                // Gửi email lên server để đăng ký connectionId
                await _connection.InvokeAsync("RegisterConnection", myEmail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ConnectAsync failed: {ex.Message}");
            }
        }


        public async Task SendNotification(string senderEmail ,string[] receiverEmails, string message, string messageType)
        {
            if (_isDisposed) return;

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
                await _connection.InvokeAsync("SendNotification", senderEmail, receiverEmails, message, messageType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send notification failed: {ex.Message}");
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
