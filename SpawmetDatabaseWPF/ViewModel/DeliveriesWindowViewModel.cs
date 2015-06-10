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

        public DeliveriesWindowViewModel(DeliveriesWindow window)
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
                };
                win.Show();
            });

            RefreshCommand = new Command(() =>
            {
                var win = new DeliveriesWindow(_window.Left, _window.Top);
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
        }

        public override void Load()
        {
            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            var deliveries = DbContext.Deliveries.ToList();

            Deliveries = new ObservableCollection<Delivery>(deliveries);
        }

        private void LoadDeliveryPartSet()
        {
            var delivery = SelectedDelivery;

            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(delivery.Id);

                OnPartSetStartLoading();
            }
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
