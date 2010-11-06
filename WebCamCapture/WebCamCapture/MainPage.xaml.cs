using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebCamCapture
{
    public partial class MainPage : UserControl
    {
        ObservableCollection<WriteableBitmap> images = new ObservableCollection<WriteableBitmap>();
        CaptureSource source;
        bool on;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            App.Current.Exit += ((s, args) =>
            {
                if (this.source != null & this.source.State == CaptureState.Started)
                    this.source.Stop();                
            });
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.source = new CaptureSource();

            this.deviceList.ItemsSource = 
                CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();

            this.deviceList.SelectedIndex = 0;
           
            this.source.VideoCaptureDevice = 
                (VideoCaptureDevice)this.deviceList.SelectedItem;

            this.snapshots.ItemsSource = this.images;
            this.source.CaptureImageCompleted += new EventHandler<CaptureImageCompletedEventArgs>(source_CaptureImageCompleted);
        }

        void source_CaptureImageCompleted(object sender, CaptureImageCompletedEventArgs e)
        {
            this.images.Add(e.Result);
        }

        void SetSource()
        {
            if (on)
                this.source.Stop();

            this.source.VideoCaptureDevice = (VideoCaptureDevice)this.deviceList.SelectedItem;

            if (on)
                this.source.Start();
        }

        void DeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SetSource();
        }

        void ToggleDevice(object sender, RoutedEventArgs e)
        {
            if (this.on)
            {
                this.videoSurface.Fill = new SolidColorBrush(Colors.White);
                this.source.Stop();
                this.toggleButton.Content = "Turn On";
                this.takePictureButton.IsEnabled = false;
                this.on = false;
            }
            else
            {
                if (CaptureDeviceConfiguration.AllowedDeviceAccess
                    || CaptureDeviceConfiguration.RequestDeviceAccess())
                {
                    this.source.Start();
                    var vidBrush = new VideoBrush();
                    vidBrush.SetSource(this.source);
                    this.videoSurface.Fill = vidBrush;
                    this.toggleButton.Content = "Turn Off";
                    this.takePictureButton.IsEnabled = true;
                    this.on = true;
                }
            }
        }

        void TakePictureClick(object sender, RoutedEventArgs e)
        {
            this.source.CaptureImageAsync();
        }
    }
}
