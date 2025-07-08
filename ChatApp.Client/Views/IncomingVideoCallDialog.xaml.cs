using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for IncomingVideoCallDialog.xaml
    /// </summary>
    public partial class IncomingVideoCallDialog : Window
    {
        private NotificationHub _notificationHub;
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private readonly string _callerName;
        private DispatcherTimer _callTimer;
        private TimeSpan _callDuration = TimeSpan.Zero;
        private bool _isMicOn = true;
        private bool _isCamOn = true;
        private WaveInEvent _waveIn;
        private VoiceCallHub _voiceHub;
        private bool _isInCall = false;
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;
        private bool _isRemoteEnded = false;

        public IncomingVideoCallDialog(string fromEmail, string toEmail, string callerName)
        {
            InitializeComponent();
            _fromEmail = fromEmail;
            _toEmail = toEmail;
            _callerName = callerName;

            txtCaller.Text = _callerName;

            // Start the socket hub
            _notificationHub = new NotificationHub(_toEmail);
        }

        private async void form_loading(object sender, EventArgs e)
        {
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "cancel_video_call" || messageType == "end_video_call")
                {
                    _isRemoteEnded = true;
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
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

            if (!_isRemoteEnded)
            {
                await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "end_video_call");
            }
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "accept_video_call");

            spCallRequest.Visibility = Visibility.Collapsed;
            gridInCall.Visibility = Visibility.Visible;
            txtCallDuration.Visibility = Visibility.Visible;

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

            _voiceHub = new VoiceCallHub(_toEmail);

            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1))
            {
                BufferDuration = TimeSpan.FromSeconds(5),
                DiscardOnBufferOverflow = true
            };

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();

            await _voiceHub.ConnectAsync((senderEmail, audioData) =>
            {
                if (senderEmail != _toEmail && audioData != null && audioData.Length > 0)
                {
                    _bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
                }
            });

            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1)
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
        }

        private void ToggleCam_Click(object sender, RoutedEventArgs e)
        {
            _isCamOn = !_isCamOn;
            btnToggleCam.Content = _isCamOn ? "Tắt camera" : "Bật camera";
            imgLocalVideo.Visibility = _isCamOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void EndCall_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "end_video_call");

            _isRemoteEnded = true;

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

        private async void Decline_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "cancel_video_call");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

    }
}
