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
using SpawmetDatabaseWPF.CommonWindows;
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

        private MachinesWindowViewModel _viewModel;

        private static bool _firstLaunch = true;

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

            _viewModel = new MachinesWindowViewModel(this, config);
            DataContext = _viewModel;

            ModulesDataGrid.RowEditEnding += RowEditEndingHandler;

            _viewModel.PartSetStartLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = true;
            };
            _viewModel.PartSetCompletedLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = false;
            };

            _viewModel.ModulesStartLoading += delegate
            {
                ModuleProgressBar.IsIndeterminate = true;
            };
            _viewModel.ModulesCompletedLoading += delegate
            {
                ModuleProgressBar.IsIndeterminate = false;
            };

            _viewModel.OrdersStartLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = true;
            };
            _viewModel.OrdersCompletedLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = false;
            };

            this.Closed += delegate
            {
                _viewModel.Dispose();
            };

            this.SizeChanged += delegate
            {
                var standardPartSetGridHeightBinding = StandardPartSetDataGrid.GetBindingExpression(DataGrid.HeightProperty);
                standardPartSetGridHeightBinding.UpdateTarget();

                var standardPartSetGridWidthBinding = StandardPartSetDataGrid.GetBindingExpression(DataGrid.WidthProperty);
                standardPartSetGridWidthBinding.UpdateTarget();

                var binding = ModulesDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = ModulesDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                var ordersListBoxHeightBinding = OrdersListBox.GetBindingExpression(HeightProperty);
                ordersListBoxHeightBinding.UpdateTarget();

                var ordersListBoxWidthBinding = OrdersListBox.GetBindingExpression(WidthProperty);
                ordersListBoxWidthBinding.UpdateTarget();
            };
            
            //_viewModel.Load();

            if (_firstLaunch)
            {
                _viewModel.Load();

                _firstLaunch = false;
            }
            else
            {
                Loaded += async delegate
                {
                    await _viewModel.LoadAsync();
                };
            }
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            StandardPartSetDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            ModulesDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void Select(Machine machine)
        {
            _viewModel.SelectElement(machine);
        }

        protected async void RowEditEndingHandler(object sender, DataGridRowEditEndingEventArgs e)
        {
            ModulesDataGrid.RowEditEnding -= RowEditEndingHandler;
            //if (e.EditAction == DataGridEditAction.Commit)
            //{
            //    OnDataGridEditCommited();
            //}
            //else // DataGridEditAction.Cancel
            //{
            //    OnDataGridEditCanceled();
            //}
            //throw new Exception("row edit ending");
            CommitEdit();
            await Task.Run(() =>
            {
                _viewModel.IsSaving = true;
                lock (_viewModel.DbContextLock)
                {
                    _viewModel.DbContext.SaveChanges();
                }
                _viewModel.IsSaving = false;
            });
            
            _viewModel.DbContextMediator.NotifyContextChanged(_viewModel);

            ModulesDataGrid.RowEditEnding += RowEditEndingHandler;
        }
    }
}
