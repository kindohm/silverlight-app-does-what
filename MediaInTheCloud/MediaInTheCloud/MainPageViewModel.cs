using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Browser;

namespace MediaInTheCloud
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        bool allowedDeviceAccess;
        bool recordingAudio;
        Brush webcamBrush;
        CaptureSource captureSource;
        TabItem selectedTab;
        PictureTaker pictureTaker;
        AudioRecorder audioRecorder;
        ObservableCollection<TabItem> tabs;
        ObservableCollection<MediaItem> localMediaItems;
        ObservableCollection<ServerMediaItem> serverMediaItems;
        ReadOnlyCollection<AudioCaptureDevice> audioDevices;
        ReadOnlyCollection<VideoCaptureDevice> videoDevices;
        VideoCaptureDevice selectedVideoDevice;
        AudioCaptureDevice selectedAudioDevice;
        RequestDeviceAccessCommand requestDeviceAccessCommand;
        Dispatcher dispatcher;
        Uploader uploader;
        ServerMonitor serverMonitor;

        public MainPageViewModel(Dispatcher dispatcher)
        {
            this.audioRecorder = new AudioRecorder(dispatcher);
            this.audioRecorder.RecorderStopped += new EventHandler(RecorderStopped);
            this.audioRecorder.RecorderStarted += new EventHandler(RecorderStarted);
            this.pictureTaker = new PictureTaker();
            this.pictureTaker.TakePictureCompleted +=
                new EventHandler<CaptureImageCompletedEventArgs>(TakePictureCompleted);

            this.serverMonitor = new ServerMonitor(this.ServerMediaItems);
            this.serverMonitor.DownloadComplete += ((s, args) =>
            {
                if (!App.Current.IsRunningOutOfBrowser)
                {
                    var items = from i in this.ServerMediaItems
                                where i.IsAudio == false
                                orderby i.Name descending
                                select i;
                    var item = items.FirstOrDefault();
                    if (item != null)
                    {
                        HtmlPage.Window.Invoke("lastImageUploaded", item.Url);
                    }
                }
            
            });

            this.uploader = new Uploader(dispatcher, this.LocalMediaItems);
            this.uploader.QueueEmptied += ((s, args) =>
            {
                this.serverMonitor.Download();
            });


            this.serverMonitor.Download();
            this.AllowedDeviceAccess = CaptureDeviceConfiguration.AllowedDeviceAccess;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand TakePicture { get { return new RelayCommand(this.pictureTaker.TakePicture); } }
        public ICommand RecordAudio { get { return new RelayCommand(this.audioRecorder.Record); } }
        public ICommand StopRecordingAudio { get { return new RelayCommand(this.audioRecorder.Stop); } }

        public ICommand RequestDeviceAccess
        {
            get
            {
                if (this.requestDeviceAccessCommand == null)
                {
                    this.requestDeviceAccessCommand = new RequestDeviceAccessCommand();
                    this.requestDeviceAccessCommand.Completed += ((s, args) =>
                    {
                        this.AllowedDeviceAccess = CaptureDeviceConfiguration.AllowedDeviceAccess;
                        this.OnSelectedTabChanged();
                    });
                }
                return this.requestDeviceAccessCommand;
            }
        }

        public bool RecordingAudio
        {
            get { return this.recordingAudio; }
            set
            {
                this.recordingAudio = value;
                this.OnPropertyChanged("RecordingAudio");
            }
        }

        public bool AllowedDeviceAccess
        {
            get
            {
                return this.allowedDeviceAccess;
            }
            set
            {
                this.allowedDeviceAccess = value;
                this.OnPropertyChanged("AllowedDeviceAccess");
            }
        }

        public CaptureSource CaptureSource
        {
            get
            {
                if (this.captureSource == null)
                {
                    this.captureSource = new CaptureSource();
                    this.pictureTaker.CaptureSource = this.captureSource;
                }
                return this.captureSource;
            }
            private set
            {
                this.captureSource = value;
                this.pictureTaker.CaptureSource = this.captureSource;
                this.audioRecorder.CaptureSource = this.captureSource;
            }
        }

        public ObservableCollection<MediaItem> LocalMediaItems
        {
            get
            {
                if (this.localMediaItems == null)
                {
                    this.localMediaItems = new ObservableCollection<MediaItem>();
                    this.localMediaItems.CollectionChanged += ((s, args) =>
                    {
                        this.OnPropertyChanged("LocalMediaItems");
                    });
                    this.OnPropertyChanged("LocalMediaItems");
                }
                return this.localMediaItems;
            }
        }

        public ObservableCollection<ServerMediaItem> ServerMediaItems
        {
            get
            {
                if (this.serverMediaItems == null)
                {
                    this.serverMediaItems = new ObservableCollection<ServerMediaItem>();
                    this.serverMediaItems.CollectionChanged += ((s, args) =>
                    {
                        this.OnPropertyChanged("ServerMediaItems");
                    });
                    this.OnPropertyChanged("ServerMediaItems");
                }
                return this.serverMediaItems;
            }
        }

        public AudioCaptureDevice SelectedAudioDevice
        {
            get
            {
                return this.selectedAudioDevice;
            }
            set
            {
                this.selectedAudioDevice = value;
                this.CaptureSource.AudioCaptureDevice = this.selectedAudioDevice;
                this.OnPropertyChanged("SelectedAudioDevice");
            }
        }

        public VideoCaptureDevice SelectedVideoDevice
        {
            get
            {
                return this.selectedVideoDevice;
            }
            set
            {
                this.selectedVideoDevice = value;

                bool needRestart = this.CaptureSource.State == CaptureState.Started || this.SelectedTab.Content is WebcamControl;
                if (needRestart)
                    this.CaptureSource.Stop();
                this.CaptureSource.VideoCaptureDevice = this.selectedVideoDevice;
                if (needRestart)
                    this.StartVideoCapture();
                this.OnPropertyChanged("SelectedVideoDevice");
            }
        }

        public TabItem SelectedTab
        {
            get
            {
                return this.selectedTab;
            }
            set
            {
                this.selectedTab = value;
                this.OnSelectedTabChanged();
            }
        }

        public Brush WebcamBrush
        {
            get
            {
                return this.webcamBrush;
            }
            set
            {
                this.webcamBrush = value;
                this.OnPropertyChanged("WebcamBrush");
            }
        }

        public ObservableCollection<TabItem> Tabs
        {
            get
            {
                if (this.tabs == null)
                    this.LoadTabs();
                return this.tabs;
            }
        }

        public ReadOnlyCollection<AudioCaptureDevice> AudioDevices
        {
            get
            {
                if (this.audioDevices == null)
                {
                    this.audioDevices = CaptureDeviceConfiguration.GetAvailableAudioCaptureDevices();
                    this.OnPropertyChanged("AudioDevices");
                    this.SelectedAudioDevice = this.audioDevices[0];
                }
                return this.audioDevices;
            }
        }

        public ReadOnlyCollection<VideoCaptureDevice> VideoDevices
        {
            get
            {
                if (this.videoDevices == null)
                {
                    this.videoDevices = CaptureDeviceConfiguration.GetAvailableVideoCaptureDevices();
                    this.OnPropertyChanged("VideoDevices");
                    this.SelectedVideoDevice = this.videoDevices[0];
                }
                return this.videoDevices;
            }
        }

        void OnSelectedTabChanged()
        {
            if (this.selectedTab.Content is WebcamControl)
            {
                //this.CaptureSource.Stop();
                this.CaptureSource = new CaptureSource();
                this.CaptureSource.AudioCaptureDevice = null;
                this.CaptureSource.VideoCaptureDevice = this.SelectedVideoDevice;
                this.audioRecorder.CaptureSource = null;
                if (this.CaptureSource.VideoCaptureDevice != null)
                    this.StartVideoCapture();
            }
            else
            {
                this.CaptureSource.Stop();
                this.CaptureSource.VideoCaptureDevice = null;
                this.CaptureSource.AudioCaptureDevice = this.SelectedAudioDevice;
                this.audioRecorder.CaptureSource = this.CaptureSource;

                var brush = new SolidColorBrush(Colors.Black);
                this.WebcamBrush = brush;
            }
            this.OnPropertyChanged("SelectedTab");
        }

        void StartVideoCapture()
        {
            var brush = new VideoBrush();
            brush.SetSource(this.CaptureSource);
            this.WebcamBrush = brush;
            if (this.AllowedDeviceAccess)
                this.CaptureSource.Start();
        }

        void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(
                    this, new PropertyChangedEventArgs(propertyName));
        }

        void LoadTabs()
        {
            this.tabs = new ObservableCollection<TabItem>();
            var webcamTab = new TabItem() { Header = "Webcam" };
            var micTab = new TabItem() { Header = "Microphone" };
            webcamTab.Content = new WebcamControl();
            micTab.Content = new MicrophoneControl();
            this.tabs.Add(webcamTab);
            this.tabs.Add(micTab);
            this.OnPropertyChanged("Tabs");
            this.SelectedTab = this.tabs[0];
        }

        void TakePictureCompleted(object sender, CaptureImageCompletedEventArgs e)
        {
            this.LocalMediaItems.Add(
                new MediaItem()
                {
                    Name = GetNextFileName(false),
                    DisplayImage = e.Result,
                    Data = MediaItem.GetJpg(e.Result)
                });
        }

        void RecorderStopped(object sender, EventArgs e)
        {
            var uri = new Uri("/MediaInTheCloud;component/Sound_Icon.jpg", UriKind.Relative);
            var imageSource = new BitmapImage(uri);
            this.LocalMediaItems.Add(
                new AudioMediaItem()
                {
                    Name = GetNextFileName(true),
                    DisplayImage = imageSource,
                    IsAudio = true,
                    IsLocalAudio = true,
                    Data = this.audioRecorder.Data,
                    AudioFormat = this.audioRecorder.AudioFormat
                });
            this.RecordingAudio = false;

        }


        void RecorderStarted(object sender, EventArgs e)
        {
            this.AllowedDeviceAccess = CaptureDeviceConfiguration.AllowedDeviceAccess;
            this.RecordingAudio = true;
        }

        static string GetNextFileName(bool isAudio)
        {
            var now = DateTime.Now;
            return now.Year.ToString() + "." +
                now.Month.ToString() + "." +
                now.Day.ToString() + "." +
                now.Hour.ToString() + "." +
                now.Minute.ToString() + "." +
                now.Second.ToString() + "." +
                now.Millisecond.ToString() + (isAudio ? ".wav" : ".jpg");
        }

    }
}
