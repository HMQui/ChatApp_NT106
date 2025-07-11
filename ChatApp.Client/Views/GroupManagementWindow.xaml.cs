using System.Windows;
using ChatApp.Common.DTOs;
using ChatApp.Client.Hub;
using ChatApp.Common.DAO;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.Forms.MessageBox;
using SWC = System.Windows.Controls;
using System.IO;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Net.Http;
using System.Text;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for GroupManagementWindow.xaml
    /// </summary>
    public partial class GroupManagementWindow : Window
    {
        GroupDTO _groupInfo;
        List<GroupMembersDTO> _groupMembers;
        List<UserDTO> _userNotInGroup;
        NotificationHub _notificationHub;
        ChatGroupHub _chatGroupHub;

        public GroupManagementWindow(GroupDTO groupInfo, NotificationHub notificationHub, ChatGroupHub chatGroupHub)
        {
            InitializeComponent();

            _groupInfo = groupInfo;
            _notificationHub = notificationHub;
            _chatGroupHub = chatGroupHub;
        }

        private async void form_loading(object sender, RoutedEventArgs e)
        {
            _groupMembers = GroupMembersDAO.Instance.GetMembersByGroupId(_groupInfo.Id);
            _userNotInGroup = GroupMembersDAO.Instance.GetFriendsNotInGroup(_groupInfo.Id, _groupInfo.CreatedBy);

            InitUI();
        }

        private void form_closing(object sender, CancelEventArgs e)
        {
            
        }

        private void InitUI()
        {
            GroupNameTextBox.Text = _groupInfo.GroupName;
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

            InitMember();
        }

        private void InitMember()
        {
            // Thành viên nhóm
            foreach (var member in _groupMembers)
            {
                if (string.IsNullOrEmpty(member.Avatar))
                {
                    member.Avatar = "https://static.thenounproject.com/png/2309777-200.png";
                }
            }

            GroupMembersList.ItemsSource = null;
            GroupMembersList.ItemsSource = _groupMembers;
            MemberCountTextBlock.Text = $"Thành viên ({_groupMembers.Count})";

            // Danh sách bạn bè chưa tham gia nhóm
            foreach (var friend in _userNotInGroup)
            {
                if (string.IsNullOrEmpty(friend.AvatarUrl))
                {
                    friend.AvatarUrl = "https://static.thenounproject.com/png/2309777-200.png";
                }
            }
            FriendsNotInGroupList.ItemsSource = null;
            FriendsNotInGroupList.ItemsSource = _userNotInGroup;
            FriendCountTextBlock.Text = $"Bạn bè chưa tham gia nhóm ({_userNotInGroup.Count})";
        }

        private async void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            SWC.Button button = sender as SWC.Button;
            if (button == null) return;

            var member = button.Tag as GroupMembersDTO;
            if (member == null) return;

            if (member.Email == _groupInfo.CreatedBy)
            {
                MessageBox.Show("Bạn không thể xoá người tạo nhóm.");
                return;
            }

            var notice = new NoticeDTO
            {
                Email = member.Email,
                Title = $"Thông báo nhóm",
                Message = $"Bạn đã bị xóa khỏi nhóm {_groupInfo.GroupName}",
                IsSeen = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            NoticeDAO.Instance.AddNotice(notice);

            // Thông báo xóa thành viên thành công
            await _notificationHub.SendNotification(_groupInfo.CreatedBy, [member.Email], $"{_groupInfo.GroupName}||Bạn đã bị xóa khỏi nhóm!", "group_notification");

            byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{member.NickName} đã bị xoá khỏi nhóm.");
            await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, member.Email, "group_notification", DateTime.Now);

            // Cập nhật database
            await Task.Run(() =>
            {
                bool result = GroupMembersDAO.Instance.RemoveMemberFromGroup(_groupInfo.Id, member.Email);
                if (!result)
                {
                    MessageBox.Show("Xoá thành viên thất bại. Vui lòng thử lại.");
                    return;
                }
            });

            // Cập nhật lại UI
            _userNotInGroup = GroupMembersDAO.Instance.GetFriendsNotInGroup(_groupInfo.Id, _groupInfo.CreatedBy);
            _groupMembers.Remove(member);
            InitMember();
        }

        private async void AddFriendToGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as SWC.Button;
            if (button == null) return;

            var user = button.Tag as UserDTO;
            if (user == null) return;

            // Thêm vào database
            GroupMembersDTO newMem = null;

            await Task.Run(() =>
            {
                newMem = GroupMembersDAO.Instance.AddMemberToGroup(_groupInfo.Id, user.Email, user.FullName);
            });

            if (newMem == null)
            {
                MessageBox.Show("Thêm thành viên thất bại. Vui lòng thử lại.");
                return;
            } 

            // Cập nhật lại UI
            _groupMembers.Add(newMem);
            _userNotInGroup.Remove(user);
            InitMember();

            var notice = new NoticeDTO
            {
                Email = user.Email,
                Title = $"Thông báo nhóm",
                Message = $"Bạn đã được thêm vào nhóm {_groupInfo.GroupName}",
                IsSeen = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            NoticeDAO.Instance.AddNotice(notice);

            // Thông báo thêm thành viên thành công
            await _notificationHub.SendNotification(_groupInfo.CreatedBy, [user.Email], $"{_groupInfo.GroupName}||Bạn đã được thêm vào nhóm!", "group_notification");
           
            byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{user.FullName} đã được thêm vào nhóm.");
            await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, user.FullName, "group_notification", DateTime.Now);
        }

        private async void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
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

            var requestBody = new
            {
                GroupId = _groupInfo.Id,
                ImageBytes = fileBytes
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.PostAsync("http://localhost:5000/api/groups/update-avatar", content);
                    var respString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResp = Newtonsoft.Json.Linq.JObject.Parse(respString);
                        string newAvatarUrl = jsonResp["avatarUrl"]?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(newAvatarUrl))
                        {
                            byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{_groupInfo.CreatedAt} - Admin đã đổi ảnh đại diện mới.");
                            await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, _groupInfo.CreatedBy, "group_notification", DateTime.Now);

                            _groupInfo.Avatar_URL = newAvatarUrl;

                            GroupAvatar.ImageSource = new BitmapImage(new Uri(newAvatarUrl, UriKind.RelativeOrAbsolute));
                            MessageBox.Show("Đổi ảnh nhóm thành công.");
                        }
                        else
                        {
                            MessageBox.Show("API không trả về URL ảnh mới.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Đổi ảnh nhóm thất bại: {respString}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gọi API: " + ex.Message);
                }
            }
        }

        private async void SaveGroupInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string newGroupName = GroupNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newGroupName))
            {
                MessageBox.Show("Tên nhóm không được để trống.");
                return;
            }

            var requestBody = new
            {
                GroupId = _groupInfo.Id,
                NewGroupName = newGroupName
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.PostAsync("http://localhost:5000/api/groups/update-name", content);
                    var respString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{_groupInfo.CreatedAt} - Admin đã đổi tên nhóm mới.");
                        await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, _groupInfo.CreatedBy, "group_notification", DateTime.Now);

                        MessageBox.Show("Đổi tên nhóm thành công.");
                        _groupInfo.GroupName = newGroupName;
                    }
                    else
                    {
                        MessageBox.Show($"Đổi tên nhóm thất bại: {respString}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gọi API: " + ex.Message);
                }
            }
        }

        private async void DisbandGroupButton_Click(object sender, RoutedEventArgs e)
        {
            // Sử dụng MessageBox từ System.Windows (WPF)
            MessageBoxResult result = System.Windows.MessageBox.Show(
                "Bạn có chắc chắn muốn giải tán nhóm này?",
                "Xác nhận giải tán nhóm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool isDeleted = GroupDAO.Instance.DeleteGroup(_groupInfo.Id);

                    if (isDeleted)
                    {
                        string notificationMessage = $"{DateTime.Now:dd/MM/yyyy HH:mm} - Nhóm đã bị giải tán bởi quản trị viên.";
                        byte[] messageContent = Encoding.UTF8.GetBytes(notificationMessage);

                        await _chatGroupHub.SendGroupMessageAsync(
                            _groupInfo.Id,
                            messageContent,
                            _groupInfo.CreatedBy,
                            "group_notification",
                            DateTime.Now);

                        System.Windows.MessageBox.Show("Đã giải tán nhóm thành công!");
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Giải tán nhóm thất bại!");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi khi giải tán nhóm: {ex.Message}");
                }
            }
        }
    }
}
