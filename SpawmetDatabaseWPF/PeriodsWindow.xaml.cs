using System;
using System.Collections.Generic;
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
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for PeriodsWindow.xaml
    /// </summary>
    public partial class PeriodsWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return PeriodsDataGrid; } }

        private PeriodsWindowViewModel _viewModel;

        public PeriodsWindow()
            : this(40, 40)
        {
        }

        public PeriodsWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y,
            })
        {
        }

        public PeriodsWindow(WindowConfig config)
        {
            InitializeComponent();

            _viewModel = new PeriodsWindowViewModel(this, config);
            DataContext = _viewModel;

            Loaded += async delegate
            {
                await _viewModel.LoadAsync();
            };

            this.Closed += delegate
            {
                _viewModel.Dispose();
            };

            this.SizeChanged += delegate
            {

            };
        }

        public void CommitEdit()
        {
            PeriodsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private async void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //var datePicker = (DatePicker)sender;
            //datePicker.SelectedDateChanged -= DatePicker_OnSelectedDateChanged;

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
            _viewModel.Mediator.NotifyContextChange(_viewModel);

            //datePicker.SelectedDateChanged += DatePicker_OnSelectedDateChanged;

            //StartCalendar.OnApplyTemplate();
        }
    }
}
