
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace MediaInTheCloud
{
    public class ServerMediaItem : MediaItem
    {
        ImageSource displayImage;

        public string Url
        {
            get;
            set;
        }

        public override ImageSource DisplayImage
        {
            get
            {
                if (this.displayImage == null)
                {
                    if (!this.IsAudio)
                    {
                        var uri = new Uri(this.Url);
                        this.displayImage = new BitmapImage(uri);
                    }
                    else
                    {
                        var uri = new Uri("/MediaInTheCloud;component/Sound_Icon.jpg", UriKind.Relative);
                        this.displayImage = new BitmapImage(uri);
                    }
                }
                return this.displayImage;
            }
        }

        public override bool Equals(object obj)
        {

            if (obj is ServerMediaItem)
            {
                return this.Name == ((ServerMediaItem)obj).Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
