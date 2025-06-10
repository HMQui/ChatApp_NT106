using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Server.Services;
using System.Text;
using ChatApp.Common.DAO;

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

        public async Task JoinGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task SendGroupMessage(int groupId, byte[] data, string senderEmail, string messageType, DateTime sendAt, string originalFileName = "")
        {
            string payload = string.Empty;
            var sender = AccountDAO.Instance.GetUserInfoByEmail(senderEmail);

            if (messageType == "text")
            {
                payload = Encoding.UTF8.GetString(data);
                MessageDAO.Instance.InsertMessage(sender.Id, null, payload, "text", sendAt, groupId);
            }
            else if (messageType == "image")
            {
                string fileName = $"img_{Guid.NewGuid()}.jpg";
                string imageUrl = await _s3Service.UploadImageAsync(data, fileName);
                MessageDAO.Instance.InsertMessage(sender.Id, null, imageUrl, "image", sendAt, groupId);
                payload = imageUrl;
            }
            else if (messageType == "file")
            {
                string fileName = originalFileName;
                string fileUrl = await _s3Service.UploadFileAsync(data, fileName);
                MessageDAO.Instance.InsertMessage(sender.Id, null, fileUrl, "file", sendAt, groupId);
                payload = fileUrl;
            }

            // Get all members of the group
            var members = GroupDAO.Instance.GetGroupMembers(groupId);
            var memberEmails = members.Select(m => m.User.Email).ToList();

            // Send message to all group members
            await Clients.Group(groupId.ToString()).SendAsync("ReceiveGroupMessage", groupId, senderEmail, payload, messageType, sendAt);

            // Send notifications to offline members
            foreach (var memberEmail in memberEmails)
            {
                if (memberEmail != senderEmail && !UserConnections.ContainsKey(memberEmail))
                {
                    await NotificationDAO.Instance.InsertNotification(
                        senderEmail,
                        memberEmail,
                        $"New message in group {groupId}",
                        "group_message"
                    );
                }
            }
        }
    }
} 