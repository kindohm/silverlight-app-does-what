using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkAwareness
{
    public partial class MainPage : UserControl
    {
        ObservableCollection<string> historyItems = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.history.ItemsSource = historyItems;

            NetworkChange.NetworkAddressChanged += 
                new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
            
            
            this.RunDetection();
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            this.RunDetection();
        }

        void RunDetection()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.exclamationBox.Text = "FTW! ";
                this.infoBox.Text = "Available";
                this.infoBox.Foreground = new SolidColorBrush(Colors.Green);
                this.AddHistory(true);
            }
            else
            {
                this.exclamationBox.Text = "Oh noes! ";
                this.infoBox.Text = "Unavailable";
                this.infoBox.Foreground = new SolidColorBrush(Colors.Red);
                this.AddHistory(false);
            }
        }

        void AddHistory(bool available)
        {
            string status = available ? "Available" : "Unavailable";
            this.historyItems.Insert(0, 
                string.Format("Network changed to '{0}' at {1}\n", status, DateTime.Now.ToString()));
        }

    }
}
