using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatApp.Common.DAO;
using ChatApp.Common;

namespace ChatApp.Client.Views
{
    public partial class ResetPassword : Window
    {
        private string emailAddress;

        public ResetPassword(string email)
        {
            InitializeComponent();
            emailAddress = email;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ErrorMessage.Text = "Vui lòng nhập cả hai mật khẩu!";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (newPassword.Length < 6)
            {
                ErrorMessage.Text = "Mật khẩu phải có ít nhất 6 ký tự!";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (newPassword != confirmPassword)
            {
                ErrorMessage.Text = "Mật khẩu xác nhận không khớp!";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            // Băm mật khẩu mới
            string hashedPassword = HandlePassword.HashPassword(newPassword);

            // Cập nhật mật khẩu trong cơ sở dữ liệu
            var updateData = new Dictionary<string, object>
            {
                { "password", hashedPassword }
            };
            var conditions = new Dictionary<string, object>
            {
                { "email", emailAddress }
            };

            int updateResult = AccountDAO.Instance.UpdateFields(updateData, conditions);
            if (updateResult > 0)
            {
                ErrorMessage.Foreground = System.Windows.Media.Brushes.Green;
                ErrorMessage.Text = "Đặt lại mật khẩu thành công!";
                ErrorMessage.Visibility = Visibility.Visible;

                // Xóa token hiện tại để tránh đăng nhập tự động
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.Hide(); // Ẩn thay vì đóng ngay
                    new SignIn().Show();
                    this.Close(); // Đóng sau khi form mới hiển thị
                };
                timer.Start();
            }
            else
            {
                ErrorMessage.Text = "Lỗi khi đặt lại mật khẩu. Vui lòng thử lại!";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Xóa token hoặc session
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Ẩn form hiện tại trước, sau đó hiển thị SignIn và đóng
            this.Hide();
            SignIn signInForm = new SignIn();
            signInForm.Show();
            this.Close();
        }
    }
}