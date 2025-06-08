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
using System.Windows.Documents;

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
        private List<string> _emojiFiles;
        private int _currentEmojiPage = 1;
        private const int EmojisPerPage = 40;

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

            // Load emoji files
            /*string emojiDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "emoji");
            _emojiFiles = Directory.GetFiles(emojiDirectory, "*.png")
                                 .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)))
                                 .ToList();*/

            // Load initial emoji page
            //LoadEmojiPage(1);

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
                if (messageType == "voice_call")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IncomingCallDialog incomingCallDialog = new IncomingCallDialog(senderEmail, _fromEmail);
                        incomingCallDialog.ShowDialog();
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
                MessageBox.Show("Bạn đã bị người này block");
                return;
            }

            // Get the text content including emojis
            TextRange textRange = new TextRange(TextMess.Document.ContentStart, TextMess.Document.ContentEnd);
            string message = textRange.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await _chatOneOnOneHub.SendMessageAsync(_toEmail, data, "text", DateTime.Now);
                await _notificationHub.SendNotification(_fromEmail, [_toEmail], "Có tin nhắn mới", "message");
                
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

                Border border = new Border
                {
                    Background = (SWM.Brush)new BrushConverter().ConvertFromString(bgColor),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10),
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
                        Tag = msg.Message,
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
            var bgColor = isFromMe ? "#C8E6C9" : "#FFFFFF";

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
            // Handle text changes if needed
        }
        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = true;
            LoadEmojiPage(1); // Load the first page of emojis when the popup opens

        }

        private void LoadEmojiPage(int page)
        {
            EmojiPanel.Children.Clear();
            _currentEmojiPage = page;

            int startIndex = (page - 1) * EmojisPerPage;
            int endIndex = Math.Min(startIndex + EmojisPerPage, _emojiFiles.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var emojiButton = new SWC.Button
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(2),
                    Tag = _emojiFiles[i],
                    Padding = new Thickness(2),
                    Background = SWM.Brushes.Transparent,
                    BorderThickness = new Thickness(0)
                };

                try
                {
                    var image = new SWC.Image
                    {
                        Source = new BitmapImage(new Uri(_emojiFiles[i])),
                        Width = 24,
                        Height = 24,
                        Stretch = Stretch.Uniform
                    };
                    emojiButton.Content = image;
                }
                catch
                {
                    emojiButton.Content = "?";
                }

                emojiButton.Click += EmojiSelected_Click;
                EmojiPanel.Children.Add(emojiButton);
            }

            EmojiPageText.Text = $"Page {page} of {Math.Ceiling(_emojiFiles.Count / (double)EmojisPerPage)}";
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

        private void EmojiSelected_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as SWC.Button;
            if (button != null)
            {
                string emojiPath = button.Tag.ToString();

                // Tạo hình ảnh emoji
                var emojiImage = new SWC.Image
                {
                    Source = new BitmapImage(new Uri(emojiPath)),
                    Width = 20,
                    Height = 20,
                    Stretch = Stretch.Uniform
                };

                var inlineContainer = new System.Windows.Documents.InlineUIContainer(emojiImage);

                // Lấy vị trí con trỏ hiện tại
                var caret = TextMess.CaretPosition;

                // Đảm bảo caret nằm trong một Paragraph
                Paragraph para = caret.Paragraph;
                if (para == null)
                {
                    // Nếu chưa có Paragraph, thêm mới
                    para = new Paragraph();
                    TextMess.Document.Blocks.Add(para);
                    caret = para.ContentStart;
                }

                // Chèn emoji vào vị trí con trỏ
                Inline prevInline = caret.GetAdjacentElement(LogicalDirection.Backward) as Inline;
                if (prevInline != null)
                {
                    para.Inlines.InsertAfter(prevInline, inlineContainer);
                }
                else
                {
                    para.Inlines.Add(inlineContainer);
                }

                // Đưa con trỏ ra sau emoji vừa chèn
                TextMess.CaretPosition = inlineContainer.ElementEnd;
                // Đặt focus lại vào RichTextBox
                TextMess.Focus();
                EmojiPopup.IsOpen = false;
            }
        }
        private void CloseEmojiPopup_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = false;
        }


        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            CallingDialog callingDialog = new CallingDialog(_fromEmail, _toEmail, UserName.Text);

            callingDialog.ShowDialog();

        }
    }

   

}
