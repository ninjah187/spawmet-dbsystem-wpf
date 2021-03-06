﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpawmetDatabaseWPF.Commands
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ExecuteCommand _whatExecute;
        private CanExecuteCommand _whenExecute;

        public Command(ExecuteCommand what)
            : this(what, null)
        {
        }

        public Command(ExecuteCommand what, CanExecuteCommand when)
        {
            _whatExecute = what;
            _whenExecute = when;
        }

        public void Execute(object parameter = null)
        {
            if (CanExecute(parameter))
            {
                _whatExecute();
            }
        }

        public bool CanExecute(object parameter = null)
        {
            if (_whenExecute == null)
            {
                return true;
            }
            else
            {
                return _whenExecute();
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

    //public abstract class Command : ICommand
    //{
    //    public event EventHandler CanExecuteChanged;

    //    protected Action _whatExecute;
    //    protected Func<bool> _whenExecute;

    //    public Command(Action what, Func<bool> when)
    //    {
    //        _whatExecute = what;
    //        _whenExecute = when;
    //    }

    //    public abstract bool CanExecute(object parameter);

    //    public abstract void Execute(object paremeter);

    //    protected void OnCanExecuteChanged()
    //    {
    //        if (CanExecuteChanged != null)
    //        {
    //            CanExecuteChanged(this, EventArgs.Empty);
    //        }
    //    }
    //}
}
