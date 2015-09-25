using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Events;

namespace SpawmetDatabaseWPF.ViewModel
{
    public abstract class SpawmetWindowViewModel : INotifyPropertyChanged, IDbContextChangesNotifier, IDisposable
    {
        public event ElementSelectedEventHandler<IModelElement> ElementSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<ConnectionState> ConnectionChanged;

        public event EventHandler DataGridEditCanceled;
        public event EventHandler DataGridEditCommited;

        public event EventHandler DbContextChanged;

        private ISpawmetWindow _window;
        protected WindowConfig WindowConfig { get; private set; }
        protected const int Offset = 40;

        internal SpawmetDBContext DbContext = new SpawmetDBContext();
        private const string connectionStringName = "SpawmetDBContext";
        internal object DbContextLock = new object();

        private Timer _connectionCheckTimer;

        public IModelElement SelectedElement { get; protected set; }

        private bool _isConnected;
        public virtual bool IsConnected
        {
            get { return _isConnected; }
            protected set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();

                    var state = _isConnected ? ConnectionState.Ok : ConnectionState.Lost;
                    OnConnectionChanged(state);
                }
            }
        }

        private volatile bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            set
            {
                if (_isSaving != value)
                {
                    _isSaving = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public DbContextMediator Mediator { get; set; } // observes and notifies to other windows any changes occured in DbContext

        public ICommand SaveDbStateCommand { get; protected set; }

        public ICommand NewMachinesWindowCommand { get; protected set; }

        public ICommand NewPartsWindowCommand { get; protected set; }

        public ICommand NewOrdersWindowCommand { get; protected set; }

        public ICommand NewClientsWindowCommand { get; protected set; }

        public ICommand NewDeliveriesWindowCommand { get; protected set; }

        public ICommand NewArchiveWindowCommand { get; protected set; }
        
        public ICommand NewPeriodsWindowCommand { get; protected set; }

        public virtual ICommand RefreshCommand { get; protected set; }

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

        public ICommand CancelEditCommand { get; protected set; }

        protected SpawmetWindowViewModel(ISpawmetWindow window)
            : this(window, null)
        {
        }

        protected SpawmetWindowViewModel(ISpawmetWindow window, WindowConfig config)
        {
            _window = window;

            _window.Closing += delegate
            {
                if (IsConnected == false)
                {
                    return;
                }

                _window.CommitEdit();

                lock (DbContextLock)
                {
                    DbContext.SaveChanges();
                }
            };

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

            //_window.DataGrid.CellEditEnding += CellEditEndingHandler;
            _window.DataGrid.RowEditEnding += RowEditEndingHandler;

            _connectionCheckTimer = new Timer(1000);
            _connectionCheckTimer.Elapsed += delegate
            {
                IsConnected = IsDatabaseServerAvailable();
            };
            _connectionCheckTimer.Start();

            this.ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    SearchExpression = "";
                }
            };

            Mediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            Mediator.ContextChanged += async (sender, notifier) =>
            {
                if (notifier == this)
                {
                    return;
                }
                Mediator.NotifyContextChange(this);
                //throw new Exception();

                await ReloadContextAsync(SelectedElement);
            };
        }

        public virtual void Dispose()
        {
            lock (DbContextLock)
            {
                DbContext.Dispose();
            }

            _connectionCheckTimer.Stop();
            _connectionCheckTimer.Close();
            _connectionCheckTimer.Dispose();
        }

        protected virtual void InitializeCommands()
        {
            SaveDbStateCommand = new Command(async () =>
            {
                _window.CommitEdit();

                //int rowsChangedCount = 0;
                IsSaving = true;
                await Task.Run(() =>
                {
                    lock (DbContextLock) // otherwise there's big chance to InvalidOperationException be thrown (unexpected connection state)
                    {
                        //rowsChangedCount = DbContext.SaveChanges();
                        DbContext.SaveChanges();
                    }
                });
                IsSaving = false;

                //if (rowsChangedCount != 0)
                //{
                //    Mediator.NotifyContextChange(this);
                //}
                //IsSaving = true;
                //var saveTask = DbContext.SaveChangesAsync();
                //await saveTask;
                //IsSaving = false;
                //MessageBox.Show("Save complete");
            });

            var config = new WindowConfig()
            {
                Left = _window.Left + Offset,
                Top = _window.Top + Offset
            };

            NewMachinesWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<MachinesWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new MachinesWindow(config).Show();
                }
            });

            NewPartsWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<PartsWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new PartsWindow(config).Show();
                }
            });

            NewOrdersWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<OrdersWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new OrdersWindow(config).Show();
                }
            });

            NewClientsWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<ClientsWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new ClientsWindow(config).Show();                    
                }
            });

            NewDeliveriesWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<DeliveriesWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new DeliveriesWindow(config).Show();                    
                }
            });

            NewArchiveWindowCommand = new Command(() =>
            {
                var windows = Application.Current.Windows.OfType<ArchiveWindow>();
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new ArchiveWindow(config).Show();
                }
            });

            NewPeriodsWindowCommand = new Command(() =>
            {
                var window = Application.Current.Windows.OfType<PeriodsWindow>().FirstOrDefault();
                if (window != null)
                {
                    window.Focus();
                }
                else
                {
                    new PeriodsWindow().Show();
                }
            });

            //CellEditEndingCommand = new Command(() =>
            //{
            //    var command = CellEditEndingCommand;

            //    CellEditEndingCommand = null;
            //    SaveDbStateCommand.Execute(null);

            //    CellEditEndingCommand = command;
            //});

            NewSearchWindowCommand = new Command(() =>
            {
                throw new NotImplementedException();
            });

            CancelEditCommand = new Command(() =>
            {
                _window.DataGrid.CancelEdit(DataGridEditingUnit.Row);
            });
        }

        public abstract void Load();

        public abstract Task LoadAsync();

        public abstract void SelectElement(IModelElement element);

        protected void FinishLoading()
        {
            IsConnected = true;

            if (WindowConfig.SelectedElement != null)
            {
                Application.Current.Dispatcher.Invoke(() => SelectElement(WindowConfig.SelectedElement));
            }
        }

        public void ReloadContext()
        {
            lock (DbContextLock)
            {
                DbContext.Dispose();
                DbContext = new SpawmetDBContext();
            }
            Load();
        }

        public async Task ReloadContextAsync()
        {
            await Task.Run(() =>
            {
                ReloadContext();
            });
        }

        public void ReloadContext(IModelElement element)
        {
            ReloadContext();
            SelectElement(element);
        }

        public virtual async Task ReloadContextAsync(IModelElement element)
        {
            await ReloadContextAsync();

            if (element != null)
            {
                SelectElement(element);
            }
        }

        public bool IsDatabaseServerAvailable()
        {
            using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings[connectionStringName]
                .ConnectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (MySqlException)
                {
                    return false;
                }
            }
        }

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

        protected void OnConnectionChanged(ConnectionState state)
        {
            if (ConnectionChanged != null)
            {
                ConnectionChanged(this, state);
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
            //if (e.EditAction == DataGridEditAction.Commit)
            //{
            //    OnDataGridEditCommited();
            //}
            //else // DataGridEditAction.Cancel
            //{
            //    OnDataGridEditCanceled();
            //}
            //throw new Exception("row edit ending");
            SaveDbStateCommand.Execute(null);
            _window.DataGrid.RowEditEnding += RowEditEndingHandler;
        }

        private void OnDataGridEditCanceled()
        {
            if (DataGridEditCanceled != null)
            {
                DataGridEditCanceled(this, EventArgs.Empty);
            }
        }

        private void OnDataGridEditCommited()
        {
            if (DataGridEditCommited != null)
            {
                DataGridEditCommited(this, EventArgs.Empty);
            }
        }

        private void OnDbContextChanged()
        {
            if (DbContextChanged != null)
            {
                DbContextChanged(this, EventArgs.Empty);
            }
        }
    }
}
