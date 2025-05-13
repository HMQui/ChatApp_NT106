namespace ChatApp.Client.Views
{
    partial class VerifySignUp
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
            label1 = new Label();
            label2 = new Label();
            lbCountDown = new Label();
            btnBack = new Button();
            tbVerifyCode = new TextBox();
            lbHelpText = new Label();
            llbReSend = new LinkLabel();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 163);
            label1.ForeColor = Color.FromArgb(37, 37, 37);
            label1.Location = new Point(273, 58);
            label1.Name = "label1";
            label1.Size = new Size(258, 46);
            label1.TabIndex = 0;
            label1.Text = "Xác thực email";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(140, 231);
            label2.Name = "label2";
            label2.Size = new Size(480, 23);
            label2.TabIndex = 1;
            label2.Text = "Mã xác nhận đã được gửi về email của bạn, và sẽ hết hạn sau";
            // 
            // lbCountDown
            // 
            lbCountDown.AutoSize = true;
            lbCountDown.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lbCountDown.Location = new Point(626, 226);
            lbCountDown.Name = "lbCountDown";
            lbCountDown.Size = new Size(0, 31);
            lbCountDown.TabIndex = 2;
            lbCountDown.UseCompatibleTextRendering = true;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(642, 352);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(94, 29);
            btnBack.TabIndex = 3;
            btnBack.Text = "Trở về";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // tbVerifyCode
            // 
            tbVerifyCode.Font = new Font("Segoe UI", 20F);
            tbVerifyCode.Location = new Point(229, 146);
            tbVerifyCode.Name = "tbVerifyCode";
            tbVerifyCode.PlaceholderText = "Nhập mã xác thực";
            tbVerifyCode.Size = new Size(360, 52);
            tbVerifyCode.TabIndex = 4;
            tbVerifyCode.TextChanged += tbVerifyCode_TextChanged;
            // 
            // lbHelpText
            // 
            lbHelpText.AutoSize = true;
            lbHelpText.ForeColor = Color.Red;
            lbHelpText.Location = new Point(229, 211);
            lbHelpText.Name = "lbHelpText";
            lbHelpText.Size = new Size(0, 20);
            lbHelpText.TabIndex = 5;
            // 
            // llbReSend
            // 
            llbReSend.AutoSize = true;
            llbReSend.Font = new Font("Segoe UI", 12F);
            llbReSend.Location = new Point(485, 276);
            llbReSend.Name = "llbReSend";
            llbReSend.Size = new Size(72, 28);
            llbReSend.TabIndex = 6;
            llbReSend.TabStop = true;
            llbReSend.Text = "Gửi lại.";
            llbReSend.LinkClicked += llbReSend_LinkClicked;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(229, 276);
            label3.Name = "label3";
            label3.Size = new Size(250, 28);
            label3.TabIndex = 7;
            label3.Text = "Bạn chưa nhận được email?";
            // 
            // VerifySignUp
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(label3);
            Controls.Add(llbReSend);
            Controls.Add(lbHelpText);
            Controls.Add(tbVerifyCode);
            Controls.Add(btnBack);
            Controls.Add(lbCountDown);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "VerifySignUp";
            Text = "VerifySignUp";
            FormClosing += VerifySignUp_FormClosing;
            Load += VerifySignUp_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label lbCountDown;
        private Button btnBack;
        private TextBox tbVerifyCode;
        private Label lbHelpText;
        private LinkLabel llbReSend;
        private Label label3;
    }
}