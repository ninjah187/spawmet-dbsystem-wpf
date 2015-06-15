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
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public partial class PartsWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        public PartsWindow()
            : this(40, 40)
        {
        }

        public PartsWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y
            })
        {
        }

        public PartsWindow(WindowConfig config)
        {
            InitializeComponent();

            var viewModel = new PartsWindowViewModel(this, config);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                string id = "";
                string name = "";
                string amount = "";

                if (e.Element != null)
                {
                    var part = (Part)e.Element;
                    id = part.Id.ToString();
                    name = part.Name;
                    amount = part.Amount.ToString();
                }

                IdTextBlock.Text = id;
                NameTextBlock.Text = name;
                AmountTextBlock.Text = amount;
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

            this.SizeChanged += delegate
            {
                var binding = MachinesListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = MachinesListBox.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                binding = OrdersListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = OrdersListBox.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                binding = DeliveriesListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = DeliveriesListBox.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };

            viewModel.Load();
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit();
        }
    }
}
