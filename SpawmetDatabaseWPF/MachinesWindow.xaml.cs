using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using SpawmetDatabase;
using SpawmetDatabase.FileCreators;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /***********************************************************************************/
    /*** Main rule for loading data to UI from another threads:                      ***/
    /***   - readonly data (like OrdersListBox in MachinesWindow) can be loaded from ***/
    /***     another context (with using Include() to load all related data, in      ***/
    /***     order to display Signature).                                            ***/
    /***   - read/write data (like StandardPartSetGrid in MachinesWindow) MUST be    ***/
    /***     loaded from main _dbContext (with using lock and Include).              ***/
    /***********************************************************************************/

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
                    Disconnected("Kod błędu: MWxDGIS.");
                    return null;
                }
            }
        }

        private SpawmetDBContext _dbContext;
        private object _dbContextLock;

        // BackgroundWorker objects which load data into detailed info item sources.
        private BackgroundWorker _partsBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;

        public MachinesWindow()
            : this(0, 0)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        public MachinesWindow(double x, double y)
            : this(null, x, y)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        // Additionaly it selects specific Machine item.
        public MachinesWindow(Machine selectedMachine, double x, double y)
        {
            InitializeComponent();

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
                Machine machine;
                try
                {
                    machine = DataGridItemsSource.First(m => m.Id == selectedMachine.Id);
                }
                catch (ArgumentNullException exc)
                {
                    machine = null;
                }
                catch (NullReferenceException exc)
                {
                    machine = null;
                }
                catch (InvalidOperationException exc)
                {
                    machine = null;
                }

                MainDataGrid.SelectedItem = machine;
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _partsBackgroundWorker.Dispose();
                _ordersBackgroundWorker.Dispose();
            };

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                Disconnected("Kod błędu: 00.");
            }

            Left = x;
            Top = y;
        }

        // Creates SpawmetDBContext object, fills MainDataGrid with data and initializes BackgroundWorker classes.
        private void Initialize()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }

            _dbContext = new SpawmetDBContext();
            _dbContextLock = new object();

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();

            if (_partsBackgroundWorker != null)
            {
                _partsBackgroundWorker.Dispose();
            }
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }

            _partsBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int)e.Argument;
                List<StandardPartSetElement> result;
                lock (_dbContextLock)
                {
                    result = _dbContext.StandardPartSets
                        .Where(el => el.Machine.Id == machineId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (ICollection<StandardPartSetElement>)e.Result;
                StandardPartSetDataGrid.ItemsSource = source;

                StandardPartSetProgressBar.IsIndeterminate = false;
            };
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int)e.Argument;
                var context = new SpawmetDBContext();
                var result = context.Orders
                    .Where(m => m.Machine.Id == machineId)
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .OrderBy(o => o.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                ICollection<Order> source = null;
                try
                {
                    source = (ICollection<Order>)e.Result;
                }
                catch (TargetInvocationException exc)
                {
                    Disconnected("Kod błędu: MWxOBW_OC.");
                    return;
                }
                OrdersListBox.ItemsSource = source;

                OrdersProgressBar.IsIndeterminate = false;
            };
        }

        private void LoadDataIntoSource()
        {
            try
            {
                _dbContext.Machines.Load();
                ConnectMenuItem.IsEnabled = false;

                PartsMenuItem.IsEnabled = true;
                OrdersMenuItem.IsEnabled = true;
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: MWxLDIS.");
            }
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
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        /*** Add new Machine. ***/
        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddMachineWindow(this, _dbContext).Show();
        }

        /*** Delete selected Machine items. ***/
        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Machine>();
            foreach (var item in selected)
            {
                try
                {
                    toDelete.Add((Machine)item);
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
            }
            new DeleteMachineWindow(this, _dbContext, toDelete).Show();
        }

        /*** Save database state. ***/
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

        /*** Refresh window. ***/
        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            Machine selectedMachine = null;
            try
            {
                selectedMachine = (Machine)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedMachine = null;
            }
            try
            {
                new MachinesWindow(selectedMachine, Left, Top).Show();
                this.Close();
            }
            catch (ProviderIncompatibleException exc)
            {
                Disconnected("Kod błędu: 01a.");
            }
        }

        /*** Adds specified Amount from StandardPartSetElement to Amount from Part. ***/
        private void CraftPartButton_OnClick(object sender, RoutedEventArgs e)
        {
            StandardPartSetElement selectedElement = null;
            try
            {
                selectedElement = (StandardPartSetElement)StandardPartSetDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedElement = null;
                return;
            }

            var part = selectedElement.Part;
            part.Amount += selectedElement.Amount;
            _dbContext.SaveChanges();
        }

        /*** Creates temp .xps file from selected Machine items and then shows print dialog. ***/
        private void PrintContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;

            if (selected.Count == 0 ||
                selected == null)
            {
                return;
            }

            var machines = new List<Machine>();
            foreach (var item in selected)
            {
                try
                {
                    machines.Add((Machine)item);
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
            }

            var printDialog = new PrintDialog();
            printDialog.PageRangeSelection = PageRangeSelection.AllPages;
            printDialog.UserPageRangeEnabled = false;
            printDialog.SelectedPagesEnabled = false;

            bool? print = printDialog.ShowDialog();
            if (print == true)
            {
                string description = machines.Count == 1
                ? machines.First().Name
                : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                var printWindow = new PrintWindow(machines, printDialog, this);
                printWindow.Show();
            }
        }

        /*** Prepares selected Machine items and shows save file dialog. ***/
        private void SaveToFileContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;

            if (selected.Count == 0 ||
                selected == null)
            {
                return;
            }

            var machines = new List<Machine>();

            foreach (var item in selected)
            {
                try
                {
                    machines.Add((Machine)item);
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Plik Word 2007 (*.docx)|*.docx|Plik PDF (*.pdf)|*.pdf";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = machines.Count == 1
                ? machines.First().Name
                : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

            if (saveFileDialog.ShowDialog() == true)
            {
                new SaveFileWindow(machines, saveFileDialog.FileName, this).Show();
            }
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** END                                                                         ***/
        /***********************************************************************************/

        /***********************************************************************************/
        /*** StandardPartSetDataGrid ContextMenu event OnClick handlers.                 ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        /*** Add new StandardPartSetElement with selected Order and existing Part. ***/
        private void AddPartItem_OnClick(object sender, RoutedEventArgs e)
        {
            Machine machine = null;
            try
            {
                machine = (Machine)MainDataGrid.SelectedItem;
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

        /*** Delete selected StandardPartSetElement. ***/
        private void DeletePartItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dataGrid = StandardPartSetDataGrid;
            var machine = (Machine)MainDataGrid.SelectedItem;
            var partSetElement = (StandardPartSetElement)dataGrid.SelectedItem;

            if (partSetElement == null)
            {
                return;
            }

            try
            {
                _dbContext.StandardPartSets.Remove(partSetElement);
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 04.");
            }

            StandardPartSetDataGrid.ItemsSource = machine.StandardPartSet; //.OrderBy(element => element.Part.Id);
        }

        /***********************************************************************************/
        /*** StandardPartSetDataGrid ContextMenu event OnClick handlers.                 ***/
        /*** END                                                                         ***/
        /***********************************************************************************/

        /***********************************************************************************/
        /*** Top ContextMenu event OnClick handlers.                                     ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        private void PartsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new PartsWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 06.");
                return;
            }
        }

        private void OrdersMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new OrdersWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 06a.");
                return;
            }
        }

        private void ClientsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new ClientsWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu 06b.");
                return;
            }
        }

        private void DeliveriesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new DeliveriesWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu 06b.");
                return;
            }
        }

        private void ConnectMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new MachinesWindow(this.Left, this.Top).Show();
            }
            catch (ProviderIncompatibleException exc)
            {
                Disconnected("Kod błędu: 01.");
            }
        }

        /***********************************************************************************/
        /*** Top ContextMenu event OnClick handlers.                                     ***/
        /*** END                                                                         ***/
        /***********************************************************************************/

        private void Disconnected()
        {
            Disconnected("");
        }

        private void Disconnected(string message)
        {
            MainDataGrid.IsEnabled = false;
            DetailsStackPanel.IsEnabled = false;
            PartsMenuItem.IsEnabled = false;
            OrdersMenuItem.IsEnabled = false;
            ClientsMenuItem.IsEnabled = false;
            DeliveriesMenuItem.IsEnabled = false;
            FillDetailedInfo(null);
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }

    }
}
