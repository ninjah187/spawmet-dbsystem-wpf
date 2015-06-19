using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DeleteOrderWindow.xaml
    /// </summary>
    public partial class DeleteOrderWindow : Window
    {
        public event EventHandler<IEnumerable<Order>> OrdersDeleted;
        public event EventHandler WorkCompleted;

        public event EventHandler<Exception> ConnectionLost;

        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Order> _orders;

        private readonly BackgroundWorker _mainWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteOrderWindow(SpawmetDBContext dbContext, IEnumerable<Order> orders)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _orders = orders;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.ConnectionLost += ConnectionLostHandler;

            _initWorker = new BackgroundWorker();
            _initWorker.WorkerSupportsCancellation = true;
            _initWorker.DoWork += InitWorker_DoWork;
            _initWorker.RunWorkerCompleted += InitWorker_RunWorkerCompleted;

            _mainWorker = new BackgroundWorker();
            _mainWorker.WorkerReportsProgress = true;
            _mainWorker.WorkerSupportsCancellation = true;
            _mainWorker.DoWork += MainWorker_DoWork;
            _mainWorker.ProgressChanged += MainWorker_ProgressChanged;
            _mainWorker.RunWorkerCompleted += MainWorker_RunWorkerCompleted;

            this.Closed += (sender, e) =>
            {
                _initWorker.Dispose();
                _mainWorker.Dispose();
            };

            _initWorker.RunWorkerAsync();
            
        }

        private void InitWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var order in _orders)
                {
                    if (_initWorker.CancellationPending)
                    {
                        return;
                    }

                    _totalCount += order.AdditionalPartSet.Count();
                    _totalCount++;
                }
            }
            catch (Exception exc)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnConnectionLost(exc);
                });
            }
        }

        private void InitWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DeleteProgressBar.Minimum = 0;
            DeleteProgressBar.Maximum = _totalCount;
            DeleteProgressBar.Value = 0;
            CounterTextBlock.Text = "0 z " + _totalCount;

            TitleTextBlock.Text = "Usuwanie...";

            _mainWorker.RunWorkerAsync();
        }

        private void MainWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var order in _orders)
                {
                    if (_mainWorker.CancellationPending)
                    {
                        return;
                    }

                    int count = order.AdditionalPartSet.Count;
                    _dbContext.AdditionalPartSets.RemoveRange(order.AdditionalPartSet);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _mainWorker.ReportProgress(0);
                }
            }
            catch (Exception exc)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnConnectionLost(exc);
                });
            }
        }

        private void MainWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeleteProgressBar.Value = _deletedCount;
            CounterTextBlock.Text = _deletedCount + " z " + _totalCount;
        }

        private void MainWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _deletedCount += _orders.Count();

            _dbContext.Orders.RemoveRange(_orders);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception exc)
            {
                OnConnectionLost(exc);
                return;
            }

            OnOrdersDeleted(_orders);

            DeleteProgressBar.Value += _deletedCount;
            CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

            //parentWindow.FillDetailedInfo(null);

            OnWorkCompleted();

            this.Close();
        }

        private void ConnectionLostHandler(object sender, Exception exc)
        {
            _initWorker.CancelAsync();
            _mainWorker.CancelAsync();

            _initWorker.RunWorkerCompleted -= InitWorker_RunWorkerCompleted;
            _mainWorker.DoWork -= MainWorker_DoWork;
            _mainWorker.ProgressChanged -= MainWorker_ProgressChanged;
            _mainWorker.RunWorkerCompleted -= MainWorker_RunWorkerCompleted;

            this.Close();

            this.ConnectionLost -= ConnectionLostHandler;
        }

        private void OnOrdersDeleted(IEnumerable<Order> orders)
        {
            if (OrdersDeleted != null)
            {
                OrdersDeleted(this, orders);
            }
        }

        private void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }

        private void OnConnectionLost(Exception exc)
        {
            if (ConnectionLost != null)
            {
                ConnectionLost(this, exc);
            }
        }
    }
}
