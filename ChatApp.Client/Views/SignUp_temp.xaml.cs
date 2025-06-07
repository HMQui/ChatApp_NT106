using ChatApp.Common.DAO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ChatApp.Client.Views
{
    public partial class SignUp_temp : Window
    {
        private bool _isPasswordVisible;
        private bool _isCfPasswordVisible;
        public SignUp_temp()
        {
            InitializeComponent();
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            EmailTextBox.GotFocus += (s, e) => UpdateEmailPlaceholderVisibility();
            EmailTextBox.LostFocus += (s, e) => UpdateEmailPlaceholderVisibility();

            UsernameTextBox.TextChanged += UsernameTextBox_TextChanged;
            UsernameTextBox.GotFocus += (s, e) => UpdateUsernamePlaceholderVisibility();
            UsernameTextBox.LostFocus += (s, e) => UpdateUsernamePlaceholderVisibility();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEmailPlaceholderVisibility();
            lbHelpText.Text = "";
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEmailPlaceholderVisibility();
            lbHelpText.Text = "";
        }

        private void UpdateEmailPlaceholderVisibility()
        {
            if (string.IsNullOrEmpty(EmailTextBox.Text) && !EmailTextBox.IsFocused)
            {
                PlaceholderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateUsernamePlaceholderVisibility()
        {
            if (string.IsNullOrEmpty(UsernameTextBox.Text) && !UsernameTextBox.IsFocused)
            {
                UsernamePlaceholderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                UsernamePlaceholderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Chuyển sang hiển thị mật khẩu
                PasswordTextBoxControl.Text = PasswordBoxControl.Password;
                PasswordTextBoxControl.Visibility = Visibility.Visible;
                PasswordBoxControl.Visibility = Visibility.Collapsed;
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-eye-30.png"));
            }
            else
            {
                // Chuyển sang ẩn mật khẩu
                PasswordBoxControl.Password = PasswordTextBoxControl.Text;
                PasswordTextBoxControl.Visibility = Visibility.Collapsed;
                PasswordBoxControl.Visibility = Visibility.Visible;
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-hide-30.png"));
            }
        }

        private void ToggleConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isCfPasswordVisible = !_isCfPasswordVisible;

            if (_isCfPasswordVisible)
            {
                // Chuyển sang hiển thị mật khẩu
                ConfirmPasswordTextBoxControl.Text = ConfirmPasswordBoxControl.Password;
                ConfirmPasswordTextBoxControl.Visibility = Visibility.Visible;
                ConfirmPasswordBoxControl.Visibility = Visibility.Collapsed;
                ConfirmEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-eye-30.png"));
            }
            else
            {
                // Chuyển sang ẩn mật khẩu
                ConfirmPasswordBoxControl.Password = ConfirmPasswordTextBoxControl.Text;
                ConfirmPasswordTextBoxControl.Visibility = Visibility.Collapsed;
                ConfirmPasswordBoxControl.Visibility = Visibility.Visible;
                ConfirmEyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-hide-30.png"));
            }
        }

        private void PasswordBoxControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBoxControl.Visibility == Visibility.Visible)
            {
                PasswordTextBoxControl.Text = PasswordBoxControl.Password;
            }
        }

        private void PasswordTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordBoxControl.Visibility == Visibility.Visible)
            {
                PasswordBoxControl.Password = PasswordTextBoxControl.Text;
            }
        }

        private void ConfirmPasswordBoxControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ConfirmPasswordTextBoxControl.Visibility == Visibility.Visible)
            {
                ConfirmPasswordTextBoxControl.Text = ConfirmPasswordBoxControl.Password;
            }
        }

        private void ConfirmPasswordTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ConfirmPasswordBoxControl.Visibility == Visibility.Visible)
            {
                ConfirmPasswordBoxControl.Password = ConfirmPasswordTextBoxControl.Text;
            }
        }
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Ẩn placeholder khi focus
            PasswordPlaceholderPanel.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hiển thị placeholder nếu không có nội dung
            if (sender == PasswordBoxControl && string.IsNullOrEmpty(PasswordBoxControl.Password))
            {
                PasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
            else if (sender == PasswordTextBoxControl && string.IsNullOrEmpty(PasswordTextBoxControl.Text))
            {
                PasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
        }
        private void CfPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Ẩn placeholder khi focus
            ConfirmPasswordPlaceholderPanel.Visibility = Visibility.Collapsed;
        }

        private void CfPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hiển thị placeholder nếu không có nội dung
            if (sender == ConfirmPasswordBoxControl && string.IsNullOrEmpty(ConfirmPasswordBoxControl.Password))
            {
                ConfirmPasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
            else if (sender == ConfirmPasswordTextBoxControl && string.IsNullOrEmpty(ConfirmPasswordTextBoxControl.Text))
            {
                ConfirmPasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SignIn loginForm = new SignIn();
            loginForm.ShowDialog();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBoxControl.Password;
            string confirmPassword = ConfirmPasswordBoxControl.Password;
            string username = UsernameTextBox.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(username))
            {
                lbHelpText.Text = "Vui lòng nhập đầy đủ các thông tin cần thiết";
                return;
            }
            else if (!IsValidEmail(email))
            {
                lbHelpText.Text = "Email không hợp lệ";
                return;
            }
            else if (password.Length < 6)
            {
                lbHelpText.Text = "Mật khẩu phải có ít nhất 6 ký tự";
                return;
            }
            else if (password != confirmPassword)
            {
                lbHelpText.Text = "Mật khẩu và xác nhận mật khẩu không khớp nhau";
                return;
            }

            if (AccountDAO.Instance.CheckExistEmail(email))
            {
                lbHelpText.Text = "Email đã tồn tại";
                return;
            }

            string randomCode = GenerateRandomCode.CreateCode();
            int result = AccountDAO.Instance.InsertAccountNotVerify(email, password, randomCode, username);
            if (result == 1)
            {
                VerifySignUp verifySignUp = new VerifySignUp(randomCode, email);
                this.Hide();
                verifySignUp.ShowDialog();
                this.Close();
            }
        }
    }
}