using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Server.Services;
using ChatApp.Common.DAO;

namespace ChatApp.Server.Hubs
{
    public class ProfileHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly S3Service _s3Service;

        public ProfileHub(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public Task Register(string email)
        {
            UserConnections[email] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public async Task UpdateAvatar(byte[] imageData, string email)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException("Image data cannot be empty.");
            }

            string fileName = $"avatar_{Guid.NewGuid()}.jpg";
            string avatarUrl = await _s3Service.UploadImageAsync(imageData, fileName);

            var fieldsToUpdate = new Dictionary<string, object> { { "avatar_url", avatarUrl } };
            var conditions = new Dictionary<string, object> { { "email", email } };
            int rowsAffected = AccountDAO.Instance.UpdateFields(fieldsToUpdate, conditions);

            if (rowsAffected > 0)
            {
                // Gửi sự kiện cập nhật avatar đến client
                if (UserConnections.TryGetValue(email, out var connectionId))
                {
                    await Clients.Client(connectionId)
                        .SendAsync("AvatarUpdated", avatarUrl);
                }
            }
            else
            {
                throw new Exception("Failed to update avatar URL in database.");
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