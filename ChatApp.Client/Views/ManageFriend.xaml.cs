using ChatApp.Common.DAO;
using System.Windows;
using ChatApp.Client.Services;
using ChatApp.Common.DTOs;
using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using SWM = System.Windows.Media;
using SWC = System.Windows.Controls;
using MessageBox = System.Windows.Forms.MessageBox;


namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for ManageFriend.xaml
    /// </summary>
    public partial class ManageFriend : Window
    {
        private string _fromEmail;
        private StatusAccountHub _statusHub;
        private NotificationHub _notificationHub;
        private UserService _userService;
        private List<UserFriendDTO> listFriend;
        private List<UserFriendDTO> friendRequest;
        private UserDTO _newFriend;
        public ManageFriend(string email)
        {
            InitializeComponent();
            
            _fromEmail = email;

            // Start the socket hub
            _statusHub = new StatusAccountHub();
            _notificationHub = new NotificationHub(_fromEmail);

            // Initialize services
            _userService = new UserService();

            // Initialize list friend
            listFriend = FriendDAO.Instance.GetFriendsWithStatus(_fromEmail);
            friendRequest = FriendDAO.Instance.GetFriendsRequest(_fromEmail);

            InitializeListFriend();
            InitializeFriendRequest();
        }

        private async void form_load(object sender, RoutedEventArgs e)
        {
            await _statusHub.SetOnline(_fromEmail);
            _userService.SetOnlineStatusInDB(_fromEmail);

            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "unfriend")
                {
                    listFriend.RemoveAll(friend => friend.Email == message);
                }
                foreach (var friend in listFriend)
                {
                    if (messageType == "block" && friend.Email == message)
                    {
                        friend.FriendStatus = "blocked";
                    }
                    else if (messageType == "unblock" && friend.Email == message)
                    {
                        friend.FriendStatus = "accepted";
                    }
                }

                if (messageType == "friend_request")
                {
                    friendRequest = FriendDAO.Instance.GetFriendsRequest(_fromEmail);
                }
                if (messageType == "accept_friend_request")
                {
                    var newFriend = friendRequest.FirstOrDefault(f => f.Email == senderEmail);
                    if (newFriend != null)
                    {
                        friendRequest.Remove(newFriend);

                        newFriend.FriendStatus = "accepted";
                        listFriend.Add(newFriend);
                    }
                }
                else if (messageType == "avoid_friend_request")
                {
                    friendRequest.RemoveAll(r => r.Email == senderEmail);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    InitializeListFriend();
                    InitializeFriendRequest();
                });
            });
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
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

        private void InitializeListFriend()
        {
            ListFriend.Children.Clear();

            foreach (var friend in listFriend)
            {
                // Grid chính
                var grid = new Grid
                {
                    Margin = new Thickness(0, 15, 0, 0)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Avatar
                Ellipse avatar = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (!string.IsNullOrEmpty(friend.AvatarUrl))
                {
                    ImageBrush imageBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(friend.AvatarUrl)),
                        Stretch = Stretch.UniformToFill
                    };
                    avatar.Fill = imageBrush;
                }
                else
                {
                    avatar.Fill = SWM.Brushes.LightGray;
                }

                // Tên và trạng thái
                StackPanel infoPanel = new StackPanel
                {
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(infoPanel, 1);
                infoPanel.Children.Add(new TextBlock
                {
                    Text = friend.FullName,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                });
                infoPanel.Children.Add(new TextBlock
                {
                    Text = GetFriendStatusText(friend.FriendStatus),
                    FontSize = 12,
                    Foreground = SWM.Brushes.Gray
                });

                // Nút thao tác
                StackPanel actionPanel = new StackPanel
                {
                    Orientation = SWC.Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(actionPanel, 2);

                // Nút 1: Chặn / Bỏ chặn / Bị chặn
                var btn1 = new SWC.Button
                {
                    Width = 70,
                    Height = 25,
                    Margin = new Thickness(0, 0, 5, 0),
                    BorderThickness = new Thickness(1),
                    Tag = friend.Email
                };

                if (friend.FriendStatus == "block")
                {
                    btn1.Content = "Bỏ chặn";
                    btn1.Background = SWM.Brushes.Transparent;
                    btn1.BorderBrush = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55");
                    btn1.Foreground = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55");
                    btn1.Click += HandleUnblockFriend;
                }
                else if (friend.FriendStatus == "blocked")
                {
                    btn1.Content = "Bị chặn";
                    btn1.Background = SWM.Brushes.LightGray;
                    btn1.Foreground = SWM.Brushes.White;
                    btn1.BorderBrush = SWM.Brushes.Gray;
                    btn1.IsEnabled = false;
                }
                else if (friend.FriendStatus == "accepted")
                {
                    btn1.Content = "Chặn";
                    btn1.Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#D9534F"); // đỏ
                    btn1.Foreground = SWM.Brushes.White;
                    btn1.BorderBrush = SWM.Brushes.Transparent;
                    btn1.Click += HandleBlockFriend;
                }

                // Nút 2: Hủy kết bạn
                var btn2 = new SWC.Button
                {
                    Content = "Hủy",
                    Width = 50,
                    Height = 25,
                    Margin = new Thickness(0),
                    Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#f0ad4e"), // màu cam
                    Foreground = SWM.Brushes.White,
                    BorderThickness = new Thickness(0),
                    Tag = friend.Email
                };
                btn2.Click += HandleRemoveFriend;

                actionPanel.Children.Add(btn1);
                actionPanel.Children.Add(btn2);

                // Gán các thành phần vào Grid
                grid.Children.Add(avatar);
                grid.Children.Add(infoPanel);
                grid.Children.Add(actionPanel);

                // Thêm vào StackPanel chính
                ListFriend.Children.Add(grid);
            }
        }

        private void InitializeFriendRequest()
        {
            FriendRequest.Children.Clear();

            foreach (var friend in friendRequest)
            {
                var grid = new Grid
                {
                    Margin = new Thickness(0, 15, 0, 0)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Avatar
                Ellipse avatar = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (!string.IsNullOrEmpty(friend.AvatarUrl))
                {
                    ImageBrush imageBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(friend.AvatarUrl)),
                        Stretch = Stretch.UniformToFill
                    };
                    avatar.Fill = imageBrush;
                }
                else
                {
                    avatar.Fill = SWM.Brushes.LightGray;
                }

                // Tên và trạng thái
                StackPanel infoPanel = new StackPanel
                {
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(infoPanel, 1);
                infoPanel.Children.Add(new TextBlock
                {
                    Text = friend.FullName,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                });
                infoPanel.Children.Add(new TextBlock
                {
                    Text = GetFriendStatusText(friend.FriendStatus),
                    FontSize = 12,
                    Foreground = SWM.Brushes.Gray
                });

                // Nút thao tác
                StackPanel actionPanel = new StackPanel
                {
                    Orientation = SWC.Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(actionPanel, 2);

                if (friend.FriendStatus == "send_request")
                {
                    var btn = new SWC.Button
                    {
                        Content = "Đợi chấp nhận",
                        Width = 100,
                        Height = 25,
                        Margin = new Thickness(0),
                        IsEnabled = false,
                        Background = SWM.Brushes.LightGray,
                        Foreground = SWM.Brushes.White,
                        BorderBrush = SWM.Brushes.Gray
                    };
                    actionPanel.Children.Add(btn);
                }
                else if (friend.FriendStatus == "receive_request")
                {
                    var acceptBtn = new SWC.Button
                    {
                        Content = "Chấp nhận",
                        Width = 80,
                        Height = 25,
                        Margin = new Thickness(0, 0, 5, 0),
                        Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55"),
                        Foreground = SWM.Brushes.White,
                        BorderThickness = new Thickness(0),
                        Tag = friend.Email
                    };
                    acceptBtn.Click += HandleAcceptFriendRequest;

                    var denyBtn = new SWC.Button
                    {
                        Content = "Từ chối",
                        Width = 70,
                        Height = 25,
                        Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#D9534F"),
                        Foreground = SWM.Brushes.White,
                        BorderThickness = new Thickness(0),
                        Tag = friend.Email
                    };
                    denyBtn.Click += HandleRejectFriendRequest;

                    actionPanel.Children.Add(acceptBtn);
                    actionPanel.Children.Add(denyBtn);
                }

                grid.Children.Add(avatar);
                grid.Children.Add(infoPanel);
                grid.Children.Add(actionPanel);

                FriendRequest.Children.Add(grid);
            }
        }

        private void HandleRejectFriendRequest(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;
            string friendEmail = button?.Tag as string;

            if (string.IsNullOrEmpty(friendEmail)) return;

            // Tìm user trong listRequest
            var request = friendRequest.FirstOrDefault(f => f.Email == friendEmail);
            if (request != null)
            {
                // Xoá khỏi listRequest
                friendRequest.Remove(request);

                // Cập nhật lại giao diện
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    InitializeListFriend();
                    InitializeFriendRequest();
                });
            }

            FriendDAO.Instance.UnfriendByEmail(_fromEmail, friendEmail);

            _notificationHub.SendNotification(_fromEmail, [friendEmail], friendEmail, "avoid_friend_request");
        }

        private void HandleAcceptFriendRequest(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;

            string friendEmail = button.Tag as string;

            var friend = friendRequest.FirstOrDefault(f => f.Email == friendEmail);
            if (friend != null)
            {
                friendRequest.Remove(friend);

                friend.FriendStatus = "accepted";
                listFriend.Add(friend);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    InitializeListFriend();
                    InitializeFriendRequest();
                });
            }

            FriendDAO.Instance.AcceptFriendRequest(_fromEmail, friendEmail);

            _notificationHub.SendNotification(_fromEmail, [friendEmail], friendEmail, "accept_friend_request");
        }

        private void RenderSearchedUser(string fullName, string? avatarUrl, string subText, bool canAddFriend, string email = "")
        {
            // StackPanel chứa kết quả
            var container = new StackPanel
            {
                Orientation = SWC.Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Avatar
            var avatar = new Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = SWM.Brushes.LightGray
            };

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    avatar.Fill = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(avatarUrl)),
                        Stretch = Stretch.UniformToFill
                    };
                }
                catch
                {
                    // Giữ LightGray nếu load ảnh lỗi
                }
            }

            // Thông tin người dùng
            var infoPanel = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };
            infoPanel.Children.Add(new TextBlock
            {
                Text = fullName,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            infoPanel.Children.Add(new TextBlock
            {
                Text = subText,
                FontSize = 12,
                Foreground = SWM.Brushes.Gray
            });

            container.Children.Add(avatar);
            container.Children.Add(infoPanel);

            // Nút kết bạn nếu có thể
            if (canAddFriend)
            {
                var addButton = new SWC.Button
                {
                    Content = "Kết bạn",
                    Width = 70,
                    Height = 25,
                    Margin = new Thickness(20, 0, 0, 0),
                    Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55"),
                    Foreground = SWM.Brushes.White,
                    BorderThickness = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = email // để gửi kết bạn nếu cần
                };

                addButton.Click += HandleSendFriendRequest;

                container.Children.Add(addButton);
            }

            // Thêm vào NewFriends panel
            NewFriends.Children.Add(container);
        }

        private void HandleSendFriendRequest(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;

            string friendEmail = button.Tag as string;

            FriendDAO.Instance.SendFriendRequest(_fromEmail, friendEmail);

            NewFriends.Children.Clear();

            friendRequest = FriendDAO.Instance.GetFriendsRequest(_fromEmail);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                InitializeListFriend();
                InitializeFriendRequest();
            });

            _notificationHub.SendNotification(_fromEmail, [friendEmail], friendEmail, "friend_request");
        }

        private void HandleRemoveFriend(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;

            string friendEmail = button.Tag as string;

            FriendDAO.Instance.UnfriendByEmail(_fromEmail, friendEmail);

            listFriend.RemoveAll(friend => friend.Email == friendEmail);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                InitializeListFriend();
                InitializeFriendRequest();
            });

            _notificationHub.SendNotification(_fromEmail, [friendEmail], _fromEmail, "unfriend");
        }

        private void HandleBlockFriend(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;

            string friendEmail = button.Tag as string;

            FriendDAO.Instance.BlockFriendByEmail(_fromEmail, friendEmail);

            button.Content = "Bỏ chặn";
            button.Background = SWM.Brushes.Transparent;
            button.BorderBrush = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55");
            button.Foreground = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#2E7D55");
            button.Click += HandleUnblockFriend;
                
            _notificationHub.SendNotification(_fromEmail, [friendEmail], _fromEmail, "block");
        }

        private void HandleUnblockFriend(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;

            string friendEmail = button.Tag as string;

            FriendDAO.Instance.UnblockFriendByEmail(_fromEmail, friendEmail);

            button.Content = "Chặn";
            button.Background = (SWM.Brush)new SWM.BrushConverter().ConvertFromString("#D9534F");
            button.Foreground = SWM.Brushes.White;
            button.BorderBrush = SWM.Brushes.Transparent;
            button.Click += HandleBlockFriend;

            _notificationHub.SendNotification(_fromEmail, [friendEmail], _fromEmail, "unblock");
        }

        private void FindFriendClick(object sender, RoutedEventArgs e)
        {
            string email = FindFriendEmail.Text;
            NewFriends.Children.Clear();
            var existingFriend = listFriend.FirstOrDefault(f =>
            f.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (email == _fromEmail)
            {
                MessageBox.Show("Lỗi tìm kiếm", "Thông báo");
                return;
            }

            if (existingFriend != null)
            {
                RenderSearchedUser(existingFriend.FullName, existingFriend.AvatarUrl, "Đây đã là bạn", false);
                return;
            }
            var user = AccountDAO.Instance.SearchUsersByEmail(email);

            if (user == null)
            {
                MessageBox.Show("Không tìm thấy người dùng", "Thông báo");
                return;
            }

            RenderSearchedUser(user.FullName, user.AvatarUrl, "Kết quả tìm kiếm", true, user.Email);
            _newFriend = AccountDAO.Instance.SearchUsersByEmail(email);
        }

        private string GetFriendStatusText(string status)
        {
            return status switch
            {
                "friend" => "Đã kết bạn",
                "block" => "Đã chặn",
                "blocked" => "Đã bị chặn",
                _ => ""
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            MainForm mainForm = new MainForm(_fromEmail);

            mainForm.ShowDialog();
            this.Close();
        }

        private void FindFriendEmail_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
