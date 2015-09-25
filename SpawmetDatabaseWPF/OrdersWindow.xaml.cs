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

            this.SizeChanged += delegate
            {
                var binding = AdditionalPartSetDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = AdditionalPartSetDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                binding = ModulesListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = ModulesListBox.GetBindingExpression(WidthProperty);
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

        // had to do it this way because with EventTriggers there were problems with constantly running SaveDbStateCommand
        private async void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //_viewModel.SaveDbStateCommand.Execute(null);

            //var datePicker = (DatePicker) sender;
            //datePicker.SelectedDateChanged -= DatePicker_OnSelectedDateChanged;

            CommitEdit();
            int rowsChangedCount = 0;
            await Task.Run(() =>
            {
                _viewModel.IsSaving = true;
                lock (_viewModel.DbContextLock)
                {
                    rowsChangedCount = _viewModel.DbContext.SaveChanges();
                }
                _viewModel.IsSaving = false;
            });

            if (rowsChangedCount != 0)
            {
                _viewModel.DbContextMediator.NotifyContextChanged(_viewModel);
            }

            //sdatePicker.SelectedDateChanged -= DatePicker_OnSelectedDateChanged;
        }
    }
}
