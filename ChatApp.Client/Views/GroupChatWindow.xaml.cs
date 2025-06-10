using ChatApp.Client.Services;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;
using ChatApp.Client.Hub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Documents;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;
using WPF = System.Windows;
using WPFControls = System.Windows.Controls;
using WPFMedia = System.Windows.Media;
using WPFImaging = System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ChatApp.Client.Views
{
    public partial class GroupChatWindow : WPF.Window
    {
        private readonly int _groupId;
        private readonly string _userEmail;
        private readonly int _userId;
        private readonly GroupService _groupService;
        private readonly ChatGroupHub _chatGroupHub;
        private List<MessageDTO> _messages;
        private List<string> _emojiFiles;
        private int _currentEmojiPage = 1;
        private const int EmojisPerPage = 40;
        private bool _isAdmin;
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

        public GroupChatWindow(int groupId, string userEmail)
        {
            InitializeComponent();

            _groupId = groupId;
            _userEmail = userEmail;
            _userId = AccountDAO.Instance.GetUserInfoByEmail(userEmail).Id;
            _groupService = new GroupService();
            _chatGroupHub = new ChatGroupHub();
            _messages = new List<MessageDTO>();

            LoadGroupInfo();
            LoadMembers();
            LoadMessages();
            LoadEmojis();
            InitializeHub();
        }

        private async void InitializeHub()
        {
            try
            {
                await _chatGroupHub.ConnectAsync();
                await _chatGroupHub.Register(_userEmail);
                await _chatGroupHub.JoinGroup(_groupId);

                _chatGroupHub.MessageReceived += (groupId, senderEmail, message, messageType, sendAt) =>
                {
                    if (groupId == _groupId)
                    {
                        var newMsg = new MessageDTO
                        {
                            SenderEmail = senderEmail,
                            Message = message,
                            MessageType = messageType,
                            SentAt = sendAt
                        };

                        Dispatcher.Invoke(() =>
                        {
                            RenderNewMessage(newMsg);
                        });
                    }
                };

                _chatGroupHub.ConnectionClosed += async () =>
                {
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        MessageBox.Show("Connection lost. Attempting to reconnect...");
                        await ReconnectHub();
                    });
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing hub: {ex.Message}");
            }
        }

        private async Task ReconnectHub()
        {
            try
            {
                await _chatGroupHub.ConnectAsync();
                await _chatGroupHub.Register(_userEmail);
                await _chatGroupHub.JoinGroup(_groupId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reconnecting: {ex.Message}");
            }
        }

        private void LoadGroupInfo()
        {
            try
            {
                var group = _groupService.GetUserGroups(_userId)
                    .FirstOrDefault(g => g.Id == _groupId);

                if (group != null)
                {
                    GroupName.Text = group.GroupName;
                    _isAdmin = group.CreatedBy == _userId;
                    GroupInfoButton.IsEnabled = _isAdmin;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group info: {ex.Message}");
            }
        }

        private void LoadMembers()
        {
            try
            {
                var members = _groupService.GetGroupMembers(_groupId);
                MembersList.Children.Clear();

                foreach (var member in members)
                {
                    var memberPanel = new WPFControls.StackPanel
                    {
                        Orientation = WPFControls.Orientation.Horizontal,
                        Margin = new WPF.Thickness(5)
                    };

                    var avatar = new WPFControls.Image
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new WPF.Thickness(0, 0, 10, 0)
                    };

                    if (!string.IsNullOrEmpty(member.User.AvatarUrl))
                    {
                        try
                        {
                            avatar.Source = new WPFImaging.BitmapImage(new Uri(member.User.AvatarUrl));
                        }
                        catch
                        {
                            // Use default avatar if loading fails
                            avatar.Source = new WPFImaging.BitmapImage(new Uri("pack://application:,,,/Resources/default_avatar.png"));
                        }
                    }

                    var nameBlock = new WPFControls.TextBlock
                    {
                        Text = member.User.FullName,
                        VerticalAlignment = WPF.VerticalAlignment.Center
                    };

                    if (member.UserId == _userId)
                    {
                        nameBlock.Text += " (You)";
                    }

                    memberPanel.Children.Add(avatar);
                    memberPanel.Children.Add(nameBlock);

                    if (_isAdmin && member.UserId != _userId)
                    {
                        var removeButton = new WPFControls.Button
                        {
                            Content = "×",
                            Width = 20,
                            Height = 20,
                            Margin = new WPF.Thickness(5, 0, 0, 0),
                            Background = WPFMedia.Brushes.Red,
                            Foreground = WPFMedia.Brushes.White,
                            BorderThickness = new WPF.Thickness(0)
                        };
                        removeButton.Click += (s, e) => RemoveMember(member.UserId);
                        memberPanel.Children.Add(removeButton);
                    }

                    MembersList.Children.Add(memberPanel);
                }

                MemberCount.Text = $"{members.Count} members";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}");
            }
        }

        private void LoadMessages()
        {
            try
            {
                var messages = MessageDAO.Instance.GetMessages(new Dictionary<string, object>
                {
                    { "group_id", _groupId }
                });

                foreach (var message in messages)
                {
                    RenderMessage(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading messages: {ex.Message}");
            }
        }

        private void LoadEmojis()
        {
            try
            {
                string emojiDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emojis");
                if (Directory.Exists(emojiDirectory))
                {
                    _emojiFiles = Directory.GetFiles(emojiDirectory, "*.png").ToList();
                    UpdateEmojiDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading emojis: {ex.Message}");
            }
        }

        private void UpdateEmojiDisplay()
        {
            try
            {
                EmojiContainer.Children.Clear();
                int startIndex = (_currentEmojiPage - 1) * EmojisPerPage;
                int endIndex = Math.Min(startIndex + EmojisPerPage, _emojiFiles.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    if (File.Exists(_emojiFiles[i]))
                    {
                        var emojiButton = new WPFControls.Button
                        {
                            Width = 40,
                            Height = 40,
                            Margin = new WPF.Thickness(2),
                            Background = WPFMedia.Brushes.Transparent,
                            BorderThickness = new WPF.Thickness(0)
                        };

                        var emojiImage = new WPFControls.Image
                        {
                            Source = new WPFImaging.BitmapImage(new Uri(_emojiFiles[i])),
                            Width = 30,
                            Height = 30
                        };

                        emojiButton.Content = emojiImage;
                        emojiButton.Click += (s, e) => InsertEmoji(_emojiFiles[i]);
                        EmojiContainer.Children.Add(emojiButton);
                    }
                }

                EmojiPageInfo.Text = $"Page {_currentEmojiPage} of {Math.Ceiling(_emojiFiles.Count / (double)EmojisPerPage)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating emoji display: {ex.Message}");
            }
        }

        private void InsertEmoji(string emojiPath)
        {
            try
            {
                if (File.Exists(emojiPath))
                {
                    var emojiImage = new WPFControls.Image
                    {
                        Source = new WPFImaging.BitmapImage(new Uri(emojiPath)),
                        Width = 20,
                        Height = 20
                    };

                    var block = new BlockUIContainer(emojiImage);
                    TextMess.Document.Blocks.Add(block);
                    EmojiPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting emoji: {ex.Message}");
            }
        }

        private void RenderMessage(MessageDTO message)
        {
            try
            {
                if (message == null) return;

                var messagePanel = new WPFControls.StackPanel
                {
                    Margin = new WPF.Thickness(10, 5, 10, 5),
                    HorizontalAlignment = message.SenderEmail == _userEmail ? WPF.HorizontalAlignment.Right : WPF.HorizontalAlignment.Left
                };

                var senderBlock = new WPFControls.TextBlock
                {
                    Text = message.SenderEmail,
                    FontSize = 12,
                    Foreground = WPFMedia.Brushes.Gray,
                    Margin = new WPF.Thickness(0, 0, 0, 2)
                };

                var contentPanel = new WPFControls.Border
                {
                    Background = message.SenderEmail == _userEmail ? new WPFMedia.SolidColorBrush(WPFMedia.Color.FromRgb(220, 248, 198)) : WPFMedia.Brushes.White,
                    CornerRadius = new WPF.CornerRadius(10),
                    Padding = new WPF.Thickness(10),
                    BorderBrush = WPFMedia.Brushes.LightGray,
                    BorderThickness = new WPF.Thickness(1)
                };

                if (message.MessageType == "text")
                {
                    contentPanel.Child = new WPFControls.TextBlock
                    {
                        Text = message.Message,
                        TextWrapping = WPF.TextWrapping.Wrap
                    };
                }
                else if (message.MessageType == "image")
                {
                    try
                    {
                        var image = new WPFControls.Image
                        {
                            Source = new WPFImaging.BitmapImage(new Uri(message.Message)),
                            MaxWidth = 300,
                            MaxHeight = 300,
                            Stretch = WPFMedia.Stretch.Uniform
                        };
                        contentPanel.Child = image;
                    }
                    catch
                    {
                        contentPanel.Child = new WPFControls.TextBlock
                        {
                            Text = "Error loading image",
                            Foreground = WPFMedia.Brushes.Red
                        };
                    }
                }
                else if (message.MessageType == "file")
                {
                    var filePanel = new WPFControls.StackPanel();
                    var fileName = Path.GetFileName(message.Message);
                    filePanel.Children.Add(new WPFControls.TextBlock { Text = fileName });
                    
                    var downloadButton = new WPFControls.Button
                    {
                        Content = "Download",
                        Margin = new WPF.Thickness(0, 5, 0, 0)
                    };
                    downloadButton.Click += (s, e) => DownloadFile(message.Message);
                    filePanel.Children.Add(downloadButton);
                    
                    contentPanel.Child = filePanel;
                }

                messagePanel.Children.Add(senderBlock);
                messagePanel.Children.Add(contentPanel);
                MessageContainer.Children.Add(messagePanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rendering message: {ex.Message}");
            }
        }

        private void RenderNewMessage(MessageDTO message)
        {
            try
            {
                RenderMessage(message);
                ChatMessages.ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rendering new message: {ex.Message}");
            }
        }

        private async void SendTextMess_Click(object sender, WPF.RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(TextMess.Document.ContentStart, TextMess.Document.ContentEnd);
                string message = textRange.Text.Trim();

                if (!string.IsNullOrEmpty(message))
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    await _chatGroupHub.SendGroupMessage(_groupId, data, _userEmail, "text", DateTime.Now);
                    TextMess.Document.Blocks.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void AttachmentButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png | All files (*.*) | *.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(filePath);

                    if (fileInfo.Length > MaxFileSize)
                    {
                        MessageBox.Show($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
                        return;
                    }

                    string extension = Path.GetExtension(filePath).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        SendImage(filePath);
                    }
                    else
                    {
                        SendFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling attachment: {ex.Message}");
            }
        }

        private async void SendImage(string imagePath)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                await _chatGroupHub.SendGroupMessage(_groupId, imageData, _userEmail, "image", DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending image: {ex.Message}");
            }
        }

        private async void SendFile(string filePath)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                await _chatGroupHub.SendGroupMessage(_groupId, fileData, _userEmail, "file", DateTime.Now, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}");
            }
        }

        private void DownloadFile(string fileUrl)
        {
            try
            {
                if (!File.Exists(fileUrl))
                {
                    MessageBox.Show("File not found");
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = Path.GetFileName(fileUrl)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.Copy(fileUrl, saveFileDialog.FileName, true);
                    MessageBox.Show("File downloaded successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}");
            }
        }

        private void EmojiButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            EmojiPopup.IsOpen = true;
        }

        private void PreviousEmojiPage_Click(object sender, WPF.RoutedEventArgs e)
        {
            if (_currentEmojiPage > 1)
            {
                _currentEmojiPage--;
                UpdateEmojiDisplay();
            }
        }

        private void NextEmojiPage_Click(object sender, WPF.RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling(_emojiFiles.Count / (double)EmojisPerPage);
            if (_currentEmojiPage < totalPages)
            {
                _currentEmojiPage++;
                UpdateEmojiDisplay();
            }
        }

        private void GroupInfo_Click(object sender, WPF.RoutedEventArgs e)
        {
            try
            {
                if (_isAdmin)
                {
                    var groupInfoWindow = new GroupInfoWindow(_groupId);
                    if (groupInfoWindow.ShowDialog() == true)
                    {
                        LoadGroupInfo();
                        LoadMembers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling group info: {ex.Message}");
            }
        }

        private void RemoveMember(int memberId)
        {
            try
            {
                if (_isAdmin)
                {
                    var result = MessageBox.Show("Are you sure you want to remove this member?", "Confirm", 
                        WinForms.MessageBoxButtons.YesNo, WinForms.MessageBoxIcon.Question);

                    if (result == WinForms.DialogResult.Yes)
                    {
                        if (_groupService.RemoveMember(_groupId, memberId))
                        {
                            LoadMembers();
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove member.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing member: {ex.Message}");
            }
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
            try
            {
                await _chatGroupHub.LeaveGroup(_groupId);
                await _chatGroupHub.DisconnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing window: {ex.Message}");
            }
        }
    }
} 