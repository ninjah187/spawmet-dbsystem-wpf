using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Windows.Searching;
using Application = System.Windows.Application;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class DeliveriesWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler PartSetStartLoading;
        public event EventHandler PartSetCompletedLoading;

        private BackgroundWorker _partsBackgroundWorker;

        private DeliveriesWindow _window;

        private ObservableCollection<Delivery> _deliveries;
        public ObservableCollection<Delivery> Deliveries
        {
            get { return _deliveries; }
            set
            {
                if (_deliveries != value)
                {
                    _deliveries = value;
                    OnPropertyChanged();
                }
            }
        }

        private Delivery _selectedDelivery;
        public Delivery SelectedDelivery
        {
            get { return _selectedDelivery; }
            set
            {
                if (_selectedDelivery != value)
                {
                    _selectedDelivery = value;
                    OnPropertyChanged();
                    OnElementSelected(_selectedDelivery);
                    SelectedElement = _selectedDelivery;
                    LoadDeliveryPartSet();
                }
            }
        }

        private ObservableCollection<DeliveryPartSetElement> _deliveryPartSet;
        public ObservableCollection<DeliveryPartSetElement> DeliveryPartSet
        {
            get { return _deliveryPartSet; }
            set
            {
                if (_deliveryPartSet != value)
                {
                    _deliveryPartSet = value;
                    OnPropertyChanged();
                }
            }
        }

        private DeliveryPartSetElement _selectedPartSetElement;
        public DeliveryPartSetElement SelectedPartSetElement
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

        public ICommand AddDeliveryCommand { get; private set; }

        public ICommand DeleteDeliveriesCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand AddPartToDeliveryCommand { get; private set; }

        public ICommand DeletePartFromDeliveryCommand { get; private set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public ICommand GoToPartCommand { get; protected set; }

        public DeliveriesWindowViewModel(DeliveriesWindow window)
            : this(window, null)
        {
        }

        public DeliveriesWindowViewModel(DeliveriesWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();

            ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    Deliveries = null;
                    DeliveryPartSet = null;
                }
                else
                {
                    if (Deliveries == null)
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
                var deliveryId = (int) e.Argument;
                List<DeliveryPartSetElement> result;
                lock (DbContextLock)
                {
                    result = DbContext.DeliveryPartSets
                        .Where(el => el.Delivery.Id == deliveryId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (ICollection<DeliveryPartSetElement>) e.Result;
                DeliveryPartSet = new ObservableCollection<DeliveryPartSetElement>(source);

                OnPartSetCompletedLoading();
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AddDeliveryCommand = new Command(() =>
            {
                var win = new AddDeliveryWindow(DbContext);
                win.DeliveryAdded += (sender, delivery) =>
                {
                    Deliveries.Add(delivery);
                };
                win.Show();
            });

            DeleteDeliveriesCommand = new Command(() =>
            {
                var selected = GetSelectedDeliveries();
                if (selected == null)
                {
                    return;
                }

                string msg = selected.Count == 1
                    ? "Czy chcesz usunąć zaznaczoną dostawę?"
                    : "Czy chcesz usunąć zaznaczone dostawy?";

                var confirmWin = new ConfirmWindow(msg);
                confirmWin.Confirmed += delegate
                {
                    var win = new DeleteDeliveryWindow(DbContext, selected);
                    win.DeliveriesDeleted += (sender, deliveries) =>
                    {
                        foreach (var delivery in deliveries)
                        {
                            Deliveries.Remove(delivery);
                        }
                    };
                    win.WorkCompleted += delegate
                    {
                        DeliveryPartSet = null;

                        OnElementSelected(null);
                    };
                    win.ConnectionLost += delegate
                    {
                        IsConnected = false;
                    };
                    win.Owner = _window;
                    win.ShowDialog();
                };
                confirmWin.Show();
            });

            RefreshCommand = new Command(() =>
            {
                SaveDbStateCommand.Execute(null);

                var config = GetWindowConfig();
                var win = new DeliveriesWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            AddPartToDeliveryCommand = new Command(() =>
            {
                if (SelectedDelivery == null)
                {
                    return;
                }

                var win = new AddPartToDelivery(DbContext, SelectedDelivery);
                win.PartAdded += (sender, element) =>
                {
                    DeliveryPartSet.Add(element);
                };
                win.Show();
            });

            DeletePartFromDeliveryCommand = new Command(() =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var element = DbContext.DeliveryPartSets
                    .Single(el => el.Part.Id == SelectedPartSetElement.Part.Id
                                  && el.Delivery.Id == SelectedPartSetElement.Delivery.Id);
                DbContext.DeliveryPartSets.Remove(element);
                DbContext.SaveChanges();

                LoadDeliveryPartSet();
            });

            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchDeliveriesWindow(_window, DbContext);
                win.WorkCompleted += (sender, deliveries) =>
                {
                    Deliveries = new ObservableCollection<Delivery>(deliveries);

                    DeliveryPartSet = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie", win);
                };
                win.Show();
            });

            GoToPartCommand = new Command(() =>
            {
                var partSetElement = SelectedPartSetElement;
                if (partSetElement == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<PartsWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(partSetElement.Part);
                }
                else
                {
                    var config = new WindowConfig()
                    {
                        Left = _window.Left + Offset,
                        Top = _window.Top + Offset,
                        SelectedElement = partSetElement.Part
                    };
                    var window = new PartsWindow(config);
                    window.Show();
                }
            });
        }

        public override void Load()
        {
            LoadDeliveries();

            FinishLoading();
        }

        public override async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                LoadDeliveries();
            });

            FinishLoading();
        }

        private void LoadDeliveries()
        {
            var deliveries = DbContext.Deliveries.ToList();

            Deliveries = new ObservableCollection<Delivery>(deliveries);
        }

        private void LoadDeliveryPartSet()
        {
            var delivery = SelectedDelivery;

            if (delivery == null)
            {
                DeliveryPartSet = null;
                return;
            }

            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(delivery.Id);

                OnPartSetStartLoading();
            }
        }

        public override void SelectElement(IModelElement element)
        {
            var delivery = Deliveries.Single(e => e.Id == element.Id);

            SelectedDelivery = delivery;

            _window.DataGrid.SelectedItem = delivery;
            _window.DataGrid.ScrollIntoView(delivery);
        }

        private List<Delivery> GetSelectedDeliveries()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Delivery>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Delivery)item);
            }

            return selected;
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedDelivery;

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
