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
    /// Interaction logic for DeletingPartWindow.xaml
    /// </summary>
    public partial class DeletingPartWindow : Window
    {
        public event EventHandler<IEnumerable<Part>> PartsDeleted;
        public event EventHandler WorkCompleted;

        private readonly SpawmetDBContext _dbContext;
        private readonly IEnumerable<Part> _parts;

        private readonly BackgroundWorker _backgroundWorker;
        private readonly BackgroundWorker _initWorker;

        private int _deletedCount = 0;
        private int _totalCount = 0;

        public DeletingPartWindow(SpawmetDBContext dbContext, IEnumerable<Part> parts)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _parts = parts;

            _initWorker = new BackgroundWorker();
            _initWorker.DoWork += (sender, e) =>
            {
                foreach (var part in _parts)
                {
                    _totalCount += part.AdditionalPartSets.Count + part.DeliveryPartSets.Count
                                   + part.StandardPartSets.Count;
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
                foreach (var part in _parts)
                {
                    int count = part.StandardPartSets.Count;
                    _dbContext.StandardPartSets.RemoveRange(part.StandardPartSets);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _backgroundWorker.ReportProgress(0);

                    count = part.AdditionalPartSets.Count;
                    _dbContext.AdditionalPartSets.RemoveRange(part.AdditionalPartSets);
                    _dbContext.SaveChanges();

                    _deletedCount += count;
                    _backgroundWorker.ReportProgress(0);

                    count = part.DeliveryPartSets.Count;
                    _dbContext.DeliveryPartSets.RemoveRange(part.DeliveryPartSets);
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
                _deletedCount += _parts.Count();

                _dbContext.Parts.RemoveRange(_parts);
                _dbContext.SaveChanges();

                OnPartsDeleted(_parts);

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
    }
}
