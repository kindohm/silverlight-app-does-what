using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WebCamFun
{
    public partial class CameraControl : UserControl
    {
        bool on;
        bool spinning;
        CaptureSource source;

        public CameraControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ControlLoaded);
            App.Current.Exit += ((s, args) =>
            {
                if (this.source != null)
                    this.source.Stop();
            });
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            source = new CaptureSource();
            this.deviceList.ItemsSource = CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();
            this.deviceList.SelectedIndex = 0;

            foreach (DoubleAnimation animation in this.spinStoryboard.Children)
            {
                int next = random.Next(5, 25);
                animation.Duration =
                    new Duration(TimeSpan.FromSeconds(next));
                if (next > 15)
                    animation.To = -animation.To;
            }
        }

        void ToggleDevice(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                on = false;
                source.Stop();
                toggleButton.Content = "Turn On";
                this.videoSurface.Fill = new SolidColorBrush(Colors.White);
            }
            else
            {
                if (CaptureDeviceConfiguration.AllowedDeviceAccess
                                   || CaptureDeviceConfiguration.RequestDeviceAccess())
                {
                    if (this.container.Child is TextBox)
                    {
                        this.container.Child = this.videoSurface;
                    }

                    try
                    {
                        source.Start();
                        var brush = new VideoBrush();
                        brush.SetSource(source);
                        this.videoSurface.Fill = brush;
                        //toggleButton.Content = "Turn Off";

                        var canvas = new Canvas();
                        canvas.Width = 70;
                        canvas.Height = 50;

                        var text = new TextBlock();
                        text.Text = "Turn Off";
                        canvas.Children.Add(text);
                        text.SetValue(Canvas.LeftProperty, 5d);
                        text.SetValue(Canvas.TopProperty, 5d);

                        canvas.Background = brush;

                        toggleButton.Content = canvas;


                        on = true;
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.HandleDeviceInUseError(ex);
                    }

                }
            }
        }

        void SpinClick(object sender, RoutedEventArgs e)
        {
            var spinButton = sender as Button;
            if (this.spinning)
            {
                spinButton.Content = "Spin";
                spinning = false;
                this.spinStoryboard.Stop();
            }
            else
            {
                spinButton.Content = "Stop";
                this.spinning = true;
                this.spinStoryboard.Begin();
            }
        }

        void DeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.on)
                this.source.Stop();

            if (this.container.Child is TextBox)
                this.container.Child = this.videoSurface;

            source.VideoCaptureDevice = (VideoCaptureDevice)this.deviceList.SelectedItem;

            if (this.on)
            {
                try
                {
                    this.source.Start();
                }
                catch (InvalidOperationException ex)
                {
                    this.HandleDeviceInUseError(ex);
                }
            }
        }

        void HandleDeviceInUseError(Exception ex)
        {
            var textBox = new TextBox();
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.Text = "Device already in use: " + ex.Message;
            textBox.Foreground = new SolidColorBrush(Colors.Red);
            textBox.FontSize = 25;
            textBox.VerticalAlignment = VerticalAlignment.Center;
            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            this.container.Child = textBox;
        }
    }
}
