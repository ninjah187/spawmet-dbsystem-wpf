using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
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
    /// Interaction logic for OrdersWindow.xaml
    /// </summary>
    public partial class OrdersWindow : Window
    {
        public ObservableCollection<Order> DataGridItemsSource
        {
            get
            {
                try
                {
                    return _dbContext.Orders.Local;
                }
                catch (ProviderIncompatibleException exc)
                {
                    throw new ProviderIncompatibleException();
                }
            }
        }

        private SpawmetDBContext _dbContext;
        private object _dbContextLock;

        private BackgroundWorker _partsBackgroundWorker;

        public OrdersWindow()
            : this(0, 0)
        {
            
        }

        public OrdersWindow(double x, double y)
            : this(null, x, y)
        {
            
        }

        public OrdersWindow(Order selectedOrder, double x, double y)
        {
            InitializeComponent();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Order order;
                try
                {
                    order = (Order)MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    FillDetailedInfo(null);
                    return;
                }
                if (order != null)
                {
                    FillDetailedInfo(order);
                }
            };

            this.Loaded += (sender, e) =>
            {
                Order order;
                try
                {
                    order = DataGridItemsSource.First(o => o.Id == selectedOrder.Id);
                }
                catch (NullReferenceException exc)
                {
                    order = null;
                }
                catch (InvalidOperationException exc)
                {
                    order = null;
                }

                MainDataGrid.SelectedItem = order;
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

            ClientComboBoxColumn.ItemsSource = _dbContext.Clients.ToList();
            //ClientComboBoxColumn.SelectedItemBinding = new Binding("Client");
            MachineComboBoxColumn.ItemsSource = _dbContext.Machines.ToList();
            //MachineComboBoxColumn.SelectedItemBinding = new Binding("Machine");

            if (_partsBackgroundWorker != null)
            {
                _partsBackgroundWorker.Dispose();
            }

            _partsBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var orderId = (int) e.Argument;
                List<AdditionalPartSetElement> result;
                lock (_dbContextLock)
                {
                    result = _dbContext.AdditionalPartSets
                        .Where(el => el.Order.Id == orderId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Id)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                ICollection<AdditionalPartSetElement> source = null;
                try
                {
                    source = (List<AdditionalPartSetElement>) e.Result;
                }
                catch (TargetInvocationException exc)
                {
                    Disconnected("Kod błędu: pBW.");
                    return;
                }
                AdditionalPartSetDataGrid.ItemsSource = source;

                AdditionalPartSetProgressBar.IsIndeterminate = false;
            };
        }

        private void LoadDataIntoSource()
        {
            try
            {
                //_dbContext.Orders.Load();
                _dbContext.Orders
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .Load();
                //include client and machine ???
                ConnectMenuItem.IsEnabled = false;

                MachinesMenuItem.IsEnabled = true;
                PartsMenuItem.IsEnabled = true;
                ClientsMenuItem.IsEnabled = true;
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu 03.");
            }
        }

        public void FillDetailedInfo(Order order)
        {
            if (order == null)
            {
                IdTextBlock.Text = "";
                ClientTextBlock.Text = "";
                MachineTextBlock.Text = "";
                StartDateTextBlock.Text = "";
                SendDateTextBlock.Text = "";
                StatusTextBlock.Text = "";
                RemarksTextBlock.Text = "";

                AdditionalPartSetDataGrid.ItemsSource = null;
                AdditionalPartSetProgressBar.IsIndeterminate = false;
                return;
            }

            string statusName = order.Status.Value.GetDescription();
            /*switch (order.Status)
            {
                case OrderStatus.Start:
                    statusName = "Nowe";
                    break;
            }*/

            string clientName = order.Client != null ? order.Client.Name : "";
            string machineName = order.Machine != null ? order.Machine.Name : "";
            string startDate = order.StartDate != null ? order.StartDate.Value.ToShortDateString() : "";
            string sendDate = order.SendDate != null ? order.SendDate.Value.ToShortDateString() : "";

            IdTextBlock.Text = order.Id.ToString();
            ClientTextBlock.Text = clientName;
            MachineTextBlock.Text = machineName;
            StartDateTextBlock.Text = startDate;
            SendDateTextBlock.Text = sendDate;
            StatusTextBlock.Text = statusName;
            RemarksTextBlock.Text = order.Remarks;

            if (_partsBackgroundWorker.IsBusy == false)
            {
                AdditionalPartSetProgressBar.IsIndeterminate = true;

                _partsBackgroundWorker.RunWorkerAsync(order.Id);
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

            new AddOrderWindow(this, _dbContext).Show();
        }

        private void DeleteContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = MainDataGrid.SelectedItems;
            var toDelete = new List<Order>();
            foreach (var item in selected)
            {
                try
                {
                    toDelete.Add((Order) item);
                }
                catch (InvalidCastException exc)
                {
                    continue;
                }
            }
            new DeleteOrderWindow(this, _dbContext, toDelete).Show();

            //if (DataGridItemsSource == null)
            //{
            //    return;
            //}

            //var selected = MainDataGrid.SelectedItems;
            //var toDelete = new List<Order>();
            //foreach (var item in selected)
            //{
            //    Order order = null;
            //    try
            //    {
            //        order = (Order) item;
            //    }
            //    catch (InvalidCastException exc)
            //    {
            //        continue;
            //    }
            //    toDelete.Add(order);
            //}
            //try
            //{
            //    foreach (var order in toDelete)
            //    {
            //        Delete(order);
            //    }
            //}
            //catch (EntityException exc)
            //{
            //    Disconnected("Kod błędu: 05.");
            //    //throw exc;
            //}
        }

        private void Delete(Order order)
        {
            foreach (var additionalPartSetElement in order.AdditionalPartSet.ToList())
            {
                _dbContext.AdditionalPartSets.Remove(additionalPartSetElement);
                _dbContext.SaveChanges();
            }

            order.Client = null;
            order.Machine = null;

            lock (_dbContextLock)
            {
                _dbContext.Orders.Remove(order);
                _dbContext.SaveChanges();
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
                Disconnected("Kod błędu: OWxSCMI_OC.");
            }
        }

        private void RefreshContextMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            Order selectedOrder = null;
            try
            {
                selectedOrder = (Order)MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                selectedOrder = null;
            }
            try
            {
                new OrdersWindow(selectedOrder, Left, Top).Show();
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

        private void AddPartItem_OnClick(object sender, RoutedEventArgs e)
        {
            Order order = null;
            try
            {
                order = (Order) MainDataGrid.SelectedItem;
            }
            catch (InvalidCastException exc)
            {
                order = null;
            }
            finally
            {
                if (order != null)
                {
                    new AddPartToOrderWindow(this, _dbContext, order).Show();
                }
            }
        }

        private void DeletePartItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dataGrid = AdditionalPartSetDataGrid;
            var order = (Order) MainDataGrid.SelectedItem;
            var partSetElement = (AdditionalPartSetElement) dataGrid.SelectedItem;

            if (partSetElement == null)
            {
                return;
            }

            try
            {
                _dbContext.AdditionalPartSets.Remove(partSetElement);
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected("Kod błędu: 04.");
                return;
            }

            AdditionalPartSetDataGrid.ItemsSource = order.AdditionalPartSet;
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
                new OrdersWindow(this.Left, this.Top).Show();
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
            ClientsMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
            MessageBox.Show("Brak połączenia z serwerem.\n" + message, "Błąd");
        }

        private void CraftPartButton_OnClick(object sender, RoutedEventArgs e)
        {
            AdditionalPartSetElement selectedElement = null;
            try
            {
                selectedElement = (AdditionalPartSetElement) AdditionalPartSetDataGrid.SelectedItem;
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
    }
}
