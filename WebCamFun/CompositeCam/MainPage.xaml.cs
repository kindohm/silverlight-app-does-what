using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CompositeCam
{
    public partial class MainPage : UserControl
    {
        List<CaptureSource> sources;
        Random random;

        bool on;

        public MainPage()
        {
            InitializeComponent();
            this.random = new Random();
            App.Current.Exit += ((s, args) =>
            {
                this.StopAll();
            });

            this.Loaded += ((s, args) =>
            {
                this.sources = new List<CaptureSource>();
            });
        }

        void StopAll()
        {
            foreach (var source in this.sources)
                source.Stop();
            this.sources.Clear();
        }

        void StartAll()
        {
            if (CaptureDeviceConfiguration.AllowedDeviceAccess ||
                CaptureDeviceConfiguration.RequestDeviceAccess())
            {
                foreach (var source in this.sources)
                    source.Start();
            }
        }

        void TurnOnClicked(object sender, RoutedEventArgs e)
        {
            if (this.on)
            {
                this.StopAll();
                this.on = false;
                this.turnOnButton.Content = "Turn On";
                this.LayoutRoot.Children.Clear();
            }
            else
            {
                double opacityIncrement = 0;
                var devices =
                    CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();
                bool first = true;

                foreach (var device in devices)
                {
                    var videoSurface = new Rectangle()
                    {
                        Width = 600,
                        Height = 450,
                        Opacity = 1 + opacityIncrement
                    };

                    var source = new CaptureSource();
                    source.VideoCaptureDevice = device;
                    this.sources.Add(source);
                    var brush = new VideoBrush();
                    brush.SetSource(source);
                    videoSurface.Fill = brush;
                    this.LayoutRoot.Children.Add(videoSurface);
                    opacityIncrement -= 0.5;

                    if (!first)
                    {

                        // add some schweet randomness.
                        // by schweet, I mean useless.
                        // by useless, I mean fully awesome.

                        var translate = new TranslateTransform();
                        videoSurface.RenderTransform = translate;

                        var projection = new PlaneProjection();
                        videoSurface.Projection = projection;

                        var storyboard = new Storyboard();

                        var animation = this.GetBaseAnimation();
                        Storyboard.SetTarget(animation, translate);
                        Storyboard.SetTargetProperty(animation, new PropertyPath("X"));
                        storyboard.Children.Add(animation);

                        animation = this.GetBaseAnimation();
                        Storyboard.SetTarget(animation, translate);
                        Storyboard.SetTargetProperty(animation, new PropertyPath("Y"));
                        storyboard.Children.Add(animation);

                        animation = this.GetBaseAnimation();
                        Storyboard.SetTarget(animation, projection);
                        Storyboard.SetTargetProperty(animation, new PropertyPath("RotationX"));
                        storyboard.Children.Add(animation);

                        animation = this.GetBaseAnimation();
                        Storyboard.SetTarget(animation, projection);
                        Storyboard.SetTargetProperty(animation, new PropertyPath("RotationY"));
                        storyboard.Children.Add(animation);

                        animation = this.GetBaseAnimation();
                        Storyboard.SetTarget(animation, projection);
                        Storyboard.SetTargetProperty(animation, new PropertyPath("RotationZ"));
                        storyboard.Children.Add(animation);


                        storyboard.Begin();
                    }
                    first = false;
                }
                this.StartAll();
                this.on = true;
                this.turnOnButton.Content = "Turn Off";
            }
        }

        DoubleAnimation GetBaseAnimation()
        {
            var animation = new DoubleAnimation();
            animation.From = this.random.Next(-10, 10);
            animation.To = this.random.Next(-10, 10);
            animation.Duration = TimeSpan.FromSeconds(this.random.Next(2, 6));
            animation.AutoReverse = true;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.EasingFunction = new QuadraticEase();
            return animation;
        }
    }
}
