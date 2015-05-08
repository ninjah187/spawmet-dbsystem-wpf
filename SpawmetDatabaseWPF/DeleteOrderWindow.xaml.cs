﻿using System;
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
        private readonly OrdersWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Order> _orders;

        private readonly BackgroundWorker _backgroundWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeleteOrderWindow(OrdersWindow parentWindow, SpawmetDBContext dbContext, IEnumerable<Order> orders)
        {
            InitializeComponent();

            _parentWindow = parentWindow;
            _dbContext = dbContext;
            _orders = orders;

            _initWorker = new BackgroundWorker();
            _initWorker.DoWork += (sender, e) =>
            {
                foreach (var order in _orders)
                {
                    _totalCount += order.AdditionalPartSet.Count();
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
                foreach (var order in _orders)
                {
                    int count = order.AdditionalPartSet.Count;
                    _dbContext.AdditionalPartSets.RemoveRange(order.AdditionalPartSet);
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
                _deletedCount += _orders.Count();

                //_dbContext.Orders.RemoveRange(_orders);
                //_dbContext.SaveChanges();
                using (var context = new SpawmetDBContext())
                {
                    var toDelete = new List<Order>();
                    foreach (var order in _orders)
                    {
                        toDelete.Add(context.Orders.Single(o => o.Id == order.Id));
                    }
                    context.Orders.RemoveRange(toDelete);
                    context.SaveChanges();
                }

                DeleteProgressBar.Value += _deletedCount;
                CounterTextBlock.Text = _deletedCount + " z " + _totalCount;

                parentWindow.Refresh();
                //parentWindow.FillDetailedInfo(null);

                this.Close();
            };

            this.Closed += (sender, e) =>
            {
                _initWorker.Dispose();
                _backgroundWorker.Dispose();
            };

            _initWorker.RunWorkerAsync();
            
        }
    }
}
