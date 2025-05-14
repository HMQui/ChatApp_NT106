using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;

namespace ChatApp.Server.Hubs
{
    public class NotificationHub : Hub
    {
        // Lưu trữ email <=> connectionId
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        // Đăng ký khi client kết nối
        public Task RegisterConnection(string email)
        {
            _connections[email] = Context.ConnectionId;
            Console.WriteLine($"Registered: {email} -> {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        // Khi client gửi thông báo
        public async Task SendNotification(string senderEmail, string[] receiverEmails, string message, string notificationType)
        {
            foreach (var receiverEmail in receiverEmails)
            {
                Console.WriteLine(receiverEmail, senderEmail);
                // Lưu vào DB
                NotificationDTO newNotify = NotificationDAO.Instance.InsertNotification(
                    senderEmail,
                    receiverEmail,
                    message,
                    notificationType
                );

                // Gửi realtime nếu người nhận đang kết nối
                if (_connections.TryGetValue(receiverEmail, out string connectionId))
                {
                    Console.WriteLine(receiverEmail, connectionId);
                    await Clients.Client(connectionId).SendAsync(
                        "HaveANotification",
                        newNotify.Id,
                        newNotify.SenderEmail,
                        newNotify.Message,
                        newNotify.NotificationType
                    );
                }
                else
                {
                    Console.WriteLine($"User {receiverEmail} is not connected.");
                }
            }
        }



        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Tìm và xoá connection khi disconnect
            var disconnected = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(disconnected.Key))
            {
                _connections.TryRemove(disconnected.Key, out _);
                Console.WriteLine($"Disconnected: {disconnected.Key}");
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
