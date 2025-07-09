using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ChatApp.Client.Views
{
    /// <summary>
    /// Interaction logic for VideoCallDialog.xaml
    /// </summary>
    public partial class VideoCallDialog : Window
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
        private bool _isRemoteEnded = false;

        //biến camera
        private VideoCaptureDevice _videoDevice;
        private FilterInfoCollection _videoDevices;
        private bool _isCamOn = false;
        private VideoCallHub _videoHub;
        private System.Timers.Timer _videoTimer;

        private bool _isClosing = false;

        public VideoCallDialog(string fromEmail, string toEmail, string username)
        {
            InitializeComponent();
            _fromEmail = fromEmail;
            _toEmail = toEmail;
            _user = username;

            // Start the socket hub
            _notificationHub = new NotificationHub(_fromEmail);
        }

        // loading form
        private async void form_loading(object sender, EventArgs e)
        {
            txtReceiver.Text = _user; // You can map toEmail to a display name if needed
            // đợi phản hồi từ server nếu có thông báo mới
            await _notificationHub.ConnectAsync((id, senderEmail, message, messageType) =>
            {
                if (messageType == "accept_video_call")
                {
                    Dispatcher.Invoke(async () =>
                    {
                        // UI
                        spCallState.Visibility = Visibility.Collapsed;
                        gridInCall.Visibility = Visibility.Visible;
                        txtCallDuration.Visibility = Visibility.Visible;

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
                            if (senderEmail != _fromEmail && audioData != null && audioData.Length > 0)
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
                                await _voiceHub.SendAudioAsync(_toEmail, buffer);
                            }
                        };
                        _waveIn.StartRecording();
                        _isInCall = true;

                        // ==== VIDEO HUB ====

                        _videoHub = new VideoCallHub(_fromEmail);

                        // 1. Connect để lắng nghe frame từ người khác
                        await _videoHub.ConnectAsync((senderEmail, frameBytes) =>
                        {
                            if (senderEmail != _fromEmail && frameBytes != null && frameBytes.Length > 0)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    using (MemoryStream ms = new MemoryStream(frameBytes))
                                    {
                                        Bitmap bmp = new Bitmap(ms);
                                        imgRemoteVideo.Source = ConvertBitmapToImageSource(bmp);
                                    }
                                });
                            }
                        });

                        // 2. Bật webcam và gửi frame
                        var videoDevices = new AForge.Video.DirectShow.FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
                        if (videoDevices.Count > 0)
                        {
                            _videoDevice = new AForge.Video.DirectShow.VideoCaptureDevice(videoDevices[0].MonikerString);

                            _videoDevice.NewFrame += (s, eventArgs) =>
                            {
                                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                                {
                                    // Resize nhỏ
                                    Bitmap resized = new Bitmap(320, 240);
                                    using (Graphics g = Graphics.FromImage(resized))
                                    {
                                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        g.DrawImage(bitmap, 0, 0, 320, 240);
                                    }

                                    byte[] jpgBytes;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        resized.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        jpgBytes = ms.ToArray();
                                    }

                                    if (_isInCall && _isCamOn)
                                    {
                                        _videoHub.SendFrameAsync(_toEmail, jpgBytes);
                                    }

                                    resized.Dispose();
                                }
                            };

                            _videoDevice.Start();
                        }
                    });
                }
                if (messageType == "end_video_call")
                {
                    _isRemoteEnded = true;
                    Dispatcher.Invoke(() => this.Close());
                }
                if (messageType == "cancel_video_call")
                {
                    Dispatcher.Invoke(() => this.Close());
                }
            });


            // gửi thông báo gọi đến user
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "video call", "video_call");
        }

        private async void form_closing(object sender, CancelEventArgs e)
        {
            await CleanupCallAsync();
        }

        private async void EndCall_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_fromEmail, [_toEmail], "video call", "end_video_call");

            _isRemoteEnded = true;

            await CleanupCallAsync();

            this.Close();
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _isRemoteEnded = true;

            await _notificationHub.SendNotification(_fromEmail, new[] { _toEmail }, "video call", "end_video_call");

            await CleanupCallAsync(false);

            this.Close();
        }

        private void ToggleMic_Click(object sender, RoutedEventArgs e)
        {
            _isMicOn = !_isMicOn;
            btnToggleMic.Content = _isMicOn ? "Tắt mic" : "Bật mic";
        }

        private void ToggleCam_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCamOn)
            {
                // Bật cam
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count > 0)
                {
                    _videoDevice = new VideoCaptureDevice(_videoDevices[0].MonikerString);
                    _videoDevice.NewFrame += VideoDevice_NewFrame;
                    _videoDevice.Start();

                    btnToggleCam.Content = "Tắt camera";
                    _isCamOn = true;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy webcam!");
                }
            }
            else
            {
                // Tắt cam
                if (_videoDevice != null && _videoDevice.IsRunning)
                {
                    _videoDevice.SignalToStop();
                    _videoDevice.WaitForStop();
                    _videoDevice.NewFrame -= VideoDevice_NewFrame;
                    _videoDevice = null;
                }

                btnToggleCam.Content = "Bật camera";
                _isCamOn = false;
            }
        }

        private async void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!_isInCall || !_isCamOn || _isClosing) return;

            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    byte[] jpegBytes = BitmapToJpeg(bitmap);
                    await _videoHub.SendFrameAsync(_toEmail, jpegBytes);
                }
            }
            catch
            {
                // ignore để không crash NewFrame
            }

        }

        private byte[] BitmapToJpeg(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public static BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;

                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
                img.Freeze();
                return img;
            }
        }

        private async Task CleanupCallAsync(bool sendEndNotification = true)
        {
            if (_isClosing) return;
            _isClosing = true;

            try
            {
                if (_videoDevice != null && _videoDevice.IsRunning)
                {
                    _videoDevice.NewFrame -= VideoDevice_NewFrame;
                    _videoDevice.SignalToStop();
                    _videoDevice.WaitForStop();
                    _videoDevice.Stop();
                    _videoDevice = null;
                }
            }
            catch
            {
                // ignore
            }

            try
            {
                if (_videoHub != null)
                {
                    await _videoHub.DisposeAsync();
                    _videoHub = null;
                }
            }
            catch { }

            try
            {
                imgRemoteVideo.Source = null;
                imgLocalVideo.Source = null;
            }
            catch { }

            try
            {
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;
            }
            catch { }

            try
            {
                if (_voiceHub != null)
                {
                    await _voiceHub.DisposeAsync();
                    _voiceHub = null;
                }
            }
            catch { }

            try
            {
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _waveOut = null;
            }
            catch { }

            _bufferedWaveProvider = null;
            _isInCall = false;
            _callTimer?.Stop();

            if (!_isRemoteEnded && sendEndNotification)
            {
                try
                {
                    await _notificationHub.SendNotification(_fromEmail, [_toEmail], "video call", "end_video_call");
                }
                catch
                {
                    // ignore
                }
            }
        }

    }
}
