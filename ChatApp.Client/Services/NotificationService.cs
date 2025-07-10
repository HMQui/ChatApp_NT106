using ChatApp.Client.Hub;
using ChatApp.Client.Views;

namespace ChatApp.Client.Services
{
    public class NotificationService : IDisposable
    {
        private NotificationHub _notificationHub;
        private bool _isNotificationHubConnected;
        private readonly string _fromEmail;

        public NotificationService(string fromEmail)
        {
            _fromEmail = fromEmail;
        }

        public async Task ConnectNotificationHub()
        {
            if (_notificationHub == null)
            {
                _notificationHub = new NotificationHub(_fromEmail);
            }

            if (!_isNotificationHubConnected)
            {
                await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (messageType == "single_message")
                        {
                            string title = "";
                            string mess = "";

                            var parts = message.Split(new[] { "||" }, StringSplitOptions.None);
                            if (parts.Length >= 3)
                            {
                                title = parts[0].Trim();
                                mess = parts[1].Trim();
                            }
                            else
                            {
                                title = "Thông báo";
                                mess = message;
                            }

                            var toast = new ToastMess(title, mess);
                            toast.Show();
                        }
                        else if (messageType == "single_voice_call")
                        {
                            string fromEmail = "";
                            string toEmail = "";
                            string name = "";

                            var parts = message.Split(new[] { "||" }, StringSplitOptions.None);
                            if (parts.Length >= 3)
                            {
                                fromEmail = parts[0].Trim();
                                toEmail = parts[1].Trim();
                                name = parts[2].Trim();
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IncomingCallDialog incomingCallDialog = new IncomingCallDialog(toEmail, fromEmail);
                                incomingCallDialog.ShowDialog();
                            });
                        }
                        else if (messageType == "single_viceo_call")
                        {
                            string fromEmail = "";
                            string toEmail = "";
                            string name = "";

                            var parts = message.Split(new[] { "||" }, StringSplitOptions.None);
                            if (parts.Length >= 3)
                            {
                                fromEmail = parts[0].Trim();
                                toEmail = parts[1].Trim();
                                name = parts[2].Trim();
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IncomingVideoCallDialog incomingVideoCallDialog = new IncomingVideoCallDialog(toEmail, fromEmail, name);
                                incomingVideoCallDialog.ShowDialog();
                            });
                        }
                        else if (messageType == "unblock")
                        {
                            // Chỉ hiển thị toast
                        }
                        else
                        {
                            // Tin nhắn thông thường
                        }
                    });
                });

                _isNotificationHubConnected = true;
            }
        }

        public void Dispose()
        {
            if (_notificationHub != null)
            {
                _notificationHub.DisconnectAsync();
                _notificationHub = null;
            }
            _isNotificationHubConnected = false;
        }
    }
}