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
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddMachineWindow.xaml
    /// </summary>
    public partial class AddMachineWindow : Window
    {
        private MachinesWindow _parentWindow;
        private SpawmetDBContext _dbContext;

        public AddMachineWindow(MachinesWindow parentWindow, SpawmetDBContext dbContext)
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
            PriceTextBox.GotFocus += TextBox_GotFocus;

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
            Decimal price = 0;
            try
            {
                price = Decimal.Parse(PriceTextBox.Text);
            }
            catch (FormatException exc)
            {
                MessageBox.Show("Cena musi być liczbą.", "Błąd");
                return;
            }

            var machine = new Machine()
            {
                Name = name,
                Price = price,
            };

            _dbContext.Machines.Add(machine);
            _dbContext.SaveChanges();

            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox) sender).SelectAll();
        }
    }
}
