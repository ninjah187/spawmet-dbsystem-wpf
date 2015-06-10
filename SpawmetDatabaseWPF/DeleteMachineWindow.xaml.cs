using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        //private readonly MachinesWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Machine> _machines;

        private readonly BackgroundWorker _backgroundWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteMachineWindow(/*MachinesWindow parentWindow, */SpawmetDBContext dbContext, IEnumerable<Machine> machines)
        {
            InitializeComponent();

            //_parentWindow = parentWindow;
            _dbContext = dbContext;
            _machines = machines;

            _initWorker = new BackgroundWorker();
            _initWorker.DoWork += (sender, e) =>
            {
                foreach (var machine in _machines)
                {
                    _totalCount += machine.StandardPartSet.Count + machine.Orders.Count;
                    foreach (var order in machine.Orders)
                    {
                        _totalCount += order.AdditionalPartSet.Count;
                    }
                }
                _totalCount += _machines.Count();
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
                foreach (var machine in _machines)
                {
                    int count = machine.StandardPartSet.Count;
                    _dbContext.StandardPartSets.RemoveRange(machine.StandardPartSet);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _backgroundWorker.ReportProgress(0);

                    foreach (var order in machine.Orders)
                    {
                        count = order.AdditionalPartSet.Count;
                        _dbContext.AdditionalPartSets.RemoveRange(order.AdditionalPartSet);
                        _dbContext.SaveChanges();

                        _deletedCount += count;
                        _backgroundWorker.ReportProgress(0);
                    }

                    count = machine.Orders.Count;
                    _dbContext.Orders.RemoveRange(machine.Orders);
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
                _deletedCount += _machines.Count();

                _dbContext.Machines.RemoveRange(_machines);
                _dbContext.SaveChanges();

                OnMachinesDeleted(_machines);

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
    }
}
