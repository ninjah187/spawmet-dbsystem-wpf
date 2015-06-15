using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using SpawmetDatabase;
using SpawmetDatabase.FileCreators;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    /***********************************************************************************/
    /*** Main rule for loading data to UI from another threads:                      ***/
    /***   - readonly data (like OrdersListBox in MachinesWindow) can be loaded from ***/
    /***     another context (with using Include() to load all related data, in      ***/
    /***     order to display Signature).                                            ***/
    /***   - read/write data (like StandardPartSetGrid in MachinesWindow) MUST be    ***/
    /***     loaded from main _dbContext (with using lock and Include).              ***/
    /***********************************************************************************/

    public partial class MachinesWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        public MachinesWindow()
            : this(40, 40)
        {
        }

        public MachinesWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y,
            })
        {
        }

        public MachinesWindow(WindowConfig config)
        {
            InitializeComponent();

            var viewModel = new MachinesWindowViewModel(this, config);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                string id = "";
                string name = "";
                string price = "";

                if (e.Element != null)
                {
                    var machine = (Machine) e.Element;

                    id = machine.Id.ToString();
                    name = machine.Name;
                    price = machine.Price.ToString();
                }

                IdTextBlock.Text = id;
                NameTextBlock.Text = name;
                PriceTextBlock.Text = price;
            };

            viewModel.PartSetStartLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = true;
            };
            viewModel.PartSetCompletedLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = false;
            };

            viewModel.OrdersStartLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = true;
            };
            viewModel.OrdersCompletedLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = false;
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };

            this.SizeChanged += delegate
            {
                var standardPartSetGridHeightBinding = StandardPartSetDataGrid.GetBindingExpression(DataGrid.HeightProperty);
                standardPartSetGridHeightBinding.UpdateTarget();

                var standardPartSetGridWidthBinding = StandardPartSetDataGrid.GetBindingExpression(DataGrid.WidthProperty);
                standardPartSetGridWidthBinding.UpdateTarget();

                var ordersListBoxHeightBinding = OrdersListBox.GetBindingExpression(HeightProperty);
                ordersListBoxHeightBinding.UpdateTarget();

                var ordersListBoxWidthBinding = OrdersListBox.GetBindingExpression(WidthProperty);
                ordersListBoxWidthBinding.UpdateTarget();
            };

            viewModel.Load();
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit();
            StandardPartSetDataGrid.CommitEdit();
        }
    }
}
