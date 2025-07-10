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
using Cursors = System.Windows.Input.Cursors;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;

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

        public MainForm_1(string email)
        {
            InitializeComponent();
            this.email = email;

            _statusHub = new StatusAccountHub();
            _userService = new UserService();
            _circularPictureBoxService = new CircularPictureBoxService();

            ListFriend();
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

            border.Child = grid;

            border.MouseDown += async (s, e) =>
            {
                ChatWindow chatRoom = new ChatWindow(email, user.Email, user.FriendStatus);

                if (_statusHub != null)
                {
                    await _statusHub.DisconnectAsync();
                }
                _notificationService.Dispose();
                this.Hide();
                chatRoom.ShowDialog();
                this.Close();
            };

            return border;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            /*UsersContent.Children.Clear();

            string searchText = SearchTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Tìm kiếm")
            {
                var friends = FriendDAO.Instance.GetFriendsWithStatus(email);
                foreach (var friend in friends)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{friend.FullName} ({friend.Email})",
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 14,
                        Margin = new Thickness(5)
                    };
                    UsersContent.Children.Add(textBlock);
                }
            }
            else
            {
                var friends = FriendDAO.Instance.GetFriendsWithStatus(email);
                var filteredFriends = friends.Where(f =>
                    f.FullName.ToLower().Contains(searchText.ToLower()) ||
                    f.Email.ToLower().Contains(searchText.ToLower()));

                if (filteredFriends.Any())
                {
                    foreach (var friend in filteredFriends)
                    {
                        TextBlock textBlock = new TextBlock
                        {
                            Text = $"{friend.FullName} ({friend.Email})",
                            FontFamily = new FontFamily("Segoe UI"),
                            FontSize = 14,
                            Margin = new Thickness(5)
                        };
                        UsersContent.Children.Add(textBlock);
                    }
                }
                else
                {
                    TextBlock noResult = new TextBlock
                    {
                        Text = "Không tìm thấy kết quả.",
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 14,
                        Margin = new Thickness(5),
                        Foreground = System.Windows.Media.Brushes.Red
                    };
                    UsersContent.Children.Add(noResult);
                }
            }*/
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

            // Load user avatar
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(defaultAvatarUrl, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                ThumbImageBrush.ImageSource = bitmap; // Gán vào ImageBrush
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default avatar for {email}: {ex.Message}");
                ThumbImageBrush.ImageSource = null; // Clear image on error
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

        private void ThumbImage_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Tìm kiếm")
            {
                textBox.Text = string.Empty;
                textBox.Foreground = System.Windows.Media.Brushes.Black; // Đổi màu chữ khi nhập
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Tìm kiếm";
                textBox.Foreground = System.Windows.Media.Brushes.Gray; // Khôi phục màu placeholder
            }
        }
    }
}