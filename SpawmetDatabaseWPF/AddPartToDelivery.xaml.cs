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
    /// Interaction logic for AddPartToDelivery.xaml
    /// </summary>
    public partial class AddPartToDelivery : Window
    {
        private readonly DeliveriesWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly Delivery _delivery;

        public AddPartToDelivery(DeliveriesWindow parentWindow, SpawmetDBContext dbContext, Delivery delivery)
        {
            InitializeComponent();

            _parentWindow = parentWindow;
            _dbContext = dbContext;
            _delivery = delivery;

            var parts = _dbContext.Parts.ToList();
            foreach (var deliveryPartSetElement in _delivery.DeliveryPartSet)
            {
                parts.Remove(deliveryPartSetElement.Part);
            }
            MainListBox.ItemsSource = parts.OrderBy(part => part.Name);

            this.Loaded += (sender, e) =>
            {
                _parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _parentWindow.IsEnabled = true;
            };

            AmountTextBox.GotFocus += TextBox_GotFocus;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var part = (Part) MainListBox.SelectedItem;
            if (part == null)
            {
                MessageBox.Show("Brak części.", "Błąd");
                return;
            }
            int amount;
            try
            {
                amount = int.Parse(AmountTextBox.Text);
            }
            catch (FormatException exc)
            {
                MessageBox.Show("Ilość musi być liczbą.", "Błąd");
                return;
            }

            var partSetElement = new DeliveryPartSetElement()
            {
                Part = part,
                Amount = amount,
            };

            _delivery.DeliveryPartSet.Add(partSetElement);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
                return;
            }

            _parentWindow.PartsDataGrid.ItemsSource = _delivery.DeliveryPartSet
                .OrderBy(el => el.Part.Name)
                .ToList();

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
            _parentWindow.PartsMenuItem.IsEnabled = false;
            _parentWindow.FillDetailedInfo(null);
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
            _parentWindow.ConnectMenuItem.IsEnabled = true;
        }
    }
}
