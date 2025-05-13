namespace ChatApp.Client.Views
{
    partial class ChatRoom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatRoom));
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnSendMess = new Button();
            panel2 = new Panel();
            tbMess = new TextBox();
            panel1 = new Panel();
            lbStatus = new Label();
            lbName = new Label();
            ptbAvatar = new PictureBox();
            pnMess = new Panel();
            panel3 = new Panel();
            btnUploadImg = new Button();
            btnSendFile = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ptbAvatar).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(pnMess, 0, 1);
            tableLayoutPanel1.Controls.Add(panel3, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 21.2962971F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 78.7037048F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 67F));
            tableLayoutPanel1.Size = new Size(972, 472);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 89.45421F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10.5457907F));
            tableLayoutPanel2.Controls.Add(btnSendMess, 1, 0);
            tableLayoutPanel2.Controls.Add(panel2, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Font = new Font("Segoe UI", 8.25F);
            tableLayoutPanel2.Location = new Point(4, 406);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(964, 62);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // btnSendMess
            // 
            btnSendMess.BackColor = Color.FromArgb(69, 165, 117);
            btnSendMess.Dock = DockStyle.Fill;
            btnSendMess.Font = new Font("Segoe UI", 12F);
            btnSendMess.ForeColor = Color.WhiteSmoke;
            btnSendMess.Location = new Point(865, 3);
            btnSendMess.Name = "btnSendMess";
            btnSendMess.Size = new Size(96, 56);
            btnSendMess.TabIndex = 1;
            btnSendMess.Text = "Gửi";
            btnSendMess.UseVisualStyleBackColor = false;
            btnSendMess.Click += btnSendMess_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(tbMess);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(856, 56);
            panel2.TabIndex = 2;
            // 
            // tbMess
            // 
            tbMess.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbMess.Font = new Font("Segoe UI", 12F);
            tbMess.Location = new Point(0, 11);
            tbMess.Name = "tbMess";
            tbMess.Size = new Size(856, 34);
            tbMess.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(lbStatus);
            panel1.Controls.Add(lbName);
            panel1.Controls.Add(ptbAvatar);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(4, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(964, 71);
            panel1.TabIndex = 2;
            // 
            // lbStatus
            // 
            lbStatus.AutoSize = true;
            lbStatus.Location = new Point(149, 45);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(62, 20);
            lbStatus.TabIndex = 3;
            lbStatus.Text = "lbStatus";
            // 
            // lbName
            // 
            lbName.AutoSize = true;
            lbName.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lbName.ForeColor = Color.FromArgb(37, 37, 37);
            lbName.Location = new Point(149, 8);
            lbName.Name = "lbName";
            lbName.Size = new Size(108, 35);
            lbName.TabIndex = 2;
            lbName.Text = "lbName";
            // 
            // ptbAvatar
            // 
            ptbAvatar.Location = new Point(58, 8);
            ptbAvatar.Name = "ptbAvatar";
            ptbAvatar.Size = new Size(57, 57);
            ptbAvatar.TabIndex = 1;
            ptbAvatar.TabStop = false;
            // 
            // pnMess
            // 
            pnMess.AutoScroll = true;
            pnMess.Dock = DockStyle.Fill;
            pnMess.Location = new Point(4, 82);
            pnMess.Name = "pnMess";
            pnMess.Padding = new Padding(10);
            pnMess.Size = new Size(964, 282);
            pnMess.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.Controls.Add(btnSendFile);
            panel3.Controls.Add(btnUploadImg);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(4, 371);
            panel3.Name = "panel3";
            panel3.Size = new Size(964, 28);
            panel3.TabIndex = 4;
            // 
            // btnUploadImg
            // 
            btnUploadImg.FlatStyle = FlatStyle.Popup;
            btnUploadImg.Image = (Image)resources.GetObject("btnUploadImg.Image");
            btnUploadImg.Location = new Point(81, -3);
            btnUploadImg.Name = "btnUploadImg";
            btnUploadImg.Size = new Size(34, 28);
            btnUploadImg.TabIndex = 1;
            btnUploadImg.UseVisualStyleBackColor = true;
            btnUploadImg.Click += btnUploadImg_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(149, 0);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(94, 29);
            btnSendFile.TabIndex = 2;
            btnSendFile.Text = "Gửi File";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // ChatRoom
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(972, 472);
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(856, 519);
            Name = "ChatRoom";
            Text = "ChatRoom";
            Load += ChatRoom_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ptbAvatar).EndInit();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btnSendMess;
        private PictureBox ptbAvatar;
        private Panel panel1;
        private Label lbStatus;
        private Label lbName;
        private Panel pnMess;
        private Panel panel2;
        private TextBox tbMess;
        private Panel panel3;
        private Button btnUploadImg;
        private Button btnSendFile;
    }
}