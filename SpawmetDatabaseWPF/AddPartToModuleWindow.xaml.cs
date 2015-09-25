using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
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
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows;
using SpawmetDatabaseWPF.Windows.Common;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddPartToModuleWindow.xaml
    /// </summary>
    public partial class AddPartToModuleWindow : Window, INotifyPropertyChanged, IDbContextChangesNotifier
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MachineModule _module;
        public MachineModule Module
        {
            get { return _module; }
            set
            {
                if (_module != value)
                {
                    _module = value;
                    OnPropertyChanged();
                }
            }
        }

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
                }
            }
        }

        public ICommand AddPartCommand { get; set; }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        private SpawmetDBContext _dbContext;

        private int _moduleId;

        public AddPartToModuleWindow(int moduleId)
        {
            InitializeComponent();

            _moduleId = moduleId;

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);
            ContextChangedHandler = async delegate
            {
                await LoadAsync();
            };

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

                try
                {
                    Module = _dbContext.MachineModules
                        .Include(m => m.Machine)
                        //.Include(m => m.MachineModulePartSet)
                        .Single(m => m.Id == _moduleId);
                }
                catch (InvalidOperationException)
                {
                    Application.Current.Dispatcher.Invoke(Close);
                }

                var partIds = Module.MachineModulePartSet.Select(el => el.Part.Id);

                var parts = _dbContext.Parts
                    .Where(p => partIds.Any(id => id == p.Id) == false)
                    .OrderBy(p => p.Name)
                    .ToList();
                //var parts = _dbContext.Parts
                //    .Where(p => machineModulePartSet.Any(el => el.Part.Id == p.Id) == false)
                //    .OrderBy(p => p.Name)
                //    .ToList();
                Parts = new ObservableCollection<Part>(parts);
            });
            IsEnabled = true;
        }

        public async Task AddPartAsync()
        {
            if (SelectedPart == null)
            {
                return;
            }

            int amount;
            try
            {
                amount = int.Parse(AmountTextBox.Text);
            }
            catch (FormatException)
            {
                MessageWindow.Show("Ilość musi być liczbą całkowitą.", "Błąd");
                return;
            }

            var waitWin = new WaitWindow("Proszę czekać, trwa aktualizowanie stanu magazynu...");
            waitWin.Show();

            var task = Task.Run(() =>
            {
                var partSetElement = new MachineModuleSetElement()
                {
                    Amount = amount,
                    Part = SelectedPart,
                };

                Module.MachineModulePartSet.Add(partSetElement);

                foreach (var order in Module.Orders)
                {
                    if (order.Status == OrderStatus.InProgress ||
                        order.Status == OrderStatus.Done)
                    {
                        //var part = _dbContext.Parts.Single(p => p.Id == partSetElement.Part.Id);
                        
                        //part.Amount -= partSetElement.Amount;

                        partSetElement.Part.Amount -= partSetElement.Amount;
                    }
                }

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (EntityException exc)
                {
                    ExceptionWindow.Show(exc);
                }
            });
            IsEnabled = false;
            await task;
            DbContextMediator.NotifyContextChanged(this);
            
            waitWin.Close();

            Close();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AddPartAsync();
        }
    }
}
