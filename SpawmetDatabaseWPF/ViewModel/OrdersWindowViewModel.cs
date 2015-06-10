using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class OrdersWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler PartSetStartLoading;
        public event EventHandler PartSetCompletedLoading;

        private BackgroundWorker _partsBackgroundWorker;

        private OrdersWindow _window;

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
                    OnElementSelected(_selectedOrder);
                    LoadAdditionalPartSet();
                }
            }
        }

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

        private ObservableCollection<Machine> _machines;
        public ObservableCollection<Machine> Machines
        {
            get { return _machines; }
            set
            {
                if (_machines != value)
                {
                    _machines = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<AdditionalPartSetElement> _additionalPartSet;
        public ObservableCollection<AdditionalPartSetElement> AdditionalPartSet
        {
            get { return _additionalPartSet; }
            set
            {
                if (_additionalPartSet != value)
                {
                    _additionalPartSet = value;
                    OnPropertyChanged();
                }
            }
        }

        private AdditionalPartSetElement _selectedPartSetElement;
        public AdditionalPartSetElement SelectedPartSetElement
        {
            get { return _selectedPartSetElement; }
            set
            {
                if (_selectedPartSetElement != value)
                {
                    _selectedPartSetElement = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddOrderCommand { get; private set; }


        public ICommand DeleteOrdersCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand AddPartToOrderCommand { get; private set; }

        public ICommand CraftPartCommand { get; private set; }

        public ICommand DeletePartFromOrderCommand { get; private set; }

        public OrdersWindowViewModel(OrdersWindow window)
            : base(window)
        {
            _window = window;

            InitializeBackgroundWorkers();
            InitializeCommands();

            Load();
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposeBackgroundWorkers();
        }

        private void DisposeBackgroundWorkers()
        {
            if (_partsBackgroundWorker != null)
            {
                _partsBackgroundWorker.Dispose();
            }
        }

        private void InitializeBackgroundWorkers()
        {
            DisposeBackgroundWorkers();

            _partsBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var orderId = (int) e.Argument;
                List<AdditionalPartSetElement> result;
                lock (DbContextLock)
                {
                    result = DbContext.AdditionalPartSets
                        .Where(el => el.Order.Id == orderId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<AdditionalPartSetElement>) e.Result;
                AdditionalPartSet = new ObservableCollection<AdditionalPartSetElement>(source);

                OnPartSetCompletedLoading();
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AddOrderCommand = new Command(() =>
            {
                var win = new AddOrderWindow(DbContext);
                win.OrderAdded += (sender, e) =>
                {
                    Orders.Add(e);
                };
                win.Show();
            });

            DeleteOrdersCommand = new Command(() =>
            {
                var selected = GetSelectedOrders();
                if (selected == null)
                {
                    return;
                }

                var win = new DeleteOrderWindow(DbContext, selected);
                win.OrdersDeleted += (sender, orders) =>
                {
                    foreach (var order in orders)
                    {
                        Orders.Remove(order);
                    }
                };
                win.WorkCompleted += delegate
                {
                    AdditionalPartSet = null;
                };
                win.Show();
            });

            RefreshCommand = new Command(() =>
            {
                var win = new OrdersWindow(_window.Left, _window.Top);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            AddPartToOrderCommand = new Command(() =>
            {
                var order = SelectedOrder;

                if (order == null)
                {
                    return;
                }

                var win = new AddPartToOrderWindow(DbContext, order);
                win.PartAdded += (sender, partSetElement) =>
                {
                    AdditionalPartSet.Add(partSetElement);
                };
                win.Show();
            });

            CraftPartCommand = new Command(() =>
            {
                var element = SelectedPartSetElement;

                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);
                part.Amount += element.Amount;

                DbContext.SaveChanges();

                string txt = "Wypalono: " + part.Name + "\nIlość: " + element.Amount;
                MessageBox.Show(txt, "Wypalono część");
            });

            DeletePartFromOrderCommand = new Command(() =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var element = DbContext.AdditionalPartSets
                    .Single(el => el.Part.Id == SelectedPartSetElement.Part.Id
                                                                        && el.Order.Id == SelectedPartSetElement.Order.Id);
                DbContext.AdditionalPartSets.Remove(element);
                DbContext.SaveChanges();

                LoadAdditionalPartSet();
            });
        }

        public override void Load()
        {
            LoadOrders();
            LoadClients();
            LoadMachines();
        }

        private void LoadOrders()
        {
            var orders = DbContext.Orders
                .Include(o => o.Client)
                .Include(o => o.Machine)
                .ToList();

            Orders = new ObservableCollection<Order>(orders);
        }

        private void LoadClients()
        {
            var clients = DbContext.Clients.ToList();
            Clients = new ObservableCollection<Client>(clients);
        }

        private void LoadMachines()
        {
            var machines = DbContext.Machines.ToList();
            Machines = new ObservableCollection<Machine>(machines);
        }

        private void LoadAdditionalPartSet()
        {
            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(SelectedOrder.Id);

                OnPartSetStartLoading();
            }
        }

        private List<Order> GetSelectedOrders()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Order>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Order)item);
            }

            return selected;
        }

        #region Event invokers.

        private void OnPartSetStartLoading()
        {
            if (PartSetStartLoading != null)
            {
                PartSetStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnPartSetCompletedLoading()
        {
            if (PartSetCompletedLoading != null)
            {
                PartSetCompletedLoading(this, EventArgs.Empty);
            }
        }

        #endregion

    }
}
