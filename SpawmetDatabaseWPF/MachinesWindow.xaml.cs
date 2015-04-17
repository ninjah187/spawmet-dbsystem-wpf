using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
            get
            {
                try
                {
                    return _dbContext.Machines.Local;
                }
                catch (ProviderIncompatibleException exc)
                {
                    throw new ProviderIncompatibleException("in DataGridItemsSource");
                }
            }
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

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Machine machine = null;
                try
                {
                    machine = (Machine)MainDataGrid.SelectedItem;
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
                FillDetailedInfo(null);
                if (_parentWindow != null)
                {
                    _parentWindow.MachinesWindowButton.IsEnabled = false;
                }
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                if (_parentWindow != null)
                {
                    _parentWindow.MachinesWindowButton.IsEnabled = true;
                }
            };

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                Disconnected();
            }
        }

        private void Initialize()
        {
            _dbContext = new SpawmetDBContext();

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();
        }

        public void FillDetailedInfo(Machine machine)
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

            try
            {
                StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet.OrderBy(element => element.Part.Id);
                OrdersListBox.ItemsSource = machine.Orders.OrderBy(order => order.Id);
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

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
            try
            {
                _dbContext.Machines.Load();
                ConnectMenuItem.IsEnabled = false;
            }
            catch (EntityException exc)
            {
                Disconnected();
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
            try
            {
                _dbContext.StandardPartSets.Remove(partSetElement);
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

            StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet.OrderBy(element => element.Part.Id);
        }

        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddMachineWindow(this, _dbContext).Show();
        }

        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

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
            try
            {
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
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

            FillDetailedInfo(null);
        }

        private void PartsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new PartsWindow(this.Left, this.Top).Show();
            }
            catch (EntityException exc)
            {
                Disconnected();
                return;
            }
            this.Close();
        }

        private void SaveContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
            }
        }

        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectMenuItem_OnClick(sender, e);
        }

        private void ConnectMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new MachinesWindow(this.Left, this.Top).Show();
                this.Close();
            }
            catch (ProviderIncompatibleException exc)
            {
                Disconnected();
            }
        }

        private void Disconnected()
        {
            MainDataGrid.IsEnabled = false;
            DetailsStackPanel.IsEnabled = false;
            PartsMenuItem.IsEnabled = false;
            FillDetailedInfo(null);
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
            ConnectMenuItem.IsEnabled = true;
        }
    }
}
