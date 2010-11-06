using System;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;


namespace MediaInTheCloud
{
    public class Uploader
    {
        bool online;
        bool uploading;
        Dispatcher dispatcher;
        DispatcherTimer timer;
        ObservableCollection<MediaItem> localItems;
        MediaItem currentItem;

        public Uploader(Dispatcher dispatcher, ObservableCollection<MediaItem> localItems)
        {
            this.dispatcher = dispatcher;
            this.localItems = localItems;

            this.online = NetworkInterface.GetIsNetworkAvailable();

            NetworkChange.NetworkAddressChanged += ((s, args) =>
            {
                this.online = NetworkInterface.GetIsNetworkAvailable();
            });

            this.LoadTimer();
        }

        void LoadTimer()
        {
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1000);
            this.timer.Tick += new EventHandler(Tick);
            this.timer.Start();
        }

        void Tick(object sender, EventArgs e)
        {
            if (localItems.Count > 0 & this.online)
            {
                this.timer.Stop();
                var mediaItem = localItems[0];
                var uri =new Uri("http://localhost/MediaInTheCloud.Host/Home/Upload");

                byte[] data = null;
                FileUpload upload = null;
                if (mediaItem.IsAudio)
                {
                    var audio = (AudioMediaItem)mediaItem;
                    data = mediaItem.Data;
                    upload = new FileUpload(this.dispatcher, uri, data, mediaItem.Name,
                         audio.AudioFormat.SamplesPerSecond, audio.AudioFormat.BitsPerSample, audio.AudioFormat.Channels);
                }
                else
                {
                    data = mediaItem.Data;
                    upload = new FileUpload(this.dispatcher, uri, data, mediaItem.Name);
                }

                upload.UploadProgressChanged += new ProgressChangedEvent(upload_UploadProgressChanged);
                this.uploading = true;
                mediaItem.IsUploading = true;
                this.currentItem = mediaItem;
                upload.Upload();
            }

            if (this.localItems.Count == 0)
            {
                if (this.QueueEmptied != null)
                    this.QueueEmptied(this, EventArgs.Empty);
            }

        }

        void upload_UploadProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
            this.currentItem.UploadProgressPercent = args.ProgressPercentage;

            if (uploading && args.ProgressPercentage >= 100)
            {
                this.localItems.Remove(this.currentItem);


                this.currentItem.IsUploading = false;
                uploading = false;
                timer.Start();
            }
        }

        public event EventHandler QueueEmptied;
    }
}
