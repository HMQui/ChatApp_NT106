using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class VoiceCallHub : Hub
    {
        private static readonly Dictionary<string, string> ConnectedUsers = new();

        public Task Register(string email)
        {
            lock (ConnectedUsers)
            {
                ConnectedUsers[email] = Context.ConnectionId;
            }
            return Task.CompletedTask;
        }

        public async Task SendVoice(string toEmail, string fromEmail, byte[] audioData)
        {
            if (ConnectedUsers.TryGetValue(toEmail, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveVoice", fromEmail, audioData);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (ConnectedUsers)
            {
                var item = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
                if (!string.IsNullOrEmpty(item.Key))
                {
                    ConnectedUsers.Remove(item.Key);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }

}
