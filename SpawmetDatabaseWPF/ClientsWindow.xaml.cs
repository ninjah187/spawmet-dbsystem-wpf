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
    public partial class ClientsWindow : Window, ISpawmetWindow
    {
        public DataGrid DataGrid { get { return MainDataGrid; } }

        public ClientsWindow()
            : this(40, 40)
        {
        }

        public ClientsWindow(double x, double y)
            : this(new WindowConfig()
            {
                Left = x,
                Top = y
            })
        {
        }

        public ClientsWindow(WindowConfig config)
        {
            InitializeComponent();

            var viewModel = new ClientsWindowViewModel(this, config);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                string id = "";
                string name = "";
                string address = "";
                string phone = "";
                string email = "";
                string nip = "";

                if (e.Element != null)
                {
                    var client = (Client)e.Element;
                    id = client.Id.ToString();
                    name = client.Name;
                    address = client.Address;
                    phone = client.Phone;
                    email = client.Email;
                    nip = client.Nip;
                }

                IdTextBlock.Text = id;
                NameTextBlock.Text = name;
                AddressTextBlock.Text = address;
                PhoneTextBlock.Text = phone;
                EmailTextBlock.Text = email;
                NipTextBlock.Text = nip;
            };

            viewModel.OrdersStartLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = true;
            };
            viewModel.OrdersCompletedLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = false;
            };

            this.SizeChanged += delegate
            {
                var binding = OrdersListBox.GetBindingExpression(HeightProperty);
                binding.UpdateTarget();

                binding = OrdersListBox.GetBindingExpression(WidthProperty);
                binding.UpdateTarget();
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };
        }

        public void CommitEdit()
        {
            MainDataGrid.CommitEdit();
        }
    }
}
