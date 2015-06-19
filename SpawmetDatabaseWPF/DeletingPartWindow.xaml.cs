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
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Exceptions;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for DeletingPartWindow.xaml
    /// </summary>
    public partial class DeletingPartWindow : Window
    {
        public event EventHandler<IEnumerable<Part>> PartsDeleted;
        public event EventHandler WorkCompleted;

        public event EventHandler<Exception> ConnectionLost;

        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Part> _parts;

        private readonly BackgroundWorker _mainWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeletingPartWindow(SpawmetDBContext dbContext, IEnumerable<Part> parts)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _parts = parts;

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
                foreach (var part in _parts)
                {
                    if (_initWorker.CancellationPending)
                    {
                        return;
                    }

                    _totalCount += part.AdditionalPartSets.Count + part.DeliveryPartSets.Count
                                   + part.StandardPartSets.Count;
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
                if (_mainWorker.CancellationPending)
                {
                    return;
                }

                foreach (var part in _parts)
                {
                    int count = part.StandardPartSets.Count;
                    _dbContext.StandardPartSets.RemoveRange(part.StandardPartSets);
                    _dbContext.SaveChanges();
                    //try
                    //{
                    //    _dbContext.SaveChanges();
                    //}
                    //catch (Exception exc)
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        OnConnectionLost(exc);
                    //    });
                    //    return;
                    //}

                    _deletedCount += count;
                    _mainWorker.ReportProgress(0);

                    count = part.AdditionalPartSets.Count;
                    _dbContext.AdditionalPartSets.RemoveRange(part.AdditionalPartSets);
                    _dbContext.SaveChanges();
                    //try
                    //{
                    //    _dbContext.SaveChanges();
                    //}
                    //catch (Exception exc)
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        OnConnectionLost(exc);
                    //    });
                    //    return;
                    //}

                    _deletedCount += count;
                    _mainWorker.ReportProgress(0);

                    count = part.DeliveryPartSets.Count;
                    _dbContext.DeliveryPartSets.RemoveRange(part.DeliveryPartSets);
                    _dbContext.SaveChanges();
                    //try
                    //{
                    //    _dbContext.SaveChanges();
                    //}
                    //catch (Exception exc)
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        OnConnectionLost(exc);
                    //    });
                    //    return;
                    //}

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
            _deletedCount += _parts.Count();

            _dbContext.Parts.RemoveRange(_parts);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception exc)
            {
                OnConnectionLost(exc);
                return;
            }

            OnPartsDeleted(_parts);

            DeleteProgressBar.Value += _deletedCount;
            CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

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

        private void OnPartsDeleted(IEnumerable<Part> parts)
        {
            if (PartsDeleted != null)
            {
                PartsDeleted(this, parts);
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
