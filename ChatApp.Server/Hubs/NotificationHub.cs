using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;

namespace ChatApp.Server.Hubs
{
    public class NotificationHub : Hub
    {
        // Lưu trữ email <=> connectionId (một email có thể có nhiều connection)
        private static readonly ConcurrentDictionary<string, List<string>> _connections = new();

        // Đăng ký khi client kết nối
        public Task RegisterConnection(string email)
        {
            _connections.AddOrUpdate(email,
                // Nếu email chưa tồn tại, thêm mới
                key => new List<string> { Context.ConnectionId },
                // Nếu email đã tồn tại, cập nhật danh sách connection
                (key, existingConnections) =>
                {
                    // Nếu connection hiện tại chưa có trong danh sách thì thêm vào
                    if (!existingConnections.Contains(Context.ConnectionId))
                    {
                        existingConnections.Add(Context.ConnectionId);
                    }
                    return existingConnections;
                });

            Console.WriteLine($"Registered: {email} -> {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        // Khi client gửi thông báo
        public async Task SendNotification(string senderEmail, string[] receiverEmails, string message, string notificationType)
        {
            foreach (var receiverEmail in receiverEmails)
            {
                Console.WriteLine($"Sending to: {receiverEmail} from: {senderEmail}");

                // Lưu vào DB
                NotificationDTO newNotify = NotificationDAO.Instance.InsertNotification(
                    senderEmail,
                    receiverEmail,
                    message,
                    notificationType
                );

                // Gửi realtime nếu người nhận đang kết nối
                if (_connections.TryGetValue(receiverEmail, out var connectionIds))
                {
                    foreach (var connectionId in connectionIds)
                    {
                        try
                        {
                            await Clients.Client(connectionId).SendAsync(
                                "HaveANotification",
                                newNotify.Id,
                                newNotify.SenderEmail,
                                newNotify.Message,
                                newNotify.NotificationType
                            );
                            Console.WriteLine($"Notification sent to {receiverEmail} via {connectionId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending to {connectionId}: {ex.Message}");
                            // Có thể xử lý remove connection bị lỗi ở đây
                        }
                    }
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
            foreach (var pair in _connections)
            {
                if (pair.Value.Contains(Context.ConnectionId))
                {
                    pair.Value.Remove(Context.ConnectionId);
                    Console.WriteLine($"Disconnected: {pair.Key} -> {Context.ConnectionId}");

                    // Nếu không còn connection nào thì xóa luôn email khỏi dictionary
                    if (pair.Value.Count == 0)
                    {
                        _connections.TryRemove(pair.Key, out _);
                    }
                    break;
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}