using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;
using ChatApp.Server.Services;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;

namespace ChatApp.Server.Hubs
{
    public class ChatGroupHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly S3Service _s3Service;

        public ChatGroupHub(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public Task Register(string email)
        {
            UserConnections[email] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public async Task SendGroupMessage(
            int groupId,
            byte[] data,
            string senderEmail,
            string senderNickname,
            string messageType,
            DateTime sendAt,
            string originalFileName = "")
        {
            Console.WriteLine($"Received message from {senderEmail} in group {groupId} at {sendAt}: {messageType}");
            string payload = string.Empty;
            if (messageType == "text" || messageType == "emoji" || messageType == "group_notification")
            {
                payload = Encoding.UTF8.GetString(data);
            }
            else if (messageType == "image")
            {
                string fileName = $"img_{Guid.NewGuid()}.jpg";
                payload = await _s3Service.UploadImageAsync(data, fileName);
            }
            else if (messageType == "file")
            {
                string fileName = originalFileName;
                payload = await _s3Service.UploadFileAsync(data, fileName);
            }

            // Lưu vào DB
            GroupMessagesDAO.Instance.AddMessage(new GroupMessagesDTO
            {
                GroupId = groupId,
                SenderEmail = senderEmail,
                Message = payload,
                MessageType = messageType,
                SentAt = sendAt,
                IsDeleted = false
            });

            var members = GroupMembersDAO.Instance.GetMembersByGroupId(groupId);

            var connectionIdsSent = new HashSet<string>();

            foreach (var member in members)
            {
                if (UserConnections.TryGetValue(member.Email, out var connectionId) && !connectionIdsSent.Contains(connectionId))
                {
                    connectionIdsSent.Add(connectionId);

                    await Clients.Client(connectionId).SendAsync(
                        "ReceiveGroupMessage",
                        groupId,
                        senderEmail,
                        senderNickname,
                        payload,
                        messageType,
                        sendAt);
                }
            }
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
