using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Windows;
using SpawmetDatabaseWPF.Windows.Searching;
using Application = System.Windows.Application;

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
                    SelectedElement = _selectedClient;
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

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddClientCommand { get; private set; }

        public ICommand DeleteClientsCommand { get; private set; }
        
        public override ICommand RefreshCommand { get; protected set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public ICommand GoToOrderCommand { get; protected set; }

        public ClientsWindowViewModel(ClientsWindow window)
            : this(window, null)
        {
        }

        public ClientsWindowViewModel(ClientsWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();

            ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    Clients = null;
                    Orders = null;
                    OnElementSelected(null);
                }
                else
                {
                    if (Clients == null)
                    {
                        Load();
                    }
                }
            };
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
                    result = context.Orders
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

            //DeleteClientsCommand = new Command(() =>
            //{
            //    var selected = GetSelectedClients();
            //    if (selected == null)
            //    {
            //        return;
            //    }

            //    string msg = selected.Count == 1
            //        ? "Czy chcesz usunąć zaznaczoną część?"
            //        : "Czy chcesz usunąć zaznaczone części?";

            //    var confirmWin = new ConfirmWindow(msg);
            //    confirmWin.Confirmed += delegate
            //    {
            //        var win = new DeleteClientWindow(DbContext, selected);
            //        win.ClientsDeleted += (sender, clients) =>
            //        {
            //            foreach (var client in clients)
            //            {
            //                Clients.Remove(client);
            //            }
            //        };
            //        win.WorkCompleted += delegate
            //        {
            //            Orders = null;

            //            OnElementSelected(null);
            //        };
            //        win.ConnectionLost += (sender, e) =>
            //        {
            //            IsConnected = false;
            //        };
            //        win.Owner = _window;
            //        win.ShowDialog();
            //    };
            //    confirmWin.Show();
            //});

            #region DeleteClients
            DeleteClientsCommand = new Command(() =>
            {
                var clients = GetSelectedClients();
                if (clients == null)
                {
                    return;
                }

                var confirmWin = new ConfirmWindow("Czy na pewno chcesz usunąć zaznaczonych klientów?");
                confirmWin.Confirmed += async delegate
                {
                    var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie...");
                    waitWin.Show();

                    foreach (var client in clients)
                    {
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                foreach (var order in client.Orders)
                                {
                                    var additionalParts = order.AdditionalPartSet;
                                    DbContext.AdditionalPartSets.RemoveRange(additionalParts);

                                    order.MachineModules.Clear();

                                    DbContext.Orders.Remove(order);
                                }

                                DbContext.Clients.Remove(client);
                                DbContext.SaveChanges();
                            }
                        });
                        Clients.Remove(client);
                    }

                    Orders = null;

                    Mediator.NotifyContextChange(this);
                    waitWin.Close();
                };

                confirmWin.Show();
            });
            #endregion

            RefreshCommand = new Command(async () =>
            {
                //SaveDbStateCommand.Execute(null);

                _window.CommitEdit();

                IsSaving = true;
                await Task.Run(() =>
                {
                    lock (DbContextLock)
                    {
                        DbContext.SaveChanges();
                    }
                });
                IsSaving = false;

                var config = GetWindowConfig();
                var win = new ClientsWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchClientsWindow(_window, DbContext);
                win.WorkCompleted += (sender, clients) =>
                {
                    Clients = new ObservableCollection<Client>(clients);

                    Orders = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie", win);
                };
                win.Show();
            });

            GoToOrderCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (SelectedOrder == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<OrdersWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(order);
                }
                else
                {
                    var config = new WindowConfig()
                    {
                        Left = _window.Left + Offset,
                        Top = _window.Top + Offset,
                        SelectedElement = order
                    };
                    var window = new OrdersWindow(config);
                    window.Show();
                }
            });
        }

        public override void Load()
        {
            LoadClients();

            FinishLoading();
        }

        public override async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                LoadClients();
            });

            FinishLoading();
        }

        private void LoadClients()
        {
            List<Client> clients = null;
            lock (DbContextLock)
            {
                clients = DbContext.Clients.ToList();
            }

            Clients = new ObservableCollection<Client>(clients);
        }

        private void LoadOrders()
        {
            var client = SelectedClient;

            if (client == null)
            {
                Orders = null;
                return;
            }

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(client.Id);

                OnOrdersStartLoading();
            }
        }

        public override void SelectElement(IModelElement element)
        {
            var client = Clients.Single(e => e.Id == element.Id);

            SelectedClient = client;

            _window.DataGrid.SelectedItem = client;
            _window.DataGrid.ScrollIntoView(client);
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
