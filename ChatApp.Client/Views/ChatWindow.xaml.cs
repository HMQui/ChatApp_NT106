using ChatApp.Client.Services;
using ChatApp.Common.DTOs;
using System.Windows;
using ChatApp.Client.Hub;
using ChatApp.Common.DAO;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using HA = System.Windows.HorizontalAlignment;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Diagnostics;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private UserDTO _user;
        private StatusAccountHub _statusHub;
        private ChatOneOnOneHub _chatOneOnOneHub;
        private NotificationHub _notificationHub;
        private UserService _userService;
        private CircularPictureBoxService _circularPictureBoxService;
        private List<MessageDTO> _messages;
        bool isBlocked;
        public ChatWindow(string fromEmail, string toEmail, string blocked)
        {
            InitializeComponent();

            _fromEmail = fromEmail;
            _toEmail = toEmail;
            isBlocked = blocked == "blocked";
            _messages = MessageDAO.Instance.GetMessagesOneOnOne(_fromEmail, _toEmail);

            // Initialize services
            _userService = new UserService();
            _circularPictureBoxService = new CircularPictureBoxService();

            // Start the socket hub
            _statusHub = new StatusAccountHub();
            _chatOneOnOneHub = new ChatOneOnOneHub(_fromEmail);
            _notificationHub = new NotificationHub(_fromEmail);

            // Load the toUser data
            _user = AccountDAO.Instance.GetUserInfoByEmail(_toEmail);

            UserName.Text = _user.FullName;
            StatusOnOff.Text = _user.Status;
            StatusOnOff.Foreground = _user.Status == "online" ? SWM.Brushes.Green : SWM.Brushes.Red;
            StatusOnOff.Text = _user.Status == "online" ? "Online" : "Offline";

            RenderMessages(_messages, _fromEmail, _toEmail);
        }

        private async void form_load(object sender, EventArgs e)
        {
            await _statusHub.SetOnline(_fromEmail);
            _userService.SetOnlineStatusInDB(_fromEmail);

            // đợi phản hồi từ server nếu có bạn thay đổi trạng thái on off
            await _statusHub.ConnectAsync((friendEmail, status) =>
            {
                UpdateToUserStatus(friendEmail, status);
            });

            // đợi phản hồi từ server nếu có ai đó gửi tin nhắn
            await _chatOneOnOneHub.ConnectAsync((senderId, data, messageType, sendAt) =>
            {
                var newMsg = new MessageDTO
                {
                    SenderEmail = senderId,
                    ReceiverEmail = _toEmail,
                    SentAt = DateTime.Now,
                    MessageType = messageType,
                    Message = data,
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderNewMessage(newMsg, _fromEmail);
                });
                
            });

            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "block")
                {
                    isBlocked = true;
                }
                else if (messageType == "unblock")
                {
                    isBlocked = false;
                }
            });
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
            // tắt kết nối socket
            _userService.SetOfflineStatusInDB(_fromEmail);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(_fromEmail);
                await _statusHub.DisconnectAsync();
            }

            if (_notificationHub != null)
            {
                await _notificationHub.DisconnectAsync();
            }
        }

        private void UpdateToUserStatus(string email, string status)
        {
            if (email == _toEmail)
            {
                StatusOnOff.Text = status;
                StatusOnOff.Foreground = _user.Status == "online" ? SWM.Brushes.Green : SWM.Brushes.Red;
                StatusOnOff.Text = status == "online" ? "Online" : "Offline";
            }
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {

        }

        

        private async void SendTextMess_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }
            string message = TextMess.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await _chatOneOnOneHub.SendMessageAsync(_toEmail, data, "text", DateTime.Now);
                await _notificationHub.SendNotification(_fromEmail, [_toEmail], "Có tin nhắn mới", "message");

            }
        }

        public void RenderMessages(List<MessageDTO> _messages, string _fromEmail, string _toEmail)
        {
            ChatMessages.Children.Clear(); // Xóa tin nhắn cũ nếu có

            foreach (var msg in _messages)
            {
                var isFromMe = msg.SenderEmail == _fromEmail;
                var align = isFromMe ? HA.Right : HA.Left;
                var bgColor = isFromMe ? "#C8E6C9" : "#E3F2FD";
                var container = new StackPanel
                {
                    HorizontalAlignment = align,
                    MaxWidth = 300,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                Border border = new Border
                {
                    Background = (SWM.Brush)new BrushConverter().ConvertFromString(bgColor),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10)
                };

                if (msg.MessageType == "text")
                {
                    border.Child = new TextBlock { Text = msg.Message };
                }
                else if (msg.MessageType == "emoji")
                {
                    border.Child = new TextBlock { Text = msg.Message}; 
                }
                else if (msg.MessageType == "image")
                {
                    border.Padding = new Thickness(5); // nhỏ hơn để phù hợp hình
                    border.Child = new SWC.Image
                    {
                        Source = new BitmapImage(new Uri(msg.Message)),
                        Height = 150,
                        Stretch = Stretch.UniformToFill
                    };
                }
                else if (msg.MessageType == "file")
                {
                    var filePanel = new StackPanel();
                    filePanel.Children.Add(new TextBlock
                    {
                        Text = System.IO.Path.GetFileName(msg.Message),
                        FontWeight = FontWeights.Bold
                    });

                    var downloadBtn = new SWC.Button
                    {
                        Content = "Tải file",
                        Width = 100,
                        Margin = new Thickness(0, 5, 0, 0),
                        Tag = msg.Message
                    };
                    downloadBtn.Click += DownloadFile_Click;
                    filePanel.Children.Add(downloadBtn);

                    border.Child = filePanel;
                }

                container.Children.Add(border);

                // Thêm thời gian
                var timestamp = new TextBlock
                {
                    Text = msg.SentAt.ToString("dd/MM/yyyy HH:mm"),
                    FontSize = 10,
                    Foreground = SWM.Brushes.Gray,
                    Margin = new Thickness(5, 2, 0, 0),
                    HorizontalAlignment = align
                };
                container.Children.Add(timestamp);

                ChatMessages.Children.Add(container);
                ChatBox.ScrollToEnd();
            }
        }

        public void RenderNewMessage(MessageDTO msg, string fromEmail)
        {
            bool isFromMe = msg.SenderEmail == fromEmail;
            var align = isFromMe ? HA.Right : HA.Left;
            var bgColor = isFromMe ? "#C8E6C9" : "#E3F2FD";

            var container = new StackPanel
            {
                HorizontalAlignment = align,
                MaxWidth = 300,
                Margin = new Thickness(0, 5, 0, 5)
            };

            Border border = new Border
            {
                Background = (SWM.Brush)new BrushConverter().ConvertFromString(bgColor),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10)
            };

            if (msg.MessageType == "text")
            {
                border.Child = new TextBlock { Text = msg.Message };
            }
            else if (msg.MessageType == "image")
            {
                border.Padding = new Thickness(5);
                border.Child = new SWC.Image
                {
                    Source = new BitmapImage(new Uri(msg.Message)),
                    Height = 150,
                    Stretch = Stretch.UniformToFill
                };
            }
            else if (msg.MessageType == "file")
            {
                var filePanel = new StackPanel();
                filePanel.Children.Add(new TextBlock
                {
                    Text = System.IO.Path.GetFileName(msg.Message),
                    FontWeight = FontWeights.Bold
                });

                var downloadBtn = new SWC.Button
                {
                    Content = "Tải file",
                    Width = 100,
                    Margin = new Thickness(0, 5, 0, 0),
                    Tag = msg.Message
                };
                downloadBtn.Click += DownloadFile_Click;
                filePanel.Children.Add(downloadBtn);

                border.Child = filePanel;
            }

            container.Children.Add(border);

            var timestamp = new TextBlock
            {
                Text = msg.SentAt.ToString("dd/MM/yyyy HH:mm"),
                FontSize = 10,
                Foreground = SWM.Brushes.Gray,
                Margin = new Thickness(5, 2, 0, 0),
                HorizontalAlignment = align
            };

            container.Children.Add(timestamp);

            ChatMessages.Children.Add(container);
            ChatBox.ScrollToEnd();
        }

        private async void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn file để gửi",
                Filter = "Tất cả các file|*.*"
            };

            if (ofd.ShowDialog() != true) return;

            // Lấy đường dẫn, tên và chuyển thành mảng byte
            string filePath = ofd.FileName;
            string fileName = Path.GetFileName(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);

            await _chatOneOnOneHub.SendMessageAsync(_toEmail, fileBytes, "file", DateTime.Now, fileName);
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "Có tin nhắn mới", "message");
        }

        private async void SendImg_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh để gửi",
                Filter = "Ảnh |*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp"
            };

            if (ofd.ShowDialog() != true) return;

            // Lấy đường dẫn, tên và chuyển thành mảng byte
            string filePath = ofd.FileName;
            string fileName = Path.GetFileName(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);

            await _chatOneOnOneHub.SendMessageAsync(_toEmail, fileBytes, "image", DateTime.Now, fileName);
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "Có tin nhắn mới", "message");
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainForm mainForm = new MainForm(_fromEmail);

            mainForm.ShowDialog();
            this.Close();
        }

        private void TextMess_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = true;
        }

        private async void EmojiSelected_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Forms.Button;
            string emoji = btn.Text.ToString();
            if (isBlocked)
            {
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(emoji);
            await _chatOneOnOneHub.SendMessageAsync(_toEmail, data, "emoji", DateTime.Now);

            EmojiPopup.IsOpen = false;
        }

    }


}
