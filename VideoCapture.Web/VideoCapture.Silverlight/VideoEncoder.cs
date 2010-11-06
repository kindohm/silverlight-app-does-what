using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace VideoCapture.Silverlight
{
    public class VideoEncoder : VideoSink
    {
        static readonly int MaxLength = 524288000; // 500 MB

        MemoryStream stream;

        public VideoFormat Format { get; set; }
        public int SampleLength { get; set; }

        public MemoryStream Stream
        {
            get { return this.stream; }
        }

        protected override void OnCaptureStarted()
        {
            this.Format = this.CaptureSource.VideoCaptureDevice.DesiredFormat;
            this.stream = new MemoryStream();
        }

        protected override void OnCaptureStopped()
        {
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        protected override void OnFormatChange(VideoFormat format)
        {
            this.Format = format;
            Debug.WriteLine(this.Format.FramesPerSecond.ToString());
        }

        protected override void OnSample(
            long sampleTimeInHundredNanoseconds, 
            long sampleDurationInHundredNanoseconds, 
            byte[] sampleData)
        {
            if (this.stream.Length <= MaxLength) // avoid OutOfMemoryExceptions
            {
                this.SampleLength = sampleData.Length;
                for (int h = 0; h < this.Format.PixelHeight; h++)
                {
                    int startIndex = 0;

                    // flip the image upside down if necessary
                    
                    if (this.Format.Stride < 0)
                    {
                        startIndex = sampleData.Length + (this.Format.Stride * (h + 1));
                    }
                    else
                    {
                        startIndex = this.Format.Stride * h;
                    }

                    int bytesToRead = Math.Abs(this.Format.Stride);
                    this.stream.Write(sampleData, startIndex, bytesToRead);
                }

            }

        }
    }

}

