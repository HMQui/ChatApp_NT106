namespace ChatApp.Client;

partial class LoginForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        label1 = new Label();
        label2 = new Label();
        tbEmail = new TextBox();
        tbPassword = new TextBox();
        label3 = new Label();
        label4 = new Label();
        btnVisiblePassword = new Button();
        btnLogIn = new Button();
        llbForgotPassword = new LinkLabel();
        panel1 = new Panel();
        btnSignUp = new Button();
        lbHelpText = new Label();
        btnLoginGG = new Button();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 30F, FontStyle.Bold, GraphicsUnit.Point, 163);
        label1.ForeColor = Color.FromArgb(69, 165, 117);
        label1.Location = new Point(220, 83);
        label1.Name = "label1";
        label1.Size = new Size(131, 67);
        label1.TabIndex = 0;
        label1.Text = "Zola";
        label1.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        label2.ForeColor = Color.FromArgb(0, 0, 64);
        label2.Location = new Point(174, 162);
        label2.Name = "label2";
        label2.Size = new Size(237, 28);
        label2.TabIndex = 1;
        label2.Text = "Đăng nhập với mật khẩu";
        label2.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // tbEmail
        // 
        tbEmail.Location = new Point(152, 231);
        tbEmail.Name = "tbEmail";
        tbEmail.PlaceholderText = "Email";
        tbEmail.Size = new Size(290, 27);
        tbEmail.TabIndex = 2;
        tbEmail.TextChanged += tbEmail_TextChanged;
        // 
        // tbPassword
        // 
        tbPassword.Location = new Point(152, 297);
        tbPassword.Name = "tbPassword";
        tbPassword.PasswordChar = '*';
        tbPassword.PlaceholderText = "Password";
        tbPassword.Size = new Size(290, 27);
        tbPassword.TabIndex = 3;
        tbPassword.TextChanged += tbPassword_TextChanged;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 10F);
        label3.Location = new Point(60, 239);
        label3.Name = "label3";
        label3.Size = new Size(51, 23);
        label3.TabIndex = 4;
        label3.Text = "Email";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 10F);
        label4.Location = new Point(60, 301);
        label4.Name = "label4";
        label4.Size = new Size(82, 23);
        label4.TabIndex = 5;
        label4.Text = "Mật khẩu";
        // 
        // btnVisiblePassword
        // 
        btnVisiblePassword.BackColor = Color.DimGray;
        btnVisiblePassword.ForeColor = SystemColors.ButtonFace;
        btnVisiblePassword.Location = new Point(448, 301);
        btnVisiblePassword.Name = "btnVisiblePassword";
        btnVisiblePassword.Size = new Size(52, 29);
        btnVisiblePassword.TabIndex = 6;
        btnVisiblePassword.Text = "Ẩn";
        btnVisiblePassword.UseVisualStyleBackColor = false;
        btnVisiblePassword.Click += btnVisiblePassword_Click;
        // 
        // btnLogIn
        // 
        btnLogIn.BackColor = Color.FromArgb(240, 255, 234);
        btnLogIn.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
        btnLogIn.Location = new Point(60, 369);
        btnLogIn.Name = "btnLogIn";
        btnLogIn.Size = new Size(440, 41);
        btnLogIn.TabIndex = 7;
        btnLogIn.Text = "Đăng nhập";
        btnLogIn.UseVisualStyleBackColor = false;
        btnLogIn.EnabledChanged += btnLogIn_EnabledChanged;
        btnLogIn.Click += btnLogIn_Click;
        // 
        // llbForgotPassword
        // 
        llbForgotPassword.AutoSize = true;
        llbForgotPassword.Location = new Point(60, 413);
        llbForgotPassword.Name = "llbForgotPassword";
        llbForgotPassword.Size = new Size(116, 20);
        llbForgotPassword.TabIndex = 9;
        llbForgotPassword.TabStop = true;
        llbForgotPassword.Text = "Quên mật khẩu?";
        // 
        // panel1
        // 
        panel1.BackColor = SystemColors.ControlDarkDark;
        panel1.Location = new Point(186, 442);
        panel1.MaximumSize = new Size(200, 1);
        panel1.MinimumSize = new Size(200, 2);
        panel1.Name = "panel1";
        panel1.Size = new Size(200, 2);
        panel1.TabIndex = 10;
        // 
        // btnSignUp
        // 
        btnSignUp.BackColor = Color.FromArgb(240, 255, 234);
        btnSignUp.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
        btnSignUp.Location = new Point(60, 523);
        btnSignUp.Name = "btnSignUp";
        btnSignUp.Size = new Size(440, 41);
        btnSignUp.TabIndex = 11;
        btnSignUp.Text = "Tạo tài khoản";
        btnSignUp.UseVisualStyleBackColor = false;
        btnSignUp.Click += btnSignUp_Click;
        // 
        // lbHelpText
        // 
        lbHelpText.AutoSize = true;
        lbHelpText.Font = new Font("Segoe UI", 10F);
        lbHelpText.ForeColor = Color.Red;
        lbHelpText.Location = new Point(152, 343);
        lbHelpText.Name = "lbHelpText";
        lbHelpText.Size = new Size(75, 23);
        lbHelpText.TabIndex = 12;
        lbHelpText.Text = "HelpText";
        lbHelpText.Visible = false;
        // 
        // btnLoginGG
        // 
        btnLoginGG.BackColor = Color.FromArgb(240, 255, 234);
        btnLoginGG.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
        btnLoginGG.Location = new Point(60, 462);
        btnLoginGG.Name = "btnLoginGG";
        btnLoginGG.Size = new Size(440, 41);
        btnLoginGG.TabIndex = 13;
        btnLoginGG.Text = "Đăng nhập với Google";
        btnLoginGG.UseVisualStyleBackColor = false;
        btnLoginGG.Click += btnLoginGG_Click;
        // 
        // LoginForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ButtonHighlight;
        ClientSize = new Size(581, 593);
        Controls.Add(btnLoginGG);
        Controls.Add(lbHelpText);
        Controls.Add(btnSignUp);
        Controls.Add(panel1);
        Controls.Add(llbForgotPassword);
        Controls.Add(btnLogIn);
        Controls.Add(btnVisiblePassword);
        Controls.Add(label4);
        Controls.Add(label3);
        Controls.Add(tbPassword);
        Controls.Add(tbEmail);
        Controls.Add(label2);
        Controls.Add(label1);
        Name = "LoginForm";
        RightToLeft = RightToLeft.No;
        Text = "Đăng nhập";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label label1;
    private Label label2;
    private TextBox tbEmail;
    private TextBox tbPassword;
    private Label label3;
    private Label label4;
    private Button btnVisiblePassword;
    private Button btnLogIn;
    private LinkLabel llbForgotPassword;
    private Panel panel1;
    private Button btnSignUp;
    private Label lbHelpText;
    private Button btnLoginGG;
}
