using System.Windows;
using System.Windows.Controls;

namespace SilverlightApplication1
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            App.Current.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            MessageBox.Show("updated: " + e.UpdateAvailable.ToString());
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            App.Current.CheckAndDownloadUpdateAsync();
        }
    }
}
