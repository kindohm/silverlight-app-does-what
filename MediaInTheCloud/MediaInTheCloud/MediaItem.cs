using System.ComponentModel;
using System.Windows.Media;
using FluxJpeg.Core;
using System.IO;
using FluxJpeg.Core.Encoder;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace MediaInTheCloud
{
    public class MediaItem : INotifyPropertyChanged
    {
        bool isUploading;
        int uploadProgressPercent;

        public ICommand PlayAudio { get { return new PlayAudioCommand(); } }

        public string Name { get; set; }
        public virtual ImageSource DisplayImage { get; set; }
        public bool IsAudio { get; set; }
        public bool IsLocalAudio { get; set; }

        public byte[] Data
        {
            get;
            set; 
        }

        public int UploadProgressPercent
        {
            get
            {
                return this.uploadProgressPercent;
            }
            set
            {
                this.uploadProgressPercent = value;
                this.OnPropertyChanged("UploadProgressPercent");
            }
        }

        public bool IsUploading
        {
            get
            {
                return this.isUploading;
            }
            set
            {
                this.isUploading = value;
                this.OnPropertyChanged("IsUploading");
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static byte[] GetJpg(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bands = 3;
            byte[][,] raster = new byte[bands][,];

            for (int i = 0; i < bands; i++)
            {
                raster[i] = new byte[width, height];
            }

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int pixel = bitmap.Pixels[width * row + column];
                    raster[0][column, row] = (byte)(pixel >> 16);
                    raster[1][column, row] = (byte)(pixel >> 8);
                    raster[2][column, row] = (byte)pixel;
                }
            }

            ColorModel model = new ColorModel { colorspace = ColorSpace.RGB };
            FluxJpeg.Core.Image img = new FluxJpeg.Core.Image(model, raster);
            MemoryStream stream = new MemoryStream();
            JpegEncoder encoder = new JpegEncoder(img, 90, stream);
            encoder.Encode();

            stream.Seek(0, SeekOrigin.Begin);
            byte[] binaryData = new byte[stream.Length];
            long bytesRead = stream.Read(binaryData, 0, (int)stream.Length);

            return binaryData;
        }
    }
}
