using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Json;

namespace MediaInTheCloud
{
    public class ServerMonitor
    {
        ObservableCollection<ServerMediaItem> serverItems;

        public ServerMonitor(ObservableCollection<ServerMediaItem> serverItems)
        {
            this.serverItems = serverItems;
        }

        public void Download()
        {
            var client = new WebClient();
            var uri = new Uri("http://localhost/MediaInTheCloud.Host/Home/GetServerItems");
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            client.OpenReadAsync(uri);
        }

        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debug.WriteLine(e.Error.ToString());
            }
            else
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ServerMediaItem>));
                object result = serializer.ReadObject(e.Result);
                var items = (IEnumerable<ServerMediaItem>)result;
                foreach (var item in items)
                {
                    if (!this.serverItems.Contains(item))
                    {
                        this.serverItems.Add(item);
                    }
                }
                if (DownloadComplete != null)
                    this.DownloadComplete(this, EventArgs.Empty);
            }
        }

        public event EventHandler DownloadComplete;
    }
}
