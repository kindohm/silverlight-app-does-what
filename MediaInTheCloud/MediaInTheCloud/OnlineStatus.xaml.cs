using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;

namespace MediaInTheCloud
{
    public partial class OnlineStatus : UserControl
    {
        const double OfflineOpacity = .1;
        const double OnlineOpacity = 1;

        public OnlineStatus()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnlineStatus_Loaded);
        }

        void OnlineStatus_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateDisplay();
            NetworkChange.NetworkAddressChanged += ((s,args) =>{
                this.UpdateDisplay();
            });
        }

        void UpdateDisplay()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.onlineBulb.Opacity = OnlineStatus.OnlineOpacity;
                this.offlineBulb.Opacity = OnlineStatus.OfflineOpacity;
            }
            else
            {
                this.onlineBulb.Opacity = OnlineStatus.OfflineOpacity;
                this.offlineBulb.Opacity = OnlineStatus.OnlineOpacity;
            }
        }  
                
    }
}
