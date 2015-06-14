using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpawmetDatabaseWPF.Commands
{
    public class ParamCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ExecuteCommand<T> _whatExecute;
        private CanExecuteCommand<T> _whenExecute;

        public ParamCommand(ExecuteCommand<T> what)
            : this(what, null)
        {
        }

        public ParamCommand(ExecuteCommand<T> what, CanExecuteCommand<T> when)
        {
            _whatExecute = what;
            _whenExecute = when;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                var par = (T) parameter;
                _whatExecute(par);
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_whenExecute == null)
            {
                return true;
            }
            else
            {
                var par = (T) parameter;
                return _whenExecute(par);
            }
        }

        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
