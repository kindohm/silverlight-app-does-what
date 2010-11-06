using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace SampleEditor
{
    public class AudioDecoder : MediaStreamSource
    {
        const int numSamples = 512;
        
        int samplesPerSecond = 44100;
        int channels = 2;
        int bitsPerSample = 16;
        int bufferByteCount;
        byte[] sourceData;
        int index;
        long startPosition;
        long currentPosition;
        long currentTimeStamp;
        int byteRate;
        short blockAlign;

        MemoryStream memoryStream;
        Dictionary<MediaSampleAttributeKeys, string> emptySampleDict =
            new Dictionary<MediaSampleAttributeKeys, string>();
        MediaStreamDescription mediaStreamDescription;

        public AudioDecoder(byte[] sourceData, AudioFormat format)
        {
            this.samplesPerSecond = format.SamplesPerSecond;
            this.bitsPerSample = format.BitsPerSample;
            this.channels = format.Channels;

            byteRate = samplesPerSecond * channels * bitsPerSample / 8;
            blockAlign = (short)(channels * (bitsPerSample / 8));
            this.sourceData = sourceData;
            this.memoryStream = new MemoryStream();
            bufferByteCount = channels *
                bitsPerSample / 8 * numSamples;
            this.AudioBufferLength = 100;
        }

        public int StartPoint { get; set; }
        public int EndPoint { get; set; }

        protected override void OpenMediaAsync()
        {
            startPosition = currentPosition = 0;

            Dictionary<MediaStreamAttributeKeys, string> streamAttributes = 
                new Dictionary<MediaStreamAttributeKeys, string>();
            Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = 
                new Dictionary<MediaSourceAttributesKeys, string>();
            List<MediaStreamDescription> availableStreams = 
                new List<MediaStreamDescription>();

            string format = "";
            format += ToLittleEndianString(string.Format("{0:X4}", 1));  //PCM
            format += ToLittleEndianString(string.Format("{0:X4}", channels));
            format += ToLittleEndianString(string.Format("{0:X8}", samplesPerSecond));
            format += ToLittleEndianString(string.Format("{0:X8}", byteRate));
            format += ToLittleEndianString(string.Format("{0:X4}", blockAlign));
            format += ToLittleEndianString(string.Format("{0:X4}", bitsPerSample));
            format += ToLittleEndianString(string.Format("{0:X4}", 0));

            streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = format;
            mediaStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);
            availableStreams.Add(mediaStreamDescription);
            sourceAttributes[MediaSourceAttributesKeys.Duration] = "0";
            sourceAttributes[MediaSourceAttributesKeys.CanSeek] = "false";
            ReportOpenMediaCompleted(sourceAttributes, availableStreams);
        }     

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            for (int i = 0; i < numSamples; i++)
            {

                if (this.index < this.StartPoint |
                    this.index > this.EndPoint)
                    this.index = this.StartPoint;

                memoryStream.WriteByte(this.sourceData[index]);
                memoryStream.WriteByte(this.sourceData[index + 1]);
                memoryStream.WriteByte(this.sourceData[index + 2]);
                memoryStream.WriteByte(this.sourceData[index + 3]);

                index += 4;
            }

            MediaStreamSample mediaStreamSample =
                new MediaStreamSample(
                    mediaStreamDescription, 
                    memoryStream, 
                    currentPosition,                                      
                    bufferByteCount, 
                    currentTimeStamp, 
                    emptySampleDict);

            currentTimeStamp += bufferByteCount * 10000000L / byteRate;
            currentPosition += bufferByteCount;

            ReportGetSampleCompleted(mediaStreamSample);
        }

        protected override void SeekAsync(long seekToTime)
        {
            this.ReportSeekCompleted(seekToTime);
        }

        protected override void CloseMedia()
        {
            startPosition = currentPosition = 0;
            mediaStreamDescription = null;
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        string ToLittleEndianString(string bigEndianString)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bigEndianString.Length; i += 2)
                builder.Insert(0, bigEndianString.Substring(i, 2));

            return builder.ToString();
        }


    }
}
