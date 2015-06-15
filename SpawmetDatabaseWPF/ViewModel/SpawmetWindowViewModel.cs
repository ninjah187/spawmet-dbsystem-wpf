using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Events;

namespace SpawmetDatabaseWPF.ViewModel
{
    public abstract class SpawmetWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event ElementSelectedEventHandler<IModelElement> ElementSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        private ISpawmetWindow _window;
        protected WindowConfig WindowConfig { get; private set; }

        protected SpawmetDBContext DbContext = new SpawmetDBContext();
        protected object DbContextLock = new object();

        private string _searchExpression;
        public string SearchExpression
        {
            get { return _searchExpression; }
            set
            {
                if (_searchExpression != value)
                {
                    _searchExpression = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveDbStateCommand { get; protected set; }

        public ICommand NewMachinesWindowCommand { get; protected set; }

        public ICommand NewPartsWindowCommand { get; protected set; }

        public ICommand NewOrdersWindowCommand { get; protected set; }

        public ICommand NewClientsWindowCommand { get; protected set; }

        public ICommand NewDeliveriesWindowCommand { get; protected set; }

        public abstract ICommand RefreshCommand { get; protected set; }

        public virtual ICommand NewSearchWindowCommand { get; protected set; }

        private ICommand _cellEditEndingCommand;
        public ICommand CellEditEndingCommand
        {
            get { return _cellEditEndingCommand; }
            protected set
            {
                if (_cellEditEndingCommand != value)
                {
                    _cellEditEndingCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        protected SpawmetWindowViewModel(ISpawmetWindow window)
            : this(window, null)
        {
        }

        protected SpawmetWindowViewModel(ISpawmetWindow window, WindowConfig config)
        {
            _window = window;

            if (config == null)
            {
                return;
            }

            _window.Left = config.Left;
            _window.Top = config.Top;
            _window.Width = config.Width;
            _window.Height = config.Height;
            _window.WindowState = config.WindowState;

            WindowConfig = config;

            SearchExpression = "";

            _window.DataGrid.CellEditEnding += CellEditEndingHandler;
            _window.DataGrid.RowEditEnding += RowEditEndingHandler;
        }

        public virtual void Dispose()
        {
            DbContext.Dispose();
        }

        protected virtual void InitializeCommands()
        {
            SaveDbStateCommand = new Command(() =>
            {
                _window.CommitEdit();
                DbContext.SaveChanges();
            });

            const int offset = 40;
            var config = new WindowConfig()
            {
                Left = _window.Left + offset,
                Top = _window.Top + offset
            };

            NewMachinesWindowCommand = new Command(() =>
            {
                new MachinesWindow(config).Show();
            });

            NewPartsWindowCommand = new Command(() =>
            {
                new PartsWindow(config).Show();
            });

            NewOrdersWindowCommand = new Command(() =>
            {
                new OrdersWindow(config).Show();
            });

            NewClientsWindowCommand = new Command(() =>
            {
                new ClientsWindow(config).Show();
            });

            NewDeliveriesWindowCommand = new Command(() =>
            {
                new DeliveriesWindow(config).Show();
            });

            CellEditEndingCommand = new Command(() =>
            {
                var command = CellEditEndingCommand;

                CellEditEndingCommand = null;
                SaveDbStateCommand.Execute(null);

                CellEditEndingCommand = command;
            });

            NewSearchWindowCommand = new Command(() =>
            {
                throw new NotImplementedException();
            });
        }

        public abstract void Load();

        protected virtual WindowConfig GetWindowConfig()
        {
            var config = new WindowConfig()
            {
                Left = _window.Left,
                Top = _window.Top,
                Width = _window.Width,
                Height = _window.Height,
                WindowState = _window.WindowState,
            };

            return config;
        }

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

        protected void CellEditEndingHandler(object sender, DataGridCellEditEndingEventArgs e)
        {
            // remove handler to avoid stack overflow because SaveDbStateCommand invokes DataGrid.CommitEdit() which in turn invokes CellEditEnding event.
            _window.DataGrid.CellEditEnding -= CellEditEndingHandler;
            SaveDbStateCommand.Execute(null);
            _window.DataGrid.CellEditEnding += CellEditEndingHandler;
        }

        protected void RowEditEndingHandler(object sender, DataGridRowEditEndingEventArgs e)
        {
            _window.DataGrid.RowEditEnding -= RowEditEndingHandler;
            SaveDbStateCommand.Execute(null);
            _window.DataGrid.RowEditEnding += RowEditEndingHandler;
        }
    }
}
