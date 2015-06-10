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
    /// Interaction logic for DeleteDeliveryWindow.xaml
    /// </summary>
    public partial class DeleteDeliveryWindow : Window
    {
        public event EventHandler<IEnumerable<Delivery>> DeliveriesDeleted;
        public event EventHandler WorkCompleted;

        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Delivery> _deliveries;

        private readonly BackgroundWorker _backgroundWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteDeliveryWindow(SpawmetDBContext dbContext,
            IEnumerable<Delivery> deliveries)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _deliveries = deliveries;

            _initWorker = new BackgroundWorker();
            _initWorker.DoWork += (sender, e) =>
            {
                foreach (var delivery in _deliveries)
                {
                    _totalCount += delivery.DeliveryPartSet.Count();
                    _totalCount++;
                }
            };
            _initWorker.RunWorkerCompleted += (sender, e) =>
            {
                DeleteProgressBar.Minimum = 0;
                DeleteProgressBar.Maximum = _totalCount;
                DeleteProgressBar.Value = 0;
                CounterTextBlock.Text = "0 z " + _totalCount;

                TitleTextBlock.Text = "Usuwanie...";

                _backgroundWorker.RunWorkerAsync();
            };

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (sender, e) =>
            {
                foreach (var delivery in _deliveries)
                {
                    int count = delivery.DeliveryPartSet.Count;
                    _dbContext.DeliveryPartSets.RemoveRange(delivery.DeliveryPartSet);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _backgroundWorker.ReportProgress(0);
                }
            };
            _backgroundWorker.ProgressChanged += (sender, e) =>
            {
                DeleteProgressBar.Value = _deletedCount;
                CounterTextBlock.Text = _deletedCount + " z " + _totalCount;
            };
            _backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                _deletedCount += _deliveries.Count();

                _dbContext.Deliveries.RemoveRange(_deliveries);
                _dbContext.SaveChanges();
                
                OnDeliveriesDeleted(_deliveries);

                DeleteProgressBar.Value += _deletedCount;
                CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

                OnWorkCompleted();

                this.Close();
            };

            this.Closed += (sender, e) =>
            {
                _initWorker.Dispose();
                _backgroundWorker.Dispose();
            };

            _initWorker.RunWorkerAsync();
        }

        private void OnDeliveriesDeleted(IEnumerable<Delivery> deliveries)
        {
            if (DeliveriesDeleted != null)
            {
                DeliveriesDeleted(this, deliveries);
            }
        }

        private void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }
    }
}
