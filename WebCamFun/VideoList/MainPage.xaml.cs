using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace VideoList
{
    public partial class MainPage : UserControl
    {
        ObservableCollection<VideoItem> sourceItems;
        List<CaptureSource> sources;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            App.Current.Exit += ((s, args) =>
            {
                this.StopAll();
            });
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.sources = new List<CaptureSource>();
            this.sourceItems = new ObservableCollection<VideoItem>();
        }

        void Load()
        {
            if (CaptureDeviceConfiguration.AllowedDeviceAccess ||
                            CaptureDeviceConfiguration.RequestDeviceAccess())
            {
                var devices = CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();

                foreach (var device in devices)
                {
                    var videoItem = new VideoItem();
                    videoItem.Name = device.FriendlyName;

                    var source = new CaptureSource();
                    source.VideoCaptureDevice = device;
                    var videoBrush = new VideoBrush();
                    videoBrush.SetSource(source);
                    videoItem.Brush = videoBrush;
                    this.sources.Add(source);
                    this.sourceItems.Add(videoItem);
                }

                this.videoItems.ItemsSource = this.sourceItems;
                this.StartAll();
            }
        }

        void StartAll()
        {
            foreach (var item in sources)
                item.Start();
        }

        void StopAll()
        {
            foreach (var item in sources)
                item.Stop();
        }

        void LoadClick(object sender, RoutedEventArgs e)
        {
            this.Load();
        }

        private void VideoChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (VideoItem)this.videoItems.SelectedItem;

            this.videoSurface.Fill = item.Brush;


        }
    }
}
