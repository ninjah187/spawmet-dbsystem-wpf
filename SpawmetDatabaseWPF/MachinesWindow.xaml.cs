using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private BackgroundWorker _partsBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;

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
                Disconnected("Kod błędu 00.");
            }
        }

        private void Initialize()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }

            _dbContext = new SpawmetDBContext();

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();

            _partsBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += _backgroundWorker_DoWorkStandardParts;
            _partsBackgroundWorker.RunWorkerCompleted += _backgroundWorker_StandardPartsCompleted;
            _ordersBackgroundWorker.DoWork += _backgroundWorker_DoWorkOrders;
            _ordersBackgroundWorker.RunWorkerCompleted += _backgroundWorker_OrdersCompleted;
        }

        private void _backgroundWorker_DoWorkStandardParts(object sender, DoWorkEventArgs e)
        {
            //var machine = (Machine) e.Argument;
            //e.Result = machine.StandardPartSet;

            var machineId = (int) e.Argument;
            var context = new SpawmetDBContext();
            var result = context.StandardPartSets
                .Where(el => el.Machine.Id == machineId)
                .OrderBy(el => el.Part.Name)
                .ToList();
            result.ForEach(el => el.PropertyChanged += (s, args) =>
            {
                StandardPartSetDataGrid.Items.Refresh();
            });
            e.Result = result;
            context.Dispose();
        }

        private void _backgroundWorker_StandardPartsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var source = (ICollection<StandardPartSetElement>)e.Result;
            StandardPartSetDataGrid.ItemsSource = source;

            StandardPartSetProgressBar.IsIndeterminate = false;
            
            //if (_ordersBackgroundWorker.IsBusy == false)
            //{
            //    Machine machine;
            //    try
            //    {
            //        machine = (Machine)MainDataGrid.SelectedItem;
            //    }
            //    catch (InvalidCastException exc)
            //    {
            //        machine = null;
            //    }
            //    if (machine != null)
            //    {
            //        _ordersBackgroundWorker.RunWorkerAsync(machine);
            //    }
            //}
        }

        private void _backgroundWorker_DoWorkOrders(object sender, DoWorkEventArgs e)
        {
            //var machine = (Machine)e.Argument;
            //e.Result = machine.Orders;

            var machineId = (int) e.Argument;
            var context = new SpawmetDBContext();
            //var result = context.Orders
            //    .Where(m => m.Machine.Id == machineId)
            //    .OrderBy(o => o.Id)
            //    .ToList();
            var result = context.Machines
                .Single(m => m.Id == machineId)
                .Orders
                .OrderBy(o => o.Id)
                .ToList();
            e.Result = result;
            context.Dispose();
        }

        private void _backgroundWorker_OrdersCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var source = (ICollection<Order>)e.Result;
            OrdersListBox.ItemsSource = source;

            OrdersProgressBar.IsIndeterminate = false;
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

                StandardPartSetProgressBar.IsIndeterminate = false;
                OrdersProgressBar.IsIndeterminate = false;
                return;
            }

            IdTextBlock.Text = machine.Id.ToString();
            NameTextBlock.Text = machine.Name;
            PriceTextBlock.Text = machine.Price.ToString();

            StandardPartSetDataGrid.ItemsSource = null;
            OrdersListBox.ItemsSource = null;

            if (_partsBackgroundWorker.IsBusy == false && _ordersBackgroundWorker.IsBusy == false)
            {
                StandardPartSetProgressBar.IsIndeterminate = true;
                OrdersProgressBar.IsIndeterminate = true;

                _partsBackgroundWorker.RunWorkerAsync(machine.Id);
                _ordersBackgroundWorker.RunWorkerAsync(machine.Id);
            }

            //try
            //{
            //    StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet.OrderBy(element => element.Part.Id);
            //    OrdersListBox.ItemsSource = machine.Orders.OrderBy(order => order.Id);
            //}
            //catch (EntityException exc)
            //{
            //    Disconnected("Kod błędu: 02.");
            //}

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
                Disconnected("Kod błędu: 03.");
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
                Disconnected("Kod błędu: 04.");
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
                Disconnected("Kod błędu 05.");
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
                Disconnected("Kod błędu: 06.");
                return;
            }
            //this.Close();
        }

        private void SaveContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 07.");
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
                Disconnected("Kod błędu: 01.");
            }
        }

        private void Disconnected()
        {
            Disconnected("");
        }

        private void Disconnected(string message)
        {
            MainDataGrid.IsEnabled = false;
            DetailsStackPanel.IsEnabled = false;
            PartsMenuItem.IsEnabled = false;
            FillDetailedInfo(null);
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }
    }
}
