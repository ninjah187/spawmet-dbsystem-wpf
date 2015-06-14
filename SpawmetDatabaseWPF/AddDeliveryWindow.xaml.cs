using System;
using System.Collections.Generic;
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

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddDeliveryWindow.xaml
    /// </summary>
    public partial class AddDeliveryWindow : Window
    {
        public event EventHandler<Delivery> DeliveryAdded;

        private SpawmetDBContext _dbContext;

        public AddDeliveryWindow()
        {
            InitializeComponent();
        }

        public AddDeliveryWindow(SpawmetDBContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext;

            this.Loaded += (sender, e) =>
            {
            };
            this.Closed += (sender, e) =>
            {
            };

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            NameTextBox.GotFocus += TextBox_GotFocus;

            StartDatePicker.SelectedDate = DateTime.Today;

            NameTextBox.Focus();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            var date = StartDatePicker.SelectedDate;

            var delivery = new Delivery()
            {
                Name = name,
                Date = date.Value
            };

            _dbContext.Deliveries.Add(delivery);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

            OnDeliveryAdded(delivery);

            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Disconnected()
        {
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
        }

        private void OnDeliveryAdded(Delivery delivery)
        {
            if (DeliveryAdded != null)
            {
                DeliveryAdded(this, delivery);
            }
        }
    }
}
