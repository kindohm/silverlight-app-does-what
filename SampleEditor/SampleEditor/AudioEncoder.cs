using System;
using System.IO;
using System.Windows.Media;

namespace SampleEditor
{
    public class AudioEncoder : AudioSink
    {
        public const int MaxLength = 1048576;
        MemoryStream stream;
        byte[] data;
        int count = 0;

        public AudioFormat Format
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get { return this.data; }
        }

        public bool Full
        {
            get;
            set;
        }

        public int Size
        {
            get { return this.count; }
        }

        protected override void OnCaptureStarted()
        {
            this.Format = null;
            this.stream = new MemoryStream();
        }

        protected override void OnCaptureStopped()
        {
            this.stream.Seek(0, SeekOrigin.Begin);
            this.data = new byte[this.stream.Length];
            int chunkSize = 4096;
            int numBytesToRead = (int)stream.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                if (chunkSize + numBytesRead > this.data.Length)
                {
                    chunkSize = this.data.Length - numBytesRead;
                }
                int n = stream.Read(this.data, numBytesRead, chunkSize);
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
            if (this.Format != null)
            {
                throw new InvalidOperationException("Cannot change the audio format after it has already been set.");
            }

            this.Format = audioFormat;
        }

        protected override void OnSamples(long sampleTimeInHundredNanoseconds, long sampleDurationInHundredNanoseconds, byte[] sampleData)
        {
            if (!this.Full)
            {
                this.count += sampleData.Length;
                this.stream.Write(sampleData, 0, sampleData.Length);
                if (this.count > AudioEncoder.MaxLength)
                {
                    this.Full = true;
                    if (this.EncoderIsFull != null)
                        this.EncoderIsFull(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler EncoderIsFull;
    }
}
