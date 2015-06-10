using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.Events;

namespace SpawmetDatabaseWPF.ViewModel
{
    public abstract class SpawmetWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event ElementSelectedEventHandler<IModelElement> ElementSelected;

        private ISpawmetWindow _window;

        public event PropertyChangedEventHandler PropertyChanged;

        protected SpawmetDBContext DbContext = new SpawmetDBContext();
        protected object DbContextLock = new object();

        public ICommand SaveDbStateCommand { get; protected set; }

        public ICommand NewMachinesWindowCommand { get; protected set; }

        public ICommand NewPartsWindowCommand { get; protected set; }

        public ICommand NewOrdersWindowCommand { get; protected set; }

        public ICommand NewClientsWindowCommand { get; protected set; }

        public ICommand NewDeliveriesWindowCommand { get; protected set; }

        public abstract ICommand RefreshCommand { get; protected set; }

        public SpawmetWindowViewModel(ISpawmetWindow window)
        {
            _window = window;
        }

        public virtual void Dispose()
        {
            DbContext.Dispose();
        }

        protected virtual void InitializeCommands()
        {
            SaveDbStateCommand = new Command(() =>
            {
                DbContext.SaveChanges();
            });

            NewMachinesWindowCommand = new Command(() =>
            {
                new MachinesWindow(_window.Left + 10, _window.Top + 10).Show();
            });

            NewPartsWindowCommand = new Command(() =>
            {
                new PartsWindow(_window.Left + 10, _window.Top + 10).Show();
            });

            NewOrdersWindowCommand = new Command(() =>
            {
                new OrdersWindow(_window.Left + 10, _window.Top + 10).Show();
            });

            NewClientsWindowCommand = new Command(() =>
            {
                new ClientsWindow(_window.Left + 10, _window.Top + 10).Show();
            });

            NewDeliveriesWindowCommand = new Command(() =>
            {
                new DeliveriesWindow(_window.Left + 10, _window.Top + 10).Show();
            });
        }

        public abstract void Load();

        // [CallerMemberName] tag lets miss argument propertyName in OnPropertyChagned method
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnElementSelected(IModelElement element)
        {
            if (ElementSelected != null)
            {
                ElementSelected(this, new ElementSelectedEventArgs<IModelElement>(element));
            }
        }
    }
}
