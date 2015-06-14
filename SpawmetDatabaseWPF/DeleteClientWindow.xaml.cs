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
    /// Interaction logic for DeleteClientWindow.xaml
    /// </summary>
    public partial class DeleteClientWindow : Window
    {
        public event EventHandler<IEnumerable<Client>> ClientsDeleted;
        public event EventHandler WorkCompleted;

        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Client> _clients;

        private readonly BackgroundWorker _backgroundWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteClientWindow(SpawmetDBContext dbContext, IEnumerable<Client> clients)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _clients = clients;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _initWorker = new BackgroundWorker();
            _initWorker.DoWork += (sender, e) =>
            {
                foreach (var client in _clients)
                {
                    foreach (var order in client.Orders)
                    {
                        _totalCount += order.AdditionalPartSet.Count();
                        _totalCount++;
                    }
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
                foreach (var client in _clients)
                {
                    int count;
                    foreach (var order in client.Orders)
                    {
                        count = order.AdditionalPartSet.Count;
                        _dbContext.AdditionalPartSets.RemoveRange(order.AdditionalPartSet);
                        _dbContext.SaveChanges();

                        _deletedCount += count;
                        _backgroundWorker.ReportProgress(0);
                    }

                    count = client.Orders.Count;
                    _dbContext.Orders.RemoveRange(client.Orders);
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
                _deletedCount += _clients.Count();

                _dbContext.Clients.RemoveRange(_clients);
                _dbContext.SaveChanges();

                OnClientsDeleted(clients);
                //using (var context = new SpawmetDBContext())
                //{
                //    var toDelete = new List<Client>();
                //    foreach (var machine in _machines)
                //    {
                //        toDelete.Add(context.Machines.Single(m => m.Id == machine.Id));
                //    }
                //    context.Machines.RemoveRange(toDelete);
                //    context.SaveChanges();
                //}

                DeleteProgressBar.Value += _deletedCount;
                CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

                //parentWindow.Refresh();

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

        private void OnClientsDeleted(IEnumerable<Client> clients)
        {
            if (ClientsDeleted != null)
            {
                ClientsDeleted(this, clients);
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
