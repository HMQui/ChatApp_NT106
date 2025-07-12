using System.Windows;
using ChatApp.Common.DTOs;
using ChatApp.Client.Hub;
using ChatApp.Common.DAO;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.Forms.MessageBox;
using SWC = System.Windows.Controls;
using Azure.Messaging;

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

            // Thông báo xóa thành viên thành công
            await _notificationHub.SendNotification(_groupInfo.CreatedBy, [member.Email], $"{_groupInfo.GroupName}||Bạn đã bị xóa khỏi nhóm!", "group_notification");

            byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{member.NickName} đã bị xoá khỏi nhóm.");
            await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, member.Email, "group_notification", DateTime.Now);
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
            else
            {
                MessageBox.Show($"Thêm thành viên {user.FullName} vào nhóm thành công.");
            }    

            // Cập nhật lại UI
            _groupMembers.Add(newMem);
            _userNotInGroup.Remove(user);
            InitMember();

            // Thông báo thêm thành viên thành công
            await _notificationHub.SendNotification(_groupInfo.CreatedBy, [user.Email], $"{_groupInfo.GroupName}||Bạn đã được thêm vào nhóm!", "group_notification");
           
            byte[] messageContent = System.Text.Encoding.UTF8.GetBytes($"{user.FullName} đã được thêm vào nhóm.");
            await _chatGroupHub.SendGroupMessageAsync(_groupInfo.Id, messageContent, user.Email, "group_notification", DateTime.Now);
        }

        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveGroupInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisbandGroupButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
