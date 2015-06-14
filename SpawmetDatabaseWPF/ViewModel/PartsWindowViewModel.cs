﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class PartsWindowViewModel : SpawmetWindowViewModel, IDisposable
    {
        public event EventHandler MachinesStartLoading;
        public event EventHandler MachinesCompletedLoading;

        public event EventHandler OrdersStartLoading;
        public event EventHandler OrdersCompletedLoading;

        public event EventHandler DeliveriesStartLoading;
        public event EventHandler DeliveriesCompletedLoading;

        private BackgroundWorker _machinesBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;
        private BackgroundWorker _deliveriesBackgroundWorker;

        private PartsWindow _window;

        private ObservableCollection<Part> _parts;
        public ObservableCollection<Part> Parts
        {
            get { return _parts; }
            set
            {
                if (_parts != value)
                {
                    _parts = value;
                    OnPropertyChanged();
                }
            }
        }

        private Part _selectedPart;
        public Part SelectedPart
        {
            get { return _selectedPart; }
            set
            {
                if (_selectedPart != value)
                {
                    _selectedPart = value;
                    OnPropertyChanged();
                    OnElementSelected(_selectedPart);
                    LoadMachines();
                    LoadOrders();
                    LoadDeliveries();
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

        public ICommand AddPartCommand { get; private set; }

        public ICommand DeletePartsCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand CraftPartCommand { get; protected set; }

        public PartsWindowViewModel(PartsWindow window)
            : this(window, null)
        {
        }

        public PartsWindowViewModel(PartsWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();
        }

        public void Dispose()
        {
            base.Dispose();
            DisposeBackgroundWorkers();
        }

        private void DisposeBackgroundWorkers()
        {
            if (_machinesBackgroundWorker != null)
            {
                _machinesBackgroundWorker.Dispose();
            }
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }
            if (_deliveriesBackgroundWorker != null)
            {
                _deliveriesBackgroundWorker.Dispose();
            }
        }

        private void InitializeBackgroundWorkers()
        {
            DisposeBackgroundWorkers();

            _machinesBackgroundWorker = new BackgroundWorker();
            _machinesBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int) e.Argument;
                List<Machine> result;
                using (var context = new SpawmetDBContext())
                {
                    result = context.StandardPartSets
                        .Where(el => el.Part.Id == partId)
                        .Select(el => el.Machine)
                        .OrderBy(m => m.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _machinesBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Machine>) e.Result;
                Machines = new ObservableCollection<Machine>(source);

                OnMachinesCompletedLoading();
            };

            _ordersBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int) e.Argument;
                using (var context = new SpawmetDBContext())
                {
                    // .Include() may be needed
                    var result = context.AdditionalPartSets
                        .Where(el => el.Part.Id == partId)
                        .Select(el => el.Order)
                        .Include(o => o.Client)
                        .Include(o => o.Machine)
                        .OrderBy(o => o.Id)
                        .ToList();
                    e.Result = result;
                }
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Order>)e.Result;
                Orders = new ObservableCollection<Order>(source);

                OnOrdersCompletedLoading();
            };

            _deliveriesBackgroundWorker = new BackgroundWorker();
            _deliveriesBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int) e.Argument;
                using (var context = new SpawmetDBContext())
                {
                    var result = context.DeliveryPartSets
                        .Where(el => el.Part.Id == partId)
                        .Select(el => el.Delivery)
                        .OrderBy(d => d.Id)
                        .ToList();
                    e.Result = result;
                }
            };
            _deliveriesBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Delivery>) e.Result;
                Deliveries = new ObservableCollection<Delivery>(source);

                OnDeliveriesCompletedLoading();
            };
        }

        private void InitializeCommands()
        {
            base.InitializeCommands();

            AddPartCommand = new Command(() =>
            {
                var win = new AddPartWindow(DbContext);
                win.PartAdded += (sender, e) =>
                {
                    Parts.Add(e);
                };
                win.Show();
            });

            DeletePartsCommand = new Command(() =>
            {
                var selected = GetSelectedParts();

                if (selected == null)
                {
                    return;
                }

                string msg = selected.Count == 1
                    ? "Czy chcesz usunąć zaznaczoną część?"
                    : "Czy chcesz usunąć zaznaczone części?";

                var confirmWin = new ConfirmWindow(_window, msg);
                confirmWin.Confirmed += delegate
                {
                    var win = new DeletingPartWindow(DbContext, selected);
                    win.PartsDeleted += (sender, parts) =>
                    {
                        foreach (var part in parts)
                        {
                            Parts.Remove(part);
                        }
                    };
                    win.WorkCompleted += delegate
                    {
                        Machines = null;
                        Orders = null;
                        Deliveries = null;
                    };
                    win.Show();
                };
                confirmWin.Show();
            });

            RefreshCommand = new Command(() =>
            {
                SaveDbStateCommand.Execute(null);

                var config = GetWindowConfig();
                var win = new PartsWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            CraftPartCommand = new Command(() =>
            {
                if (SelectedPart == null)
                {
                    return;
                }

                var win = new CraftPartWindow(DbContext, SelectedPart);
                win.PartCrafted += (sender, e) =>
                {
                    LoadParts(); // to update MainDataGrid

                    string txt = "Wypalono: " + e.Part.Name + "\nIlość: " + e.Amount;
                    MessageWindow.Show(txt, "Wypalono część", _window);
                };
                win.Show();
            });
        }

        public override void Load()
        {
            LoadParts();

            if (WindowConfig.SelectedElement != null)
            {
                var part = Parts.Single(p => p.Id == WindowConfig.SelectedElement.Id);

                SelectedPart = part;

                _window.DataGrid.SelectedItem = SelectedPart;
                _window.DataGrid.ScrollIntoView(SelectedPart);
            }
        }

        private void LoadParts()
        {
            var parts = DbContext.Parts.ToList();

            Parts = new ObservableCollection<Part>(parts);
        }

        private void LoadMachines()
        {
            var part = SelectedPart;

            if (_machinesBackgroundWorker.IsBusy == false)
            {
                _machinesBackgroundWorker.RunWorkerAsync(part.Id);

                OnMachinesStartLoading();
            }
        }

        private void LoadOrders()
        {
            var part = SelectedPart;

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(part.Id);

                OnOrdersStartLoading();
            }
        }

        private void LoadDeliveries()
        {
            var part = SelectedPart;

            if (_deliveriesBackgroundWorker.IsBusy == false)
            {
                _deliveriesBackgroundWorker.RunWorkerAsync(part.Id);

                OnDeliveriesStartLoading();
            }
        }

        private List<Part> GetSelectedParts()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Part>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Part)item);
            }

            return selected;
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedPart;

            return config;
        }

        //private IEnumerable<Part> GetSelectedParts()
        //{
        //    if (_window.MainDataGrid.SelectedItems.Count == 0)
        //    {
        //        yield return null;
        //        yield break;
        //    }

        //    foreach (var item in _window.MainDataGrid.SelectedItems)
        //    {
        //        yield return (Part) item;
        //    }
        //}

        #region Event invokers.

        private void OnMachinesStartLoading()
        {
            if (MachinesStartLoading != null)
            {
                MachinesStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnMachinesCompletedLoading()
        {
            if (MachinesCompletedLoading != null)
            {
                MachinesCompletedLoading(this, EventArgs.Empty);
            }
        }

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

        private void OnDeliveriesStartLoading()
        {
            if (DeliveriesStartLoading != null)
            {
                DeliveriesStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnDeliveriesCompletedLoading()
        {
            if (DeliveriesCompletedLoading != null)
            {
                DeliveriesCompletedLoading(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
