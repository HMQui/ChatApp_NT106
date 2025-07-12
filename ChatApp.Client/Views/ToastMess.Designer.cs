namespace ChatApp.Client.Views
{
    partial class ToastMess
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
            components = new System.ComponentModel.Container();
            panel1 = new Panel();
            toastTimer = new System.Windows.Forms.Timer(components);
            toastHidden = new System.Windows.Forms.Timer(components);
            title = new Label();
            mess = new Label();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(72, 187, 120);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(30, 129);
            panel1.TabIndex = 0;
            // 
            // toastTimer
            // 
            toastTimer.Enabled = true;
            toastTimer.Interval = 10;
            toastTimer.Tick += toastTimer_Tick;
            // 
            // toastHidden
            // 
            toastHidden.Interval = 10;
            toastHidden.Tick += toastHidden_Tick;
            // 
            // title
            // 
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 16F);
            title.Location = new Point(49, 22);
            title.Name = "title";
            title.Size = new Size(90, 37);
            title.TabIndex = 3;
            title.Text = "label1";
            // 
            // mess
            // 
            mess.AutoSize = true;
            mess.Font = new Font("Segoe UI", 12F);
            mess.Location = new Point(49, 73);
            mess.Name = "mess";
            mess.Size = new Size(65, 28);
            mess.TabIndex = 4;
            mess.Text = "label1";
            // 
            // ToastMess
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(482, 126);
            Controls.Add(mess);
            Controls.Add(title);
            Controls.Add(panel1);
            ForeColor = Color.Black;
            FormBorderStyle = FormBorderStyle.None;
            Name = "ToastMess";
            Text = "ToastMess";
            Load += ToastMess_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private System.Windows.Forms.Timer toastTimer;
        private System.Windows.Forms.Timer toastHidden;
        private Label title;
        private Label mess;
    }
}