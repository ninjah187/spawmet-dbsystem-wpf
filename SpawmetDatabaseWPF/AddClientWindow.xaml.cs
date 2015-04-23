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
    /// Interaction logic for AddClientWindow.xaml
    /// </summary>
    public partial class AddClientWindow : Window
    {
        private ClientsWindow _parentWindow;
        private SpawmetDBContext _dbContext;

        public AddClientWindow()
        {
            InitializeComponent();
        }

        public AddClientWindow(ClientsWindow parentWindow, SpawmetDBContext dbContext)
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
            PhoneTextBox.GotFocus += TextBox_GotFocus;
            EmailTextBox.GotFocus += TextBox_GotFocus;
            NipTextBox.GotFocus += TextBox_GotFocus;
            AddressTextBox.GotFocus += TextBox_GotFocus;

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
            var phone = PhoneTextBox.Text;
            var email = EmailTextBox.Text;
            var nip = NipTextBox.Text;
            var address = AddressTextBox.Text;

            var client = new Client()
            {
                Name = name,
                Phone = phone,
                Email = email,
                Nip = nip,
                Address = address
            };

            _dbContext.Clients.Add(client);
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
