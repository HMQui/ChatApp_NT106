using ChatApp.Common.DTOs;
using ChatApp.Common.DAO;
using ChatApp.Client.Hub;
using System.Windows;
using System.ComponentModel;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls;
using HA = System.Windows.HorizontalAlignment;
using ChatApp.Client.Services;
using System.Windows.Documents;


namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for GroupChatWindow.xaml
    /// </summary>
    /// 

    public partial class GroupChatWindow : Window
    {
        private string _email = "";
        int _GroupId = 0;
        GroupMembersDTO _userInfo;
        List<GroupMembersDTO> _groupMembers;
        string[] _groupMemberEmails;
        GroupDTO _groupInfo;
        List<GroupMessagesDTO> _groupMessages;
        private StatusAccountHub _statusHub;
        private NotificationHub _notificationHub;
        private ChatGroupHub _chatGroupHub;
        private int _currentEmojiPage = 1;
        private const int EmojisPerPage = 40;
        private List<string> _emojiFiles;
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
        private readonly UserService _userService = new UserService();
        public GroupChatWindow(string email, int groupId)
        {
            InitializeComponent();

            _email = email;
            _GroupId = groupId;

            // Lấy thông tin nhóm
            var filters = new Dictionary<string, object>
            {
                { "id", _GroupId },
            };
            List<GroupDTO> groups = GroupDAO.Instance.GetGroupsWithFilters(filters);
            if (groups == null || groups.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thông tin nhóm!");
                this.Close();
                return;
            }
            _groupInfo = groups[0];

            // Lấy thông tin người dùng
            List<GroupMembersDTO> _groupMembers = GroupMembersDAO.Instance.GetMembersByGroupId(_GroupId);
            _groupMemberEmails = _groupMembers.Select(member => member.Email).ToArray();

            // Lấy tin nhắn nhóm
            _groupMessages = GroupMessagesDAO.Instance.GetMessagesByGroupId(_GroupId);

            var filter = new Dictionary<string, object>
            {
                { "email", _email }
            };
            List<GroupMembersDTO> groupMembersDTOs = GroupMembersDAO.Instance.GetFilteredGroupMembers(_groupInfo.Id, filter);
            if (groupMembersDTOs != null && groupMembersDTOs.Count > 0)
            {
                _userInfo = groupMembersDTOs[0];
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin người dùng trong nhóm!");
                this.Close();
                return;
            }
        }

        private async void form_loading(object sender, RoutedEventArgs e)
        {
            // Hiển thị thông tin nhóm
            GroupName.Text = _groupInfo.GroupName;
            if (!string.IsNullOrEmpty(_groupInfo.Avatar_URL))
            {
                try
                {
                    GroupAvatar.ImageSource = new BitmapImage(new Uri(_groupInfo.Avatar_URL, UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load avatar: " + ex.Message);
                }
            }

            _statusHub = new StatusAccountHub();
            await _statusHub.ConnectAsync((friendEmail, status) =>
            {
                // no need
            });
            await _statusHub.SetOnline(_email);

            // Hiển thị tin nhắn
            RenderGroupMessages(_groupMessages);

            // Xử lý tin nhắn mới
            _chatGroupHub = new ChatGroupHub(_email);
            await _chatGroupHub.ConnectAsync((groupId, senderEmail, senderNickname, message, messageType, sentAt) =>
            {
                if (messageType == "group_notification" && senderNickname == _email)
                {
                    MessageBox.Show("Bạn đã bị gỡ khỏi nhóm này.");

                    this.Hide();
                    MainForm_1 mainForm = new MainForm_1(_email);
                    mainForm.ShowDialog();
                    this.Close();
                }

                // Render từng message mới nhận
                var msg = new GroupMessagesDTO
                {
                    GroupId = groupId,
                    SenderEmail = senderEmail,
                    SenderNickname = senderNickname,
                    Message = message,
                    MessageType = messageType,
                    SentAt = sentAt,
                    IsDeleted = false
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderGroupMessage(msg, senderEmail == _email);
                });
            });

            // Kết nối notification hub
            _notificationHub = new NotificationHub(_email);
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
            _userService.SetOfflineStatusInDB(_email);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(_email);
                await _statusHub.DisconnectAsync();
            }

            if (_notificationHub != null)
            {
                await _notificationHub.DisconnectAsync();
            }

            if (_chatGroupHub != null)
            {
                await _chatGroupHub.DisposeAsync();
            }
        }

        private void RenderGroupMessages(List<GroupMessagesDTO> messages)
        {
            ChatMessages.Children.Clear();

            foreach (var msg in messages)
            {
                RenderGroupMessage(msg, msg.SenderEmail == _email);
            }
        }

        private void RenderGroupMessage(GroupMessagesDTO msg, bool isFromMe)
        {
            var align = isFromMe ? HA.Right : HA.Left;
            var bgColor = isFromMe ? "#daedfe" : "#FFFFFF";

            // Group Notification
            if (msg.MessageType == "group_notification")
            {
                var notificationText = new TextBlock
                {
                    Text = msg.Message,
                    FontStyle = FontStyles.Italic,
                    FontSize = 12,
                    Foreground = SWM.Brushes.Gray,
                    HorizontalAlignment = HA.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                ChatMessages.Children.Add(notificationText);
                ChatBox.ScrollToEnd();
                return;
            }

            var container = new StackPanel
            {
                HorizontalAlignment = align,
                MaxWidth = 400,
                Margin = new Thickness(0, 5, 0, 5)
            };

            UIElement messageElement;

            if (msg.IsDeleted)
            {
                var deletedBorder = new Border
                {
                    Background = SWM.Brushes.LightGray,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10),
                    Child = new TextBlock
                    {
                        Text = "Tin nhắn này đã được gỡ.",
                        FontStyle = FontStyles.Italic,
                        Foreground = SWM.Brushes.Gray
                    }
                };

                container.Children.Add(deletedBorder);
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
                else if (msg.MessageType == "emoji")
                {
                    string emojiUnicode = "";
                    string colorCode = "#000000";

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

                    border.Child = new TextBlock
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

            // Nickname + email
            var nicknameText = new TextBlock
            {
                Text = $"{(!string.IsNullOrEmpty(msg.SenderNickname) ? msg.SenderNickname : "Người này đã bị xóa")} ({msg.SenderEmail})",
                FontSize = 11,
                Foreground = SWM.Brushes.DarkSlateGray,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5, 0, 0, 2),
                HorizontalAlignment = align
            };
            container.Children.Insert(0, nicknameText);

            // Thời gian
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

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SendTextMess_Click(object sender, RoutedEventArgs e)
        {
            // Get the text content including emojis
            TextRange textRange = new TextRange(TextMess.Document.ContentStart, TextMess.Document.ContentEnd);
            string message = textRange.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, data, _userInfo.NickName, "text", DateTime.Now);
                await _notificationHub.SendNotification(_email, _groupMemberEmails, $"{_groupInfo.GroupName}||{_userInfo.NickName}||{message}", "group_message");

                // Clear the RichTextBox after sending
                TextMess.Document.Blocks.Clear();
            }
        }

        private void SendImg_Click(object sender, RoutedEventArgs e)
        {
            // Gửi ảnh
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            // Gửi file
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainForm_1 mainForm = new MainForm_1(_email);
            mainForm.ShowDialog();
            this.Close();
        }

        private void ManageGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GroupManagementWindow groupManagementWindow = new GroupManagementWindow(_groupInfo, _notificationHub, _chatGroupHub);
            groupManagementWindow.ShowDialog();
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

                EmojiPopup.IsOpen = false;
            }
        }

        private void CloseEmojiPopup_Click(object sender, RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = false;
        }

    }
}
