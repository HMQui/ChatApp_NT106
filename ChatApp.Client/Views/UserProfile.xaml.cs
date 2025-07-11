﻿using ChatApp.Common.DTOs;
using ChatApp.Common.DAO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ChatApp.Client.Services;
using ChatApp.Client.Hub;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>

    public partial class UserProfile : Window
    {
        static string _email;
        static UserDTO _userProfile;
        private readonly StatusAccountHub _statusHub;
        private readonly UserService _userService;

        public UserProfile(string email)
        {
            InitializeComponent();

            _email = email;
            _userProfile = AccountDAO.Instance.SearchUsersByEmail(_email);
            _statusHub = new StatusAccountHub();
            _userService = new UserService();

            InitUI();
        }

        public void InitUI()
        {
            if (_userProfile != null)
            {
                FullNameTextBlock.Text = _userProfile.FullName ?? "Chưa có dữ liệu";

                EmailTextBox.Text = _userProfile.Email ?? "Chưa có dữ liệu";

                CreatedAtTextBlock.Text = _userProfile.CreatedAt != default ? _userProfile.CreatedAt.ToString("dd MMMM, yyyy") : "Chưa có dữ liệu";

                PhoneTextBox.Text = _userProfile.Phone ?? "Chưa có dữ liệu";
                // Avatar
                try
                {
                    if (!string.IsNullOrEmpty(_userProfile.AvatarUrl))
                    {
                        AvatarBrush.Source = new BitmapImage(new Uri(_userProfile.AvatarUrl));
                    }
                    else
                    {
                        AvatarBrush.Source = new BitmapImage(new Uri("https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg"));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải hình ảnh avatar: {ex.Message}");
                    AvatarBrush.Source = null;
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy người dùng hoặc dữ liệu rỗng.");
                Close();
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainForm_1 mainForm = new MainForm_1(_email);
                this.Hide();
                if (!mainForm.IsLoaded) // Kiểm tra xem MainForm đã được tải chưa
                {
                    mainForm.ShowDialog();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi quay lại MainForm: {ex.Message}");
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeProfile cp = new ChangeProfile(_email);
            this.Hide();

            cp.ShowDialog();

            this.Close();
        }
    }
}