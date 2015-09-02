using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Windows;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class ArchiveWindowViewModel : SpawmetWindowViewModel
    {
        private ObservableCollection<ArchivedOrder> _orders;
        public ObservableCollection<ArchivedOrder> Orders
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

        private ArchivedOrder _selectedOrder;
        public ArchivedOrder SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                    SelectedElement = _selectedOrder;
                    LoadStandardPartSetAsync();
                    LoadAdditionalPartSetAsync();
                    LoadModulesAsync();
                }
            }
        }

        private ObservableCollection<ArchivedStandardPartSetElement> _standardPartSet;
        public ObservableCollection<ArchivedStandardPartSetElement> StandardPartSet
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

        private ObservableCollection<ArchivedAdditionalPartSetElement> _additionalPartSet;
        public ObservableCollection<ArchivedAdditionalPartSetElement> AdditionalPartSet
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

        private ObservableCollection<ArchivedMachineModule> _modules;
        public ObservableCollection<ArchivedMachineModule> Modules
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

        private ArchivedMachineModule _selectedModule;
        public ArchivedMachineModule SelectedModule
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

        private bool _isStandardPartSetLoading;
        public bool IsStandardPartSetLoading
        {
            get { return _isStandardPartSetLoading; }
            set
            {
                if (_isStandardPartSetLoading != value)
                {
                    _isStandardPartSetLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isAdditionalPartSetLoading;
        public bool IsAdditionalPartSetLoading
        {
            get { return _isAdditionalPartSetLoading; }
            set
            {
                if (_isAdditionalPartSetLoading != value)
                {
                    _isAdditionalPartSetLoading = value;
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
                    OnPropertyChanged();
                }
            }
        }

        public ICommand DeleteOrderCommand { get; protected set; }

        private ArchiveWindow _window;

        public ArchiveWindowViewModel(ArchiveWindow window)
            : this(window, null)
        {
        }

        public ArchiveWindowViewModel(ArchiveWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            DeleteOrderCommand = new Command(async () =>
            {
                var order = SelectedOrder;
                if (order == null)
                {
                    return;
                }

                var waitWin = new WaitWindow("Proszę czekać, trwa usuwanie zamówienia...");
                waitWin.Show();
                await Task.Run(() =>
                {
                    var standardParts = order.Machine.Parts;
                    DbContext.ArchivedStandardPartSets.RemoveRange(standardParts);

                    var additionalParts = order.Parts;
                    DbContext.ArchivedAdditionalPartSets.RemoveRange(additionalParts);

                    foreach (var module in order.Modules)
                    {
                        var moduleParts = module.Parts;
                        DbContext.ArchivedMachineModulePartSets.RemoveRange(moduleParts);
                    }

                    var modules = order.Modules;
                    DbContext.ArchivedModules.RemoveRange(modules);

                    DbContext.ArchivedMachines.Remove(order.Machine);
                    if (order.Client != null)
                    {
                        DbContext.ArchivedClients.Remove(order.Client);
                    }
                    DbContext.ArchivedOrders.Remove(order);
                    DbContext.SaveChanges();

                    StandardPartSet = null;
                    AdditionalPartSet = null;
                    Modules = null;
                });
                Orders.Remove(order);
                waitWin.Close();
            });
        }

        // that's fucking nonsense!!! check out spaghetti code at loading data in SpawmetWindowViewModel
        // I mean that Load() is async called in ReloadContextAsync() and it's working so there's no point in distincting Load and LoadAsync methods...
        public override void Load()
        {
            LoadOrders();

            if (WindowConfig.SelectedElement != null)
            {
                SelectElement(WindowConfig.SelectedElement);
            }
        }

        public override async Task LoadAsync()
        {
            await LoadOrdersAsync();

            //IsConnected = true;

            if (WindowConfig.SelectedElement != null)
            {
                SelectElement(WindowConfig.SelectedElement);
            }
        }

        public void LoadOrders()
        {
            List<ArchivedOrder> orders = null;
            lock (DbContextLock)
            {
                orders = DbContext.ArchivedOrders.ToList();
            }
            Orders = new ObservableCollection<ArchivedOrder>(orders);
        }

        public async Task LoadOrdersAsync()
        {
            //_window.IsEnabled = false;
            Task.Run(() => LoadOrders());
        }

        public async Task LoadStandardPartSetAsync()
        {
            var order = SelectedOrder;
            if (order == null)
            {
                return;
            }

            IsStandardPartSetLoading = true;
            await Task.Run(() =>
            {
                List<ArchivedStandardPartSetElement> parts = null;
                lock (DbContextLock)
                {
                    parts = order.Machine.Parts.ToList();
                }

                if (order == SelectedOrder)
                {
                    StandardPartSet = new ObservableCollection<ArchivedStandardPartSetElement>(parts);
                }
            });
            IsStandardPartSetLoading = false;
        }

        public async Task LoadAdditionalPartSetAsync()
        {
            var order = SelectedOrder;
            if (order == null)
            {
                return;
            }

            IsAdditionalPartSetLoading = true;
            await Task.Run(() =>
            {
                List<ArchivedAdditionalPartSetElement> parts = null;
                lock (DbContextLock)
                {
                    parts = order.Parts.ToList();
                }

                if (order == SelectedOrder)
                {
                    AdditionalPartSet = new ObservableCollection<ArchivedAdditionalPartSetElement>(parts);
                }
            });
            IsAdditionalPartSetLoading = false;
        }

        public async Task LoadModulesAsync()
        {
            var order = SelectedOrder;
            if (order == null)
            {
                return;
            }

            AreModulesLoading = true;
            await Task.Run(() =>
            {
                List<ArchivedMachineModule> modules = null;
                lock (DbContextLock)
                {
                    modules = order.Modules.ToList();
                }

                if (order == SelectedOrder)
                {
                    Modules = new ObservableCollection<ArchivedMachineModule>(modules);
                }
            });
            AreModulesLoading = false;
        }

        public override void SelectElement(IModelElement element)
        {
            var order = Orders.Single(e => e.Id == element.Id);

            SelectedOrder = order;

            _window.DataGrid.SelectedItem = order;
            _window.DataGrid.ScrollIntoView(order);
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedOrder;

            return config;
        }
    }
}
