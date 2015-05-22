using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
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
    public partial class PartsWindow : Window
    {
        public ObservableCollection<Part> DataGridItemsSource
        {
            get
            {
                try
                {
                    return _dbContext.Parts.Local;
                }
                catch (ProviderIncompatibleException exc)
                {
                    throw new ProviderIncompatibleException("in DataGridItemsSource");
                }
            }
        }

        private SpawmetDBContext _dbContext;

        // BackgroundWorker objects which load data into detailed info item sources.
        private BackgroundWorker _machinesBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;
        private BackgroundWorker _deliveriesBackgroundWorker;

        public PartsWindow()
            : this(0, 0)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        public PartsWindow(double x, double y)
            : this(null, x, y)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        // Additionaly it selects specific Part item.
        public PartsWindow(Part selectedPart, double x, double y)
        {
            InitializeComponent();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Part part = null;
                try
                {
                    part = (Part)MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    FillDetailedInfo(null);
                    return;
                }
                if (part != null)
                {
                    FillDetailedInfo(part);
                }
            };

            this.Loaded += (sender, e) =>
            {
                Part part;
                try
                {
                    part = DataGridItemsSource.First(p => p.Id == selectedPart.Id);
                }
                catch (NullReferenceException exc)
                {
                    part = null;
                }
                catch (InvalidOperationException exc)
                {
                    part = null;
                }

                MainDataGrid.SelectedItem = part;
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _machinesBackgroundWorker.Dispose();
                _ordersBackgroundWorker.Dispose();
                _deliveriesBackgroundWorker.Dispose();
            };

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                Disconnected();
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

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();

            if (_machinesBackgroundWorker != null)
            {
                _machinesBackgroundWorker.Dispose();
            }
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }
            if (_deliveriesBackgroundWorker != null)
            {
                _deliveriesBackgroundWorker.Dispose();
            }

            _machinesBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker = new BackgroundWorker();
            _deliveriesBackgroundWorker = new BackgroundWorker();
            _machinesBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int)e.Argument;
                var context = new SpawmetDBContext();
                var result = context.StandardPartSets
                    .Where(el => el.Part.Id == partId)
                    //.Include(el => el.Machine)
                    .Select(el => el.Machine)
                    .OrderBy(m => m.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _machinesBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Machine>)e.Result;
                MachinesListBox.ItemsSource = source;

                MachinesProgressBar.IsIndeterminate = false;
            };
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int) e.Argument;
                var context = new SpawmetDBContext();
                var result = context.AdditionalPartSets
                    .Where(el => el.Part.Id == partId)
                    .Select(el => el.Order)
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .OrderBy(o => o.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Order>) e.Result;
                OrdersListBox.ItemsSource = source;

                OrdersProgressBar.IsIndeterminate = false;
            };
            _deliveriesBackgroundWorker.DoWork += (sender, e) =>
            {
                var partId = (int) e.Argument;
                var context = new SpawmetDBContext();
                var result = context.DeliveryPartSets
                    .Where(el => el.Part.Id == partId)
                    .Select(el => el.Delivery)
                    .OrderBy(d => d.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _deliveriesBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (ICollection<Delivery>) e.Result;
                DeliveriesListBox.ItemsSource = source;

                DeliveriesProgressBar.IsIndeterminate = false;
            };
        }

        private void LoadDataIntoSource()
        {
            try
            {
                _dbContext.Parts.Load();
                ConnectMenuItem.IsEnabled = false;

                MachinesMenuItem.IsEnabled = true;
                OrdersMenuItem.IsEnabled = true;
            }
            catch (EntityException exc)
            {
                Disconnected();
            }
        }

        public void FillDetailedInfo(Part part)
        {
            if (part == null)
            {
                IdTextBlock.Text = "";
                NameTextBlock.Text = "";
                OriginTextBlock.Text = "";
                AmountTextBlock.Text = "";

                MachinesListBox.ItemsSource = null;
                OrdersListBox.ItemsSource = null;
                DeliveriesListBox.ItemsSource = null;

                MachinesProgressBar.IsIndeterminate = false;
                OrdersProgressBar.IsIndeterminate = false;
                DeliveriesProgressBar.IsIndeterminate = false;
                return;
            }

            string originName = part.Origin == Origin.Production ? "Produkcja" : "Zewnątrz";

            IdTextBlock.Text = part.Id.ToString();
            NameTextBlock.Text = part.Name;
            OriginTextBlock.Text = originName;
            AmountTextBlock.Text = part.Amount.ToString();

            MachinesListBox.ItemsSource = null;
            OrdersListBox.ItemsSource = null;
            DeliveriesListBox.ItemsSource = null;

            if (_machinesBackgroundWorker.IsBusy == false && _ordersBackgroundWorker.IsBusy == false &&
                _deliveriesBackgroundWorker.IsBusy == false)
            {
                MachinesProgressBar.IsIndeterminate = true;
                OrdersProgressBar.IsIndeterminate = true;
                DeliveriesProgressBar.IsIndeterminate = true;

                _machinesBackgroundWorker.RunWorkerAsync(part.Id);
                _ordersBackgroundWorker.RunWorkerAsync(part.Id);
                _deliveriesBackgroundWorker.RunWorkerAsync(part.Id);
            }
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        /*** Add new Part. ***/
        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddPartWindow(this, _dbContext).Show();
        }

        /*** Delete selected Part items. ***/
        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Part>();
            foreach (var item in selected)
            {
                try
                {
                    toDelete.Add((Part) item);
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
            }
            new DeletingPartWindow(this, _dbContext, toDelete).Show();
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
                Disconnected();
            }
        }

        /*** Refresh window. ***/
        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            Part selectedPart = null;
            try
            {
                selectedPart = (Part)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedPart = null;
            }
            try
            {
                new PartsWindow(selectedPart, Left, Top).Show();
                this.Close();
            }
            catch (ProviderIncompatibleException exc)
            {
                Disconnected("Kod błędu: 01a.");
            }
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** END                                                                         ***/
        /***********************************************************************************/

        /***********************************************************************************/
        /*** Top ContextMenu event OnClick handlers.                                     ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        private void MachinesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new MachinesWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected();
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
                Disconnected("Kod błędu: 06b.");
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
                Disconnected("Kod błędu: 06c.");
            }
        }

        private void ConnectMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new PartsWindow(this.Left, this.Top).Show();
                this.Close();
            } 
            catch (ProviderIncompatibleException exc)
            {
                Disconnected();
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
            MachinesMenuItem.IsEnabled = false;
            OrdersMenuItem.IsEnabled = false;
            FillDetailedInfo(null);
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }

    }
}
