using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for MachineModuleDetailsWindow.xaml
    /// </summary>
    public partial class MachineModuleDetailsWindow : Window, INotifyPropertyChanged, IDbContextChangesNotifier//, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Machine _machine;
        public Machine Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    OnPropertyChanged();
                }
            }
        }

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

        private ObservableCollection<MachineModuleSetElement> _parts;
        public ObservableCollection<MachineModuleSetElement> Parts
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

        private MachineModuleSetElement _selectedPartSetElement;
        public MachineModuleSetElement SelectedPartSetElement
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

        public ICommand AddPartCommand { get; set; }
        public ICommand DeletePartCommand { get; set; }
        public ICommand GoToPartCommand { get; set; }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        private SpawmetDBContext _dbContext;
        private object _dbContextLock = new object();

        private int _moduleId;

        public MachineModuleDetailsWindow(int moduleId)
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

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddPartCommand = new Command(() =>
            {
                var win = new AddPartToModuleWindow(Module.Id);
                win.Owner = this;
                win.ShowDialog();
            });

            GoToPartCommand = new Command(() =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var windows = Application.Current.Windows.OfType<PartsWindow>();
                if (windows.Any())
                {
                    var window = windows.Single();
                    window.Focus();

                    window.Select(SelectedPartSetElement.Part);
                }
                else
                {
                    var config = new WindowConfig()
                    {
                        SelectedElement = SelectedPartSetElement.Part,
                    };
                    var window = new PartsWindow(config);
                    window.Show();
                }
            });

            DeletePartCommand = new Command(async () =>
            {
                var partSetElement = SelectedPartSetElement;
                if (partSetElement == null)
                {
                    return;
                }

                //IsEnabled = false;
                var waitWin = new WaitWindow("Proszę czekać, trwa aktualizowanie magazynu...");
                waitWin.Show();

                await Task.Run(() =>
                {
                    Module.MachineModulePartSet.Remove(partSetElement);

                    foreach (var order in Module.Orders)
                    {
                        if (order.Status == OrderStatus.InProgress ||
                            order.Status == OrderStatus.Done)
                        {
                            partSetElement.Part.Amount += partSetElement.Amount;
                        }   
                    }

                    _dbContext.SaveChanges();
                });
                Parts.Remove(SelectedPartSetElement);
                DbContextMediator.NotifyContextChanged(this);

                waitWin.Close();
                //IsEnabled = true;
            });


        }

        ////public void Dispose()
        //{
        //    if (_dbContext != null)
        //    {
        //        _dbContext.Dispose();
        //    }
        //}

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
                    Module = _dbContext.MachineModules.Single(m => m.Id == _moduleId);
                }
                catch (InvalidOperationException)
                {
                    Application.Current.Dispatcher.Invoke(Close);
                } // Sequence has no elements. ??? windows are not closing properly?

                Machine = Module.Machine;
            });
            Parts = new ObservableCollection<MachineModuleSetElement>(Module.MachineModulePartSet.ToList()); // this line is outside task so loading a window looks better
            IsEnabled = true;
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
