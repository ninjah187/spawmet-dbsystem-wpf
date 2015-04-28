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
        private DeliveriesWindow _parentWindow;
        private SpawmetDBContext _dbContext;

        public AddDeliveryWindow()
        {
            InitializeComponent();
        }

        public AddDeliveryWindow(DeliveriesWindow parentWindow, SpawmetDBContext dbContext)
        {
            InitializeComponent();

            _parentWindow = parentWindow;
            _dbContext = dbContext;

            this.Loaded += (sender, e) =>
            {
                _parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _parentWindow.IsEnabled = true;
            };

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

            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Disconnected()
        {
            _parentWindow.MainDataGrid.IsEnabled = false;
            _parentWindow.DetailsStackPanel.IsEnabled = false;
            _parentWindow.MachinesMenuItem.IsEnabled = false;
            _parentWindow.FillDetailedInfo(null);
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
            _parentWindow.ConnectMenuItem.IsEnabled = true;
        }
    }
}
