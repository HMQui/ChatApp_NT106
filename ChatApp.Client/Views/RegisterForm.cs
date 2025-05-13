using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using ChatApp.Common.DAO;

namespace ChatApp.Client.Views
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            lbHelpText.Text = "";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnInVisiblePass_Click(object sender, EventArgs e)
        {
            if (tbPass.PasswordChar == '\0')
            {
                btnVisiblePass.BringToFront();
                tbPass.PasswordChar = '*';
            }
        }

        private void RegisterForm_Load_1(object sender, EventArgs e)
        {

        }

        private void btnVisiblePass_Click(object sender, EventArgs e)
        {
            if (tbPass.PasswordChar == '*')
            {
                btnInVisiblePass.BringToFront();
                tbPass.PasswordChar = '\0';
            }
        }

        private void btnInVisibleRpPass_Click(object sender, EventArgs e)
        {
            if (tbRpPass.PasswordChar == '\0')
            {
                btnVisibleRpPass.BringToFront();
                tbRpPass.PasswordChar = '*';
            }
        }

        private void btnVisibleRpPass_Click(object sender, EventArgs e)
        {
            if (tbRpPass.PasswordChar == '*')
            {
                btnInVisibleRpPass.BringToFront();
                tbRpPass.PasswordChar = '\0';
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Close();
            using (LoginForm loginForm = new LoginForm())
            {
                loginForm.ShowDialog();
            }
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            string email = tbEmail.Text;
            string password = tbPass.Text;
            string confirmPassword = tbRpPass.Text;
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
                VerifySignUp verifySignUp = new VerifySignUp(randomCode, email, this);
                this.Hide();
                verifySignUp.ShowDialog();
                this.Close();
            }
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

    }
}
