using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;

namespace InAndOutOfBrowser
{
    public partial class MainPage : UserControl
    {
        Application app = Application.Current;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.app.InstallStateChanged += new System.EventHandler(app_InstallStateChanged);
            this.app.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(app_CheckAndDownloadUpdateCompleted);
        }

        void app_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Oh noes! " + e.Error.Message);
            }
            else if (e.UpdateAvailable)
            {
                MessageBox.Show("An update was available and downloaded.");
            }
            else
            {
                MessageBox.Show("No update was available.");
            }

            this.UpdateUI();
        }

        void app_InstallStateChanged(object sender, System.EventArgs e)
        {
            this.UpdateUI();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateUI();
        }

        void UpdateUI()
        {
            this.runningInfo.Text = app.IsRunningOutOfBrowser ? "Out of Browser" : "In the Browser";
            this.installInfo.Text = app.InstallState.ToString();

            if (app.InstallState == InstallState.NotInstalled || app.InstallState == InstallState.InstallFailed)
            {
                this.installButton.IsEnabled = true;
                this.updateButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.installButton.IsEnabled = false;
                this.updateButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void InstallButtonClick(object sender, RoutedEventArgs e)
        {
            this.installButton.IsEnabled = false;
            bool result = this.app.Install();
            this.UpdateUI();
        }

        void SendBrowserMessage(string message)
        {
            if (!app.IsRunningOutOfBrowser)
            {
                HtmlPage.Window.Invoke("addBrowserMessage", message);
            }
        }

        void SendInputClick(object sender, RoutedEventArgs e)
        {
            this.SendBrowserMessage(this.input.Text);
        }

        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            this.app.CheckAndDownloadUpdateAsync();
        }
    }
}
