using System;
using System.Windows.Input;

namespace MediaInTheCloud
{
    public class RelayCommand : ICommand
    {
        Action operation;

        public RelayCommand(Action operation)
        {
            this.operation = operation;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            operation.Invoke();
        }
    }
}
