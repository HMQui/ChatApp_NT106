using ChatApp.Client.Services;
using ChatApp.Common.DTOs;
using System.Windows;
using ChatApp.Client.Hub;
using ChatApp.Common.DAO;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls;
using HA = System.Windows.HorizontalAlignment;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Documents;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>

    public partial class ChatWindow : Window
    {
        public static class AppContext
        {
            public static string CurrentChatEmail { get; set; }
        }
        private UserDTO _userInfo;
        private UserDTO _friendInfo;
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
        private List<string> _emojiFiles;
        private int _currentEmojiPage = 1;
        private const int EmojisPerPage = 40;
        private bool _isNotificationHubConnected = false;
        private readonly string defaultAvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg";

        public class EmojiItem
        {
            public string Emoji { get; set; }
            public string Color { get; set; }

            public EmojiItem(string emoji, string color)
            {
                Emoji = emoji;
                Color = color;
            }
        }
        private readonly List<EmojiItem> _emojiList = new List<EmojiItem>
        {
            new EmojiItem("😀", "#FFEB3B"),     // vàng tươi
            new EmojiItem("😂", "#4CAF50"),     // xanh lá
            new EmojiItem("😍", "#E91E63"),     // hồng đậm
            new EmojiItem("🥰", "#FF4081"),     // hồng tươi
            new EmojiItem("😭", "#2196F3"),     // xanh dương
            new EmojiItem("🤔", "#9C27B0"),     // tím
            new EmojiItem("😎", "#FFC107"),     // vàng cam
            new EmojiItem("👍", "#009688"),     // teal
            new EmojiItem("🔥", "#FF5722"),     // cam lửa
            new EmojiItem("🎉", "#673AB7"),     // tím đậm
            new EmojiItem("💯", "#F44336"),     // đỏ
            new EmojiItem("❤️", "#F44336"),     // đỏ
            new EmojiItem("🙌", "#3F51B5"),     // xanh tím
            new EmojiItem("😡", "#D32F2F"),     // đỏ đậm
            new EmojiItem("🤯", "#FF9800"),     // cam
            new EmojiItem("🥳", "#FFEB3B"),     // vàng tươi
            new EmojiItem("👀", "#607D8B"),     // xám xanh
            new EmojiItem("😱", "#00BCD4"),     // xanh cyan
            new EmojiItem("🤷‍♂️", "#795548"),  // nâu
            new EmojiItem("👋", "#8BC34A"),     // xanh non
            new EmojiItem("🙏", "#9C27B0"),     // tím
        };

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

            // Load initial emoji page
            LoadEmojiPage(1);

            RenderMessages(_messages, _fromEmail, _toEmail);
        }

        private async void form_load(object sender, EventArgs e)
        {
            _userInfo = AccountDAO.Instance.GetUserInfoByEmail(_fromEmail);
            _friendInfo = AccountDAO.Instance.GetUserInfoByEmail(_toEmail);

            LoadAvatar(_friendInfo.AvatarUrl);

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
            await ConnectNotificationHub();

            AppContext.CurrentChatEmail = _toEmail;
        }

        private void LoadAvatar(string avatarUrl)
        {
            try
            {
                string finalUrl = string.IsNullOrEmpty(avatarUrl) ? defaultAvatarUrl : avatarUrl;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(finalUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                AvatarBrush.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Avatar load failed: " + ex.Message);
                AvatarBrush.ImageSource = null;
            }
        }

        private async Task ConnectNotificationHub()
        {
            if (_notificationHub == null)
            {
                _notificationHub = new NotificationHub(_fromEmail);
            }

            if (!_isNotificationHubConnected)
            {
                await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
                {
                    if (messageType == "voice_call")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            IncomingCallDialog incomingCallDialog = new IncomingCallDialog(senderEmail, _fromEmail);
                            incomingCallDialog.ShowDialog();
                        });
                    }
                    if (messageType == "video_call")
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            IncomingVideoCallDialog incomingVideoCallDialog = new IncomingVideoCallDialog(senderEmail, _fromEmail, senderEmail);
                            incomingVideoCallDialog.ShowDialog();
                        });
                    }
                    if (messageType == "block")
                    {
                        isBlocked = true;
                    }
                    else if (messageType == "unblock")
                    {
                        isBlocked = false;
                    }

                });
                _isNotificationHubConnected = true;
            }
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
                _isNotificationHubConnected = false;
            }

            if (_chatOneOnOneHub != null)
            {
                await _chatOneOnOneHub.DisposeAsync();
            }

            AppContext.CurrentChatEmail = null;
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
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }

            // Get the text content including emojis
            TextRange textRange = new TextRange(TextMess.Document.ContentStart, TextMess.Document.ContentEnd);
            string message = textRange.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                var notice = new NoticeDTO
                {
                    Email = _toEmail,
                    Title = "Tin nhắn mới",
                    Message = $"{_userInfo.FullName} đã gửi cho bạn một tin nhắn mới.",
                    IsSeen = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                };
                NoticeDAO.Instance.AddNotice(notice);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await _chatOneOnOneHub.SendMessageAsync(_toEmail, data, "text", DateTime.Now);
                await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_userInfo.FullName}||{message}||{_fromEmail}", "single_message");
                
                // Clear the RichTextBox after sending
                TextMess.Document.Blocks.Clear();
            }
        }

        public void RenderMessages(List<MessageDTO> _messages, string _fromEmail, string _toEmail)
        {
            ChatMessages.Children.Clear(); // Xóa tin nhắn cũ nếu có

            foreach (var msg in _messages)
            {
                var isFromMe = msg.SenderEmail == _fromEmail;
                var align = isFromMe ? HA.Right : HA.Left;
                var bgColor = isFromMe ? "#C8E6C9" : "#FFFFFF";

                var container = new StackPanel
                {
                    HorizontalAlignment = align,
                    MaxWidth = 300,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                UIElement messageElement;

                if (msg.MessageType == "emoji")
                {
                    string emojiUnicode = "";
                    string colorCode = "#000000"; // default black

                    if (!string.IsNullOrEmpty(msg.Message) && msg.Message.Contains("|"))
                    {
                        var parts = msg.Message.Split('|');
                        emojiUnicode = parts[0];
                        colorCode = parts.Length > 1 ? parts[1] : "#000000";
                    }
                    else
                    {
                        emojiUnicode = msg.Message;
                    }

                    messageElement = new TextBlock
                    {
                        Text = emojiUnicode,
                        FontFamily = new SWM.FontFamily("Segoe UI Emoji"),
                        FontSize = 72,
                        Background = SWM.Brushes.Transparent,
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = (SWM.Brush)new BrushConverter().ConvertFromString(colorCode)
                    };

                    container.Children.Add(messageElement);
                }
                else
                {
                    var border = new Border
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
                }

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
            var bgColor = isFromMe ? "#C8E6C9" : "#FFFFFF";

            var container = new StackPanel
            {
                HorizontalAlignment = align,
                MaxWidth = 300,
                Margin = new Thickness(0, 5, 0, 5)
            };

            UIElement messageElement;

            if (msg.MessageType == "emoji")
            {
                string emojiUnicode = "";
                string colorCode = "#000000"; // default black

                if (!string.IsNullOrEmpty(msg.Message) && msg.Message.Contains("|"))
                {
                    var parts = msg.Message.Split('|');
                    emojiUnicode = parts[0];
                    colorCode = parts.Length > 1 ? parts[1] : "#000000";
                }
                else
                {
                    emojiUnicode = msg.Message;
                }

                messageElement = new TextBlock
                {
                    Text = emojiUnicode,
                    FontFamily = new SWM.FontFamily("Segoe UI Emoji"),
                    FontSize = 72,
                    Background = SWM.Brushes.Transparent,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (SWM.Brush)new BrushConverter().ConvertFromString(colorCode)
                };

                container.Children.Add(messageElement);
            }
            else
            {
                // Những message khác vẫn giữ Border
                var border = new Border
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
            }

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

            var notice = new NoticeDTO
            {
                Email = _toEmail,
                Title = "Tin nhắn mới",
                Message = $"{_userInfo.FullName} đã gửi cho bạn một tin nhắn mới.",
                IsSeen = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            NoticeDAO.Instance.AddNotice(notice);

            await _chatOneOnOneHub.SendMessageAsync(_toEmail, fileBytes, "file", DateTime.Now, fileName);
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_userInfo.FullName}||Gửi bạn một file||{_fromEmail}", "single_message");
        }

        private async void SendImg_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh để gửi",
                Filter = "Ảnh |*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp"
            };

            if (ofd.ShowDialog() != true) return;

            var notice = new NoticeDTO
            {
                Email = _toEmail,
                Title = "Tin nhắn mới",
                Message = $"{_userInfo.FullName} đã gửi cho bạn một tin nhắn mới.",
                IsSeen = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            NoticeDAO.Instance.AddNotice(notice);

            // Lấy đường dẫn, tên và chuyển thành mảng byte
            string filePath = ofd.FileName;
            string fileName = Path.GetFileName(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);

            await _chatOneOnOneHub.SendMessageAsync(_toEmail, fileBytes, "image", DateTime.Now, fileName);
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_userInfo.FullName}||Gửi bạn một hình ảnh||{_fromEmail}", "single_message");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainForm_1 mainForm = new MainForm_1(_fromEmail);

            mainForm.ShowDialog();
            this.Close();
        }

        private void TextMess_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Handle text changes if needed
        }

        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmojiPopup.IsOpen)
            {
                EmojiPopup.IsOpen = false;
            }
            else
            {
                EmojiPopup.IsOpen = true;
                LoadEmojiPage(1);
            }
        }

        private void LoadEmojiPage(int page)
        {
            EmojiPanel.Children.Clear();
            _currentEmojiPage = page;

            int startIndex = (page - 1) * EmojisPerPage;
            int endIndex = Math.Min(startIndex + EmojisPerPage, _emojiList.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var emojiItem = _emojiList[i];

                var emojiText = new TextBlock
                {
                    Text = emojiItem.Emoji,
                    FontFamily = new SWM.FontFamily("Segoe UI Emoji"),
                    FontSize = 32,
                    Foreground = (SWM.Brush)new BrushConverter().ConvertFromString(emojiItem.Color),
                    Background = SWM.Brushes.Transparent,
                    HorizontalAlignment = HA.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var emojiButton = new SWC.Button
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(2),
                    Content = _emojiList[i].Emoji,
                    FontSize = 24,
                    Background = SWM.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Tag = _emojiList[i].Emoji,
                    Foreground = (SWM.Brush)new BrushConverter().ConvertFromString(_emojiList[i].Color)
                };

                emojiButton.Click += EmojiSelected_Click;
                EmojiPanel.Children.Add(emojiButton);
            }

            EmojiPageText.Text = $"Page {page} of {Math.Ceiling(_emojiList.Count / (double)EmojisPerPage)}";
        }

        private void PreviousEmojiPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEmojiPage > 1)
            {
                LoadEmojiPage(_currentEmojiPage - 1);
            }
        }

        private void NextEmojiPage_Click(object sender, RoutedEventArgs e)
        {
            int maxPage = (int)Math.Ceiling(_emojiFiles.Count / (double)EmojisPerPage);
            if (_currentEmojiPage < maxPage)
            {
                LoadEmojiPage(_currentEmojiPage + 1);
            }
        }

        private async void EmojiSelected_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }

            var button = sender as SWC.Button;
            if (button != null)
            {
                string emojiUnicode = button.Tag?.ToString();

                var emojiItem = _emojiList.FirstOrDefault(x => x.Emoji == emojiUnicode);
                string colorCode = (!string.IsNullOrEmpty(emojiItem?.Color))
                    ? emojiItem.Color
                    : "#000000";

                string payload = $"{emojiUnicode}|{colorCode}";
                byte[] emojiBytes = System.Text.Encoding.UTF8.GetBytes(payload);

                var notice = new NoticeDTO
                {
                    Email = _toEmail,
                    Title = "Tin nhắn mới",
                    Message = $"{_userInfo.FullName} đã gửi cho bạn một tin nhắn mới.",
                    IsSeen = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now
                };
                NoticeDAO.Instance.AddNotice(notice);

                await _chatOneOnOneHub.SendMessageAsync(
                    _toEmail,
                    emojiBytes,
                    "emoji",
                    DateTime.Now
                );

                await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_userInfo.FullName} || Bạn có một tin nhắn mới", "single_message");

                EmojiPopup.IsOpen = false;
            }
        }

        private void CloseEmojiPopup_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = false;
        }

        //nút voice call
        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }

            CallingDialog callingDialog = new CallingDialog(_fromEmail, _toEmail, _friendInfo.FullName);

            callingDialog.ShowDialog();

            await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_fromEmail}||{_toEmail}||{_friendInfo.FullName}", "single_voice_call");

        }

        //nút video call
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }
            VideoCallDialog videoCallDialog = new VideoCallDialog(_fromEmail, _toEmail, _friendInfo.FullName);

            videoCallDialog.ShowDialog();

            await _notificationHub.SendNotification(_fromEmail, [_toEmail], $"{_fromEmail}||{_toEmail}||{_friendInfo.FullName}", "single_video_call");
        }
    }
}
