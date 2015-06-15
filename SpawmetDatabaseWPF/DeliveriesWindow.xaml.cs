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
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public partial class DeliveriesWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        public DeliveriesWindow()
            : this(40, 40)
        {
        }

        public DeliveriesWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y
            })
        {
        }

        public DeliveriesWindow(WindowConfig config)
        {
            InitializeComponent();

            var viewModel = new DeliveriesWindowViewModel(this, config);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                string id = "";
                string name = "";
                string date = "";

                if (e.Element != null)
                {
                    var delivery = (Delivery)e.Element;
                    id = delivery.Id.ToString();
                    name = delivery.Name;
                    date = delivery.Date.ToString("yyyy-MM-dd");
                }

                IdTextBlock.Text = id;
                NameTextBlock.Text = name;
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

            this.SizeChanged += delegate
            {
                var binding = PartsDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = PartsDataGrid.GetBindingExpression(WidthProperty);
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
            PartsDataGrid.CommitEdit();
        }
    }
}
