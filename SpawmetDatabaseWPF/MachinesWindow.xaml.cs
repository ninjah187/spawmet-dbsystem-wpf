using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core;
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
    /// Interaction logic for MachinesWindow.xaml
    /// </summary>
    public partial class MachinesWindow : Window
    {
        public ObservableCollection<Machine> DataGridItemsSource
        {
            get { return _dbContext.Machines.Local; }
        }

        public ObservableCollection<PartSetElement> PartSetDataGridSource { get; set; }

        private SpawmetDBContext _dbContext;

        private readonly MasterWindow _parentWindow;

        public MachinesWindow()
            : this(null)
        {
            
        }

        public MachinesWindow(double x, double y)
            : this(null)
        {
            this.Left = x;
            this.Top = y;
        }

        public MachinesWindow(MasterWindow parentWindow)
        {
            InitializeComponent();

            _parentWindow = parentWindow;

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                MessageBox.Show("Brak połączenia z serwerem");
                Application.Current.Shutdown();
            }
        }

        private void Initialize()
        {
            _dbContext = new SpawmetDBContext();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Machine machine = null;
                try
                {
                    machine = (Machine) MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    FillDetailedInfo(null);
                    return;
                }
                if (machine != null)
                {
                    FillDetailedInfo(machine);
                }
            };

            this.Loaded += (sender, e) =>
            {
                LoadDataIntoSource();
                FillDetailedInfo(null);
                if (_parentWindow != null)
                {
                    _parentWindow.MachinesWindowButton.IsEnabled = false;
                }
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.SaveChanges();
                _dbContext.Dispose();
                if (_parentWindow != null)
                {
                    _parentWindow.MachinesWindowButton.IsEnabled = true;
                }
            };
        }

        private void FillDetailedInfo(Machine machine)
        {
            if (machine == null)
            {
                IdTextBlock.Text = "";
                NameTextBlock.Text = "";
                PriceTextBlock.Text = "";

                StandardPartSetDataGrid.ItemsSource = null;
                OrdersListBox.ItemsSource = null;
                return;
            }

            IdTextBlock.Text = machine.Id.ToString();
            NameTextBlock.Text = machine.Name;
            PriceTextBlock.Text = machine.Price.ToString();
            
            StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet.OrderBy(element => element.Part.Id);
            OrdersListBox.ItemsSource = machine.Orders.OrderBy(order => order.Id);
            #region
            //string info = "";

            //info += "ID: " + machine.Id +
            //        "\nNazwa: " + machine.Name +
            //        "\nCena: " + machine.Price +
            //        "\n";
            //info += "Standardowy zestaw części:\n";
            //foreach (var part in machine.StandardPartSet)
            //{
            //    info += "- " + part.Name + "\n";
            //}
            //info += "Zamówienia:\n";
            //foreach (var order in machine.Orders)
            //{
            //    string clientName = order.Client != null ? order.Client.Name : "";

            //    string txt = clientName + ", " + order.StartDate.ToShortDateString();
            //    info += "- " + txt + "\n";
            //}

            //DetailTextBlock.Text = info;
            #endregion
        }

        private void LoadDataIntoSource()
        {
            if (DataGridItemsSource.Any())
            {
                DataGridItemsSource.Clear();
            }
            
            foreach (var machine in _dbContext.Machines)
            {
                DataGridItemsSource.Add(machine);
            }
        }

        private void AddPartItem_OnClick(object sender, RoutedEventArgs e)
        {
            Machine machine = null;
            try
            {
                machine = (Machine) MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                return;
            }
            finally
            {
                if (machine != null)
                {
                    new AddPartToMachine(this, _dbContext, machine).Show();
                }
            }
        }

        private void DeletePartItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dataGrid = StandardPartSetDataGrid;
            var machine = (Machine) MainDataGrid.SelectedItem;
            var partSetElement = (StandardPartSetElement) dataGrid.SelectedItem;

            if (partSetElement == null)
            {
                return;
            }

            //machine.StandardPartSet.Remove(partSetElement);
            _dbContext.StandardPartSets.Remove(partSetElement);
            _dbContext.SaveChanges();

            StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet.OrderBy(element => element.Part.Id);
        }

        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new AddMachineWindow(this, _dbContext).Show();
        }

        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Machine>();
            foreach (var item in selected)
            {
                Machine machine = null;
                try
                {
                    machine = (Machine) item;
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
                toDelete.Add(machine);
            }
            foreach (var machine in toDelete)
            {
                foreach (var standardPartSetElement in machine.StandardPartSet.ToList())
                {
                    _dbContext.StandardPartSets.Remove(standardPartSetElement);
                    _dbContext.SaveChanges();
                }

                var relatedOrders = _dbContext.Orders.Where(o => o.Machine.Id == machine.Id).ToList();
                foreach (var order in relatedOrders)
                {
                    foreach (var additionalPartSet in order.AdditionalPartSet.ToList())
                    {
                        _dbContext.AdditionalPartSets.Remove(additionalPartSet);
                        _dbContext.SaveChanges();
                    }
                    _dbContext.Orders.Remove(order);
                    _dbContext.SaveChanges();
                }
                _dbContext.Machines.Remove(machine);
                _dbContext.SaveChanges();
            }
            //_dbContext.SaveChanges();

            FillDetailedInfo(null);
        }

        private void PartsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new PartsWindow(this.Left, this.Top).Show();
            this.Close();
        }

        private void SaveContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _dbContext.SaveChanges();
        }
    }
}
