// this code based from http://silverlightfileupld.codeplex.com/

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Threading;

namespace MediaInTheCloud
{
    public enum FileUploadStatus
    {
        Pending,
        Uploading,
        Complete,
        Error,
        Canceled,
        Removed,
        Resizing
    }

    public class FileUpload : INotifyPropertyChanged
    {
        //const long ChunkSize = 4194304;
        const long ChunkSize = 12000;

        long fileLength;
        byte[] fileBytes;
        long bytesUploaded;
        int uploadPercent;
        FileUploadStatus status;
        Dispatcher Dispatcher;

        FileUpload(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Status = FileUploadStatus.Pending;
        }

        FileUpload(Dispatcher dispatcher, Uri uploadUrl)
            : this(dispatcher)
        {
            UploadUrl = uploadUrl;
        }

        public FileUpload(Dispatcher dispatcher, Uri uploadUrl, byte[] bytes, string fileName)
            : this(dispatcher, uploadUrl)
        {
            this.FileBytes = bytes;
            this.Name = fileName;
        }

        public FileUpload(Dispatcher dispatcher, Uri uploadUrl, byte[] bytes, string fileName,
            int sampleRate, int bitsPerSample, int channels)
            : this(dispatcher, uploadUrl)
        {
            this.FileBytes = bytes;
            this.Name = fileName;
            this.SampleRate = sampleRate;
            this.BitsPerSample = bitsPerSample;
            this.Channels = channels;
        }

        public event ProgressChangedEvent UploadProgressChanged;
        public event EventHandler StatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public Uri UploadUrl { get; set; }
        public int ImageSize { get; set; }
        public int SampleRate { get; set; }
        public int BitsPerSample { get; set; }
        public int Channels { get; set; }

        public byte[] FileBytes
        {
            get { return fileBytes; }
            set
            {
                fileBytes = value;
                this.FileLength = fileBytes.Length;
            }
        }

        public long FileLength
        {
            get { return fileLength; }
            set
            {
                fileLength = value;
                this.OnPropertyChanged("FileLength");
            }
        }

        public long BytesUploaded
        {
            get { return bytesUploaded; }
            set
            {
                bytesUploaded = value;
                this.OnPropertyChanged("BytesUploaded");
            }
        }

        public int UploadPercent
        {
            get { return uploadPercent; }
            set
            {
                uploadPercent = value;
                this.OnPropertyChanged("UploadPercent");
            }
        }

        public FileUploadStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                this.OnPropertyChanged("Status");

                this.Dispatcher.BeginInvoke(delegate()
                {
                    if (this.StatusChanged != null)
                        this.StatusChanged(this, null);
                });
            }
        }

        public void Upload()
        {
            this.UploadFileEx();
        }

        void UploadFileEx()
        {
            this.Status = FileUploadStatus.Uploading;
            long temp = FileLength - BytesUploaded;

            UriBuilder ub = new UriBuilder(UploadUrl);
            bool complete = temp <= ChunkSize;
            ub.Query = string.Format("{6}filename={0}&StartByte={1}&Complete={2}&SampleRate={3}&BitsPerSample={4}&Channels={5}", this.Name, BytesUploaded, complete, 
                this.SampleRate.ToString(),
                this.BitsPerSample.ToString(),
                this.Channels.ToString(),
                string.IsNullOrEmpty(ub.Query) ? "" : ub.Query.Remove(0, 1) + "&");

            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ub.Uri);
            webrequest.Method = "POST";
            webrequest.BeginGetRequestStream(new AsyncCallback(WriteCallback), webrequest);
        }

        void WriteCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webrequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream requestStream = webrequest.EndGetRequestStream(asynchronousResult);

            byte[] buffer = new Byte[4096];
            int bytesRead = 0;
            int tempTotal = 0;

            Stream fileStream = new MemoryStream(this.fileBytes);

            fileStream.Position = BytesUploaded;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0 && tempTotal + bytesRead < ChunkSize)
            {
                requestStream.Write(buffer, 0, bytesRead);
                requestStream.Flush();
                BytesUploaded += bytesRead;
                tempTotal += bytesRead;
                if (UploadProgressChanged != null)
                {
                    int percent = (int)(((double)BytesUploaded / (double)FileLength) * 100);
                    UploadProgressChangedEventArgs args =
                        new UploadProgressChangedEventArgs(percent, bytesRead, BytesUploaded, FileLength, this.Name);
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        UploadProgressChanged(this, args);
                    });
                }
            }

            requestStream.Close();
            webrequest.BeginGetResponse(new AsyncCallback(ReadCallback), webrequest);

        }

        void ReadCallback(IAsyncResult asynchronousResult)
        {

            HttpWebRequest webrequest = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)webrequest.EndGetResponse(asynchronousResult);
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responsestring = reader.ReadToEnd();
            reader.Close();

            if (this.BytesUploaded < this.FileLength)
                this.UploadFileEx();
            else
            {
                this.Status = FileUploadStatus.Complete;
            }

        }

        void OnPropertyChanged(string propertyName)
        {
            this.Dispatcher.BeginInvoke(delegate()
           {
               if (PropertyChanged != null)
                   PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
           });
        }
    }
}
