using System.Diagnostics;
using System.Windows.Controls;

namespace MediaInTheCloud
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public MediaElement MediaElement { get { return this.media; } }

    }
}
