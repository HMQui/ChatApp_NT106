using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace ChatApp.Client.Views
{
    public partial class CreateGroup : Window
    {
        private string _creator;
        private string _selectedImagePath = null;

        public CreateGroup(string email)
        {
            InitializeComponent();
            _creator = email;

            SelectImageButton.Click += SelectImageButton_Click;
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dlg.ShowDialog() == true)
            {
                _selectedImagePath = dlg.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    GroupImagePreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tải ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            string groupName = GroupNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedImagePath) || !File.Exists(_selectedImagePath))
            {
                MessageBox.Show("Vui lòng chọn ảnh đại diện cho nhóm!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new StringContent(_creator), "creatorEmail");
                    form.Add(new StringContent(groupName), "groupName");

                    if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
                    {
                        var imageBytes = File.ReadAllBytes(_selectedImagePath);
                        var imageContent = new ByteArrayContent(imageBytes);
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                        form.Add(imageContent, "avatar", System.IO.Path.GetFileName(_selectedImagePath));
                    }

                    // Gửi request POST
                    string apiUrl = "http://localhost:5000/api/groups/create";
                    var response = await client.PostAsync(apiUrl, form);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    this.DialogResult = true;
                    MessageBox.Show("Nhóm đã được tạo thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tạo nhóm:\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
