using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class ClientsWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler OrdersStartLoading;
        public event EventHandler OrdersCompletedLoading;

        private BackgroundWorker _ordersBackgroundWorker;

        private ClientsWindow _window;

        private ObservableCollection<Client> _clients;
        public ObservableCollection<Client> Clients
        {
            get { return _clients; }
            set
            {
                if (_clients != value)
                {
                    _clients = value;
                    OnPropertyChanged();
                }
            }
        }

        private Client _selectedClient;

        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged();
                    OnElementSelected(_selectedClient);
                    LoadOrders();
                }
            }
        }

        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get { return _orders; }
            set
            {
                if (_orders != value)
                {
                    _orders = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddClientCommand { get; private set; }

        public ICommand DeleteClientsCommand { get; private set; }
        
        public override ICommand RefreshCommand { get; protected set; }

        public ClientsWindowViewModel(ClientsWindow window)
            : base(window)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();

            Load();
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposeBackgroundWorkers();
        }

        private void DisposeBackgroundWorkers()
        {
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }
        }

        private void InitializeBackgroundWorkers()
        {
            DisposeBackgroundWorkers();

            _ordersBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var clientId = (int) e.Argument;
                List<Order> result;
                using (var context = new SpawmetDBContext())
                {
                    result = DbContext.Orders
                        .Where(o => o.Client.Id == clientId)
                        .Include(o => o.Client)
                        .Include(o => o.Machine)
                        .OrderBy(o => o.Id)
                        .ToList();
                    e.Result = result;
                }
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Order>) e.Result;
                Orders = new ObservableCollection<Order>(source);

                OnOrdersCompletedLoading();
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AddClientCommand = new Command(() =>
            {
                var win = new AddClientWindow(DbContext);
                win.ClientAdded += (sender, client) =>
                {
                    Clients.Add(client);
                };
                win.Show();
            });

            DeleteClientsCommand = new Command(() =>
            {
                var selected = GetSelectedClients();
                if (selected == null)
                {
                    return;
                }

                var win = new DeleteClientWindow(DbContext, selected);
                win.ClientsDeleted += (sender, clients) =>
                {
                    foreach (var client in clients)
                    {
                        Clients.Remove(client);
                    }
                };
                win.WorkCompleted += delegate
                {
                    Orders = null;
                };
                win.Show();
            });

            RefreshCommand = new Command(() =>
            {
                var win = new ClientsWindow(_window.Left, _window.Top);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });
        }

        public override void Load()
        {
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = DbContext.Clients.ToList();

            Clients = new ObservableCollection<Client>(clients);
        }

        private void LoadOrders()
        {
            var client = SelectedClient;

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(client.Id);

                OnOrdersStartLoading();
            }
        }

        private List<Client> GetSelectedClients()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Client>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Client)item);
            }

            return selected;
        }

        #region Event invokers.

        private void OnOrdersStartLoading()
        {
            if (OrdersStartLoading != null)
            {
                OrdersStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnOrdersCompletedLoading()
        {
            if (OrdersCompletedLoading != null)
            {
                OrdersCompletedLoading(this, EventArgs.Empty);
            }
        }

        #endregion

    }
}
