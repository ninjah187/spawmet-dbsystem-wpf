using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
    /// Interaction logic for AddPartWindow.xaml
    /// </summary>
    public partial class AddPartWindow : Window
    {
        private PartsWindow _parentWindow;
        private SpawmetDBContext _dbContext;

        public AddPartWindow(PartsWindow parentWindow, SpawmetDBContext dbContext)
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
            AmountTextBox.GotFocus += TextBox_GotFocus;

            NameTextBox.Focus();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            if (name.Length == 0)
            {
                MessageBox.Show("Brak nazwy.", "Błąd");
                return;
            }
            int amount = 0;
            try
            {
                amount = int.Parse(AmountTextBox.Text);
            }
            catch (FormatException exc)
            {
                MessageBox.Show("Ilość musi być liczbą.", "Błąd");
                return;
            }
            var origin = (Origin) OriginComboBox.SelectedIndex;

            var part = new Part()
            {
                Name = name,
                Amount = amount,
                Origin = origin,
            };
            _dbContext.Parts.Add(part);
            _dbContext.SaveChanges();

            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
    }
}
