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

        private DeliveriesWindowViewModel _viewModel;

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

            _viewModel = new DeliveriesWindowViewModel(this, config);
            DataContext = _viewModel;

            _viewModel.ElementSelected += (sender, e) =>
            {
                //string id = "";
                //string name = "";
                //string date = "";

                //if (e.Element != null)
                //{
                //    var delivery = (Delivery)e.Element;
                //    id = delivery.Id.ToString();
                //    name = delivery.Name;
                //    date = delivery.Date.ToString("yyyy-MM-dd");
                //}

                //IdTextBlock.Text = id;
                //NameTextBlock.Text = name;
                //DateTextBlock.Text = date;
            };

            _viewModel.PartSetStartLoading += delegate
            {
                PartsProgressBar.IsIndeterminate = true;
            };
            _viewModel.PartSetCompletedLoading += delegate
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
                _viewModel.Dispose();
            };

            Loaded += async delegate
            {
                await _viewModel.LoadAsync();
            };
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            PartsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void Select(Delivery delivery)
        {
            _viewModel.SelectElement(delivery);
        }
    }
}
