using ChatApp.Client.Views;
using ChatApp.Common.DAO;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ChatApp.Client;

public partial class LoginForm : Form
{
    private static readonly string secretKey = "6DOKMbMsMPPDBTLjdZAlEcFOktrQL7Yz";

    public LoginForm()
    {
        InitializeComponent();

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
                MessageBox.Show("Lỗi khi đọc file: " + ex.Message);
            }
        }
        /*else
        {
            MessageBox.Show("Không tìm thấy file auth.txt.");
        }*/

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
    private void btnVisiblePassword_Click(object sender, EventArgs e)
    {
        if (tbPassword.UseSystemPasswordChar)
        {
            tbPassword.UseSystemPasswordChar = false;
            btnVisiblePassword.Text = "Ẩn";
        }
        else
        {
            tbPassword.UseSystemPasswordChar = true;
            btnVisiblePassword.Text = "Hiện";
        }
    }

    private void btnLogIn_EnabledChanged(object sender, EventArgs e)
    {
        //if (string.IsNullOrWhiteSpace(tbPassword.Text) || string.IsNullOrWhiteSpace(tbEmail.Text))
        //{
        //    btnLogIn.Enabled = false;
        //}
        //else
        //{
        //    btnLogIn.Enabled = true;
        //}
    }

    private void btnLogIn_Click(object sender, EventArgs e)
    {
        string email = tbEmail.Text;
        string password = tbPassword.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            lbHelpText.Text = "Vui lòng nhập email và mật khẩu";
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Visible = true;
            return;
        }
        else if (!isValidEmail(email))
        {
            lbHelpText.Text = "Email không hợp lệ";
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Visible = true;
            return;
        }
        else if (password.Length < 6)
        {
            lbHelpText.Text = "Mật khẩu phải có ít nhất 6 ký tự";
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Visible = true;
            return;
        }

        if (CheckAccount(email, password))
        {
            HandleLoginSuccess(email);
        }
        else
        {
            lbHelpText.Text = "Email hoặc mật khẩu không đúng";
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Visible = true;
            return;
        }
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

    private void tbEmail_TextChanged(object sender, EventArgs e)
    {
        lbHelpText.Visible = false;
    }

    private void tbPassword_TextChanged(object sender, EventArgs e)
    {
        lbHelpText.Visible = false;
    }

    private void btnSignUp_Click(object sender, EventArgs e)
    {
        this.Hide();
        using (RegisterForm registerForm = new RegisterForm())
        {
            registerForm.ShowDialog();
        }
        this.Close();
    }

    private void btnLoginGG_Click(object sender, EventArgs e)
    {

    }
}
