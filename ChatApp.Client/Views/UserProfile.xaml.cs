using ChatApp.Common.DTOs;
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
                // Tên và email
                FullNameTextBlock.Text = _userProfile.FullName;
                EmailTextBlock.Text = _userProfile.Email;

                // Ngày tạo
                CreatedAtTextBlock.Text = _userProfile.CreatedAt.ToString("dd MMMM, yyyy");

                // Avatar
                try
                {
                    if (!string.IsNullOrEmpty(_userProfile.AvatarUrl))
                    {
                        AvatarBrush.Source = new BitmapImage(new Uri(_userProfile.AvatarUrl));
                    }
                    else
                    {
                        // Đảm bảo đường dẫn chính xác
                        AvatarBrush.Source = new BitmapImage(new Uri("https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg"));
                    }
                }
<<<<<<< HEAD
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải hình ảnh avatar: {ex.Message}");
                    // Có thể gán một hình ảnh thay thế hoặc để trống
                    AvatarBrush.Source = null;
=======
                else
                {
                    AvatarBrush.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/NoAvatar.png"));
>>>>>>> 02cbe16bbc23b23f90235352c268e8ffb726bed0
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy người dùng.");
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}