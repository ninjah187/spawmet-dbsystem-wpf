using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AddPartToOrderWindow.xaml
    /// </summary>
    public partial class AddPartToOrderWindow : Window
    {
        public event EventHandler<AdditionalPartSetElement> PartAdded;

        //public ObservableCollection<Part> ListBoxItemsSource { get; set; }

        private readonly SpawmetDBContext _dbContext;
        private readonly Order _order;

        public AddPartToOrderWindow(SpawmetDBContext dbContext, Order order)
        {
            InitializeComponent();

            _dbContext = dbContext;
            _order = order;

            var parts = _dbContext.Parts.ToList();
            foreach (var additionalPartSetElement in _order.AdditionalPartSet)
            {
                parts.Remove(additionalPartSetElement.Part);
            }
            MainListBox.ItemsSource = parts.OrderBy(part => part.Name);

            this.Loaded += (sender, e) =>
            {
            };
            this.Closed += (sender, e) =>
            {
            };

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

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

            var partSetElement = new AdditionalPartSetElement()
            {
                Part = part,
                Amount = amount,
            };

            _order.AdditionalPartSet.Add(partSetElement);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
                return;
            }

            OnPartAdded(partSetElement);

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

        private void OnPartAdded(AdditionalPartSetElement partSetElement)
        {
            if (PartAdded != null)
            {
                PartAdded(this, partSetElement);
            }
        }
    }
}
