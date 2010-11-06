using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VideoCapture.Silverlight
{
    public partial class MainPage : UserControl
    {
        CaptureSource source;
        bool on;
        VideoEncoder encoder;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.source = new CaptureSource();
            this.deviceList.ItemsSource = CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();
            this.deviceList.SelectedIndex = 0;
            this.source.VideoCaptureDevice = (VideoCaptureDevice)this.deviceList.SelectedItem;
        }

        void SetSource()
        {
            if (on)
            {
                this.source.Stop();
            }

            this.source.VideoCaptureDevice = (VideoCaptureDevice)this.deviceList.SelectedItem;

            if (on)
            {
                this.source.Start();
            }
        }

        void DeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SetSource();
        }

        void ToggleDevice(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                this.videoSurface.Fill = new SolidColorBrush(Colors.White);
                this.source.Stop();
                this.toggleButton.Content = "Record";
                this.playButton.IsEnabled = true;
                on = false;
            }
            else
            {
                if (CaptureDeviceConfiguration.AllowedDeviceAccess
                    || CaptureDeviceConfiguration.RequestDeviceAccess())
                {
                    this.encoder = new VideoEncoder();
                //C:\hods\Presentations\OfflineWebCam\VideoCapture.Web\VideoCapture.Silverlight\VideoDecoder.cs
                    this.encoder.CaptureSource = this.source;
                    
                    this.source.Start();

                    VideoBrush vidBrush = new VideoBrush();
                    vidBrush.SetSource(this.source);
                    this.videoSurface.Fill = vidBrush;
                    this.toggleButton.Content = "Stop";
                    this.playButton.IsEnabled = false;
                    on = true;
                }
            }
        }

        void PlayClick(object sender, RoutedEventArgs e)
        {
            if (this.encoder != null)
            {
                var decoder = new VideoDecoder(this.encoder.Stream, this.encoder.Format, this.encoder.SampleLength);
                this.media.SetSource(decoder);
            }
        }
    }
}
