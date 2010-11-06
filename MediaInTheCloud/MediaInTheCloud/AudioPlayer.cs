using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MediaInTheCloud
{
    public class InMemoryAudioPlayer : MediaStreamSource
    {
        MediaStreamDescription mediaStreamDescription;
        long startPosition;
        long currentPosition;
        long currentTimeStamp;
        int byteRate;
        short blockAlign;
        MemoryStream memoryStream;
        Dictionary<MediaSampleAttributeKeys, string> emptySampleDict =
            new Dictionary<MediaSampleAttributeKeys, string>();
        int bufferByteCount;
        const int numSamples = 512;
        byte[] sourceData;
        Random random = new Random();

        int SamplesPerSecond = 44100;
        int Channels = 2;
        int BitsPerSample = 16;

        int index;

        public InMemoryAudioPlayer(byte[] sourceData)
        {
            byteRate = SamplesPerSecond * Channels * BitsPerSample / 8;
            blockAlign = (short)(Channels * (BitsPerSample / 8));
            this.sourceData = sourceData;
            this.memoryStream = new MemoryStream();
            bufferByteCount = Channels *
                BitsPerSample / 8 * numSamples;
            this.AudioBufferLength = 1000;
        }

        protected override void OpenMediaAsync()
        {
            startPosition = currentPosition = 0;

            Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            List<MediaStreamDescription> availableStreams = new List<MediaStreamDescription>();

            string format = "";
            format += ToLittleEndianString(string.Format("{0:X4}", 1));  //PCM
            format += ToLittleEndianString(string.Format("{0:X4}", Channels));
            format += ToLittleEndianString(string.Format("{0:X8}", SamplesPerSecond));
            format += ToLittleEndianString(string.Format("{0:X8}", byteRate));
            format += ToLittleEndianString(string.Format("{0:X4}", blockAlign));
            format += ToLittleEndianString(string.Format("{0:X4}", BitsPerSample));
            format += ToLittleEndianString(string.Format("{0:X4}", 0));

            streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = format;
            mediaStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);
            availableStreams.Add(mediaStreamDescription);

            long duration = this.sourceData.Length * 10000000L / byteRate;

            sourceAttributes[MediaSourceAttributesKeys.Duration] = duration.ToString();
            sourceAttributes[MediaSourceAttributesKeys.CanSeek] = "false";
            ReportOpenMediaCompleted(sourceAttributes, availableStreams);
        }


        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            if (index < this.sourceData.Length)
            {
                for (int i = 0; i < numSamples; i++)
                {
                    if (index >= this.sourceData.Length)
                        break;

                    memoryStream.WriteByte(
                        this.sourceData[index]);
                    memoryStream.WriteByte(
                        this.sourceData[index + 1]);
                    index += 2;

                    if (this.Channels == 2)
                    {
                        memoryStream.WriteByte(
                            this.sourceData[index]);
                        memoryStream.WriteByte(
                            this.sourceData[index + 1]);
                        index += 2;
                    }


                }

                MediaStreamSample mediaStreamSample =
                    new MediaStreamSample(mediaStreamDescription, memoryStream, currentPosition,
                                          bufferByteCount, currentTimeStamp, emptySampleDict);

                currentTimeStamp += bufferByteCount * 10000000L / byteRate;
                currentPosition += bufferByteCount;

                ReportGetSampleCompleted(mediaStreamSample);
            }
            else
            {
                this.CloseMedia();
            }

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
            var builder = new StringBuilder();

            for (int i = 0; i < bigEndianString.Length; i += 2)
                builder.Insert(0, bigEndianString.Substring(i, 2));

            return builder.ToString();
        }


    }
}
