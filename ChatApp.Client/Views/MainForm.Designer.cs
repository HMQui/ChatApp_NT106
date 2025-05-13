namespace ChatApp.Client.Views
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            panel1 = new Panel();
            btnLogout = new Button();
            ptbAvatar = new PictureBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            pnUsers = new Panel();
            pnSearch = new Panel();
            btnSearch = new Button();
            tbSearch = new TextBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            label1 = new Label();
            pnFriendList = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ptbAvatar).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            pnSearch.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(69, 165, 117);
            panel1.Controls.Add(btnLogout);
            panel1.Controls.Add(ptbAvatar);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(127, 589);
            panel1.TabIndex = 0;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.Red;
            btnLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(15, 518);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(93, 39);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Đăng xuất";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // ptbAvatar
            // 
            ptbAvatar.BackgroundImage = (Image)resources.GetObject("ptbAvatar.BackgroundImage");
            ptbAvatar.BackgroundImageLayout = ImageLayout.Zoom;
            ptbAvatar.Cursor = Cursors.Hand;
            ptbAvatar.Location = new Point(28, 36);
            ptbAvatar.Name = "ptbAvatar";
            ptbAvatar.Size = new Size(68, 68);
            ptbAvatar.TabIndex = 0;
            ptbAvatar.TabStop = false;
            ptbAvatar.WaitOnLoad = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36.02941F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63.97059F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(127, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(815, 589);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(pnUsers, 0, 1);
            tableLayoutPanel2.Controls.Add(pnSearch, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(4, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 17.753624F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 82.246376F));
            tableLayoutPanel2.Size = new Size(286, 581);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // pnUsers
            // 
            pnUsers.Dock = DockStyle.Fill;
            pnUsers.Location = new Point(3, 106);
            pnUsers.Name = "pnUsers";
            pnUsers.Size = new Size(280, 472);
            pnUsers.TabIndex = 1;
            // 
            // pnSearch
            // 
            pnSearch.Controls.Add(btnSearch);
            pnSearch.Controls.Add(tbSearch);
            pnSearch.Dock = DockStyle.Fill;
            pnSearch.Location = new Point(3, 3);
            pnSearch.Name = "pnSearch";
            pnSearch.Size = new Size(280, 97);
            pnSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Right;
            btnSearch.BackgroundImage = (Image)resources.GetObject("btnSearch.BackgroundImage");
            btnSearch.BackgroundImageLayout = ImageLayout.Zoom;
            btnSearch.ImageAlign = ContentAlignment.TopRight;
            btnSearch.Location = new Point(213, 31);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(43, 29);
            btnSearch.TabIndex = 1;
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // tbSearch
            // 
            tbSearch.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbSearch.Location = new Point(33, 29);
            tbSearch.Name = "tbSearch";
            tbSearch.PlaceholderText = "Tìm kiếm";
            tbSearch.Size = new Size(174, 27);
            tbSearch.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(pnFriendList, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(297, 4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 12.3924265F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 87.6075745F));
            tableLayoutPanel3.Size = new Size(514, 581);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI Black", 19.8000011F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 163);
            label1.ForeColor = Color.FromArgb(37, 37, 37);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(508, 72);
            label1.TabIndex = 0;
            label1.Text = "Bạn bè";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnFriendList
            // 
            pnFriendList.Dock = DockStyle.Fill;
            pnFriendList.Location = new Point(3, 75);
            pnFriendList.Name = "pnFriendList";
            pnFriendList.Size = new Size(508, 503);
            pnFriendList.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(942, 589);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(panel1);
            MinimumSize = new Size(960, 607);
            Name = "MainForm";
            Text = "Zola";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ptbAvatar).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            pnSearch.ResumeLayout(false);
            pnSearch.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox ptbAvatar;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel pnSearch;
        private Panel pnUsers;
        private TextBox tbSearch;
        private Button btnSearch;
        private TableLayoutPanel tableLayoutPanel3;
        private Label label1;
        private Panel pnFriendList;
        private Button btnLogout;
    }
}