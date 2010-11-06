using System.ComponentModel;
using System.Windows.Media;

namespace VideoList
{
    public class VideoItem : INotifyPropertyChanged
    {
        Brush brush;
        string name;

        public Brush Brush
        {
            get
            {
                return this.brush;
            }
            set
            {
                this.brush = value;
                this.OnPropertyChanged("Brush");
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public VideoCaptureDevice Device
        {
            get;
            set;
        }

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this,
                     new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
