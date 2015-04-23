using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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
    /// Interaction logic for ClientsWindow.xaml
    /// </summary>
    public partial class ClientsWindow : Window
    {
        public ObservableCollection<Client> DataGridItemsSource
        {
            get
            {
                try
                {
                    return _dbContext.Clients.Local;
                }
                catch (ProviderIncompatibleException exc)
                {
                    throw new ProviderIncompatibleException();
                }
            }
        }

        private SpawmetDBContext _dbContext;
        //private object _dbContextLock;

        private BackgroundWorker _ordersBackgroundWorker;

        public ClientsWindow()
            : this(0, 0)
        {
            
        }

        public ClientsWindow(double x, double y)
        {
            InitializeComponent();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Client client;
                try
                {
                    client = (Client) MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    FillDetailedInfo(null);
                    return;
                }
                if (client != null)
                {
                    FillDetailedInfo(client);
                }
            };

            this.Loaded += (sender, e) =>
            {
                FillDetailedInfo(null);
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _ordersBackgroundWorker.Dispose();
            };

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                Disconnected("Kod błędu 00.");
            }

            this.Left = x;
            this.Top = y;
        }

        private void Initialize()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }

            _dbContext = new SpawmetDBContext();
            //_dbContextLock = new object();

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();

            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }

            _ordersBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var clientId = (int) e.Argument;
                List<Order> result;
                using (var context = new SpawmetDBContext())
                {
                    result = context.Orders
                        .Where(o => o.Client.Id == clientId)
                        .Include(o => o.Client)
                        .Include(o => o.Machine)
                        .OrderBy(o => o.Id)
                        .ToList();
                }
                e.Result = result;
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<Order>) e.Result;
                OrdersListBox.ItemsSource = source;

                OrdersProgressBar.IsIndeterminate = false;
            };
        }

        private void LoadDataIntoSource()
        {
            try
            {
                _dbContext.Clients.Load();
                ConnectMenuItem.IsEnabled = false;

                MachinesMenuItem.IsEnabled = true;
                PartsMenuItem.IsEnabled = true;
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu 03.");
            }
        }

        public void FillDetailedInfo(Client client)
        {
            if (client == null)
            {
                IdTextBlock.Text = "";
                NameTextBlock.Text = "";
                PhoneTextBlock.Text = "";
                EmailTextBlock.Text = "";
                NipTextBlock.Text = "";
                AddressTextBlock.Text = "";

                OrdersListBox.ItemsSource = null;
                OrdersProgressBar.IsIndeterminate = false;
                return;
            }

            IdTextBlock.Text = client.Id.ToString();
            NameTextBlock.Text = client.Name;
            PhoneTextBlock.Text = client.Phone;
            EmailTextBlock.Text = client.Email;
            NipTextBlock.Text = client.Nip;
            AddressTextBlock.Text = client.Address;

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                OrdersProgressBar.IsIndeterminate = true;

                _ordersBackgroundWorker.RunWorkerAsync(client.Id);
            }
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddClientWindow(this, _dbContext).Show();
        }

        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Client>();
            foreach (var item in selected)
            {
                Client client = null;
                try
                {
                    client = (Client) item;
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
                toDelete.Add(client);
            }
            try
            {
                foreach (var client in toDelete)
                {
                    foreach (var order in client.Orders.ToList())
                    {
                        Delete(order);
                    }

                    Delete(client);
                }
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 05.");
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
                Disconnected("Kod błędu: 07");
            }
        }

        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectMenuItem_OnClick(sender, e);
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
                Disconnected("Kod błędu: 06a.");
            }
        }

        private void PartsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new PartsWindow(this.Left + 40, this.Top + 40).Show();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 06.");
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
                Disconnected("Kod błędu: 06b.");
            }
        }

        private void ConnectMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new ClientsWindow(this.Left, this.Top).Show();
                this.Close();
            }
            catch (ProviderIncompatibleException exc)
            {
                Disconnected("Kod błędu 01.");
            }
        }

        /***********************************************************************************/
        /*** Top ContextMenu event OnClick handlers.                                     ***/
        /*** END                                                                         ***/
        /***********************************************************************************/

        private void Delete(Order order)
        {
            foreach (var additionalPartSetElement in order.AdditionalPartSet.ToList())
            {
                _dbContext.AdditionalPartSets.Remove(additionalPartSetElement);
                _dbContext.SaveChanges();
            }

            order.Client = null;
            order.Machine = null;

            //lock (_dbContextLock)
            //{
            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();
            //}
        }

        private void Delete(Client client)
        {
            _dbContext.Clients.Remove(client);
            _dbContext.SaveChanges();
        }

        private void Disconnected(string message)
        {
            MainDataGrid.IsEnabled = false;
            DetailsStackPanel.IsEnabled = false;
            PartsMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }
    }
}
