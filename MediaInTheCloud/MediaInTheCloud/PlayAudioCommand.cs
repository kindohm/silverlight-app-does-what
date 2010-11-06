using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace MediaInTheCloud
{
    public class PlayAudioCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MediaStreamSource source = null;
            var app = (App)App.Current;
            var media = app.MediaElement;

            if (parameter is ServerMediaItem)
            {
                var mediaItem = (ServerMediaItem)parameter;
            }
            else if (parameter is AudioMediaItem)
            {
                var mediaItem = (AudioMediaItem)parameter;
                source = new InMemoryAudioPlayer(mediaItem.Data);
            }
            media.Stop();
            media.SetSource(source);
            media.Play();
        }

    }
}
