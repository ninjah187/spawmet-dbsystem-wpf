using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SpawmetDatabaseWPF.Windows.Searching
{
    /// <summary>
    /// Interaction logic for SearchClientsWindow.xaml
    /// </summary>
    public partial class SearchClientsWindow : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler<List<Client>> WorkCompleted;

        private SpawmetDBContext _dbContext;
        public string RegExpr { get; private set; }

        public SearchClientsWindow(Window owner, SpawmetDBContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext;

            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            KeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        Search();
                        break;
                    case Key.Escape:
                        this.Close();
                        break;
                }
            };

            RegexTextBox.Focus();
        }

        public void Search()
        {
            OnWorkStarted();

            RegExpr = RegexTextBox.Text;

            var clients = new List<Client>();

            foreach (var client in _dbContext.Clients)
            {
                if (Regex.IsMatch(client.Name, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(client.Phone, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(client.Email, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(client.Address, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(client.Nip, RegExpr, RegexOptions.IgnoreCase))
                {
                    clients.Add(client);
                }
            }

            OnWorkCompleted(clients);
        }

        public new void Show()
        {
            this.ShowDialog();
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void OnWorkStarted()
        {
            if (WorkStarted != null)
            {
                WorkStarted(this, EventArgs.Empty);
            }
        }

        private void OnWorkCompleted(List<Client> clients)
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, clients);
            }
        }
    }
}
