using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core;
using System.Linq;
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

        private SpawmetDBContext _dbContext;

        private MasterWindow _parentWindow; 

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
                _parentWindow.MachinesWindowButton.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _parentWindow.MachinesWindowButton.IsEnabled = true;
            };
        }

        private void FillDetailedInfo(Machine machine)
        {
            if (machine == null)
            {
                BasicInfoTextBlock.Text = "";
                StandardPartSetListBox.ItemsSource = null;
                return;
            }

            string basicInfo = "";

            basicInfo += "ID: " + machine.Id +
                         "\nNazwa: " + machine.Name +
                         "\nIlość: " + machine.Price;

            BasicInfoTextBlock.Text = basicInfo;
            StandardPartSetListBox.ItemsSource = machine.StandardPartSet.OrderBy(p => p.Id);

            string ordersInfo = "Zamówienia:\n";
            foreach (var order in machine.Orders)
            {
                string clientName = order.Client != null ? order.Client.Name : "";
                string machineName = order.Machine != null ? order.Machine.Name : "";
                ordersInfo += "- " + clientName + ", " + machineName + ", " +
                              order.StartDate.ToShortDateString() + "\n";
            }

            OrdersTextBlock.Text = ordersInfo;

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
            var machine = (Machine) MainDataGrid.SelectedItem;
            new AddPartToMachine(this, _dbContext, machine).Show();
        }

        private void DeletePartItem_OnClick(object sender, RoutedEventArgs e)
        {
            var listBox = StandardPartSetListBox;
            var machine = (Machine) MainDataGrid.SelectedItem;
            var part = (Part) listBox.SelectedItem;

            machine.StandardPartSet.Remove(part);
            _dbContext.SaveChanges();

            StandardPartSetListBox.ItemsSource = machine.StandardPartSet.OrderBy(p => p.Id);
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
                var machine = (Machine) item;
                toDelete.Add(machine);
            }
            foreach (var machine in toDelete)
            {
                var relatedOrders = _dbContext.Orders.Where(o => o.Machine.Id == machine.Id);//.ToList();
                foreach (var order in relatedOrders)
                {
                    _dbContext.Orders.Remove(order);
                }
                _dbContext.Machines.Remove(machine);
            }
            _dbContext.SaveChanges();
        }
    }
}
