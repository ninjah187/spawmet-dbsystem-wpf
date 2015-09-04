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
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public partial class PartsWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        private PartsWindowViewModel _viewModel;

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

            _viewModel = new PartsWindowViewModel(this, config);
            DataContext = _viewModel;

            _viewModel.ElementSelected += (sender, e) =>
            {
                //string id = "";
                //string name = "";
                //string amount = "";

                //if (e.Element != null)
                //{
                //    var part = (Part)e.Element;
                //    id = part.Id.ToString();
                //    name = part.Name;
                //    amount = part.Amount.ToString();
                //}

                //IdTextBlock.Text = id;
                //NameTextBlock.Text = name;
                //AmountTextBlock.Text = amount;
            };

            //viewModel.ConnectionChanged += delegate
            //{
            //    //if (viewModel.IsConnected == false)
            //    //{
            //    //    //MessageWindow.Show("Brak połączenia.", "Błąd", null);
            //    //    this.Close();
            //    //}
            //};

            _viewModel.MachinesStartLoading += delegate
            {
                EnableUIElement(MachinesProgressBar);
            };
            _viewModel.MachinesCompletedLoading += delegate
            {
                DisableUIElement(MachinesProgressBar);
            };

            _viewModel.OrdersStartLoading += delegate
            {
                EnableUIElement(OrdersProgressBar);
            };
            _viewModel.OrdersCompletedLoading += delegate
            {
                DisableUIElement(OrdersProgressBar);
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

                binding = ModulesDataGrid.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = ModulesDataGrid.GetBindingExpression(WidthProperty);
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

        public void DisableUIElement(UIElement element)
        {
            Application.Current.Dispatcher.Invoke(() => element.IsEnabled = false);
        }

        public void EnableUIElement(UIElement element)
        {
            Application.Current.Dispatcher.Invoke(() => element.IsEnabled = true);
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void Select(Part part)
        {
            _viewModel.SelectElement(part);
        }
    }
}
