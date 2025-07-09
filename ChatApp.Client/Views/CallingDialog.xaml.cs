using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;


namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for CallingDialog.xaml
    /// </summary>
    
    public partial class CallingDialog : Window
    {
        private NotificationHub _notificationHub;
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private readonly string _user;
        private DispatcherTimer _callTimer;
        private TimeSpan _callDuration = TimeSpan.Zero;
        private bool _isMicOn = true;
        private WaveInEvent _waveIn;
        private VoiceCallHub _voiceHub;
        private bool _isInCall = false;
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;
        public CallingDialog(string fromEmail, string toEmail, string username)
        {
            InitializeComponent();
            _fromEmail = fromEmail;
            _toEmail = toEmail;
            _user = username;

            // Start the socket hub
            _notificationHub = new NotificationHub(_fromEmail);
        }

        private async void form_loading(object sender, EventArgs e)
        {
            txtReceiver.Text = _user; // You can map toEmail to a display name if needed
            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "accept_voice_call")
                {
                    Dispatcher.Invoke(async () =>
                    {
                        // UI
                        txtCallingStatus.Visibility = Visibility.Collapsed;
                        progressCalling.Visibility = Visibility.Collapsed;
                        btnCancel.Visibility = Visibility.Collapsed;
                        txtCallDuration.Visibility = Visibility.Visible;
                        spInCall.Visibility = Visibility.Visible;

                        // Bắt đầu đếm thời gian
                        _callTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1)
                        };
                        _callTimer.Tick += (s, ev) =>
                        {
                            _callDuration = _callDuration.Add(TimeSpan.FromSeconds(1));
                            txtCallDuration.Text = _callDuration.ToString(@"hh\:mm\:ss");
                        };
                        _callTimer.Start();

                        // Voice Call
                        _voiceHub = new VoiceCallHub(_fromEmail);

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
                            if (senderEmail != _fromEmail && audioData != null && audioData.Length > 0)
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
                                await _voiceHub.SendAudioAsync(_toEmail, buffer);
                            }
                        };
                        _waveIn.StartRecording();
                        _isInCall = true;
                    });
                }
                else if (messageType == "end_voice_call") this.Close();
            });

            // gửi thông báo gọi đến user
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "voice call", "voice_call");
        }
        private void ToggleMic_Click(object sender, RoutedEventArgs e)
        {
            _isMicOn = !_isMicOn;
            btnToggleMic.Content = _isMicOn ? "Tắt mic" : "Bật mic";
            // TODO: gửi trạng thái mic hoặc xử lý ở client
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

        private async void form_closing(object sender, CancelEventArgs e)
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _voiceHub?.DisposeAsync();
            _isInCall = false;
            if (_notificationHub != null)
            {
                await _notificationHub.DisconnectAsync();
            }
            this.Close();
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "voice call", "cancel_voice_call");
            this.Close();
        }
    }
}
