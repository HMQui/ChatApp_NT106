using ChatApp.Client.Hub;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.Forms.MessageBox;

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
        /// Video capture variables
        private VideoCaptureDevice _videoDevice;
        private FilterInfoCollection _videoDevices;
        private VideoCallHub _videoHub;

        private bool _isClosing = false;

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
            await CleanupCallAsync();
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "accept_video_call");

            spCallRequest.Visibility = Visibility.Collapsed;
            gridInCall.Visibility = Visibility.Visible;
            txtCallDuration.Visibility = Visibility.Visible;

            // Start call timer
            _callTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _callTimer.Tick += (s, ev) =>
            {
                _callDuration = _callDuration.Add(TimeSpan.FromSeconds(1));
                txtCallDuration.Text = _callDuration.ToString(@"hh\:mm\:ss");
            };
            _callTimer.Start();

            _isInCall = true;

            // Setup voice
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
                if (senderEmail != _toEmail || audioData == null || audioData.Length == 0) return;
                _bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
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

            // Setup video
            _videoHub = new VideoCallHub(_toEmail);

            await _videoHub.ConnectAsync((senderEmail, frameBytes) =>
            {
                if (senderEmail != _toEmail || frameBytes == null || frameBytes.Length == 0) return;

                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(frameBytes))
                        using (var bmp = new System.Drawing.Bitmap(ms))
                        {
                            IntPtr hBitmap = bmp.GetHbitmap();
                            try
                            {
                                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                    hBitmap,
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height)
                                );
                                imgRemoteVideo.Source = bitmapSource;
                            }
                            finally
                            {
                                DeleteObject(hBitmap);
                            }
                        }
                    }
                    catch { /* ignore frame decode error */ }
                });
            });

            if (_isCamOn)
            {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count > 0)
                {
                    _videoDevice = new VideoCaptureDevice(_videoDevices[0].MonikerString);
                    _videoDevice.NewFrame += VideoDevice_NewFrame;
                    _videoDevice.Start();
                }
            }
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

            if (_isCamOn)
            {
                var videoDevices = new AForge.Video.DirectShow.FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
                if (videoDevices.Count > 0)
                {
                    _videoDevice = new AForge.Video.DirectShow.VideoCaptureDevice(videoDevices[0].MonikerString);

                    _videoDevice.NewFrame += (s2, eventArgs2) =>
                    {
                        using (var bitmap = (System.Drawing.Bitmap)eventArgs2.Frame.Clone())
                        {
                            System.Drawing.Bitmap resized = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(320, 240));

                            byte[] jpgBytes;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                resized.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                jpgBytes = ms.ToArray();
                            }

                            Dispatcher.Invoke(() =>
                            {
                                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                    resized.GetHbitmap(),
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(resized.Width, resized.Height)
                                );
                                imgLocalVideo.Source = bitmapSource;
                            });

                            if (_isInCall && _isCamOn)
                            {
                                _videoHub.SendFrameAsync(_fromEmail, jpgBytes);
                            }

                            resized.Dispose();
                        }
                    };

                    _videoDevice.Start();
                }
            }
            else
            {
                if (_videoDevice != null && _videoDevice.IsRunning)
                {
                    _videoDevice.NewFrame -= VideoDevice_NewFrame;
                    _videoDevice.SignalToStop();
                    _videoDevice.WaitForStop();
                    _videoDevice = null;
                }
                imgLocalVideo.Source = null;
            }
        }

        private async void EndCall_Click(object sender, RoutedEventArgs e)
        {
            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "end_video_call");

            await CleanupCallAsync();

            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private async void Decline_Click(object sender, RoutedEventArgs e)
        {
            _isRemoteEnded = true;

            await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "cancel_video_call");

            await CleanupCallAsync(false);

            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private async void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!_isInCall || !_isCamOn || _isClosing) return;

            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    Bitmap resized = new Bitmap(bitmap, new System.Drawing.Size(320, 240));

                    byte[] jpgBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resized.Save(ms, ImageFormat.Jpeg);
                        jpgBytes = ms.ToArray();
                    }

                    Dispatcher.Invoke(() =>
                    {
                        imgLocalVideo.Source = ByteArrayToImage(jpgBytes);
                    });

                    if (_videoHub != null && _isInCall && _isCamOn)
                    {
                        await _videoHub.SendFrameAsync(_fromEmail, jpgBytes);
                    }

                    resized.Dispose();
                }
            }
            catch
            {
                // ignore
            }
        }

        private BitmapImage ByteArrayToImage(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
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
            catch { }

            if (_videoHub != null)
            {
                try
                {
                    await _videoHub.DisposeAsync();
                }
                catch { }
                _videoHub = null;
            }

            imgLocalVideo.Source = null;
            imgRemoteVideo.Source = null;

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
                }
            }
            catch { }

            try
            {
                _waveOut?.Stop();
                _waveOut?.Dispose();
            }
            catch { }

            _bufferedWaveProvider = null;
            _isInCall = false;
            _callTimer?.Stop();

            if (!_isRemoteEnded && sendEndNotification)
            {
                try
                {
                    await _notificationHub.SendNotification(_toEmail, [_fromEmail], "video call", "end_video_call");
                }
                catch { }
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);


    }
}
