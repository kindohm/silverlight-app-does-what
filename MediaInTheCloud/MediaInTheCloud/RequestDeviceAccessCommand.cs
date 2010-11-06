using System;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaInTheCloud
{
    public class RequestDeviceAccessCommand : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }


        public void Execute(object parameter)
        {
            CaptureDeviceConfiguration.RequestDeviceAccess();
            if (this.Completed != null)
                this.Completed(this, EventArgs.Empty);

        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler Completed;
    }
}
