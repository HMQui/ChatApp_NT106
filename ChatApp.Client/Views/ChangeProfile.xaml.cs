using ChatApp.Common.DTOs;
using ChatApp.Common.DAO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ChatApp.Client.Services;
using ChatApp.Client.Hub;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using Application = System.Windows.Application;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for ChangeProfile.xaml
    /// </summary>
    public partial class ChangeProfile : Window
    {
        static string _email;
        static UserDTO _userProfile;
        private readonly StatusAccountHub _statusHub;
        private readonly UserService _userService;
        private readonly ProfileHub _profileHub;

        public ChangeProfile(string email)
        {
            _email = email;
            _userProfile = AccountDAO.Instance.SearchUsersByEmail(_email);
            _statusHub = new StatusAccountHub();
            _userService = new UserService();
            _profileHub = new ProfileHub(email);

            InitializeComponent();
            SetupSignalR();
            InitUI();
        }

        private async void SetupSignalR()
        {
            await _profileHub.ConnectAsync(avatarUrl =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvatarBrush.Source = new BitmapImage(new Uri(avatarUrl));
                    _userProfile.AvatarUrl = avatarUrl;
                    // Kiểm tra lại từ DTB để đảm bảo đồng bộ (tùy chọn)
                    var updatedUser = AccountDAO.Instance.SearchUsersByEmail(_email);
                    if (updatedUser != null && updatedUser.AvatarUrl == avatarUrl)
                    {
                        MessageBox.Show("Avatar đã được cập nhật và lưu vào DTB!");
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật avatar thành công nhưng DTB không đồng bộ. Vui lòng thử lại.");
                    }
                });
            });
        }

        public void InitUI()
        {
            if (_userProfile != null)
            {
                FullNameTextBox.Text = _userProfile.FullName ?? "Chưa có dữ liệu";
                EmailTextBox.Text = _userProfile.Email ?? "Chưa có dữ liệu";
                PhoneTextBox.Text = _userProfile.Phone ?? "Chưa có dữ liệu";
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            UserProfile up = new UserProfile(_email);
            this.Hide();
            up.ShowDialog();
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newFullName = FullNameTextBox.Text.Trim();
            string newPhoneNumber = PhoneTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newFullName))
            {
                MessageBox.Show("Họ và tên không được để trống.");
                return;
            }

            var fieldsToUpdate = new Dictionary<string, object>
            {
                { "full_name", newFullName },
                { "phone", newPhoneNumber }
            };

            var conditions = new Dictionary<string, object>
            {
                { "email", _email }
            };

            int rowsAffected = AccountDAO.Instance.UpdateFields(fieldsToUpdate, conditions);

            if (rowsAffected > 0)
            {
                MessageBox.Show("Cập nhật thông tin thành công!");
                _userProfile.FullName = newFullName;
                _userProfile.Phone = newPhoneNumber;
                InitUI();
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại. Vui lòng thử lại.");
            }
        }

        private async void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn ảnh avatar",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;
                    byte[] imageBytes = File.ReadAllBytes(imagePath);

                    await _profileHub.UpdateAvatarAsync(imageBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải hoặc upload ảnh: {ex.Message}");
                    AvatarBrush.Source = new BitmapImage(new Uri(_userProfile.AvatarUrl ?? "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg"));
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _profileHub.DisposeAsync().AsTask().Wait();
            base.OnClosed(e);
        }
    }
}