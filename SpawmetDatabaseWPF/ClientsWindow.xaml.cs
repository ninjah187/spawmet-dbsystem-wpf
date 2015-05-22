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

        // BackgroundWorker objects which load data into detailed info item sources.
        private BackgroundWorker _ordersBackgroundWorker;

        public ClientsWindow()
            : this(0, 0)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        public ClientsWindow(double x, double y)
            : this(null, x, y)
        {   
        }

        // Constructor which creates window at specific x and y coordinates.
        // Additionaly it selects specific Client item.
        public ClientsWindow(Client selectedClient, double x, double y)
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

            // If there's selectedDelivery item in database, select this item.
            // On window loaded.
            this.Loaded += (sender, e) =>
            {
                Client client;
                try
                {
                    client = DataGridItemsSource.First(c => c.Id == selectedClient.Id);
                }
                catch (NullReferenceException exc)
                {
                    client = null;
                }
                catch (InvalidOperationException exc)
                {
                    client = null;
                }

                MainDataGrid.SelectedItem = client;
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

        // Creates SpawmetDBContext object, fills MainDataGrid with data and initializes BackgroundWorker classes.
        private void Initialize()
        {
            // In case when connection was lost and window wasn't closed, there's big chance that _dbContext wasn't disposed.
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }

            _dbContext = new SpawmetDBContext();

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

        /*** Add new Client. ***/
        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddClientWindow(this, _dbContext).Show();
        }

        /*** Delete selected Client items. ***/
        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Client>();
            foreach (var item in selected)
            {
                try
                {
                    toDelete.Add((Client) item);
                }
                catch (InvalidCastException exc)
                {
                    // Ignore objects that can't be casted to Client type.
                    continue;
                }
            }
            new DeleteClientWindow(this, _dbContext, toDelete).Show();
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
                Disconnected("Kod błędu: 07");
            }
        }

        /*** Refresh window. ***/
        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            Client selectedClient;
            try
            {
                selectedClient = (Client)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedClient = null;
            }
            try
            {
                new ClientsWindow(selectedClient, Left, Top).Show();
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
