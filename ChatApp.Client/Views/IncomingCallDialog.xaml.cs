using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for IncomingCallDialog.xaml
    /// </summary>
    public partial class IncomingCallDialog : Window
    {
        private NotificationHub _notificationHub;
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private DispatcherTimer _callTimer;
        private TimeSpan _callDuration = TimeSpan.Zero;
        private bool _isMicOn = true;
        private WaveInEvent _waveIn;
        private VoiceCallHub _voiceHub;
        private bool _isInCall = false;
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;

        public IncomingCallDialog(string fromEmail, string toEmail)
        {
            InitializeComponent();

            _fromEmail = fromEmail;
            _toEmail = toEmail;

            // Start the socket hub
            _notificationHub = new NotificationHub(_toEmail);
        }

        private async void form_loading(object sender, EventArgs e)
        {
            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "cancel_voice_call")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
                else if (messageType == "end_voice_call") this.Close();
            });
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _voiceHub?.DisposeAsync();
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _bufferedWaveProvider = null;
            _isInCall = false;
            _callTimer?.Stop();
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "voice call", "end_voice_call");
            if (_notificationHub != null)
            {
                await _notificationHub.DisconnectAsync();
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "voice call", "accept_voice_call");

            // Ẩn UI gọi đến, hiện UI đang gọi
            spCallRequest.Visibility = Visibility.Collapsed;
            spCallInProgress.Visibility = Visibility.Visible;
            txtCallDuration.Visibility = Visibility.Visible;

            // Bắt đầu đếm thời gian
            _callTimer = new DispatcherTimer();
            _callTimer.Interval = TimeSpan.FromSeconds(1);
            _callTimer.Tick += (s, ev) =>
            {
                _callDuration = _callDuration.Add(TimeSpan.FromSeconds(1));
                txtCallDuration.Text = _callDuration.ToString(@"hh\:mm\:ss");
            };
            _callTimer.Start();

            // Voice Call
            _voiceHub = new VoiceCallHub(_toEmail);

            // Tạo provider để phát âm thanh
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1))
            {
                BufferDuration = TimeSpan.FromSeconds(5), // Giảm giật nếu mạng delay
                DiscardOnBufferOverflow = true
            };

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();

            await _voiceHub.ConnectAsync((senderEmail, audioData) =>
            {
                // BỎ QUA NẾU LÀ ÂM THANH CỦA CHÍNH MÌNH
                if (senderEmail != _toEmail && audioData != null && audioData.Length > 0)
                {
                    _bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
                }
            });


            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1) // mono
            };
            _waveIn.DataAvailable += async (s, a) =>
            {
                if (_isMicOn && _isInCall)
                {
                    byte[] buffer = new byte[a.BytesRecorded];
                    Array.Copy(a.Buffer, buffer, a.BytesRecorded);
                    await _voiceHub.SendAudioAsync(_fromEmail, buffer);
                }
            };
            _waveIn.StartRecording();
            _isInCall = true;
        }

        private void ToggleMic_Click(object sender, RoutedEventArgs e)
        {
            _isMicOn = !_isMicOn;
            btnToggleMic.Content = _isMicOn ? "Tắt mic" : "Bật mic";

            // TODO: Gửi trạng thái mic lên server hoặc xử lý logic mic ở đây
        }

        private async void EndCall_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "voice call", "end_voice_call");
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _voiceHub?.DisposeAsync();
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _bufferedWaveProvider = null;
            _isInCall = false;
            _callTimer?.Stop();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
