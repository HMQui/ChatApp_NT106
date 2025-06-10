using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatApp.Common.DAO;
using ChatApp.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatApp.Client.Views
{
    public partial class VerifyForgotPassword : Window
    {
        private string correctCode;
        private string emailAddress;
        private int countdownSeconds = 300; // 5 phút
        private DispatcherTimer countdownTimer;
        private static readonly string secretKey = "6DOKMbMsMPPDBTLjdZAlEcFOktrQL7Yz";

        public VerifyForgotPassword(string verificationCode, string email)
        {
            InitializeComponent();
            correctCode = verificationCode;
            emailAddress = email;

            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            UpdateCountdownLabel();
            Loaded += VerifyForgotPassword_Load;
        }

        private async void VerifyForgotPassword_Load(object sender, RoutedEventArgs e)
        {
            //await SendVerifyCodeEmailAsync();
        }

        private async Task SendVerifyCodeEmailAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5000/");
                    var request = new
                    {
                        Email = emailAddress,
                        VerifyCode = correctCode
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("email/send/verifycode", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        ErrorMessage.Text = "Không thể gửi mã xác minh. Vui lòng thử lại.";
                        ErrorMessage.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Lỗi gửi email: {ex.Message}";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            UpdateCountdownLabel();

            if (countdownSeconds <= 0)
            {
                countdownTimer.Stop();
                this.Close();
                new ForgotPassword().Show();
            }
        }

        private void UpdateCountdownLabel()
        {
            int minutes = countdownSeconds / 60;
            int seconds = countdownSeconds % 60;
            CountdownLabel.Text = $"{minutes:D2}:{seconds:D2}";
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyCodeTextBox.Text.Length >= 6)
            {
                if (VerifyCodeTextBox.Text == correctCode)
                {
                    VerifyCodeTextBox.IsEnabled = false;
                    ErrorMessage.Foreground = System.Windows.Media.Brushes.Green;
                    ErrorMessage.Text = "Xác minh thành công!";
                    ErrorMessage.Visibility = Visibility.Visible;

                    // Tạo token trước
                    string token = GenerateToken(emailAddress);
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllText(filePath, token);

                    // Mở form ResetPassword trước khi đóng form hiện tại
                    ResetPassword resetPasswordForm = new ResetPassword(emailAddress);
                    resetPasswordForm.Show();
                    this.Close();
                }
                else
                {
                    ErrorMessage.Foreground = System.Windows.Media.Brushes.Red;
                    ErrorMessage.Text = "Mã xác minh không đúng!";
                    ErrorMessage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessage.Foreground = System.Windows.Media.Brushes.Red;
                ErrorMessage.Text = "Vui lòng nhập mã 6 chữ số!";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private async void ResendLink_Click(object sender, RoutedEventArgs e)
        {
            string newCode = GenerateRandomCode.CreateCode();
            correctCode = newCode;

            var updateData = new Dictionary<string, object>
            {
                { "verify_code", newCode }
            };
            var conditions = new Dictionary<string, object>
            {
                { "email", emailAddress }
            };
            int updateResult = AccountDAO.Instance.UpdateFields(updateData, conditions);
            if (updateResult > 0)
            {
                await SendVerifyCodeEmailAsync();
                countdownTimer.Stop();
                countdownSeconds = 300;
                UpdateCountdownLabel();
                countdownTimer.Start();
                ErrorMessage.Text = "Mã mới đã được gửi!";
                ErrorMessage.Foreground = System.Windows.Media.Brushes.Green;
                ErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorMessage.Text = "Lỗi khi cập nhật mã xác minh!";
                ErrorMessage.Foreground = System.Windows.Media.Brushes.Red;
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SignIn signInForm = new SignIn();
            signInForm.Show();
            this.Close();
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
    }
}