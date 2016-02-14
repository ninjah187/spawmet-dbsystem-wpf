using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Events;
using SpawmetDatabaseWPF.Windows;
using SpawmetDatabaseWPF.Windows.Searching;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class MachinesWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler PartSetStartLoading;
        public event EventHandler PartSetCompletedLoading;

        public event EventHandler ModulesStartLoading;
        public event EventHandler ModulesCompletedLoading;

        public event EventHandler OrdersStartLoading;
        public event EventHandler OrdersCompletedLoading;

        private BackgroundWorker _partsBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;

        private readonly MachinesWindow _window;

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
            get
            {
                return _selectedMachine;
            }
            set
            {
                if (_selectedMachine != value)
                {
                    _selectedMachine = value;
                    OnPropertyChanged();
                    //OnElementSelected(_selectedMachine);
                    SelectedElement = _selectedMachine;
                    LoadStandardPartSet();
                    LoadOrders();
                    LoadModulesAsync();
                }
            }
        }

        //private IEnumerable<Machine> _selectedMachines;
        //public IEnumerable<Machine> SelectedMachines
        //{
        //    get { return _selectedMachines; }
        //    set
        //    {
        //        if (_selectedMachines != value)
        //        {
        //            _selectedMachines = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<StandardPartSetElement> _standardPartSet;
        public ObservableCollection<StandardPartSetElement> StandardPartSet
        {
            get { return _standardPartSet; }
            set
            {
                if (_standardPartSet != value)
                {
                    _standardPartSet = value;
                    OnPropertyChanged();
                }
            }
        }

        private StandardPartSetElement _selectedPartSetElement;
        public StandardPartSetElement SelectedPartSetElement
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

        public ICommand AddMachineCommand { get; private set; }

        public ICommand DeleteMachinesCommand { get; private set; }

        public ICommand PrintDialogCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand SaveToFileCommand { get; private set; }

        public ICommand AddPartToMachineCommand { get; private set; }

        public ICommand CraftPartCommand { get; private set; }

        public ICommand CraftPartAmountCommand { get; private set; }

        public ICommand CraftModuleCommand { get; private set; }

        public ICommand CraftModuleAmountCommand { get; private set; }

        public ICommand DeletePartFromMachineCommand { get; private set; }

        public ICommand AddMachinesFromDirectoryCommand { get; private set; }
        
        public ICommand AddMachineModuleCommand { get; protected set; }

        public ICommand DeleteMachineModuleCommand { get; protected set; }

        public ICommand MachineModuleDetailsCommand { get; protected set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public ICommand GoToPartCommand { get; protected set; }

        public ICommand GoToOrderCommand { get; protected set; }

        public ICommand CopyModulesCommand { get; protected set; }

        public ICommand PasteModulesCommand { get; protected set; }

        //private SpawmetAppObserver _observer;

        public MachinesWindowViewModel(MachinesWindow window)
            : this(window, null)
        {
        }

        public MachinesWindowViewModel(MachinesWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();

            ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    Machines = null;
                    StandardPartSet = null;
                    Orders = null;
                    OnElementSelected(null);
                }
                else
                {
                    if (Machines == null)
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
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }
        }

        private void InitializeBackgroundWorkers()
        {
            DisposeBackgroundWorkers();

            _partsBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int) e.Argument;
                List<StandardPartSetElement> result;
                lock (DbContextLock)
                {
                    result = DbContext.StandardPartSets
                        .Where(el => el.Machine.Id == machineId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                ICollection<StandardPartSetElement> source;

                try
                {
                    source = (ICollection<StandardPartSetElement>) e.Result;
                }
                catch (TargetInvocationException)
                {
                    IsConnected = false;
                    OnPartSetCompletedLoading();
                    return;
                }
                
                StandardPartSet = new ObservableCollection<StandardPartSetElement>(source);

                OnPartSetCompletedLoading();
            };

            _ordersBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int)e.Argument;
                var context = new SpawmetDBContext();
                var result = context.Orders
                    .Where(m => m.Machine.Id == machineId)
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .OrderBy(o => o.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                ICollection<Order> source;

                try
                {
                    source = (ICollection<Order>) e.Result;
                }
                catch (TargetInvocationException)
                {
                    IsConnected = false;
                    OnOrdersCompletedLoading();
                    return;
                }

                Orders = new ObservableCollection<Order>(source);

                OnOrdersCompletedLoading();
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            #region AddMachine
            AddMachineCommand = new Command(() =>
            {
                var win = new AddMachineWindow(DbContext);
                win.MachineAdded += (sender, e) =>
                {
                    Machines.Add(e);

                    DbContextMediator.NotifyContextChanged(this, typeof(OrdersWindowViewModel));
                };
                win.Show();
            });
            #endregion

            #region CraftPart
            CraftPartCommand = new Command(async () =>
            {
                var element = SelectedPartSetElement;
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                await Task.Run(() =>
                {
                    IsSaving = true;
                    lock (DbContextLock)
                    {
                        part.Amount += element.Amount;
                        DbContext.SaveChanges();
                    }
                    IsSaving = false;
                });

                LoadStandardPartSet();

                DbContextMediator.NotifyContextChanged(this);

                //string txt = "Wypalono: " + element.Part.Name + "\nIlość: " + element.Amount;
                //MessageWindow.Show(txt, "Wypalono część", _window);
            });
            #endregion

            #region CraftPartAmount
            CraftPartAmountCommand = new Command(() =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var win = new CraftPartWindow(SelectedPartSetElement.Part);
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
            #endregion

            #region CraftModule
            CraftModuleCommand = new Command(async () =>
            {
                var module = SelectedModule;

                var waitWin = new WaitWindow("Wypalanie modułu");
                waitWin.Show();

                IsSaving = true;
                await Task.Run(() =>
                {
                    lock (DbContextLock)
                    {
                        foreach (var partSetElement in module.MachineModulePartSet)
                        {
                            partSetElement.Part.Amount += partSetElement.Amount;
                        }
                        DbContext.SaveChanges();
                    }
                });
                IsSaving = false;

                waitWin.Close();

                DbContextMediator.NotifyContextChanged(this);

                //MessageWindow.Show("Wypalono moduł: " + module.Name, "Wypalono moduł");
            });
            #endregion

            #region CraftModuleAmount
            CraftModuleAmountCommand = new Command(async () =>
            {
                throw new NotImplementedException(); // TODO: implement
            });
            #endregion

            #region DeleteMachines
            //DeleteMachinesCommand = new Command(() =>
            //{
            //    var selected = GetSelectedMachines();
            //    if (selected == null)
            //    {
            //        return;
            //    }

            //    string msg = selected.Count == 1
            //        ? "Czy na pewno chcesz usunąć zaznaczoną maszynę?"
            //        : "Czy na pewno chcesz usunąć zaznaczone maszyny?";

            //    var confirmWin = new ConfirmWindow(_window, msg);
            //    confirmWin.Confirmed += delegate
            //    {
            //        var win = new DeleteMachineWindow(DbContext, selected);
            //        win.MachinesDeleted += (sender, machines) =>
            //        {
            //            foreach (var machine in machines)
            //            {
            //                Machines.Remove(machine);
            //            }
            //        };
            //        win.WorkCompleted += delegate
            //        {
            //            StandardPartSet = null;
            //            Orders = null;

            //            OnElementSelected(null);
            //        };
            //        win.ConnectionLost += (sender, exc) =>
            //        {
            //            IsConnected = false;
            //            //MessageWindow.Show("Stracono połączenie.", "Błąd", _window);
            //        };
            //        win.Owner = _window;
            //        win.ShowDialog();
            //        //Task.Run(() => { win.ShowDialog(); });
            //    };
            //    confirmWin.Show();
            //});
            #endregion

            #region DeleteMachinesMuchBetter
            DeleteMachinesCommand = new Command(() =>
            {
                var machines = GetSelectedMachines();
                if (machines == null)
                {
                    return;
                }

                var confirmWin = new ConfirmWindow("Czy na pewno chcesz usunąć zaznaczone maszyny?", _window);
                confirmWin.Confirmed += async delegate
                {
                    var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie...");
                    waitWin.Show();

                    foreach (var machine in machines)
                    {
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                var standardParts = machine.StandardPartSet;
                                DbContext.StandardPartSets.RemoveRange(standardParts);

                                foreach (var module in machine.Modules)
                                {
                                    var moduleParts = module.MachineModulePartSet;
                                    DbContext.MachineModulePartSets.RemoveRange(moduleParts);

                                    module.Orders.Clear();
                                }
                                var modules = machine.Modules;
                                DbContext.MachineModules.RemoveRange(modules);

                                foreach (var order in machine.Orders)
                                {
                                    var additionalParts = order.AdditionalPartSet;
                                    DbContext.AdditionalPartSets.RemoveRange(additionalParts);
                                }
                                DbContext.Orders.RemoveRange(machine.Orders);

                                DbContext.Machines.Remove(machine);
                                DbContext.SaveChanges();
                            }
                        });
                        Machines.Remove(machine);
                    }

                    //foreach (var machine in YieldSelectedMachines())
                    //{
                    //    await Task.Run(() =>
                    //    {
                    //        var standardParts = machine.StandardPartSet;
                    //        DbContext.StandardPartSets.RemoveRange(standardParts);

                    //        foreach (var module in machine.Modules)
                    //        {
                    //            var moduleParts = module.MachineModulePartSet;
                    //            DbContext.MachineModulePartSets.RemoveRange(moduleParts);
                    //        }
                    //        var modules = machine.Modules;
                    //        DbContext.MachineModules.RemoveRange(modules);

                    //        foreach (var order in machine.Orders)
                    //        {
                    //            var additionalParts = order.AdditionalPartSet;
                    //            DbContext.AdditionalPartSets.RemoveRange(additionalParts);
                    //        }
                    //        DbContext.Orders.RemoveRange(machine.Orders);

                    //        DbContext.Machines.Remove(machine);
                    //        DbContext.SaveChanges();
                    //    });
                    //}

                    StandardPartSet = null;
                    Orders = null;
                    Modules = null;

                    DbContextMediator.NotifyContextChanged(this);
                    waitWin.Close();
                };
                confirmWin.Show();
            });
            #endregion

            #region PrintDialog
            PrintDialogCommand = new Command(() =>
            {
                var selected = GetSelectedMachines();
                if (selected == null)
                {
                    return;
                }

                var printDialog = new PrintDialog();
                printDialog.PageRangeSelection = PageRangeSelection.AllPages;
                printDialog.UserPageRangeEnabled = false;
                printDialog.SelectedPagesEnabled = false;

                bool? print = printDialog.ShowDialog();
                if (print == true)
                {
                    //var printWindow = new PrintWindow(selected, printDialog);
                    var printWindow = new PrintWindow();
                    printWindow.PrintAsync(selected, printDialog);
                    printWindow.Show();
                }
            });
            #endregion

            #region Refresh
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
                var win = new MachinesWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });
            #endregion

            #region SaveToFile
            SaveToFileCommand = new Command(() =>
            {
                var selected = GetSelectedMachines();
                if (selected == null)
                {
                    return;
                }

                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Plik Word 2007 (*.docx)|*.docx|Plik PDF (*.pdf)|*.pdf";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = selected.Count == 1
                    ? selected.First().Name
                    : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                if (saveFileDialog.ShowDialog() == true)
                {
                    new SaveFileWindow(selected, saveFileDialog.FileName).Show();
                }
            });
            #endregion

            #region AddPartToMachine
            AddPartToMachineCommand = new Command(() =>
            {
                var machine = SelectedMachine;

                if (machine == null)
                {
                    return;
                }

                var win = new AddPartToMachine(DbContext, machine);
                win.PartAdded += (sender, partSetElement) =>
                {
                    StandardPartSet.Add(partSetElement);
                };
                win.Show();
            });
            #endregion

            #region DeletePartFromMachine
            DeletePartFromMachineCommand = new Command(async () =>
            {
                var element = SelectedPartSetElement;
                if (element == null)
                {
                    return;
                }

                var waitWin = new WaitWindow("Proszę czekać, trwa aktualizacja stanu magazynu...");
                waitWin.Show();
                IsSaving = true;
                await Task.Run(() =>
                {
                    foreach (var order in element.Machine.Orders)
                    {
                        if (order.Status == OrderStatus.InProgress ||
                            order.Status == OrderStatus.Done)
                        {
                            element.Part.Amount += element.Amount;
                        }
                    }
                    DbContext.StandardPartSets.Remove(element);
                    DbContext.SaveChanges();
                });
                IsSaving = false;
                waitWin.Close();

                DbContextMediator.NotifyContextChanged(this);

                //var element = DbContext.StandardPartSets
                //    .Single(el => el.Part.Id == SelectedPartSetElement.Part.Id
                //                                                      && el.Machine.Id == SelectedPartSetElement.Machine.Id);
                //DbContext.StandardPartSets.Remove(element);
                //DbContext.SaveChanges();

                LoadStandardPartSet();
            });
            #endregion

            #region AddMachinesFromDirectory
            AddMachinesFromDirectoryCommand = new Command(() =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();

                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var win = new AddMachinesFromDirectory(dialog.SelectedPath);
                    win.MachineAdded += (sender, machine) =>
                    {
                        Machines.Add(machine);
                    };
                    win.PartSetElementAdded += (sender, element) =>
                    {
                        if (SelectedMachine == null)
                        {
                            return;
                        }

                        if (SelectedMachine.Id == element.Machine.Id)
                        {
                            StandardPartSet.Add(element);
                        }
                    };
                    win.MachineModuleAdded += (sender, module) =>
                    {
                        if (SelectedMachine == null)
                        {
                            return;
                        }

                        if (SelectedMachine.Id == module.Machine.Id)
                        {
                            Modules.Add(module);
                        }
                    };
                    win.WorkCompleted += delegate
                    {
                        Load(); // reload all data to avoid exceptions during later data manipulations
                    };
                    win.Owner = _window;
                    win.ShowDialog();
                }

                dialog.Dispose();
            });
            #endregion

            #region AddMachineModule
            AddMachineModuleCommand = new Command(() =>
            {
                if (SelectedMachine == null)
                {
                    return;
                }

                new AddMachineModuleWindow(SelectedMachine.Id).Show();
            });
            #endregion

            #region DeleteMachineModule
            DeleteMachineModuleCommand = new Command(async () =>
            {
                if (SelectedModule == null)
                {
                    return;
                }

                _window.IsEnabled = false;
                await Task.Run(() =>
                {
                    lock (DbContextLock)
                    {
                        //SelectedModule.MachineModulePartSet.Clear();

                        DbContext.MachineModulePartSets
                            .RemoveRange(DbContext.MachineModulePartSets.Where(e => e.MachineModule.Id == SelectedModule.Id));

                        //foreach (var element in SelectedModule.MachineModulePartSet.ToList())
                        //{
                        //    DbContext.MachineModulePartSets.Remove(element);
                        //}

                        SelectedModule.Orders.Clear();
                        DbContext.MachineModules.Remove(SelectedModule);
                        DbContext.SaveChanges();
                    }
                });

                var window = Application.Current.Windows
                    .OfType<MachineModuleDetailsWindow>()
                    .FirstOrDefault(w => w.Module.Id == SelectedModule.Id);
                if (window != null)
                {
                    window.Close();
                }

                Modules.Remove(SelectedModule);

                DbContextMediator.NotifyContextChanged(this);
                _window.IsEnabled = true;
            });
            #endregion

            #region MachineModuleDetails
            MachineModuleDetailsCommand = new Command(() =>
            {
                if (SelectedModule == null)
                {
                    return;
                }

                var windows = Application.Current.Windows
                    .OfType<MachineModuleDetailsWindow>()
                    .Where(w => w.Module.Id == SelectedModule.Id);
                if (windows.Any())
                {
                    var win = windows.Single();
                    win.Focus();
                }
                else
                {
                    new MachineModuleDetailsWindow(SelectedModule.Id).Show();   
                }
            });
            #endregion

            #region NewSearchWindow
            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchMachinesByName(_window, DbContext);
                win.WorkCompleted += (sender, machines) =>
                {
                    Machines = new ObservableCollection<Machine>(machines);
                    
                    StandardPartSet = null;
                    Orders = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie", win);
                };
                win.Show();
            });
            #endregion

            #region GoToPart
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
            #endregion

            #region GoToOrder
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
            #endregion

            #region CopyModules
            CopyModulesCommand = new Command(() =>
            {
                
            });
            #endregion
        }

        public override void Load()
        {
            LoadMachines();

            IsConnected = true;

            //// if some element were previously selected; needed in refreshing window
            //if (WindowConfig.SelectedElement != null)
            //{
            //    SelectElement(WindowConfig.SelectedElement);
            //}
        }

        public override async Task LoadAsync()
        {
            await LoadMachinesAsync();

            IsConnected = true;

            //if (WindowConfig.SelectedElement != null)
            //{
            //    SelectElement(WindowConfig.SelectedElement);
            //}
        }

        public void LoadMachines()
        {
            try
            {
                List<Machine> machines = null;
                lock (DbContextLock)
                {
                    machines = DbContext.Machines.ToList();
                }
                Machines = new ObservableCollection<Machine>(machines);
            }
            catch (Exception exc)
            {
                IsConnected = false;
                throw exc;
            }
        }

        public Task LoadMachinesAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    List<Machine> machines = null;
                    lock (DbContextLock)
                    {
                        machines = DbContext.Machines.ToList();
                    }
                    Machines = new ObservableCollection<Machine>(machines);
                }
                catch (Exception exc)
                {
                    IsConnected = false;
                }
            });
        }

        public void LoadStandardPartSet()
        {
            var machine = SelectedMachine;

            if (machine == null)
            {
                StandardPartSet = null;
                return;
            }

            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(machine.Id);

                OnPartSetStartLoading();
            }
        }

        public async Task LoadModulesAsync()
        {
            var machine = SelectedMachine;
            if (machine == null)
            {
                Modules = null;
                return;
            }

            OnModulesStartLoading();

            var machineId = machine.Id;

            await Task.Run(() =>
            {
                List<MachineModule> modules = null;
                //using (var context = new SpawmetDBContext()) // use other context, because data in this windows are read-only and it'll not block main context of window
                //{
                //    modules = context.MachineModules
                //        .Where(m => m.Machine.Id == machineId)
                //        //.Include(m => m.MachineModulePartSet)
                //        .OrderBy(m => m.Name)
                //        .ToList();
                //}
                lock (DbContextLock)
                {
                    modules = DbContext.MachineModules
                        .Where(m => m.Machine.Id == machineId)
                        //.Include(m => m.MachineModulePartSet)
                        .OrderBy(m => m.Name)
                        .ToList();
                }

                if (machine == SelectedMachine) // check if it has any sense; it's because SelectedOrder may change during async call
                {
                    Modules = new ObservableCollection<MachineModule>(modules);
                }
            });
            OnModulesCompletedLoading();
        }

        public void LoadOrders()
        {
            var machine = SelectedMachine;

            if (machine == null)
            {
                Orders = null;
                return;
            }

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(machine.Id);

                OnOrdersStartLoading();
            }
        }

        public override void SelectElement(IModelElement element)
        {
            if (element == null)
            {
                StandardPartSet = null;
                Modules = null;
                Orders = null;
                return;
            }

            var machine = Machines.Single(e => e.Id == element.Id);

            SelectedMachine = machine;

            _window.DataGrid.SelectedItem = machine;
            _window.DataGrid.ScrollIntoView(machine);
        }

        private List<Machine> GetSelectedMachines() // bind instead of GetSelectedMachines()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Machine>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Machine)item);
            }

            return selected;
        }

        private IEnumerable<Machine> YieldSelectedMachines()
        {
            //if (_window.MainDataGrid.SelectedItems.Count == 0)
            //{
            //    yield break;
            //}

            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                yield return (Machine)item;
            }
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedMachine;

            return config;
        }

        //private IEnumerable<Machine> GetSelectedMachines()
        //{
        //    if (_window.MainDataGrid.SelectedItems.Count == 0)
        //    {
        //        yield break;
        //    }
        //    else
        //    {
        //        foreach (var item in _window.MainDataGrid.SelectedItems)
        //        {
        //            yield return (Machine) item;
        //        }
        //    }
        //}

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

        private void OnModulesStartLoading()
        {
            if (ModulesStartLoading != null)
            {
                ModulesStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnModulesCompletedLoading()
        {
            if (ModulesCompletedLoading != null)
            {
                ModulesCompletedLoading(this, EventArgs.Empty);
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

        #endregion
    }
}
