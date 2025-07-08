using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatApp.Common.DAO;
using ChatApp.Common;

namespace ChatApp.Client.Views
{
    public partial class ForgotPassword : Window
    {
        private bool isEmailSent = false;

        public ForgotPassword()
        {
            InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Ngăn chặn nhấp đúp ngay lập tức
            if (isEmailSent || !SubmitButton.IsEnabled) return;

            SubmitButton.IsEnabled = false; // Vô hiệu hóa ngay khi nhấp

            string email = EmailTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage.Text = "Vui lòng nhập email!";
                ErrorMessage.Visibility = Visibility.Visible;
                SubmitButton.IsEnabled = true; // Kích hoạt lại nếu lỗi
                return;
            }

            // Kiểm tra email tồn tại
            if (!AccountDAO.Instance.CheckExistEmail(email))
            {
                ErrorMessage.Text = "Email không tồn tại!";
                ErrorMessage.Visibility = Visibility.Visible;
                SubmitButton.IsEnabled = true; // Kích hoạt lại nếu lỗi
                return;
            }

            // Tạo mã xác minh
            string verificationCode = GenerateRandomCode.CreateCode();

            // Lưu mã vào cơ sở dữ liệu
            var updateData = new Dictionary<string, object>
            {
                { "verify_code", verificationCode }
            };
            var conditions = new Dictionary<string, object>
            {
                { "email", email }
            };

            int updateResult = AccountDAO.Instance.UpdateFields(updateData, conditions);
            if (updateResult <= 0)
            {
                ErrorMessage.Text = "Lỗi khi lưu mã xác minh!";
                ErrorMessage.Visibility = Visibility.Visible;
                SubmitButton.IsEnabled = true; // Kích hoạt lại nếu lỗi
                return;
            }

            // Gửi email xác minh với timeout rõ ràng
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    client.Timeout = TimeSpan.FromSeconds(10); // Đặt timeout rõ ràng
                    var request = new
                    {
                        Email = email,
                        VerifyCode = verificationCode
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Console.WriteLine($"Sending request with code: {verificationCode} at {DateTime.Now}");
                    var response = await client.PostAsync("email/send/verifycode", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ErrorMessage.Foreground = System.Windows.Media.Brushes.Green;
                        ErrorMessage.Text = "Mã xác minh đã được gửi!";
                        ErrorMessage.Visibility = Visibility.Visible;
                        isEmailSent = true;

                        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        timer.Tick += (s, args) =>
                        {
                            timer.Stop();
                            this.Close();
                            new VerifyForgotPassword(verificationCode, email).Show();
                        };
                        timer.Start();
                    }
                    else
                    {
                        ErrorMessage.Text = $"Không thể gửi email. Mã lỗi: {response.StatusCode}";
                        ErrorMessage.Visibility = Visibility.Visible;
                        SubmitButton.IsEnabled = true; // Kích hoạt lại nếu thất bại
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Lỗi gửi email: {ex.Message}";
                ErrorMessage.Visibility = Visibility.Visible;
                SubmitButton.IsEnabled = true; // Kích hoạt lại nếu lỗi
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Mở SignIn trước, sau đó đóng form hiện tại
            SignIn signInForm = new SignIn();
            signInForm.Show();
            this.Close();
        }
    }
}