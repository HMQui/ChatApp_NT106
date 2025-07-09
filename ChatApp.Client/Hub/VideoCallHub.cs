using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class VideoCallHub : IAsyncDisposable
    {
        private HubConnection _connection;
        private readonly string _myEmail;
        private bool _isDisposed = false;

        public VideoCallHub(string myEmail)
        {
            _myEmail = myEmail;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/video-call")
                .WithAutomaticReconnect()
                .Build();
        }

        /// <summary>
        /// Kết nối hub và lắng nghe sự kiện ReceiveVideo
        /// </summary>
        public async Task ConnectAsync(Action<string, byte[]> onFrameReceived)
        {
            if (_isDisposed) return;

            _connection.On<string, byte[]>("ReceiveVideo", (senderEmail, frameData) =>
            {
                onFrameReceived?.Invoke(senderEmail, frameData);
            });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }

        /// <summary>
        /// Gửi video frame tới người nhận
        /// </summary>
        public async Task SendFrameAsync(string toEmail, byte[] frameData)
        {
            if (_isDisposed || frameData == null || frameData.Length == 0) return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }

            await _connection.InvokeAsync("SendVideo", toEmail, _myEmail, frameData);
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
