using ChatApp.Client.Services;
using ChatApp.Common.DAO;
using ChatApp.Common.DTOs;
using ChatApp.Client.Hub;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Windows.Forms;


namespace ChatApp.Client.Views
{
    public partial class ChatRoom : Form
    {
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private UserDTO _user;
        private StatusAccountHub _statusHub;
        private ChatOneOnOneHub _chatOneOnOneHub;
        private NotificationHub _notificationHub;
        private UserService _userService;
        private CircularPictureBoxService _circularPictureBoxService;
        private List<MessageDTO> _messages;
        public ChatRoom(string fromEmail, string toEmail)
        {
            InitializeComponent();

            _fromEmail = fromEmail;
            _toEmail = toEmail;

            // Initialize services
            _userService = new UserService();
            _circularPictureBoxService = new CircularPictureBoxService();

            // Start the socket hub
            _statusHub = new StatusAccountHub();
            _chatOneOnOneHub = new ChatOneOnOneHub(_fromEmail);
            _notificationHub = new NotificationHub(_fromEmail);

            // Load the toUser data
            _user = AccountDAO.Instance.GetUserInfoByEmail(_toEmail);

            // UI initialization
            lbName.Text = _user.FullName;
            lbStatus.Text = _user.Status;
            lbStatus.ForeColor = _user.Status == "online" ? Color.Green : Color.Red;
            lbStatus.Text = _user.Status == "online" ? "Online" : "Offline";
            string defaultAvatarUrl = "https://miamistonesource.com/wp-content/uploads/2018/05/no-avatar-25359d55aa3c93ab3466622fd2ce712d1.jpg";
            string avatarUrl = string.IsNullOrEmpty(_user.AvatarUrl) ? defaultAvatarUrl : _user.AvatarUrl;
            ptbAvatar.LoadAsync(avatarUrl);

            ptbAvatar.LoadCompleted += (s, e) =>
            {
                _circularPictureBoxService.MakePictureBoxCircular(ptbAvatar);
            };
        }

        // Handling update status for to user
        private void UpdateToUserStatus(string email, string status)
        {
            if (email == _toEmail)
            {
                lbStatus.Text = status;
                lbStatus.ForeColor = status == "online" ? Color.Green : Color.Red;
                lbStatus.Text = status == "online" ? "Online" : "Offline";
            }
        }

        // Closing the chat room
        private async void ChatRoom_FormClosing(object sender, FormClosingEventArgs e)
        {
            // tắt kết nối socket
            _userService.SetOfflineStatusInDB(_fromEmail);
            if (_statusHub != null)
            {
                await _statusHub.SetOffline(_fromEmail);
                await _statusHub.DisconnectAsync();
            }
        }

        // Loading the chat room
        private async void ChatRoom_Load(object sender, EventArgs e)
        {
            await _statusHub.SetOnline(_fromEmail);
            _userService.SetOnlineStatusInDB(_fromEmail);

            // đợi phản hồi từ server nếu có bạn thay đổi trạng thái on off
            await _statusHub.ConnectAsync((friendEmail, status) =>
            {
                UpdateToUserStatus(friendEmail, status);
            });

            // đợi phản hồi từ server nếu có ai đó gửi tin nhắn
            await _chatOneOnOneHub.ConnectAsync((senderId, data, messageType) =>
            {
                if (senderId == _fromEmail) return;
                if (data == null) return;
                MessageBox.Show($"From: {senderId}\nType: {messageType}\nData: {data}", "New Message");

                if (messageType == "text")
                {
                    string message = data;
                }
                else if (messageType == "image")
                {
                    string imageUrl = data;
                }
                else if (messageType == "file")
                {
                    string fileUrl = data;
                }
            });

            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                MessageBox.Show($"From: {senderEmail}\nType: {messageType}\nMessage: {message}");
            });
        }

        //nút gửi tin nhắn dạng text
        private async void btnSendMess_Click(object sender, EventArgs e)
        {
            string message = tbMess.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await _chatOneOnOneHub.SendMessageAsync(_toEmail, data, "text");
                await _notificationHub.SendNotification(_fromEmail ,[_toEmail], "Có tin nhắn mới", "message");
            }
        }

        // nút gửi ảnh
        private async void btnUploadImg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageBytes = File.ReadAllBytes(filePath);

                    /*gửi ảnh lên server để xử lý (dạng byte)*/
                    await _chatOneOnOneHub.SendMessageAsync(_toEmail, imageBytes, "image");
                    await _notificationHub.SendNotification(_fromEmail ,[_toEmail], "Có tin nhắn mới", "message");
                }
            }
        }

        //nút gủi file
        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Chọn file để gửi";
                openFileDialog.Filter = "Tất cả các file|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(filePath);
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    try
                    {
                        /*Gửi file lên server để xử lý (dạng byte)*/
                        await _chatOneOnOneHub.SendMessageAsync(_toEmail, fileBytes, "file", fileName);
                        await _notificationHub.SendNotification(_fromEmail ,[_toEmail], "Có tin nhắn mới", "message");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi gửi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
