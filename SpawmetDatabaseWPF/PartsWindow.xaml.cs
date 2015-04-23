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
    /// <summary>
    /// Interaction logic for PartsWindow.xaml
    /// </summary>
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

        private MasterWindow _parentWindow;

        private BackgroundWorker _machinesBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;
        private BackgroundWorker _deliveriesBackgroundWorker;

        public PartsWindow(double x, double y)
            : this(null)
        {
            this.Left = x;
            this.Top = y;
        }

        public PartsWindow(MasterWindow parentWindow)
        {
            InitializeComponent();

            _parentWindow = parentWindow;

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
                FillDetailedInfo(null);
                if (_parentWindow != null)
                {
                    _parentWindow.PartsWindowButton.IsEnabled = false;
                }
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _machinesBackgroundWorker.Dispose();
                _ordersBackgroundWorker.Dispose();
                _deliveriesBackgroundWorker.Dispose();
                if (_parentWindow != null)
                {
                    _parentWindow.PartsWindowButton.IsEnabled = true;
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
                //var result = new List<Order>();
                //foreach (var additionalPartSetElement in context.AdditionalPartSets)
                //{
                //    if (additionalPartSetElement.Part.Id == partId)
                //    {
                //        result.Add(additionalPartSetElement.Order);
                //    }
                //}
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
                var result = context.Parts.Single(p => p.Id == partId).Deliveries;
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

            //try
            //{
            //    var relatedMachines = new List<Machine>();
            //    foreach (var standardPartSetElement in part.StandardPartSets)
            //    {
            //        relatedMachines.Add(standardPartSetElement.Machine);
            //    }
            //    MachinesListBox.ItemsSource = relatedMachines.OrderBy(machine => machine.Id);

            //    var relatedOrders = new List<Order>();
            //    foreach (var additionalPartSetElement in part.AdditionalPartSets)
            //    {
            //        relatedOrders.Add(additionalPartSetElement.Order);
            //    }
            //    OrdersListBox.ItemsSource = relatedOrders.OrderBy(order => order.Id);

            //    var relatedDeliveries = new List<Delivery>();
            //    foreach (var delivery in part.Deliveries)
            //    {
            //        relatedDeliveries.Add(delivery);
            //    }
            //    DeliveriesListBox.ItemsSource = relatedDeliveries.OrderBy(delivery => delivery.Id);
            //}
            //catch (EntityException exc)
            //{
            //    Disconnected();
            //}
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

        private void AddPartMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new AddPartWindow(this, _dbContext).Show();
        }

        private void DeletePartMenuItem_OnClick(object sender, RoutedEventArgs e)
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
            try
            {
                foreach (var part in toDelete)
                {
                    foreach (var standardPartSetElement in part.StandardPartSets.ToList())
                    {
                        _dbContext.StandardPartSets.Remove(standardPartSetElement);
                        _dbContext.SaveChanges();
                    }
                    foreach (var additionalPartSetElement in part.AdditionalPartSets.ToList())
                    {
                        _dbContext.AdditionalPartSets.Remove(additionalPartSetElement);
                        _dbContext.SaveChanges();
                    }
                    foreach (var delivery in part.Deliveries.ToList())
                    {
                        _dbContext.Deliveries.Remove(delivery);
                        _dbContext.SaveChanges();
                    }
                    _dbContext.Parts.Remove(part);
                    _dbContext.SaveChanges();

                    FillDetailedInfo(null);
                }
            }
            catch (EntityException exc)
            {
                Disconnected();
            }
        }

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
            //this.Close();
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
            //this.Close();
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
                new PartsWindow(this.Left, this.Top).Show();
                this.Close();
            } 
            catch (ProviderIncompatibleException exc)
            {
                Disconnected();
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
            MachinesMenuItem.IsEnabled = false;
            OrdersMenuItem.IsEnabled = false;
            FillDetailedInfo(null);
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }
    }
}
