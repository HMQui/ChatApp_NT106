using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;
using ChatApp.Common.DAO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ChatApp.Common;
using System.Net.WebSockets;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatApp.Client.Views
{
    public partial class VerifySignUp : Form
    {
        private string correctCode;
        private string inputCode = "";
        private string emailAddress;
        private int countdownSeconds = 300;
        private System.Windows.Forms.Timer countdownTimer;
        private RegisterForm registerForm;
        private List<TextBox> codeBoxes;
        private static readonly string secretKey = "6DOKMbMsMPPDBTLjdZAlEcFOktrQL7Yz";
        public VerifySignUp(string verificationCode, string email, RegisterForm parentForm)
        {
            InitializeComponent();
            correctCode = verificationCode;
            emailAddress = email;
            registerForm = parentForm;

            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            UpdateCountdownLabel();
        }

        // Call api /email/send/verifycode
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

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Verification email sent successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send email. StatusCode: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}");
            }
        }

        private bool DeleteAccountFromDB(Dictionary<string, object> conditions)
        {
            int result = AccountDAO.Instance.DeleteByConditions(conditions);
            return result == 1;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            UpdateCountdownLabel();

            if (countdownSeconds <= 0)
            {
                var conditions = new Dictionary<string, object>
                {
                    { "email", emailAddress }
                };
                DeleteAccountFromDB(conditions);
                countdownTimer.Stop();
                this.Hide();
                registerForm.ShowDialog();
                this.Close();
            }
        }

        private void UpdateCountdownLabel()
        {
            int minutes = countdownSeconds / 60;
            int seconds = countdownSeconds % 60;
            lbCountDown.Text = $"{minutes:D2}:{seconds:D2}";
        }

        //Send verify code for the first time loading
        private async void VerifySignUp_Load(object sender, EventArgs e)
        {
            await SendVerifyCodeEmailAsync();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void VerifySignUp_FormClosing(object sender, FormClosingEventArgs e)
        {
            var conditions = new Dictionary<string, object>
            {
                { "email", emailAddress },
                { "is_verified", 0 }
            };
            DeleteAccountFromDB(conditions);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            var conditions = new Dictionary<string, object>
            {
                { "email", emailAddress },
                { "is_verified", 0 }
            };
            DeleteAccountFromDB(conditions);
            RegisterForm registerForm = new RegisterForm();
            this.Hide();
            registerForm.ShowDialog();
            this.Close();
        }

        private void tbVerifyCode_TextChanged(object sender, EventArgs e)
        {
            if (tbVerifyCode.Text.Length >= 6)
            {
                if (tbVerifyCode.Text == correctCode)
                {
                    tbVerifyCode.Enabled = false;
                    lbHelpText.ForeColor = Color.Green;
                    var updateFields = new Dictionary<string, object>
                    {
                        { "is_verified", 1 }
                    };
                    var conditions = new Dictionary<string, object>
                    {
                        { "email", emailAddress }
                    };
                    AccountDAO.Instance.UpdateFields(updateFields, conditions);
                    lbHelpText.Text = "Đăng ký thành công!";
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 2000;
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        this.Hide();
                        string token = GenerateToken(emailAddress);
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Services\auth.txt");
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        using (StreamWriter writer = new StreamWriter(file))
                        {
                            writer.Write(token);
                        }
                        using (MainForm mainForm = new MainForm(emailAddress))
                        {
                            mainForm.ShowDialog();
                        }
                        this.Close();
                    };
                    timer.Start();
                }
                else
                {
                    lbHelpText.ForeColor = Color.Red;
                    lbHelpText.Text = "Mã xác thực không chính xác!";
                }
            }
            else
            {
                lbHelpText.ForeColor = Color.Red;
                lbHelpText.Text = "";
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
        private async void llbReSend_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
            AccountDAO.Instance.UpdateFields(updateData, conditions);
            await SendVerifyCodeEmailAsync();
            countdownTimer.Stop();
            countdownSeconds = 300;
            UpdateCountdownLabel();
            countdownTimer.Start();
        }
    }
}
