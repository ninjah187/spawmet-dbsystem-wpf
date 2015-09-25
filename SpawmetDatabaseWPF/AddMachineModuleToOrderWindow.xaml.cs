using System;
using System.Collections.Generic;
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
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows;
using SpawmetDatabaseWPF.Windows.Common;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddMachineModuleToOrderWindow.xaml
    /// </summary>
    public partial class AddMachineModuleToOrderWindow : Window, INotifyPropertyChanged, IDbContextChangesNotifier
    {
        public event EventHandler<MachineModule> ModuleAdded;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<MachineModule> _modules;
        public List<MachineModule> Modules
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
        //private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        private SpawmetDBContext _dbContext;

        private int _orderId;

        private WindowsEnablementController _winController;

        public AddMachineModuleToOrderWindow(int orderId)
        {
            InitializeComponent();

            _orderId = orderId;

            _winController = new WindowsEnablementController();

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);
            //Mediator.ContextChanged += async (sender, notifier) =>
            //{
            //    if (notifier != this)
            //    {
            //        await LoadAsync();
            //    }
            //};

            Loaded += async delegate
            {
                await LoadAsync();
            };

            Closed += delegate
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }

                DbContextMediator.Subscribers.Remove(this);
            };

            SizeChanged += delegate
            {
                var binding = ModulesListBox.GetBindingExpression(ListBox.HeightProperty);
                binding.UpdateTarget();

                binding = ModulesListBox.GetBindingExpression(ListBox.WidthProperty);
                binding.UpdateTarget();
            };
        }

        public async Task LoadAsync()
        {
            IsEnabled = false;
            await Task.Run(() =>
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }
                _dbContext = new SpawmetDBContext();

                //try
                //{
                    Order = _dbContext.Orders
                        .Include(o => o.Machine)
                        .Include(o => o.Client)
                        .FirstOrDefault(o => o.Id == _orderId);
                //}
                //catch (InvalidOperationException)
                //{
                //    Application.Current.Dispatcher.Invoke(Close);
                //}

                if (Order == null)
                {
                    Application.Current.Dispatcher.Invoke(Close);
                    return;
                }

                var modulesIds = Order.MachineModules.Select(m => m.Id);

                Modules = Order.Machine.Modules
                    .Where(m => modulesIds.Any(id => id == m.Id) == false)
                    .OrderBy(m => m.Name)
                    .ToList();
            });
            IsEnabled = true;
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var module = SelectedModule;
            if (module == null)
            {
                return;
            }

            IsEnabled = false;
            await Task.Run(() =>
            {
                Order.MachineModules.Add(module);
                _dbContext.SaveChanges();
            });
            
            // to samo co w obsłudze zdarzenia OrdersWindowViewModel.ModuleAdded
            if (Order.Status == OrderStatus.InProgress ||
                Order.Status == OrderStatus.Done)
            {
                //_winController.DisableWindows();

                var win = new WaitWindow("Proszę czekać, trwa aktualizacja stanu magazynu...");
                win.Show();
                await Task.Run(() =>
                {
                    foreach (var element in module.MachineModulePartSet)
                    {
                        var part = _dbContext.Parts.Single(p => p.Id == element.Part.Id);

                        part.Amount -= element.Amount;
                    }
                    _dbContext.SaveChanges();
                });
                win.Close();
                //_winController.EnableWindows();
            }

            DbContextMediator.NotifyContextChanged(this);

            //OnModuleAdded(module);

            Close();
        }

        private void OnModuleAdded(MachineModule module)
        {
            if (ModuleAdded != null)
            {
                ModuleAdded(this, module);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
