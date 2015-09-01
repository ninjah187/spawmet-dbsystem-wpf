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
    /// Interaction logic for ArchiveWindow.xaml
    /// </summary>
    public partial class ArchiveWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        private readonly ArchiveWindowViewModel _viewModel;

        public ArchiveWindow()
            : this(40, 40)
        {
        }

        public ArchiveWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y
            })
        {
        }

        public ArchiveWindow(WindowConfig config)
        {
            InitializeComponent();

            _viewModel = new ArchiveWindowViewModel(this, config);
            DataContext = _viewModel;

            Loaded += async delegate
            {
                await _viewModel.LoadAsync();
            };

            Closed += delegate
            {
                _viewModel.Dispose();
            };

            SizeChanged += delegate
            {
                var binding = StandardPartSetDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = StandardPartSetDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                binding = AdditionalPartSetDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = AdditionalPartSetDataGrid.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();

                binding = ModulesListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = ModulesListBox.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();
            };
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
