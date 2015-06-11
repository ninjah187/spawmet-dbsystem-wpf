using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
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
    public partial class OrdersWindow : Window, ISpawmetWindow
    {
        public OrdersWindow()
            : this(40, 40)
        {
        }

        public OrdersWindow(double x, double y)
        {
            InitializeComponent();

            Left = x;
            Top = y;

            var viewModel = new OrdersWindowViewModel(this);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                var order = (Order) e.Element;

                IdTextBlock.Text = order.Id.ToString();
                ClientTextBlock.Text = order.Client.Name;
                MachineTextBlock.Text = order.Machine.Name;

                string startDate = "";
                startDate = order.StartDate != null
                    ? order.StartDate.Value.ToString("yyyy-MM-dd")
                    : "";
                StartDateTextBlock.Text = startDate;

                string sendDate = "";
                sendDate = order.SendDate != null
                    ? order.SendDate.Value.ToString("yyyy-MM-dd")
                    : "";
                SendDateTextBlock.Text = sendDate;

                string status = "";
                status = order.Status != null
                    ? order.Status.Value.GetDescription()
                    : "";
                StatusTextBlock.Text = status;

                RemarksTextBlock.Text = order.Remarks;
            };

            viewModel.PartSetStartLoading += delegate
            {
                AdditionalPartSetProgressBar.IsIndeterminate = true;
            };
            viewModel.PartSetCompletedLoading += delegate
            {
                AdditionalPartSetProgressBar.IsIndeterminate = false;
            };

            this.SizeChanged += delegate
            {
                var binding = AdditionalPartSetDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = AdditionalPartSetDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };
        }
    }
}
