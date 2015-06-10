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
    public partial class DeliveriesWindow : Window
    {
        public ObservableCollection<Delivery> DataGridItemsSource
        {
            get
            {
                try
                {
                    return _dbContext.Deliveries.Local;
                }
                catch (ProviderIncompatibleException exc)
                {
                    throw new ProviderIncompatibleException();
                }
            }
        }

        private SpawmetDBContext _dbContext;
        private object _dbContextLock;

        // BackgroundWorker objects which load data into detailed info item sources.
        private BackgroundWorker _partsBackgroundWorker;

        public DeliveriesWindow()
            : this(0, 0)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        public DeliveriesWindow(double x, double y)
            : this(null, x, y)
        {
        }

        // Constructor which creates window at specific x and y coordinates.
        // Additionaly it selects specific Delivery item.
        public DeliveriesWindow(Delivery selectedDelivery, double x, double y)
        {
            InitializeComponent();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Delivery delivery;
                try
                {
                    delivery = (Delivery)MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    FillDetailedInfo(null);
                    return;
                }
                if (delivery != null)
                {
                    FillDetailedInfo(delivery);
                }
            };

            this.Loaded += (sender, e) =>
            {
                // If there's selectedDelivery item in database, select this item.
                // On window loaded.
                Delivery delivery;
                try
                {
                    delivery = DataGridItemsSource.First(d => d.Id == selectedDelivery.Id);
                }
                catch (NullReferenceException exc)
                {
                    delivery = null;
                }
                catch (InvalidOperationException exc)
                {
                    delivery = null;
                }

                MainDataGrid.SelectedItem = delivery;
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _partsBackgroundWorker.Dispose();
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
            _dbContextLock = new object();

            LoadDataIntoSource();

            MainDataGrid.Items.Refresh();

            if (_partsBackgroundWorker != null)
            {
                _partsBackgroundWorker.Dispose();
            }

            _partsBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var deliveryId = (int) e.Argument;
                List<DeliveryPartSetElement> result;
                lock (_dbContextLock)
                {
                    result = _dbContext.DeliveryPartSets
                        .Where(el => el.Delivery.Id == deliveryId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Id)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (List<DeliveryPartSetElement>) e.Result;
                PartsDataGrid.ItemsSource = source;

                PartsProgressBar.IsIndeterminate = false;
            };
        }

        private void LoadDataIntoSource()
        {
            try
            {
                _dbContext.Deliveries.Load();

                ConnectMenuItem.IsEnabled = false;
                MachinesMenuItem.IsEnabled = true;
                PartsMenuItem.IsEnabled = true;
                OrdersMenuItem.IsEnabled = true;
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu 03.");
            }
        }

        public void FillDetailedInfo(Delivery delivery)
        {
            if (delivery == null)
            {
                IdTextBlock.Text = "";
                NameTextBlock.Text = "";
                DateTextBlock.Text = "";

                PartsDataGrid.ItemsSource = null;
                PartsProgressBar.IsIndeterminate = false;
                return;
            }

            string date = delivery.Date != null ? delivery.Date.ToShortDateString() : "";

            IdTextBlock.Text = delivery.Id.ToString();
            NameTextBlock.Text = delivery.Name;
            DateTextBlock.Text = date;

            if (_partsBackgroundWorker.IsBusy == false)
            {
                PartsProgressBar.IsIndeterminate = true;

                _partsBackgroundWorker.RunWorkerAsync(delivery.Id);
            }
        }

        /***********************************************************************************/
        /*** MainDataGrid ContextMenu event OnClick handlers.                            ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        /*** Add new Delivery. ***/
        private void AddContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataGridItemsSource == null)
            {
                return;
            }

            new AddDeliveryWindow(this, _dbContext).Show();
        }

        /*** Delete selected Delivery items. ***/
        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Delivery>();
            foreach (var item in selected)
            {
                try
                {
                    toDelete.Add((Delivery) item);
                }
                catch (InvalidCastException exc)
                {
                    // Ignore objects that can't be casted to Delivery type.
                    continue;
                }
            }
            new DeleteDeliveryWindow(this, _dbContext, toDelete).Show();
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
            Delivery selectedDelivery = null;
            try
            {
                selectedDelivery = (Delivery)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedDelivery = null;
            }
            try
            {
                new DeliveriesWindow(selectedDelivery, Left, Top).Show();
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
        /*** AdditionalPartSetDataGrid ContextMenu event OnClick handlers.               ***/
        /*** BEGIN                                                                       ***/
        /***********************************************************************************/

        /*** Add new DeliveryPartSetElement with selected Delivery and existing Part. ***/
        private void AddPartItem_OnClick(object sender, RoutedEventArgs e)
        {
            Delivery delivery = null;
            try
            {
                delivery = (Delivery)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                delivery = null;
            }
            finally
            {
                if (delivery != null)
                {
                    new AddPartToDelivery(this, _dbContext, delivery).Show();
                }
            }
        }

        /*** Delete selected DeliveryPartElement. ***/
        private void DeletePartItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dataGrid = PartsDataGrid;
            var delivery = (Delivery) MainDataGrid.SelectedItem;
            var partSetElement = (DeliveryPartSetElement)dataGrid.SelectedItem;

            if (partSetElement == null)
            {
                return;
            }

            try
            {
                _dbContext.DeliveryPartSets.Remove(partSetElement);
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 04.");
                return;
            }

            PartsDataGrid.ItemsSource = delivery.DeliveryPartSet;
        }

        /***********************************************************************************/
        /*** AdditionalPartSetDataGrid ContextMenu event OnClick handlers.               ***/
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
                //new MachinesWindow(this.Left + 40, this.Top + 40).Show();
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

        private void ConnectMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                new DeliveriesWindow(this.Left, this.Top).Show();
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
            MachinesMenuItem.IsEnabled = false;
            PartsMenuItem.IsEnabled = false;
            //ClientsMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }

    }
}
