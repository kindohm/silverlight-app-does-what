
using System;
using System.Windows.Media;
namespace MediaInTheCloud
{
    public class PictureTaker
    {
        CaptureSource captureSource;

        public event EventHandler<CaptureImageCompletedEventArgs> TakePictureCompleted;

        public CaptureSource CaptureSource
        {
            get
            {
                return this.captureSource;
            }
            set
            {
                if (this.captureSource != null)
                    this.captureSource.CaptureImageCompleted -=
                        CaptureImageCompleted;

                this.captureSource = value;
                this.captureSource.CaptureImageCompleted +=
                    new EventHandler<CaptureImageCompletedEventArgs>(CaptureImageCompleted);
            }
        }

        void CaptureImageCompleted(object sender,
            CaptureImageCompletedEventArgs e)
        {
            if (this.TakePictureCompleted != null)
                this.TakePictureCompleted(this, e);
        }

        public void TakePicture()
        {
            if (this.CaptureSource != null)
                this.CaptureSource.CaptureImageAsync();
        }
    }
}
