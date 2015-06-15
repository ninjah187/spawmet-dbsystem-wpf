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
    public partial class SearchDeliveriesWindow : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler<List<Delivery>> WorkCompleted;

        private SpawmetDBContext _dbContext;
        public string RegExpr { get; protected set; }

        public SearchDeliveriesWindow(Window owner, SpawmetDBContext dbContext)
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

            var deliveries = new List<Delivery>();

            foreach (var delivery in _dbContext.Deliveries)
            {
                if (Regex.IsMatch(delivery.Name, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(delivery.Date.ToShortDateString(), RegExpr, RegexOptions.IgnoreCase))
                {
                    deliveries.Add(delivery);
                }
            }

            OnWorkCompleted(deliveries);
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

        private void OnWorkCompleted(List<Delivery> deliveries)
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, deliveries);
            }
        }
    }
}
