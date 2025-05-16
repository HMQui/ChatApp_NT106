using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        private bool _isPasswordVisible;

        public static readonly DependencyProperty IsPasswordVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPasswordVisible),
                typeof(bool),
                typeof(SignIn),
                new PropertyMetadata(false));

        public bool IsPasswordVisible
        {
            get => (bool)GetValue(IsPasswordVisibleProperty);
            set => SetValue(IsPasswordVisibleProperty, value);
        }

        public SignIn()
        {
            InitializeComponent();
            // Synchronize PasswordBox and TextBox
            PasswordBoxControl.PasswordChanged += PasswordBoxControl_PasswordChanged;
            PasswordTextBoxControl.TextChanged += PasswordTextBoxControl_TextChanged;

            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            EmailTextBox.GotFocus += (s, e) => UpdateEmailPlaceholderVisibility();
            EmailTextBox.LostFocus += (s, e) => UpdateEmailPlaceholderVisibility();
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEmailPlaceholderVisibility();
        }

        private void UpdateEmailPlaceholderVisibility()
        {
            if (string.IsNullOrEmpty(EmailTextBox.Text) && !EmailTextBox.IsFocused)
            {
                PlaceholderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderPanel.Visibility = Visibility.Collapsed;
            }
        }
        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Chuyển sang hiển thị mật khẩu
                PasswordTextBoxControl.Text = PasswordBoxControl.Password;
                PasswordTextBoxControl.Visibility = Visibility.Visible;
                PasswordBoxControl.Visibility = Visibility.Collapsed;
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-eye-30.png"));
            }
            else
            {
                // Chuyển sang ẩn mật khẩu
                PasswordBoxControl.Password = PasswordTextBoxControl.Text;
                PasswordTextBoxControl.Visibility = Visibility.Collapsed;
                PasswordBoxControl.Visibility = Visibility.Visible;
                EyeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/icons8-hide-30.png"));
            }
        }

        private void PasswordBoxControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Cập nhật TextBox nếu đang ở chế độ hiển thị mật khẩu
            if (_isPasswordVisible)
            {
                PasswordTextBoxControl.Text = PasswordBoxControl.Password;
            }
        }

        private void PasswordTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            // Cập nhật PasswordBox để giữ đồng bộ
            if (_isPasswordVisible)
            {
                PasswordBoxControl.Password = PasswordTextBoxControl.Text;
            }

        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Ẩn placeholder khi focus
            PasswordPlaceholderPanel.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Hiển thị placeholder nếu không có nội dung
            if (sender == PasswordBoxControl && string.IsNullOrEmpty(PasswordBoxControl.Password))
            {
                PasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
            else if (sender == PasswordTextBoxControl && string.IsNullOrEmpty(PasswordTextBoxControl.Text))
            {
                PasswordPlaceholderPanel.Visibility = Visibility.Visible;
            }
        }
    }

    // Boolean Negation Converter
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return value;
        }
    }
   

   
}