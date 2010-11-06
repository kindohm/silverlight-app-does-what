using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaInTheCloud
{
    public class AudioRecorder : AudioSink
    {
        public static readonly int MaxLength = 1048576;

        bool audioFormatSet;
        int count;
        MemoryStream stream;
        Dispatcher dispatcher;

        public AudioRecorder(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public AudioFormat AudioFormat { get; private set; }
        public byte[] Data { get; private set; }
        public bool Full { get; private set; }

        public void Record()
        {
            if (this.CaptureSource != null)
            {
                if (CaptureDeviceConfiguration.AllowedDeviceAccess ||
                    CaptureDeviceConfiguration.RequestDeviceAccess())
                {
                    if (this.RecorderStarted != null)
                        this.RecorderStarted(this, EventArgs.Empty);
                    this.CaptureSource.Start();
                }
            }
        }

        public void Stop()
        {
            this.ExecuteStop();
        }

        void ExecuteStop()
        {
            this.dispatcher.BeginInvoke(() =>
                {
                    if (this.CaptureSource != null)
                    {
                        this.CaptureSource.Stop();
                        if (this.RecorderStopped != null)
                            this.RecorderStopped(this, EventArgs.Empty);
                    }
                });
        }

        protected override void OnCaptureStarted()
        {
            this.audioFormatSet = false;
            this.Full = false;
            this.count = 0;
            this.Data = null;
            this.stream = new MemoryStream();
        }

        protected override void OnCaptureStopped()
        {
            // the classic MSDN implementation

            this.stream.Seek(0, SeekOrigin.Begin);
            this.Data = new byte[this.stream.Length];
            int chunkSize = 4096;
            int numBytesToRead = (int)stream.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                if (chunkSize + numBytesRead > this.Data.Length)
                {
                    chunkSize = this.Data.Length - numBytesRead;
                }
                int n = stream.Read(this.Data, numBytesRead, chunkSize);
                if (n == 0)
                {
                    break;
                }
                numBytesRead += n;
                numBytesToRead -= n;
            }
            this.stream.Close();
        }

        protected override void OnFormatChange(AudioFormat audioFormat)
        {
            if (this.audioFormatSet)
                throw new AudioFormatChangedException(
                    "Cannot change the audio format after it has already been assigned. I'm lazy like that.");
            this.audioFormatSet = true;
            this.AudioFormat = audioFormat;
        }

        protected override void OnSamples(long sampleTimeInHundredNanoseconds, long sampleDurationInHundredNanoseconds, byte[] sampleData)
        {
            if (!this.Full)
            {
                this.count += sampleData.Length;
                Debug.WriteLine(this.count.ToString());
                this.stream.Write(sampleData, 0, sampleData.Length);
                if (this.count > AudioRecorder.MaxLength)
                {
                    this.Full = true;
                    this.ExecuteStop();
                }
            }
        }

        public event EventHandler RecorderStarted;
        public event EventHandler RecorderStopped;
        public event EventHandler RecorderIsFull;
    }
}
