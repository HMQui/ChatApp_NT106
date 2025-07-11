using ChatApp.Common.DTOs;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for GroupManagementUserWindow.xaml
    /// </summary>
    public partial class GroupManagementUserWindow : Window
    {
        GroupDTO _groupInfo;
        string _email = "";
        List<GroupMembersDTO> _groupMembers;
        GroupMembersDTO _userInfo;
        NotificationHub _notificationHub;
        ChatGroupHub _chatGroupHub;
        public GroupManagementUserWindow(GroupDTO groupInfo, string email, NotificationHub notificationHub, ChatGroupHub chatGroupHub)
        {
            InitializeComponent();

            _groupInfo = groupInfo;
            _email = email;
            _notificationHub = notificationHub;
            _chatGroupHub = chatGroupHub;
        }

        private async void form_loading(object sender, RoutedEventArgs e)
        {
            _groupMembers = GroupMembersDAO.Instance.GetMembersByGroupId(_groupInfo.Id);
            foreach (GroupMembersDTO member in _groupMembers)
            {
                if (member.Email == _email)
                {
                    _userInfo = member;
                    break;
                }
            }

            InitUI();
        }

        private void InitUI()
        {
            // Cập nhật thông tin nhóm cho binding
            var viewModel = new
            {
                GroupInfo = _groupInfo,
                MemberCount = _groupMembers?.Count ?? 0,
            };

            this.DataContext = viewModel;

            // Xử lý Avatar mặc định nếu thiếu
            foreach (var member in _groupMembers)
            {
                if (string.IsNullOrEmpty(member.Avatar))
                {
                    member.Avatar = "https://static.thenounproject.com/png/2309777-200.png";
                }
            }

            // Load danh sách thành viên vào ItemsControl
            GroupMembersList.ItemsSource = null;
            GroupMembersList.ItemsSource = _groupMembers;
        }


        private void form_closing(object sender, CancelEventArgs e)
        {

        }

        private async void LeaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                "Bạn có chắc chắn muốn rời nhóm này?",
                "Xác nhận rời nhóm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool isDeleted = GroupMembersDAO.Instance.RemoveMemberFromGroup(_groupInfo.Id, _email);

                    if (isDeleted)
                    {
                        string notificationMessage = $"{DateTime.Now:dd/MM/yyyy HH:mm} - {_userInfo.NickName} đã rời khỏi nhóm";
                        byte[] messageContent = Encoding.UTF8.GetBytes(notificationMessage);

                        await _chatGroupHub.SendGroupMessageAsync(
                            _groupInfo.Id,
                            messageContent,
                            _groupInfo.CreatedBy,
                            "group_notification",
                            DateTime.Now);

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
