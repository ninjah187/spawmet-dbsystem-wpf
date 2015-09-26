using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for OrderPriceCalculatorWindow.xaml
    /// </summary>
    public partial class OrderPriceCalculatorWindow : Window, IDbContextChangesNotifier, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private decimal _inPrice;
        public decimal InPrice
        {
            get { return _inPrice; }
            set
            {
                if (_inPrice != value)
                {
                    _inPrice = value;
                    OnPropertyChanged();
                    InPriceChanged();
                }
            }
        }

        private decimal _outPrice;
        public decimal OutPrice
        {
            get { return _outPrice; }
            set
            {
                if (_outPrice != value)
                {
                    _outPrice = value;
                    OnPropertyChanged();
                    OutPriceChanged();
                }
            }
        }

        private decimal _discount;
        public decimal Discount
        {
            get { return _discount; }
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged();
                    DiscountChanged();
                }
            }
        }

        private decimal _discountPercentage;
        public decimal DiscountPercentage
        {
            get { return _discountPercentage; }
            set
            {
                if (_discountPercentage != value)
                {
                    _discountPercentage = value;
                    OnPropertyChanged();
                    DiscountPercentageChanged();
                }
            }
        }

        private Order _order;
        public Order Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged();
                }
            }
        }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindowViewModel), typeof(PeriodsWindowViewModel) };

        private SpawmetDBContext _dbContext;

        private int _orderId;

        private WindowsEnablementController _winEnablementController;

        public OrderPriceCalculatorWindow(int orderId)
        {
            InitializeComponent();

            _orderId = orderId;

            _winEnablementController = new WindowsEnablementController(this);

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);

            _winEnablementController.DisableWindows();

            Loaded += async delegate
            {
                await LoadAsync();
            };

            Closed += delegate
            {
                _dbContext.Dispose();
                DbContextMediator.Subscribers.Remove(this);

                _winEnablementController.EnableWindows();
            };
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }
                _dbContext = new SpawmetDBContext();

                Order = _dbContext.Orders.Single(o => o.Id == _orderId);

                InPrice = Order.Price;
                Discount = Order.Discount;
                
                OutPrice = InPrice - Discount;
            });
        }

        private void InPriceChanged()
        {
            DiscountPercentage = InPrice != 0 ? Discount / InPrice : 0;
            OutPrice = InPrice - Discount;
        }

        private void OutPriceChanged()
        {
            InPrice = OutPrice + Discount;
            //Discount = OutPrice - InPrice;
            DiscountPercentage = InPrice != 0 ? Discount / InPrice : 0;
        }

        private void DiscountChanged()
        {
            DiscountPercentage = InPrice != 0 ? Discount / InPrice : 0;
            OutPrice = InPrice - Discount;
        }

        private void DiscountPercentageChanged()
        {
            Discount = DiscountPercentage * InPrice;
            OutPrice = InPrice - Discount;
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
