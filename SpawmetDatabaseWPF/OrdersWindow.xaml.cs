﻿using System;
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
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public partial class OrdersWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        private OrdersWindowViewModel _viewModel;

        public OrdersWindow()
            : this(40, 40)
        {
        }

        public OrdersWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y
            })
        {
        }

        public OrdersWindow(WindowConfig config)
        {
            InitializeComponent();

            _viewModel = new OrdersWindowViewModel(this, config);
            DataContext = _viewModel;

            _viewModel.ElementSelected += (sender, e) =>
            {
                //string id = "";
                //string client = "";
                //string machine = "";
                //string startDate = "";
                //string sendDate = "";
                //string status = "";
                //string remarks = "";

                //if (e.Element != null)
                //{
                //    var order = (Order) e.Element;
                //    id = order.Id.ToString();
                //    client = order.Client != null ? order.Client.Name : "";
                //    machine = order.Machine.Name;

                //    startDate = order.StartDate != null
                //    ? order.StartDate.Value.ToString("yyyy-MM-dd")
                //    : "";

                //    sendDate = order.SendDate != null
                //    ? order.SendDate.Value.ToString("yyyy-MM-dd")
                //    : "";

                //    status = order.Status != null
                //    ? order.Status.Value.GetDescription()
                //    : "";

                //    remarks = order.Remarks;
                //}

                //IdTextBlock.Text = id;
                //ClientTextBlock.Text = client;
                //MachineTextBlock.Text = machine;
                //StartDateTextBlock.Text = startDate;
                //SendDateTextBlock.Text = sendDate;
                //StatusTextBlock.Text = status;
                //RemarksTextBlock.Text = remarks;
            };

            //_viewModel.PartSetStartLoading += delegate
            //{
            //    AdditionalPartSetProgressBar.IsIndeterminate = true;
            //};
            //_viewModel.PartSetCompletedLoading += delegate
            //{
            //    AdditionalPartSetProgressBar.IsIndeterminate = false;
            //};

            this.SizeChanged += delegate
            {
                var binding = AdditionalPartSetDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = AdditionalPartSetDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();
            };

            this.Closed += delegate
            {
                _viewModel.Dispose();
            };

            //_viewModel.Load();
            Loaded += async delegate
            {
                await _viewModel.LoadAsync();
            };
        }

        public void CommitEdit()
        {
            //MainDataGrid.CommitEdit();
            //AdditionalPartSetDataGrid.CommitEdit();

            MainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            AdditionalPartSetDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void Select(Order order)
        {
            _viewModel.SelectElement(order);
        }

        private void StatusComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count != 0 && e.AddedItems.Count != 0)
            {
                var oldStatus = (OrderStatus) e.RemovedItems[0];
                var newStatus = (OrderStatus) e.AddedItems[0];

                _viewModel.ChangeStatus(oldStatus, newStatus);
            }
        }
    }
}
