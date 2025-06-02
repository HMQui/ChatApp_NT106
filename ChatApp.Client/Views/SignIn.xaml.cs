using ChatApp.Common.DAO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        private static readonly string secretKey = "6DOKMbMsMPPDBTLjdZAlEcFOktrQL7Yz";
        private bool _isPasswordVisible;

        public static readonly DependencyProperty IsPasswordVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPasswordVisible),
                typeof(bool),
                typeof(SignIn),
                new PropertyMetadata(false));

        public bool IsPasswordVisible
        {
            get => (bool)GetValue(IsPasswordVisibleProperty);
            set => SetValue(IsPasswordVisibleProperty, value);
        }

        public SignIn()
        {
            InitializeComponent();
            // Synchronize PasswordBox and TextBox
            PasswordBoxControl.PasswordChanged += PasswordBoxControl_PasswordChanged;
            PasswordTextBoxControl.TextChanged += PasswordTextBoxControl_TextChanged;

            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            EmailTextBox.GotFocus += (s, e) => UpdateEmailPlaceholderVisibility();
            EmailTextBox.LostFocus += (s, e) => UpdateEmailPlaceholderVisibility();

            /* Authentication */
            string filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt"));


            if (File.Exists(filePath))
            {
                try
                {
                    string token = File.ReadAllText(filePath).Trim();

                    if (!string.IsNullOrEmpty(token))
                    {
                        string currEmail = ExtractEmailFromToken(token);
                        if (AccountDAO.Instance.CheckExistEmail(currEmail))
                        {
                            HandleLoginSuccess(currEmail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi khi đọc file: " + ex.Message);
                }
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
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

        private void PasswordBoxControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Cập nhật TextBox nếu đang ở chế độ hiển thị mật khẩu
            if (_isPasswordVisible)
            {
                PasswordTextBoxControl.Text = PasswordBoxControl.Password;
            }
            lbHelpText.Text = "";
        }

        private void PasswordTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            // Cập nhật PasswordBox để giữ đồng bộ
            if (_isPasswordVisible)
            {
                PasswordBoxControl.Password = PasswordTextBoxControl.Text;
                lbHelpText.Text = "";
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

        private static string GenerateToken(string email, int expireDays = 7)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Email, email)
        };

            var token = new JwtSecurityToken(
                issuer: "zola",
                audience: "zola",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string ExtractEmailFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "zola",
                ValidateAudience = true,
                ValidAudience = "zola",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var emailClaim = principal.FindFirst(ClaimTypes.Email);
                return emailClaim?.Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void HandleLoginSuccess(string email)
        {
            string token = GenerateToken(email);
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(token);
            }

            this.Hide();
            using (MainForm mainForm = new MainForm(email))
            {
                mainForm.ShowDialog();
            }
            this.Close();
        }

        private bool CheckAccount(string email, string password)
        {
            return AccountDAO.Instance.CheckAccount(email, password);
        }

        private bool isValidEmail(string email)
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

        private void click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password;

            if (PasswordBoxControl.Visibility == Visibility.Visible)
            {
                password = PasswordBoxControl.Password;
            }
            else
            {
                password = PasswordTextBoxControl.Text;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                lbHelpText.Text = "Vui lòng nhập email và mật khẩu";
                return;
            }
            else if (!isValidEmail(email))
            {
                lbHelpText.Text = "Email không hợp lệ";
                return;
            }
            else if (password.Length < 6)
            {
                lbHelpText.Text = "Mật khẩu phải có ít nhất 6 ký tự";
                return;
            }

            if (CheckAccount(email, password))
            {
                HandleLoginSuccess(email);
            }
            else
            {
                lbHelpText.Text = "Email hoặc mật khẩu không đúng";
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SignUp signUp = new SignUp();
            signUp.ShowDialog();
            this.Close();
        }
    }

    // Boolean Negation Converter
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }
    }
   

   
}