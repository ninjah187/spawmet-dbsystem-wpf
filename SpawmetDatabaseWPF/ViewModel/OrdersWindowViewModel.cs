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
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;

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

        public ICommand ChangeStatusCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand AddPartToOrderCommand { get; private set; }

        public ICommand CraftPartCommand { get; private set; }

        public ICommand DeletePartFromOrderCommand { get; private set; }

        public OrdersWindowViewModel(OrdersWindow window)
            : this(window, null)
        {
        }

        public OrdersWindowViewModel(OrdersWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeBackgroundWorkers();
            InitializeCommands();
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

            ChangeStatusCommand = new ParamCommand<Order>((order) =>
            {
                OrderStatus? oldStatus;
                OrderStatus? newStatus;

                oldStatus = order.Status;
                SaveDbStateCommand.Execute(null);
                newStatus = order.Status;

                switch (oldStatus)
                {
                    case OrderStatus.New:
                        switch (newStatus)
                        {
                            case OrderStatus.InProgress:
                                ApplyPartSets(SelectedOrder);
                                break;

                            case OrderStatus.Done:
                                ApplyPartSets(SelectedOrder);
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                        break;

                    case OrderStatus.InProgress:
                        switch (newStatus)
                        {
                            case OrderStatus.New:
                                UndoApplyPartSets(SelectedOrder);
                                break;

                            case OrderStatus.Done:
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                        break;

                    case OrderStatus.Done:
                        switch (newStatus)
                        {
                            case OrderStatus.New:
                                UndoApplyPartSets(SelectedOrder);
                                break;

                            case OrderStatus.InProgress:
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            });

            DeleteOrdersCommand = new Command(() =>
            {
                var selected = GetSelectedOrders();
                if (selected == null)
                {
                    return;
                }

                string msg = selected.Count == 1
                    ? "Czy chcesz usunąć zaznaczone zamówienie?"
                    : "Czy chcesz usunąć zaznaczone zamówienia?";

                var confirmWin = new ConfirmWindow(_window, msg);
                confirmWin.Confirmed += delegate
                {
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
                };
                confirmWin.Show();
            });

            RefreshCommand = new Command(() =>
            {
                SaveDbStateCommand.Execute(null);

                var config = GetWindowConfig();
                var win = new OrdersWindow(config);
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

            if (WindowConfig.SelectedElement != null)
            {
                var order = Orders.Single(o => o.Id == WindowConfig.SelectedElement.Id);

                SelectedOrder = order;

                _window.DataGrid.SelectedItem = SelectedOrder;
                _window.DataGrid.ScrollIntoView(SelectedOrder);
            }
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

        private void ApplyPartSets(Order order)
        {
            foreach (var element in order.Machine.StandardPartSet)
            {
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                part.Amount -= element.Amount;
            }
            foreach (var element in order.AdditionalPartSet)
            {
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                part.Amount -= element.Amount;
            }
            DbContext.SaveChanges();
        }

        private void UndoApplyPartSets(Order order)
        {
            foreach (var element in order.Machine.StandardPartSet)
            {
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                part.Amount += element.Amount;
            }
            foreach (var element in order.AdditionalPartSet)
            {
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                part.Amount += element.Amount;
            }
            DbContext.SaveChanges();
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedOrder;

            return config;
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
