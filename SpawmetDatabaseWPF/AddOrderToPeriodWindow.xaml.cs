using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddOrderToPeriodWindow.xaml
    /// </summary>
    public partial class AddOrderToPeriodWindow : Window, INotifyPropertyChanged, IDbContextChangesNotifier
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Period _period;
        public Period Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    OnPropertyChanged();
                }
            }
        }

        //private DateTime _startDate;
        //public DateTime StartDate
        //{
        //    get { return _startDate; }
        //    set
        //    {
        //        if (_startDate != value)
        //        {
        //            _startDate = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //private DateTime _endDate;
        //public DateTime EndDate
        //{
        //    get { return _endDate; }
        //    set
        //    {
        //        if (_endDate != value)
        //        {
        //            _endDate = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

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

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        //private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        private SpawmetDBContext _dbContext;

        private int _periodId;

        public AddOrderToPeriodWindow(int periodId)
        {
            InitializeComponent();

            _periodId = periodId;

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);

            Loaded += async delegate
            {
                await LoadAsync();
            };

            Closed += delegate
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }

                DbContextMediator.Subscribers.Remove(this);
            };
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AddOrdersAsync();
        }

        public async Task LoadAsync()
        {
            IsEnabled = false;
            await Task.Run(() =>
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }
                _dbContext = new SpawmetDBContext();

                Period = _dbContext.Periods.Single(p => p.Id == _periodId);

                var orders = _dbContext.Orders.Where(o => o.Period == null).ToList();
                Orders = new ObservableCollection<Order>(orders);
            });
            IsEnabled = true;
        }

        public async Task AddOrdersAsync()
        {
            var orders = GetSelectedOrders();
            if (orders == null)
            {
                return;
            }

            var waitWin = new WaitWindow("Proszę czekać, trwa dodawanie rekordów...");
            waitWin.Show();

            foreach (var order in orders)
            {
                await Task.Run(() =>
                {
                    Period.Orders.Add(order);
                    _dbContext.SaveChanges();
                });
            }

            DbContextMediator.NotifyContextChanged(this);
            waitWin.Close();

            Close();
        }

        public List<Order> GetSelectedOrders()
        {
            if (OrdersDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var orders = new List<Order>();
            foreach (var order in OrdersDataGrid.SelectedItems)
            {
                orders.Add((Order) order);
            }

            return orders;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
