using ChatApp.Client.Hub;
using ChatApp.Client.Services;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MediaBrushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.IO.Path;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls;
using HA = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.Forms.MessageBox;
using Microsoft.VisualBasic.ApplicationServices;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace ChatApp.Client.Views
{
    public partial class MainForm_1 : Window
    {
        private readonly string email;
        private List<UserFriendDTO> friends;
        private StatusAccountHub _statusHub;
        private UserService _userService;
        private CircularPictureBoxService _circularPictureBoxService;
        private readonly string defaultAvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg";
        private NotificationService _notificationService;
        private NotificationHub _notificationHub;
        private bool _isNotificationHubConnected = false;
        private List<NoticeDTO> _notifications;
        private bool _hasUnreadNotifications = false;

        public MainForm_1(string email)
        {
            InitializeComponent();
            this.email = email;

            _statusHub = new StatusAccountHub();
            _userService = new UserService();
            _circularPictureBoxService = new CircularPictureBoxService();

            ListFriend();
            ListGroups();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _notificationService = new NotificationService(email);
            await _notificationService.ConnectNotificationHub();

            await _statusHub.SetOnline(email);
            _userService.SetOnlineStatusInDB(email);

            await _statusHub.ConnectAsync((friendEmail, status) =>
            {
                Dispatcher.Invoke(() => UpdateFriendStatus(friendEmail, status));
            });

            ConnectNotificationHub();

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(defaultAvatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                ThumbImageBrush.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default avatar for {email}: {ex.Message}");
                ThumbImageBrush.ImageSource = null;
            }
            LoadNotifications();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _userService.SetOfflineStatusInDB(email);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(email);
                await _statusHub.DisconnectAsync();
            }
            _notificationService.Dispose();

            if (_notificationHub != null)
            {
                await _notificationHub.DisconnectAsync();
                _isNotificationHubConnected = false;
            }
        }

        private void UpdateNotificationBadge()
        {
            bool hasUnread = _notifications.Any(n => !n.IsSeen && !n.IsDeleted);
            NotificationBadge.Visibility = hasUnread ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadNotifications()
        {
            _notifications = NoticeDAO.Instance.GetNoticesByEmail(email);
            Dispatcher.Invoke(() =>
            {
                UpdateNotificationBadge();
            });
        }

        private void LoadNotificationList()
        {
            NotificationListPanel.Children.Clear();

            var visibleNotifications = _notifications
                .Where(n => !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt);
            if (_notifications == null || _notifications.Count == 0 || !_notifications.Any(n => !n.IsDeleted))
            {
                var textBlock = new TextBlock
                {
                    Text = "Không có thông báo mới.",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = SWM.Brushes.Gray,
                    HorizontalAlignment = HA.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10),
                };

                NotificationListPanel.Children.Clear();
                NotificationListPanel.Children.Add(textBlock);
                return;
            }

            foreach (var notice in visibleNotifications)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var titleStack = new StackPanel
                {
                    Orientation = WpfOrientation.Horizontal
                };

                var titleBlock = new TextBlock
                {
                    Text = notice.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(37, 37, 37))
                };

                var deleteButton = new SWC.Button
                {
                    Content = "X",
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(5, 0, 0, 0),
                    Background = SWM.Brushes.Transparent,
                    BorderBrush = SWM.Brushes.Transparent,
                    Foreground = SWM.Brushes.Gray,
                    Cursor = Cursors.Hand,
                    HorizontalAlignment = HA.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(0),
                    Tag = notice
                };
                deleteButton.Click += DeleteNotificationButton_Click;

                titleStack.Children.Add(titleBlock);
                titleStack.Children.Add(deleteButton);

                Grid.SetRow(titleStack, 0);
                grid.Children.Add(titleStack);

                var messageBlock = new TextBlock
                {
                    Text = notice.Message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                    FontSize = 12
                };

                var timeBlock = new TextBlock
                {
                    Text = notice.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    FontSize = 10,
                    Foreground = SWM.Brushes.Gray,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                var messageStack = new StackPanel();
                messageStack.Children.Add(messageBlock);
                messageStack.Children.Add(timeBlock);

                Grid.SetRow(messageStack, 1);
                grid.Children.Add(messageStack);

                border.Child = grid;

                NotificationListPanel.Children.Add(border);
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationPopup.IsOpen)
            {
                NotificationPopup.IsOpen = false;
            }
            else
            {
                LoadNotificationList();
                NotificationPopup.IsOpen = true;

                foreach (var noti in _notifications)
                {
                    if (!noti.IsSeen)
                    {
                        noti.IsSeen = true;
                        noti.SeenAt = DateTime.Now;

                        NoticeDAO.Instance.MarkAsSeen(noti.Id, email);
                    }
                }

                UpdateNotificationBadge();
            }
        }

        private void DeleteNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as SWC.Button;
            var notice = btn?.Tag as NoticeDTO;

            if (notice != null)
            {
                notice.IsDeleted = true;
                notice.DeletedAt = DateTime.Now;

                NoticeDAO.Instance.SoftDeleteNotice(notice.Id, email);

                LoadNotificationList();
                UpdateNotificationBadge();
            }
        }

        private async Task ConnectNotificationHub()
        {
            if (_notificationHub == null)
            {
                _notificationHub = new NotificationHub(email);
            }

            if (!_isNotificationHubConnected)
            {
                await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
                {
                    LoadNotifications();
                });
                _isNotificationHubConnected = true;
            }
        }

        private void ListFriend()
        {
            FriendListPanel.Children.Clear();
            friends = FriendDAO.Instance.GetFriendsWithStatus(email);
            if (friends.Count == 0)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "Không có bạn bè nào",
                    FontFamily = new FontFamily("Segoe UI"),
                    FontStyle = FontStyles.Italic,
                    FontSize = 14,
                    Margin = new Thickness(10)
                };
                FriendListPanel.Children.Add(textBlock);
            }
            else
            {
                foreach (var friend in friends)
                {
                    FriendListPanel.Children.Add(CreateFriendPanel(friend));
                }
            }
        }

        private void ListGroups()
        {
            GroupsContent.Children.Clear();

            var groups = GroupDAO.Instance.GetGroupsByUserEmail(email);
            if (groups == null || groups.Count == 0)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "Không có nhóm nào",
                    FontFamily = new FontFamily("Segoe UI"),
                    FontStyle = FontStyles.Italic,
                    FontSize = 14,
                    Margin = new Thickness(10)
                };
                GroupsContent.Children.Add(textBlock);
            }
            else
            {
                foreach (var group in groups)
                {
                    GroupsContent.Children.Add(CreateGroupPanel(group));
                }
            }
        }

        private Border CreateGroupPanel(GroupDTO group)
        {
            string avatarUrl = string.IsNullOrEmpty(group.Avatar_URL) ? defaultAvatarUrl : group.Avatar_URL;

            var border = new Border
            {
                Style = (Style)FindResource("FriendPanel"),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Avatar ellipse
            var ellipse = new Ellipse { Width = 40, Height = 40, Margin = new Thickness(5) };
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(avatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ellipse.Fill = new ImageBrush { ImageSource = bitmap };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading avatar for {group.GroupName}: {ex.Message}");
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(defaultAvatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ellipse.Fill = new ImageBrush { ImageSource = bitmap };
            }
            Grid.SetColumn(ellipse, 0);

            // StackPanel với thông tin group
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };

            var nameTextBlock = new TextBlock
            {
                Text = group.GroupName,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            stackPanel.Children.Add(nameTextBlock);

            var createdByTextBlock = new TextBlock
            {
                Text = $"Created by: {group.CreatedBy}",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = MediaBrushes.Gray
            };
            stackPanel.Children.Add(createdByTextBlock);

            var createdAtTextBlock = new TextBlock
            {
                Text = $"Created at: {group.CreatedAt:dd/MM/yyyy}",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = MediaBrushes.Gray
            };
            stackPanel.Children.Add(createdAtTextBlock);

            Grid.SetColumn(stackPanel, 1);

            // Status - giả sử nhóm nào cũng "Active"
            var statusTextBlock = new TextBlock
            {
                Text = "🟢 Active",
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = FontStyles.Italic,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Green),
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusTextBlock, 2);

            grid.Children.Add(ellipse);
            grid.Children.Add(stackPanel);
            grid.Children.Add(statusTextBlock);

            border.MouseEnter += (s, e) =>
            {
                border.Background = new SolidColorBrush(Color.FromRgb(229, 229, 229));
            };
            border.MouseLeave += (s, e) =>
            {
                border.Background = null;
            };

            border.Child = grid;

            border.MouseDown += async (s, e) =>
            {
                _notificationService.Dispose();
                await _notificationHub.DisconnectAsync();
                GroupChatWindow chatRoom = new GroupChatWindow(email, group.Id);
                this.Hide();
                chatRoom.ShowDialog();
                this.Close();
            };

            return border;
        }

        private Border CreateFriendPanel(UserFriendDTO user)
        {
            string avatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? defaultAvatarUrl : user.AvatarUrl;

            var border = new Border
            {
                Style = (Style)FindResource("FriendPanel"),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var ellipse = new Ellipse { Width = 40, Height = 40, Margin = new Thickness(5) };
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(avatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                ellipse.Fill = new ImageBrush { ImageSource = bitmap };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading avatar for {user.Email}: {ex.Message}");
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(defaultAvatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                ellipse.Fill = new ImageBrush { ImageSource = bitmap };
            }
            Grid.SetColumn(ellipse, 0);

            var stackPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            var nameTextBlock = new TextBlock
            {
                Text = user.FullName,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            var emailTextBlock = new TextBlock
            {
                Text = user.Email,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Blue),
                Cursor = Cursors.Hand
            };
            stackPanel.Children.Add(nameTextBlock);
            stackPanel.Children.Add(emailTextBlock);
            Grid.SetColumn(stackPanel, 1);

            var statusTextBlock = new TextBlock
            {
                Name = "StatusTextBlock",
                Text = user.Status == "online" ? "🟢 Online" : "⚫ Offline",
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = FontStyles.Italic,
                FontSize = 12,
                Foreground = user.Status == "online" ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Gray),
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusTextBlock, 2);

            grid.Children.Add(ellipse);
            grid.Children.Add(stackPanel);
            grid.Children.Add(statusTextBlock);

            // Thêm hiệu ứng hover
            border.MouseEnter += (s, e) =>
            {
                border.Background = new SolidColorBrush(Color.FromArgb(229, 229, 229, 229)); 
            };
            border.MouseLeave += (s, e) =>
            {
                border.Background = null;
            };

            border.Child = grid;

            border.MouseDown += async (s, e) =>
            {
                _notificationService.Dispose();
                await _notificationHub.DisconnectAsync();
                ChatWindow chatRoom = new ChatWindow(email, user.Email, user.FriendStatus);
                this.Hide();
                chatRoom.ShowDialog();
                this.Close();
            };

            return border;
        }

        private void UpdateFriendStatus(string email, string newStatus)
        {
            if (Dispatcher.CheckAccess())
            {
                foreach (var child in FriendListPanel.Children)
                {
                    if (child is Border border && border.Child is Grid grid)
                    {
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is StackPanel stackPanel)
                            {
                                foreach (var spChild in stackPanel.Children)
                                {
                                    if (spChild is TextBlock tb && tb.Text == email)
                                    {
                                        var statusTextBlock = grid.Children.OfType<TextBlock>()
                                            .FirstOrDefault(t => t.Name == "StatusTextBlock");
                                        if (statusTextBlock != null)
                                        {
                                            statusTextBlock.Text = newStatus == "online" ? "🟢 Online" : "⚫ Offline";
                                            statusTextBlock.Foreground = newStatus == "online" ?
                                                new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Gray);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Dispatcher.Invoke(() => UpdateFriendStatus(email, newStatus));
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _userService.SetOfflineStatusInDB(email);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(email);
                await _statusHub.DisconnectAsync();
            }
            string str = "";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, str);

            this.Hide();
            SignIn login = new SignIn();
            login.ShowDialog();
            this.Close();
        }

        private async void ThumbImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _userService.SetOfflineStatusInDB(email);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(email);
                await _statusHub.DisconnectAsync();
            }
            this.Hide();
            UserProfile userProfile = new UserProfile(email);
            userProfile.ShowDialog();
            this.Close();
        }

        private async void ManageFriendButton_Click(object sender, RoutedEventArgs e)
        {
            _userService.SetOfflineStatusInDB(email);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(email);
                await _statusHub.DisconnectAsync();
            }
            this.Hide();
            ManageFriend manageFriend = new ManageFriend(email);
            manageFriend.ShowDialog();
            this.Close();
        }

        private async void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            CreateGroup createGroup = new CreateGroup(email);
            bool? result = createGroup.ShowDialog();

            if (result == true)
            {
                ListGroups();
            }
        }
    }
}