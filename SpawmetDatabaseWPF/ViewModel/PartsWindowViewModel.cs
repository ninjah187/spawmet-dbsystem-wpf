using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
        //private BackgroundWorker _deliveriesBackgroundWorker;

        private bool _areModulesLoading;
        public bool AreModulesLoading
        {
            get { return _areModulesLoading; }
            set
            {
                if (_areModulesLoading != value)
                {
                    _areModulesLoading = value;
                    OnPropertyChanged();
                }
            }
        }

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
                    SelectedElement = _selectedPart;
                    LoadMachines();
                    LoadOrders();
                    LoadModulesAsync();
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

        private Machine _selectedMachine;
        public Machine SelectedMachine
        {
            get { return _selectedMachine; }
            set
            {
                if (_selectedMachine != value)
                {
                    _selectedMachine = value;
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

        private MachineModule _selectedModule;
        public MachineModule SelectedModule
        {
            get { return _selectedModule; }
            set
            {
                if (_selectedModule != value)
                {
                    _selectedModule = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddPartCommand { get; private set; }

        public ICommand DeletePartsCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand CraftPartAmountCommand { get; protected set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public ICommand GoToMachineCommand { get; protected set; }

        public ICommand GoToOrderCommand { get; protected set; }

        public ICommand GoToModuleCommand { get; protected set; }

        public ICommand PartsRaportCommand { get; protected set; }

        //private SpawmetAppObserver _observer;

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

            ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    Parts = null;
                    Machines = null;
                    Orders = null;
                    Modules = null;
                    OnElementSelected(null);
                }
                else
                {
                    if (Parts == null)
                    {
                        Load();
                    }
                }
            };
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
            //if (_deliveriesBackgroundWorker != null)
            //{
            //    _deliveriesBackgroundWorker.Dispose();
            //}
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

            //_deliveriesBackgroundWorker = new BackgroundWorker();
            //_deliveriesBackgroundWorker.DoWork += (sender, e) =>
            //{
            //    var partId = (int) e.Argument;
            //    using (var context = new SpawmetDBContext())
            //    {
            //        var result = context.DeliveryPartSets
            //            .Where(el => el.Part.Id == partId)
            //            .Select(el => el.Delivery)
            //            .OrderBy(d => d.Id)
            //            .ToList();
            //        e.Result = result;
            //    }
            //};
            //_deliveriesBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            //{
            //    var source = (List<Delivery>) e.Result;
            //    Deliveries = new ObservableCollection<Delivery>(source);

            //    OnDeliveriesCompletedLoading();
            //};
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

            //DeletePartsCommand = new Command(() =>
            //{
            //    var selected = GetSelectedParts();

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
            //        var win = new DeletingPartWindow(DbContext, selected);
            //        win.PartsDeleted += (sender, parts) =>
            //        {
            //            foreach (var part in parts)
            //            {
            //                Parts.Remove(part);
            //            }
            //        };
            //        win.WorkCompleted += delegate
            //        {
            //            Machines = null;
            //            Orders = null;
            //            Deliveries = null;

            //            OnElementSelected(null);
            //        };
            //        win.ConnectionLost += (sender, exc) =>
            //        {
            //            IsConnected = false;
            //        };
            //        win.Owner = _window;
            //        win.ShowDialog();
            //    };
            //    confirmWin.Show();
            //});

            #region DeleteParts
            DeletePartsCommand = new Command(() =>
            {
                var parts = GetSelectedParts();
                if (parts == null)
                {
                    return;
                }

                var confirmWin = new ConfirmWindow("Czy na pewno chcesz usunąć zaznaczone części?");
                confirmWin.Confirmed += async delegate
                {
                    var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie...");
                    waitWin.Show();

                    foreach (var part in parts)
                    {
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                var standardParts = part.StandardPartSets;
                                DbContext.StandardPartSets.RemoveRange(standardParts);

                                var additionalParts = part.AdditionalPartSets;
                                DbContext.AdditionalPartSets.RemoveRange(additionalParts);

                                var moduleParts = part.MachineModulePartSets;
                                DbContext.MachineModulePartSets.RemoveRange(moduleParts);

                                DbContext.Parts.Remove(part);
                                DbContext.SaveChanges();
                            }
                        });
                        Parts.Remove(part);
                    }

                    Machines = null;
                    Orders = null;
                    Modules = null;

                    DbContextMediator.NotifyContextChanged(this);
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
                var win = new PartsWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            CraftPartAmountCommand = new Command(() =>
            {
                if (SelectedPart == null)
                {
                    return;
                }

                var win = new CraftPartWindow(SelectedPart);
                win.WorkStarted += delegate
                {
                    IsSaving = true;
                };
                win.WorkCompleted += delegate
                {
                    IsSaving = false;
                };
                win.Owner = _window;
                win.ShowDialog();
            });

            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchPartsWindow(_window, DbContext);
                win.WorkCompleted += (sender, parts) =>
                {
                    Parts = new ObservableCollection<Part>(parts);

                    Machines = null;
                    Orders = null;
                    Modules = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie.", win);
                };
                win.ShowDialog();
            });

            GoToMachineCommand = new Command(() =>
            {
                var machine = SelectedMachine;
                if (machine == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<MachinesWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(machine);
                }
                else
                {
                    var config = new WindowConfig()
                    {
                        Left = _window.Left + Offset,
                        Top = _window.Top + Offset,
                        SelectedElement = machine
                    };
                    var window = new MachinesWindow(config);
                    window.Show();
                }
            });

            GoToOrderCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (order == null)
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

            #region GoToModuleCommand
            GoToModuleCommand = new Command(() =>
            {
                var module = SelectedModule;
                if (module == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<MachineModuleDetailsWindow>();
                MachineModuleDetailsWindow window;
                if ((window = windows.FirstOrDefault(w => w.Module.Id == module.Id)) != null)
                {
                    window.Focus();
                }
                else
                {
                    window = new MachineModuleDetailsWindow(module.Id);
                    window.Show();
                }
            });
            #endregion

            PartsRaportCommand = new Command(() =>
            {
                //var parts = Parts.Where(p => p.Amount <= 0).OrderBy(p => p.Name).ToList();

                var win = new PartsRaportWindow();
                win.Owner = _window;
                win.ShowDialog();
            });
        }

        public override void Load()
        {
            LoadParts();

            IsConnected = true;
            //FinishLoading();
        }

        public override async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                LoadParts();
            });

            IsConnected = true;
            //FinishLoading();
        }

        public void LoadParts()
        {
            try
            {
                List<Part> parts = null;

                if (SearchExpression != "")
                {
                    parts = new List<Part>();
                    foreach (var part in DbContext.Parts)
                    {
                        if (Regex.IsMatch(part.Name, SearchExpression, RegexOptions.IgnoreCase))
                        {
                            parts.Add(part);
                        }
                    }
                    //parts = DbContext.Parts
                    //    .Where(p => p.Name == SearchExpression)
                    //    .ToList();
                } 
                else
                {
                    parts = DbContext.Parts.ToList();
                }

                Parts = new ObservableCollection<Part>(parts);
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }

        public void LoadMachines()
        {
            var part = SelectedPart;

            if (part == null)
            {
                Machines = null;
                return;
            }

            if (_machinesBackgroundWorker.IsBusy == false)
            {
                _machinesBackgroundWorker.RunWorkerAsync(part.Id);

                OnMachinesStartLoading();
            }
        }

        public void LoadOrders()
        {
            var part = SelectedPart;

            if (part == null)
            {
                Orders = null;
                return;
            }

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(part.Id);

                OnOrdersStartLoading();
            }
        }

        public async Task LoadModulesAsync()
        {
            var part = SelectedPart;
            if (part == null)
            {
                Modules = null;
                return;
            }

            var partId = part.Id;

            AreModulesLoading = true;

            await Task.Run(() =>
            {
                List<MachineModule> modules = null;
                lock (DbContextLock)
                {
                    modules = DbContext.MachineModules
                        .Where(m => m.MachineModulePartSet.Any(e => e.Part.Id == partId))
                        .OrderBy(m => m.Name)
                        .ToList();
                }

                if (part == SelectedPart)
                {
                    Modules = new ObservableCollection<MachineModule>(modules);
                }
            });

            AreModulesLoading = false;
        }

        public override void SelectElement(IModelElement element)
        {
            if (element == null)
            {
                Machines = null;
                Orders = null;
                Modules = null;
                return;
            }

            var part = Parts.Single(e => e.Id == element.Id);

            SelectedPart = part;

            _window.DataGrid.SelectedItem = part;
            _window.DataGrid.ScrollIntoView(part);
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
