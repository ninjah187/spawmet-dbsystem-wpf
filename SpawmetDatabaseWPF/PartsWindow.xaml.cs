using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
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
    public partial class PartsWindow : Window, ISpawmetWindow
    {
        public PartsWindow()
            : this(40, 40)
        {
        }

        public PartsWindow(double x, double y)
        {
            InitializeComponent();

            Left = x;
            Top = y;

            var viewModel = new PartsWindowViewModel(this);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                var part = (Part) e.Element;

                IdTextBlock.Text = part.Id.ToString();
                NameTextBlock.Text = part.Name;
                AmountTextBlock.Text = part.Amount.ToString();
            };

            viewModel.MachinesStartLoading += delegate
            {
                MachinesProgressBar.IsIndeterminate = true;
            };
            viewModel.MachinesCompletedLoading += delegate
            {
                MachinesProgressBar.IsIndeterminate = false;
            };

            viewModel.OrdersStartLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = true;
            };
            viewModel.OrdersCompletedLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = false;
            };

            viewModel.DeliveriesStartLoading += delegate
            {
                DeliveriesProgressBar.IsIndeterminate = true;
            };
            viewModel.DeliveriesCompletedLoading += delegate
            {
                DeliveriesProgressBar.IsIndeterminate = false;
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };
        }
    }
}
