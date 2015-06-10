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
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public partial class DeliveriesWindow : Window, ISpawmetWindow
    {
        public DeliveriesWindow()
            : this(40, 40)
        {
        }

        public DeliveriesWindow(double x, double y)
        {
            InitializeComponent();

            Left = x;
            Top = y;

            var viewModel = new DeliveriesWindowViewModel(this);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                var delivery = (Delivery) e.Element;

                IdTextBlock.Text = delivery.Id.ToString();
                NameTextBlock.Text = delivery.Name;

                string date = "";
                date = delivery.Date.ToString("yyyy-MM-dd");

                DateTextBlock.Text = date;
            };

            viewModel.PartSetStartLoading += delegate
            {
                PartsProgressBar.IsIndeterminate = true;
            };
            viewModel.PartSetCompletedLoading += delegate
            {
                PartsProgressBar.IsIndeterminate = false;
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };
        }
    }
}
