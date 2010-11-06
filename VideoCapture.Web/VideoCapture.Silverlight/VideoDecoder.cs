using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace VideoCapture.Silverlight
{
    public class VideoDecoder : MediaStreamSource
    {

        //MemoryStream _videoStream;
        MemoryStream inputStream;
        int _frameTime;
        int sampleLength;
        VideoFormat format;

        MediaStreamDescription _videoDescription;

        long currentPosition;
        long _currentVideoTimeStamp;

        readonly Dictionary<MediaSampleAttributeKeys, string> _emptySampleDict = new Dictionary<MediaSampleAttributeKeys, string>();

        public VideoDecoder(MemoryStream inputStream, VideoFormat format, int sampleLength)
        {
            this.format = format;
            this.sampleLength = sampleLength;
            this.inputStream = inputStream;
            this.inputStream.Seek(0, SeekOrigin.Begin);
        }

        void GetVideoSample()
        {
            // seems like creating a new stream is only way to avoid out of memory and
            // actually figure out the correct offset. that can't be right.
            MemoryStream _frameStream = new MemoryStream();

            byte[] buffer = new byte[this.sampleLength];
            //_videoStream.Read(buffer, 0, sampleSize);
            this.inputStream.Read(buffer, 0, this.sampleLength);
            _frameStream.Write(buffer, 0, buffer.Length);

            // Send out the next sample
            MediaStreamSample msSamp = new MediaStreamSample(
                _videoDescription,
                _frameStream,
                0,
                this.sampleLength,
                _currentVideoTimeStamp,
                _emptySampleDict);

            _currentVideoTimeStamp += _frameTime;


            ReportGetSampleCompleted(msSamp);
        }


        protected override void CloseMedia()
        {
            // no implementation needed
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            switch (mediaStreamType)
            {
                case MediaStreamType.Video:
                    GetVideoSample();
                    break;
                case MediaStreamType.Audio:
                    Debug.WriteLine("audio?!");
                    break;
            }
        }

        protected override void OpenMediaAsync()
        {
            _frameTime = (int)TimeSpan.FromSeconds((double)1 / 30).Ticks;
            

            // Init
            Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            List<MediaStreamDescription> availableStreams = new List<MediaStreamDescription>();

            // Stream Description 
            Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

            streamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            streamAttributes[MediaStreamAttributeKeys.Height] = format.PixelHeight.ToString();
            streamAttributes[MediaStreamAttributeKeys.Width] = format.PixelWidth.ToString();

            MediaStreamDescription msd = new MediaStreamDescription(MediaStreamType.Video, streamAttributes);

            _videoDescription = msd;
            availableStreams.Add(_videoDescription);

            // a zero timespan is an infinite video
            sourceAttributes[MediaSourceAttributesKeys.Duration] =
                TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);
            sourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            ReportOpenMediaCompleted(sourceAttributes, availableStreams);
        }

        protected override void SeekAsync(long seekToTime)
        {
            currentPosition = seekToTime;
            ReportSeekCompleted(currentPosition);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }
    }
}
