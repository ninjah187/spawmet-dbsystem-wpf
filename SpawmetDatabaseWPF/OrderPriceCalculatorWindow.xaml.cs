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
using SpawmetDatabaseWPF.Commands;
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

        private ObservableCollection<MachineModule> _modules;
        public ObservableCollection<MachineModule> Modules
        {
            get { return _modules; }
            set
            {
                if (_modules != value)
                {
                    _modules = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _defaultPrice;
        public decimal DefaultPrice
        {
            get { return _defaultPrice; }
            set
            {
                if (_defaultPrice != value)
                {
                    _defaultPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private decimal _defaultInDifference;
        public decimal DefaultInDifference
        {
            get { return _defaultInDifference; }
            set
            {
                if (_defaultInDifference != value)
                {
                    _defaultInDifference = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CancelCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindowViewModel), typeof(PeriodsWindowViewModel) };

        private SpawmetDBContext _dbContext;

        private readonly int _orderId;

        private readonly WindowsEnablementController _winEnablementController;

        public OrderPriceCalculatorWindow(int orderId)
        {
            InitializeComponent();

            InitializeCommands();

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

        private void InitializeCommands()
        {
            CancelCommand = new Command(() =>
            {
                Close();
            });

            SaveCommand = new Command(async () =>
            {
                await Task.Run(() =>
                {
                    Order.InPrice = InPrice;
                    Order.Discount = Discount;
                    Order.DiscountPercentage = DiscountPercentage;
                    Order.OutPrice = OutPrice;

                    _dbContext.SaveChanges();
                });

                DbContextMediator.NotifyContextChanged(this, _contextChangeInfluencedTypes);
                Close();
            });
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

                Order = _dbContext.Orders
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .Single(o => o.Id == _orderId);

                var modules = Order.MachineModules.ToList();
                Modules = new ObservableCollection<MachineModule>(modules);

                DefaultPrice = Order.Machine.Price;
                DefaultPrice += Modules.Sum(m => m.Price);

                InPrice = Order.InPrice;
                Discount = Order.Discount;
                DiscountPercentage = Order.DiscountPercentage;
                OutPrice = Order.OutPrice;
            });
        }

        private void InPriceChanged()
        {
            //perc = perc < 0 ? -perc : perc;
            _discountPercentage = _inPrice != 0 ? _discount / _inPrice * 100 : 0;
            _outPrice = _inPrice - _discount;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var binding = DiscountPercentageTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();

                binding = OutPriceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();
            });

            DefaultInDifference = InPrice - DefaultPrice;
        }

        private void OutPriceChanged()
        {
            //_inPrice = OutPrice + Discount;
            _discount = _inPrice - _outPrice;
            _discountPercentage = _inPrice != 0 ? _discount / _inPrice * 100 : 0;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var binding = DiscountPriceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();

                binding = DiscountPercentageTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();
            });
        }

        private void DiscountChanged()
        {
            _discountPercentage = _inPrice != 0 ? _discount / _inPrice * 100 : 0;
            _outPrice = _inPrice - _discount;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var binding = DiscountPercentageTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();

                binding = OutPriceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();
            });
        }

        private void DiscountPercentageChanged()
        {
            _discount = _discountPercentage * _inPrice / 100;
            _outPrice = _inPrice - _discount;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var binding = DiscountPriceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();

                binding = OutPriceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateTarget();
            });
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void InPrice_OnKeyDown(object sender, KeyEventArgs e)
        {
            InPriceChanged();
        }

        private void OutPrice_OnKeyDown(object sender, KeyEventArgs e)
        {
            OutPriceChanged();
        }

        private void Discount_OnKeyDown(object sender, KeyEventArgs e)
        {
            DiscountChanged();
        }

        private void DiscountPercentageTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            DiscountPercentageChanged();
        }

        private void ResetInPriceButton_OnClick(object sender, RoutedEventArgs e)
        {
            InPrice = DefaultPrice;
            OutPrice = InPrice - Discount;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CancelCommand.Execute(null);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveCommand.Execute(null);
        }
    }
}
