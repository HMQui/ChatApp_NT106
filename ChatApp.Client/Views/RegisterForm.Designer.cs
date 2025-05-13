namespace ChatApp.Client.Views
{
    partial class RegisterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterForm));
            pictureBox1 = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            tbEmail = new TextBox();
            tbPass = new TextBox();
            btnInVisiblePass = new Button();
            btnVisiblePass = new Button();
            tbRpPass = new TextBox();
            btnVisibleRpPass = new Button();
            btnInVisibleRpPass = new Button();
            btnSignUp = new Button();
            btnBack = new Button();
            lbHelpText = new Label();
            label5 = new Label();
            tbName = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(69, 165, 117);
            pictureBox1.Location = new Point(-1, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(184, 451);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.Location = new Point(402, 64);
            label1.Name = "label1";
            label1.Size = new Size(151, 46);
            label1.TabIndex = 1;
            label1.Text = "Đăng ký";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(281, 167);
            label2.Name = "label2";
            label2.Size = new Size(46, 20);
            label2.TabIndex = 2;
            label2.Text = "Email";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(281, 215);
            label3.Name = "label3";
            label3.Size = new Size(70, 20);
            label3.TabIndex = 3;
            label3.Text = "Mật khẩu";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(281, 262);
            label4.Name = "label4";
            label4.Size = new Size(130, 20);
            label4.TabIndex = 4;
            label4.Text = "Nhập lại mật khẩu";
            // 
            // tbEmail
            // 
            tbEmail.Location = new Point(417, 164);
            tbEmail.Name = "tbEmail";
            tbEmail.Size = new Size(246, 27);
            tbEmail.TabIndex = 5;
            tbEmail.TextChanged += textBox1_TextChanged;
            // 
            // tbPass
            // 
            tbPass.Location = new Point(417, 208);
            tbPass.Name = "tbPass";
            tbPass.PasswordChar = '*';
            tbPass.Size = new Size(246, 27);
            tbPass.TabIndex = 6;
            tbPass.TextChanged += tbPass_TextChanged;
            // 
            // btnInVisiblePass
            // 
            btnInVisiblePass.BackgroundImageLayout = ImageLayout.Stretch;
            btnInVisiblePass.Image = (Image)resources.GetObject("btnInVisiblePass.Image");
            btnInVisiblePass.Location = new Point(625, 206);
            btnInVisiblePass.Name = "btnInVisiblePass";
            btnInVisiblePass.Size = new Size(41, 29);
            btnInVisiblePass.TabIndex = 8;
            btnInVisiblePass.Text = "v";
            btnInVisiblePass.UseVisualStyleBackColor = true;
            btnInVisiblePass.Click += btnInVisiblePass_Click;
            // 
            // btnVisiblePass
            // 
            btnVisiblePass.BackgroundImageLayout = ImageLayout.None;
            btnVisiblePass.Image = (Image)resources.GetObject("btnVisiblePass.Image");
            btnVisiblePass.Location = new Point(625, 206);
            btnVisiblePass.Name = "btnVisiblePass";
            btnVisiblePass.Size = new Size(41, 29);
            btnVisiblePass.TabIndex = 9;
            btnVisiblePass.UseVisualStyleBackColor = true;
            btnVisiblePass.Click += btnVisiblePass_Click;
            // 
            // tbRpPass
            // 
            tbRpPass.Location = new Point(417, 255);
            tbRpPass.Name = "tbRpPass";
            tbRpPass.PasswordChar = '*';
            tbRpPass.Size = new Size(246, 27);
            tbRpPass.TabIndex = 10;
            tbRpPass.TextChanged += tbRpPass_TextChanged;
            // 
            // btnVisibleRpPass
            // 
            btnVisibleRpPass.BackgroundImageLayout = ImageLayout.None;
            btnVisibleRpPass.Image = (Image)resources.GetObject("btnVisibleRpPass.Image");
            btnVisibleRpPass.Location = new Point(625, 255);
            btnVisibleRpPass.Name = "btnVisibleRpPass";
            btnVisibleRpPass.Size = new Size(41, 29);
            btnVisibleRpPass.TabIndex = 12;
            btnVisibleRpPass.UseVisualStyleBackColor = true;
            btnVisibleRpPass.Click += btnVisibleRpPass_Click;
            // 
            // btnInVisibleRpPass
            // 
            btnInVisibleRpPass.BackgroundImageLayout = ImageLayout.Stretch;
            btnInVisibleRpPass.Image = (Image)resources.GetObject("btnInVisibleRpPass.Image");
            btnInVisibleRpPass.Location = new Point(625, 255);
            btnInVisibleRpPass.Name = "btnInVisibleRpPass";
            btnInVisibleRpPass.Size = new Size(41, 29);
            btnInVisibleRpPass.TabIndex = 11;
            btnInVisibleRpPass.Text = "v";
            btnInVisibleRpPass.UseVisualStyleBackColor = true;
            btnInVisibleRpPass.Click += btnInVisibleRpPass_Click;
            // 
            // btnSignUp
            // 
            btnSignUp.BackColor = Color.LightCoral;
            btnSignUp.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnSignUp.ForeColor = Color.White;
            btnSignUp.Location = new Point(417, 373);
            btnSignUp.Name = "btnSignUp";
            btnSignUp.Size = new Size(212, 47);
            btnSignUp.TabIndex = 13;
            btnSignUp.Text = "Đăng ký";
            btnSignUp.UseVisualStyleBackColor = false;
            btnSignUp.Click += btnSignUp_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(667, 391);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(94, 29);
            btnBack.TabIndex = 14;
            btnBack.Text = "Trở về";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // lbHelpText
            // 
            lbHelpText.AutoSize = true;
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Location = new Point(369, 347);
            lbHelpText.Name = "lbHelpText";
            lbHelpText.Size = new Size(0, 20);
            lbHelpText.TabIndex = 15;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(281, 313);
            label5.Name = "label5";
            label5.Size = new Size(73, 20);
            label5.TabIndex = 16;
            label5.Text = "Họ và tên";
            // 
            // tbName
            // 
            tbName.Location = new Point(417, 310);
            tbName.Name = "tbName";
            tbName.Size = new Size(246, 27);
            tbName.TabIndex = 17;
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            Controls.Add(tbName);
            Controls.Add(label5);
            Controls.Add(lbHelpText);
            Controls.Add(btnBack);
            Controls.Add(btnSignUp);
            Controls.Add(btnVisibleRpPass);
            Controls.Add(tbRpPass);
            Controls.Add(btnVisiblePass);
            Controls.Add(btnInVisiblePass);
            Controls.Add(tbPass);
            Controls.Add(tbEmail);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Controls.Add(btnInVisibleRpPass);
            ForeColor = Color.FromArgb(37, 37, 37);
            Name = "RegisterForm";
            Text = "Đăng ký";
            Load += RegisterForm_Load_1;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox tbEmail;
        private TextBox tbPass;
        private Button btnInVisiblePass;
        private Button btnVisiblePass;
        private TextBox tbRpPass;
        private Button btnVisibleRpPass;
        private Button btnInVisibleRpPass;
        private Button btnSignUp;
        private Button btnBack;
        private Label lbHelpText;
        private Label label5;
        private TextBox tbName;
    }
}