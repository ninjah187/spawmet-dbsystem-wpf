using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Windows;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class PeriodsWindowViewModel : SpawmetWindowViewModel
    {
        private ObservableCollection<Period> _periods;
        public ObservableCollection<Period> Periods
        {
            get { return _periods; }
            set
            {
                if (_periods != value)
                {
                    _periods = value;
                    OnPropertyChanged();
                }
            }
        }

        private Period _selectedPeriod;
        public Period SelectedPeriod
        {
            get { return _selectedPeriod; }
            set
            {
                if (_selectedPeriod != value)
                {
                    _selectedPeriod = value;
                    LoadOrdersAsync();
                    OnPropertyChanged();
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

        public ICommand AddPeriodCommand { get; protected set; }

        public ICommand DeletePeriodsCommand { get; protected set; }

        public ICommand AddOrdersCommand { get; protected set; }

        public ICommand DeleteOrdersCommand { get; protected set; }

        private readonly PeriodsWindow _window;

        public PeriodsWindowViewModel(PeriodsWindow window)
            : this(window, null)
        {
        }

        public PeriodsWindowViewModel(PeriodsWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            
            // todo: connection changed event handle
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            #region AddPeriod
            AddPeriodCommand = new Command(() =>
            {
                var win = new AddPeriodWindow();
                win.ShowDialog();
            });
            #endregion

            #region DeletePeriods
            DeletePeriodsCommand = new Command(() =>
            {
                var periods = GetSelectedPeriods();
                if (periods == null)
                {
                    return;
                }

                var confirmWin = new ConfirmWindow("Czy na pewno chcesz usunąć zaznaczone maszyny?", _window);
                confirmWin.Confirmed += async delegate
                {
                    var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie...");
                    waitWin.Show();

                    foreach (var period in periods)
                    {
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                DbContext.Periods.Remove(period);
                                DbContext.SaveChanges();
                            }
                        });
                        Periods.Remove(period);
                    }

                    Orders = null;

                    Mediator.NotifyContextChange(this);
                    waitWin.Close();
                };

                confirmWin.Show();
            });
            #endregion

            #region AddOrder
            AddOrdersCommand = new Command(() =>
            {
                var period = SelectedPeriod;
                if (period == null)
                {
                    return;
                }

                new AddOrderToPeriodWindow(period.Id).Show();
            });
            #endregion
        }

        public override void Load()
        {
            
        }

        public override async Task LoadAsync()
        {
            await LoadPeriodsAsync();

            IsConnected = true;

            if (WindowConfig.SelectedElement != null)
            {
                SelectElement(WindowConfig.SelectedElement);
            }
        }

        public void LoadPeriods()
        {
            List<Period> periods = null;
            lock (DbContextLock)
            {
                periods = DbContext.Periods.ToList();
            }
            Periods = new ObservableCollection<Period>(periods);
        }

        public async Task LoadPeriodsAsync()
        {
            await Task.Run(() => LoadPeriods());
        }

        public async Task LoadOrdersAsync()
        {
            var period = SelectedPeriod;
            if (period == null)
            {
                return;
            }

            List<Order> orders = null;
            await Task.Run(() =>
            {
                orders = period.Orders.ToList();
            });

            if (period == SelectedPeriod)
            {
                Orders = new ObservableCollection<Order>(orders);
            }
        }

        public override async Task ReloadContextAsync(IModelElement element)
        {
            lock (DbContextLock)
            {
                DbContext.Dispose();
                DbContext = new SpawmetDBContext();
            }

            await LoadPeriodsAsync();

            IsConnected = true;

            if (element != null)
            {
                SelectElement(element);
            }
        }

        public override void SelectElement(IModelElement element)
        {
            if (element == null)
            {
                Orders = null;
                return;
            }

            var period = Periods.Single(e => e.Id == element.Id);

            SelectedPeriod = period;

            _window.DataGrid.SelectedItem = period;
            _window.DataGrid.ScrollIntoView(period);
        }

        private List<Period> GetSelectedPeriods()
        {
            if (_window.PeriodsDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Period>();
            foreach (var item in _window.PeriodsDataGrid.SelectedItems)
            {
                selected.Add((Period)item);
            }

            return selected;
        }
    }
}
