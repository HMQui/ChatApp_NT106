using ChatApp.Common.DAO;
using System.Windows;
using System.Windows.Controls;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
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

        private void tbPass_TextChanged(object sender, EventArgs e)
        {
            lbHelpText.Text = "";
        }

        private void tbRpPass_TextChanged(object sender, EventArgs e)
        {
            lbHelpText.Text = "";
        }

        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbHelpText.Text = "";
        }

        private void tbEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbHelpText.Text = "";
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();

            SignIn loginForm = new SignIn();
            loginForm.ShowDialog();
            this.Close();
        }

        private void btnSignUp_Click_1(object sender, RoutedEventArgs e)
        {
            string email = tbEmail.Text;
            string password = tbPass.Password;
            string confirmPassword = tbRpPass.Password;
            string name = tbName.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(name))
            {
                lbHelpText.Text = "Vui lòng nhập đầy đủ các thông tin cần thiết";
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
            else if (password != confirmPassword)
            {
                lbHelpText.Text = "Mật khẩu và xác nhận mật khẩu không khớp nhau";
                return;
            }

            if (AccountDAO.Instance.CheckExistEmail(email))
            {
                lbHelpText.Text = "Email đã tồn tại";
                return;
            }

            string randomCode = GenerateRandomCode.CreateCode();
            int result = AccountDAO.Instance.InsertAccountNotVerify(email, password, randomCode, name);
            if (result == 1)
            {
                // gửi mail
                VerifySignUp verifySignUp = new VerifySignUp(randomCode, email);
                this.Hide();
                verifySignUp.ShowDialog();
                this.Close();
            }
        }
    }
}
