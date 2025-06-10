using ChatApp.Common.DTOs;
using ChatApp.Common.DAO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        static string _email;
        static UserDTO _userProfile;
        public UserProfile(string email)
        {
            InitializeComponent();

            _email = email;
            _userProfile = AccountDAO.Instance.SearchUsersByEmail(_email);

            InitUI();

        }

        public void InitUI()
        {
            if (_userProfile != null)
            {
                // Tên và email
                FullNameTextBlock.Text = _userProfile.FullName;
                EmailTextBlock.Text = _userProfile.Email;

                // Ngày tạo (định dạng tùy ý)
                CreatedAtTextBlock.Text = _userProfile.CreatedAt.ToString("dd MMMM, yyyy");

                // Avatar
                if (!string.IsNullOrEmpty(_userProfile.AvatarUrl))
                {
                    AvatarBrush.Source = new BitmapImage(new Uri(_userProfile.AvatarUrl));
                }
                /*else
                {
                    AvatarBrush.Source = new BitmapImage(new Uri("/Assets/Images/NoAvatar.png"));
                }*/
            }
            else
            {
                MessageBox.Show("Không tìm thấy người dùng.");
                Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
