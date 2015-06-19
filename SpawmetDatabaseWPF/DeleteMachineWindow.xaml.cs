using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for DeleteMachineWindow.xaml
    /// </summary>
    public partial class DeleteMachineWindow : Window
    {
        public event EventHandler<IEnumerable<Machine>> MachinesDeleted;
        public event EventHandler WorkCompleted;

        public event EventHandler<Exception> ConnectionLost;

        //private readonly MachinesWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Machine> _machines;

        private readonly BackgroundWorker _mainWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteMachineWindow(/*MachinesWindow parentWindow, */SpawmetDBContext dbContext, IEnumerable<Machine> machines)
        {
            InitializeComponent();

            //_parentWindow = parentWindow;
            _dbContext = dbContext;
            _machines = machines;

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
                foreach (var machine in _machines)
                {
                    if (_initWorker.CancellationPending)
                    {
                        return;
                    }

                    _totalCount += machine.StandardPartSet.Count + machine.Orders.Count;
                    foreach (var order in machine.Orders)
                    {
                        _totalCount += order.AdditionalPartSet.Count;
                    }
                }

                if (_initWorker.CancellationPending)
                {
                    return;
                }
                _totalCount += _machines.Count();
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
                foreach (var machine in _machines)
                {
                    if (_mainWorker.CancellationPending)
                    {
                        return;
                    }

                    int count = machine.StandardPartSet.Count;
                    _dbContext.StandardPartSets.RemoveRange(machine.StandardPartSet);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _mainWorker.ReportProgress(0);

                    foreach (var order in machine.Orders)
                    {
                        count = order.AdditionalPartSet.Count;
                        _dbContext.AdditionalPartSets.RemoveRange(order.AdditionalPartSet);
                        _dbContext.SaveChanges();

                        _deletedCount += count;
                        _mainWorker.ReportProgress(0);
                    }

                    count = machine.Orders.Count;
                    _dbContext.Orders.RemoveRange(machine.Orders);
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
            _deletedCount += _machines.Count();

            _dbContext.Machines.RemoveRange(_machines);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception exc)
            {
                OnConnectionLost(exc);
                return;
            }

            OnMachinesDeleted(_machines);

            DeleteProgressBar.Value += _deletedCount;
            CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

            //parentWindow.Refresh();

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

        private void OnMachinesDeleted(IEnumerable<Machine> machines)
        {
            if (MachinesDeleted != null)
            {
                MachinesDeleted(this, machines);
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
