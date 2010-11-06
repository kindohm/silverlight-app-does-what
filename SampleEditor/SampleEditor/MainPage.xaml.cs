using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SampleEditor
{
    public partial class MainPage : UserControl
    {
        bool playing;
        bool recording;
        CaptureSource source;
        AudioEncoder encoder;
        DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            App.Current.Exit += ((s, args) =>
            {
                if (this.source != null)
                {
                    this.source.Stop();
                }
            });
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var devices = CaptureDeviceConfiguration.GetAvailableAudioCaptureDevices();

            if (devices.Count == 0)
            {
                this.ShowNoDevices();
            }
            else
            {
                this.deviceList.ItemsSource = devices;
                this.deviceList.SelectionChanged += new SelectionChangedEventHandler(deviceList_SelectionChanged);
                this.deviceList.SelectedIndex = 0;
                this.progressPanel.Visibility = Visibility.Collapsed;
                this.progress.Maximum = AudioEncoder.MaxLength;
                this.timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(200)
                };
                this.timer.Tick += new EventHandler(timer_Tick);                    
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (this.encoder != null)
            {
                this.progress.Value = this.encoder.Size;
                double percent = this.progress.Value / this.progress.Maximum * 100;
                this.byteInfo.Text = string.Format("Recorded {0} Bytes", encoder.Size.ToString("#,###,###"));
                this.percentInfo.Text = string.Format("{0}% Full", percent.ToString("0.00"));
            }
        }

        void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = this.deviceList.SelectedItem as AudioCaptureDevice;
            this.formatList.ItemsSource = device.GetFriendlySupportedFormats();
            this.formatList.SelectedIndex = 0;
        }

        void RecordClicked(object sender, RoutedEventArgs e)
        {
            this.stopButton.IsEnabled = true;
            this.recordButton.IsEnabled = false;
            this.playButton.IsEnabled = false;
            this.waveDisplay.ResetSelection();


            this.source = new CaptureSource();
            this.source.AudioCaptureDevice =
                CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();

            this.source.AudioCaptureDevice.DesiredFormat =
                ((SupportedFormat)this.formatList.SelectedItem).Format;
            this.encoder = new AudioEncoder();
            this.timer.Start();
            this.progressPanel.Visibility = Visibility.Visible;
            this.encoder.EncoderIsFull += ((s, args) =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        this.Stop();
                    });
                });
            this.encoder.CaptureSource = this.source;

            if (CaptureDeviceConfiguration.AllowedDeviceAccess
                || CaptureDeviceConfiguration.RequestDeviceAccess())
            {
                this.source.Start();
                this.recording = true;
                this.playButton.IsEnabled = false;
                this.formatList.IsEnabled = false;
                this.deviceList.IsEnabled = false;
            }
        }


        void PlayLoopClicked(object sender, RoutedEventArgs e)
        {
            this.stopButton.IsEnabled = true;
            this.recordButton.IsEnabled = false;
            this.playButton.IsEnabled = false;
            this.playing = true;
            this.waveDisplay.Play();
        }

        void StopLoopClicked(object sender, RoutedEventArgs e)
        {
            this.Stop();
        }

        void Stop()
        {
            this.playButton.IsEnabled = true;
            this.recordButton.IsEnabled = true;
            this.stopButton.IsEnabled = false;
            if (this.playing)
            {
                this.playing = false;
                this.waveDisplay.Stop();
            }
            else if (this.recording)
            {
                recording = false;
                this.source.Stop();
                this.waveDisplay.Load(this.encoder.Data, this.encoder.Format);
                this.playButton.IsEnabled = true;
                this.formatList.IsEnabled = true;
                this.deviceList.IsEnabled = true;
                this.timer.Stop();
                this.progressPanel.Visibility = Visibility.Collapsed;
            }
        }

        void ShowNoDevices()
        {
            this.LayoutRoot.Children.Clear();
            this.LayoutRoot.Background = new SolidColorBrush(Colors.LightGray);
            var text = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Margin= new Thickness(10)
            };
            text.Text = "You do not have any microphone devices available for this app to use.\n\nSorry.";
            this.LayoutRoot.Children.Add(text);
        }
    }
}
