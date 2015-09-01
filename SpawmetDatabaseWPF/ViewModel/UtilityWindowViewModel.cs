using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase;

namespace SpawmetDatabaseWPF.ViewModel
{
    public abstract class UtilityWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler WorkStarted;
        public event EventHandler WorkCompleted;

        protected SpawmetWindowViewModel ParentViewModel;

        public ICommand DoWork { get; protected set; }

        protected UtilityWindowViewModel(SpawmetWindowViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;

            //_parentViewModel.ConnectionChanged += (sender, connectionState) =>
            //{
            //    if (connectionState.)
            //};
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnWorkStarted()
        {
            if (WorkStarted != null)
            {
                WorkStarted(this, EventArgs.Empty);
            }
        }

        protected void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }
    }
}
