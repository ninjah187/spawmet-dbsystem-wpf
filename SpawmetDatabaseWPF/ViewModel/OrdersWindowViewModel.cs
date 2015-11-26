using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
using PrintDialog = System.Windows.Controls.PrintDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class OrdersWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler ModulesStartLoading;
        public event EventHandler ModulesCompletedLoading;

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
                    SelectedElement = _selectedOrder;
                    LoadAdditionalPartSet();
                    LoadModulesAsync();
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

        public ICommand AddOrderCommand { get; protected set; }

        public ICommand DeleteOrdersCommand { get; protected set; }

        public ICommand ChangeStatusCommand { get; protected set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand AddPartToOrderCommand { get; protected set; }

        public ICommand CraftPartCommand { get; protected set; }

        public ICommand CraftPartAmountCommand { get; protected set; }

        public ICommand DeletePartFromOrderCommand { get; protected set; }

        public ICommand PrintDialogCommand { get; protected set; }

        public ICommand SaveToFileCommand { get; protected set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public ICommand AddMachineModuleCommand { get; protected set; }

        public ICommand DeleteMachineModuleCommand { get; protected set; }

        public ICommand MachineModuleDetailsCommand { get; protected set; }

        public ICommand SendMailToClientCommand { get; protected set; }

        public ICommand GoToMachineCommand { get; protected set; }

        public ICommand GoToClientCommand { get; protected set; }

        public ICommand GoToPartCommand { get; protected set; }

        public ICommand GoToModuleCommand { get; protected set; }

        public ICommand ArchiveCommand { get; protected set; }

        public ICommand OrderPriceCalculatorCommand { get; protected set; }

        private bool _arePartsLoading;
        public bool ArePartsLoading
        {
            get { return _arePartsLoading; }
            set
            {
                if (_arePartsLoading != value)
                {
                    _arePartsLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _areModulesLoading;
        public bool AreModulesLoading
        {
            get { return _areModulesLoading; }
            set
            {
                if (_areModulesLoading != value)
                {
                    _areModulesLoading = value;
                }
            }
        }

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

            ConnectionChanged += delegate
            {
                if (IsConnected == false)
                {
                    Orders = null;
                    AdditionalPartSet = null;
                    OnElementSelected(null);
                }
                else
                {
                    if (Orders == null)
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

                ArePartsLoading = false;
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AddOrderCommand = new Command(() =>
            {
                var win = new AddOrderWindow(DbContext);
                win.OrderAdded += async (sender, e) =>
                {
                    Orders.Add(e);

                    // TODO: throw new Exception("Zrób tu wait window");
                    switch (e.Status)
                    {
                        case OrderStatus.InProgress:
                            await ApplyPartSetsAsync(e);
                            break;

                        case OrderStatus.Done:
                            await ApplyPartSetsAsync(e);
                            break;
                    }

                    DbContextMediator.NotifyContextChanged(this);
                };
                win.Show();
            });

            ChangeStatusCommand = new ParamCommand<Order>(async (order) =>
            {
                OrderStatus? oldStatus;
                OrderStatus? newStatus;

                oldStatus = order.Status;
                //SaveDbStateCommand.Execute(null);
                //newStatus = order.Status;

                // SaveDbStateCommand is async so create task that will save dbContext and then will
                // be continued by task which applies part sets

                
                //changeStatusTask.Wait();
                //await changeStatusTask;
            });

            #region DeleteOrders
            DeleteOrdersCommand = new Command(() =>
            {
                var orders = GetSelectedOrders();
                if (orders == null)
                {
                    return;
                }

                var confirmWin = new ConfirmWindow("Czy na pewno chcesz usunąć zaznaczone zamówienia?");
                confirmWin.Confirmed += async delegate
                {
                    var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie...");
                    waitWin.Show();

                    foreach (var order in orders)
                    {
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                var additionalParts = order.AdditionalPartSet;
                                DbContext.AdditionalPartSets.RemoveRange(additionalParts);

                                order.MachineModules.Clear();

                                DbContext.Orders.Remove(order);
                                DbContext.SaveChanges();
                            }
                        });
                        Orders.Remove(order);
                    }

                    AdditionalPartSet = null;
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

                //_window.Close();

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
                win.PartAdded += async (sender, partSetElement) =>
                {
                    AdditionalPartSet.Add(partSetElement);

                    if (partSetElement.Order.Status == OrderStatus.InProgress ||
                        partSetElement.Order.Status == OrderStatus.Done)
                    {
                        IsSaving = true;
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                var part = DbContext.Parts.Single(p => p.Id == partSetElement.Part.Id);

                                part.Amount -= partSetElement.Amount;

                                DbContext.SaveChanges();
                            }
                        });
                        IsSaving = false;

                        LoadAdditionalPartSet();

                        DbContextMediator.NotifyContextChanged(this);
                    }
                };
                win.Show();
            });

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

                LoadAdditionalPartSet();

                DbContextMediator.NotifyContextChanged(this);

                string txt = "Wypalono: " + element.Part.Name + "\nIlość: " + element.Amount;
                MessageWindow.Show(txt, "Wypalono część", _window);
            });

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

            //CraftPartCommand = new Command(() =>
            //{
            //    var element = SelectedPartSetElement;

            //    var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);
            //    part.Amount += element.Amount;

            //    DbContext.SaveChanges();

            //    string txt = "Wypalono: " + part.Name + "\nIlość: " + element.Amount;
            //    MessageBox.Show(txt, "Wypalono część");
            //});

            DeletePartFromOrderCommand = new Command(async () =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var element = DbContext.AdditionalPartSets
                    .Single(el => el.Part.Id == SelectedPartSetElement.Part.Id
                                                                        && el.Order.Id == SelectedPartSetElement.Order.Id);

                if (element.Order.Status == OrderStatus.InProgress ||
                    element.Order.Status == OrderStatus.Done)
                {
                    IsSaving = true;
                    await Task.Run(() =>
                    {
                        lock (DbContextLock)
                        {
                            var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                            part.Amount += element.Amount;

                            DbContext.SaveChanges();
                        }
                    });
                    IsSaving = false;

                    //lock (DbContextLock)
                    //{
                    //    LoadAdditionalPartSet();
                    //}
                }
                
                //lock (DbContextLock)
                //{
                    DbContext.AdditionalPartSets.Remove(element);
                    DbContext.SaveChanges();
                //}

                LoadAdditionalPartSet();

                DbContextMediator.NotifyContextChanged(this);
            });

            PrintDialogCommand = new Command(() =>
            {
                var selected = GetSelectedOrders();
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
                    var printWindow = new PrintWindow();
                    printWindow.PrintAsync(selected, printDialog);
                    printWindow.Show();
                }
            });

            SaveToFileCommand = new Command(() =>
            {
                var selected = GetSelectedOrders();
                if (selected == null)
                {
                    return;
                }

                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Plik Word 2007 (*.docx)|*.docx|Plik PDF (*.pdf)|*.pdf";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = selected.Count == 1
                    ? selected.First().Machine.Name
                    : "Wykaz zamówień, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                if (saveFileDialog.ShowDialog() == true)
                {
                    new SaveFileWindow(selected, saveFileDialog.FileName).Show();
                }
            });

            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchOrdersWindow(_window, DbContext);
                win.WorkCompleted += (sender, orders) =>
                {
                    Orders = new ObservableCollection<Order>(orders);

                    AdditionalPartSet = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie", win);
                };
                win.Show();
            });

            SendMailToClientCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                if (order.Client == null)
                {
                    MessageWindow.Show("Zamówienie nie jest powiązane z żadnym klientem.", "Błąd", null);
                    return;
                }

                if (order.Client.Email == "")
                {
                    MessageWindow.Show("Klient powiązany z zamówieniem nie ma przypisanego adresu e-mail.", "Błąd", null);
                    return;
                }

                var win = new SendConfirmationWindow(order);
                win.WorkCompleted += delegate
                {
                    order.ConfirmationSent = true;
                    MessageWindow.Show("Wysyłanie potwierdzenia zakończono powodzeniem.", "Zakończono", null);
                };
                win.Owner = _window;
                win.ShowDialog();
            });

            #region AddMachineModule
            AddMachineModuleCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                var win = new AddMachineModuleToOrderWindow(order.Id);
                win.ModuleAdded += async (sender, mod) =>
                {
                    MachineModule module = null;

                    module = DbContext.MachineModules.Single(m => m.Id == mod.Id);

                    Modules.Add(module);

                    if (order.Status == OrderStatus.InProgress ||
                        order.Status == OrderStatus.Done)
                    {
                        IsSaving = true;
                        await Task.Run(() =>
                        {
                            lock (DbContextLock)
                            {
                                foreach (var element in module.MachineModulePartSet)
                                {
                                    var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                                    part.Amount -= element.Amount;
                                }
                                DbContext.SaveChanges();
                            }
                        });

                        DbContextMediator.NotifyContextChanged(this);

                        IsSaving = false;
                    }
                };
                win.ShowDialog();
            });
            #endregion

            #region DeleteMachineModule
            DeleteMachineModuleCommand = new Command(async () =>
            {
                var module = SelectedModule;
                var order = SelectedOrder;
                if (module == null)
                {
                    return;
                }

                _window.IsEnabled = false;
                IsSaving = true;
                await Task.Run(() =>
                {
                    lock (DbContextLock)
                    {
                        order.MachineModules.Remove(module);
                        if (order.Status == OrderStatus.InProgress ||
                            order.Status == OrderStatus.Done)
                        {
                            foreach (var element in module.MachineModulePartSet)
                            {
                                element.Part.Amount += element.Amount;
                            }
                        }
                        DbContext.SaveChanges();
                    }
                });

                DbContextMediator.NotifyContextChanged(this);

                Modules.Remove(module);
                IsSaving = false;
                _window.IsEnabled = true;
            });
            #endregion

            GoToMachineCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<MachinesWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(order.Machine);
                }
                else
                {
                    //NewMachinesWindowCommand.Execute(null);
                    var config = new WindowConfig()
                    {
                        Left = _window.Left + Offset,
                        Top = _window.Top + Offset,
                        SelectedElement = order.Machine
                    };
                    var window = new MachinesWindow(config);
                    window.Show();
                }
            });

            GoToClientCommand = new Command(() =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<ClientsWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(order.Client);
                }
                else
                {
                    var config = new WindowConfig()
                    {
                        Left = _window.Left + Offset,
                        Top = _window.Top + Offset,
                        SelectedElement = order.Client
                    };
                    var window = new ClientsWindow(config);
                    window.Show();
                }
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

            #region GoToModule
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

            #region Archive
            ArchiveCommand = new Command(async () =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                var waitWin = new WaitWindow("Proszę czekać, trwa archiwizowanie...");
                waitWin.Show();
                
                await Task.Run(() =>
                {
                    var archivedOrder = new ArchivedOrder(order);

                    DbContext.ArchivedOrders.Add(archivedOrder);

                    DbContext.SaveChanges();
                });

                DbContextMediator.NotifyContextChanged(this);

                waitWin.Close();
            });
            #endregion

            #region OrderPriceCalculator
            OrderPriceCalculatorCommand = new Command(() =>
            {
                if (SelectedOrder == null)
                {
                    return;
                }

                new OrderPriceCalculatorWindow(SelectedOrder.Id).Show();
            });
            #endregion
        }

        public override void Load()
        {
            LoadOrders();
            LoadClients();
            LoadMachines();

            IsConnected = true;

            //if (WindowConfig.SelectedElement != null)
            //{
            //    SelectElement(WindowConfig.SelectedElement);
            //}
        }

        public override async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                LoadOrders();
                LoadClients();
                LoadMachines();
            });

            IsConnected = true;

            //if (WindowConfig.SelectedElement != null)
            //{
            //    SelectElement(WindowConfig.SelectedElement);
            //}
        }

        private void LoadOrders()
        {
            List<Order> orders;
            lock (DbContextLock)
            {
                orders = DbContext.Orders
                .Include(o => o.Client)
                .Include(o => o.Machine)
                .ToList();
            }

            Orders = new ObservableCollection<Order>(orders);
        }

        private void LoadClients()
        {
            List<Client> clients;
            //var clients = DbContext.Clients.ToList();
            lock (DbContextLock)
            {
                clients = DbContext.Clients.ToList();
            }
            Clients = new ObservableCollection<Client>(clients);
        }

        private void LoadMachines()
        {
            //var machines = DbContext.Machines.ToList();
            List<Machine> machines;
            lock (DbContextLock)
            {
                machines = DbContext.Machines.ToList();
            }
            Machines = new ObservableCollection<Machine>(machines);
        }

        private void LoadAdditionalPartSet()
        {
            if (SelectedOrder == null)
            {
                AdditionalPartSet = null;
                return;
            }

            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(SelectedOrder.Id);

                ArePartsLoading = true;
            }
        }

        public async Task LoadModulesAsync()
        {
            var order = SelectedOrder;
            if (order == null)
            {
                Modules = null;
                return;
            }

            AreModulesLoading = true;

            await Task.Run(() =>
            {
                List<MachineModule> modules = null;
                lock (DbContextLock)
                {
                    //modules = DbContext.MachineModules
                    //    .Where(m => m.Orders.Contains(order))
                    //    .OrderBy(m => m.Name)
                    //    .ToList();
                    modules = order.MachineModules.OrderBy(m => m.Name).ToList();
                }

                if (order == SelectedOrder) // check if it has any sense; it's because SelectedOrder may change during async call                    
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
                AdditionalPartSet = null;
                Modules = null;
                return;
            }

            var order = Orders.Single(o => o.Id == element.Id);

            SelectedOrder = order;

            _window.DataGrid.SelectedItem = order;
            _window.DataGrid.ScrollIntoView(order);
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

        public async void ChangeStatus(OrderStatus oldStatus, OrderStatus newStatus)
        {
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

            switch (oldStatus)
            {
                case OrderStatus.New:
                    switch (newStatus)
                    {
                        case OrderStatus.InProgress:
                            await ApplyPartSetsAsync(SelectedOrder);
                            DbContextMediator.NotifyContextChanged(this);
                            // zamiast async metody i await, powinno wystarczyć
                            // Task.Run(() => { ApplyPartSets(SelectedOrder) });
                            break;

                        case OrderStatus.Done:
                            await ApplyPartSetsAsync(SelectedOrder);
                            DbContextMediator.NotifyContextChanged(this);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case OrderStatus.InProgress:
                    switch (newStatus)
                    {
                        case OrderStatus.New:
                            await UndoApplyPartSetsAsync(SelectedOrder);
                            DbContextMediator.NotifyContextChanged(this);
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
                            await UndoApplyPartSetsAsync(SelectedOrder);
                            DbContextMediator.NotifyContextChanged(this);
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
            foreach (var module in order.MachineModules)
            {
                foreach (var element in module.MachineModulePartSet)
                {
                    var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                    part.Amount -= element.Amount;
                }
            }
            DbContext.SaveChanges();
            //SaveDbStateCommand.Execute(null);
        }

        private async Task ApplyPartSetsAsync(Order order)
        {
            //return Task.Run(() =>
            //{
            //    IsSaving = true;
            //    lock (DbContextLock)
            //    {
            //        ApplyPartSets(order);
            //    }
            //    IsSaving = false;
            //});

            var win = new WaitWindow("Proszę czekać, trwa aktualizacja stanu magazynu...");

            IsSaving = true;
            var task = Task.Run(() =>
            {
                lock (DbContextLock)
                {
                    ApplyPartSets(order);
                }
            });
            win.Show();
            await task;
            IsSaving = false;

            win.Close();
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
            foreach (var module in order.MachineModules)
            {
                foreach (var element in module.MachineModulePartSet)
                {
                    var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);

                    part.Amount += element.Amount;
                }
            }
            DbContext.SaveChanges();
        }

        private async Task UndoApplyPartSetsAsync(Order order)
        {
            //return Task.Run(() =>
            //{
            //    IsSaving = true;
            //    lock (DbContextLock)
            //    {
            //        UndoApplyPartSets(order);
            //    }
            //    IsSaving = false;
            //});
            var win = new WaitWindow("Proszę czekać, trwa aktualizacja stanu magazynu...");

            IsSaving = true;
            var task = Task.Run(() =>
            {
                lock (DbContextLock)
                {
                    UndoApplyPartSets(order);
                }
            });
            win.Show();
            await task;
            IsSaving = false;

            win.Close();
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedOrder;

            return config;
        }

        #region Event invokers.

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
