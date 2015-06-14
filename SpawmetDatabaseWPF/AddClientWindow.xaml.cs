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
        public event EventHandler<Client> ClientAdded;

        private SpawmetDBContext _dbContext;

        public AddClientWindow()
        {
            InitializeComponent();
        }

        public AddClientWindow(SpawmetDBContext dbContext)
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

            OnClientAdded(client);

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

        private void OnClientAdded(Client client)
        {
            if (ClientAdded != null)
            {
                ClientAdded(this, client);
            }
        }
    }
}
