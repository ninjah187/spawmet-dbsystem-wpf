using System;
using System.Collections.Generic;
using System.Data;
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
using SpawmetDatabaseWPF.CommonWindows;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddOrderWindow.xaml
    /// </summary>
    public partial class AddOrderWindow : Window
    {
        public event EventHandler<Order> OrderAdded;

        private SpawmetDBContext _dbContext;

        public AddOrderWindow()
        {
            InitializeComponent();
        }

        public AddOrderWindow(SpawmetDBContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext;

            ClientComboBox.ItemsSource = _dbContext.Clients.OrderBy(c => c.Name).ToList();
            MachineComboBox.ItemsSource = _dbContext.Machines.OrderBy(m => m.Name).ToList();

            this.Loaded += (sender, e) =>
            {

            };
            this.Closed += (sender, e) =>
            {

            };

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            RemarksTextBox.GotFocus += TextBox_GotFocus;
            PriceTextBox.GotFocus += TextBox_GotFocus;

            StartDatePicker.SelectedDate = DateTime.Today;
            SendDatePicker.SelectedDate = DateTime.Today;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var client = (Client) ClientComboBox.SelectedItem;
            var machine = (Machine) MachineComboBox.SelectedItem;
            var serialNumber = SerialNumberTextBox.Text;
            if (machine == null)
            {
                MessageBox.Show("Wybierz maszynę.", "Błąd");
                return;
            }
            var startDate = StartDatePicker.SelectedDate;
            var sendDate = SendDatePicker.SelectedDate;
            var status = (OrderStatus) StatusComboBox.SelectedIndex;
            var remarks = RemarksTextBox.Text;

            Decimal price = 0;
            try
            {
                price = Decimal.Parse(PriceTextBox.Text);
            }
            catch (FormatException exc)
            {
                MessageWindow.Show("Cena musi być liczbą.", "Błąd", null);
                return;
            }

            var order = new Order()
            {
                //Id = FindSmallestFreeId(),
                Client = client,
                Machine = machine,
                SerialNumber = serialNumber,
                StartDate = startDate,
                SendDate = sendDate,
                Status = status,
                Price = machine.Price,
                Remarks = remarks
            };
            _dbContext.Orders.Add(order);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

            OnOrderAdded(order);

            this.Close();
        }

        private int FindSmallestFreeId()
        {
            var ids = _dbContext.Orders.Select(o => o.Id).ToList();
            int id = -1;
            for (int i = 0; i < ids.Count - 1; i++)
            {
                if (ids[i] == ids[i + 1] - 1)
                {
                    continue;
                }

                id = ids[i] + 1;
                return id;
            }
            return id;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Disconnected()
        {
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
        }

        private void OnOrderAdded(Order order)
        {
            if (OrderAdded != null)
            {
                OrderAdded(this, order);
            }
        }
    }
}
