using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SampleEditor
{
    public class SupportedFormat
    {
        AudioFormat format;

        public SupportedFormat(AudioFormat format)
        {
            this.format = format;
            this.FriendlyName =
                string.Format("{0}, {1}, {2}",
                this.format.SamplesPerSecond,
                this.format.BitsPerSample,
                this.format.Channels);
        }

        public AudioFormat Format
        {
            get { return this.format; }
        }

        public string FriendlyName
        {
            get;
            private set;
        }
    }

    public static class Extensions
    {
        public static ObservableCollection<SupportedFormat> GetFriendlySupportedFormats(this AudioCaptureDevice device)
        {
            var list = new ObservableCollection<SupportedFormat>();
            foreach (var format in device.SupportedFormats)
            {
                if (format.Channels == 2 & format.BitsPerSample == 16)
                {
                    list.Add(new SupportedFormat(format));
                }
            }
            return list;
        }
    }
}
