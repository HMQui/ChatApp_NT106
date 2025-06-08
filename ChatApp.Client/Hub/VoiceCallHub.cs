using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Hub
{
    public class VoiceCallHub : IAsyncDisposable
    {
        private HubConnection _connection;
        private readonly string _myEmail;
        private bool _isDisposed = false;

        public VoiceCallHub(string myEmail)
        {
            _myEmail = myEmail;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/socket/voice-call") // Server-side endpoint
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync(Action<string, byte[]> onAudioReceived)
        {
            if (_isDisposed) return;

            _connection.On<string, byte[]>("ReceiveVoice", (senderEmail, audioData) =>
            {
                onAudioReceived?.Invoke(senderEmail, audioData);
            });

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }
        }

        public async Task SendAudioAsync(string toEmail, byte[] audioBuffer)
        {
            if (_isDisposed || audioBuffer == null || audioBuffer.Length == 0) return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Register", _myEmail);
            }

            await _connection.InvokeAsync("SendVoice", toEmail, _myEmail, audioBuffer);
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
