using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Server.Services;
using System.Text;
using ChatApp.Common.DAO;

namespace ChatApp.Server.Hubs
{
    public class ChatOneOnOneHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly S3Service _s3Service;

        public ChatOneOnOneHub(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public Task Register(string email)
        {
            UserConnections[email] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public async Task SendMessage(string receiverEmail, byte[] data, string senderEmail, string messageType, string originalFileName = "")
        {
            string payload = string.Empty;

            if (messageType == "text")
            {
                payload = Encoding.UTF8.GetString(data);
                MessageDAO.Instance.InsertMessage(senderEmail, receiverEmail, payload, "text", DateTime.Now);
            }
            else if (messageType == "image")
            {
                string fileName = $"img_{Guid.NewGuid()}.jpg";
                string imageUrl = await _s3Service.UploadImageAsync(data, fileName);
                MessageDAO.Instance.InsertMessage(senderEmail, receiverEmail, imageUrl, "image", DateTime.Now);
                payload = imageUrl;
            }
            else if (messageType == "file")
            {
                string fileName = originalFileName;
                string fileUrl = await _s3Service.UploadFileAsync(data, fileName);
                MessageDAO.Instance.InsertMessage(senderEmail, receiverEmail, fileUrl, "file", DateTime.Now);
                payload = fileUrl;

            }


            if (UserConnections.TryGetValue(receiverEmail, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId)
                    .SendAsync("ReceiveMessage", senderEmail, payload, messageType);
            }

            /*if (UserConnections.TryGetValue(senderEmail, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId)
                    .SendAsync("ReceiveMessage", senderEmail, payload, messageType);
            }*/
        }



        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (user.Key != null)
            {
                UserConnections.TryRemove(user.Key, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
