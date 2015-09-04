using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for ArchivedModuleDetailsWindow.xaml
    /// </summary>
    public partial class ArchivedModuleDetailsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ArchivedMachine _machine;
        public ArchivedMachine Machine
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

        private ArchivedMachineModule _module;
        public ArchivedMachineModule Module
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

        private ObservableCollection<ArchivedMachineModuleSetElement> _parts;
        public ObservableCollection<ArchivedMachineModuleSetElement> Parts
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

        private SpawmetDBContext _dbContext;

        private int _moduleId;

        public ArchivedModuleDetailsWindow(int archivedModuleId)
        {
            InitializeComponent();

            _moduleId = archivedModuleId;

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
                    Module = _dbContext.ArchivedModules.Single(m => m.Id == _moduleId);
                }
                catch (InvalidOperationException)
                {
                    Application.Current.Dispatcher.Invoke(Close);
                } // wtf ?

                Machine = Module.Order.Machine;
            });
            Parts = new ObservableCollection<ArchivedMachineModuleSetElement>(Module.Parts.ToList());
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
