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
    /// Interaction logic for SearchOrdersWindow.xaml
    /// </summary>
    public partial class SearchOrdersWindow : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler<List<Order>> WorkCompleted;

        private SpawmetDBContext _dbContext;
        public string RegExpr { get; private set; }

        public SearchOrdersWindow(Window owner, SpawmetDBContext dbContext)
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

            var orders = new List<Order>();

            foreach (var order in _dbContext.Orders)
            {
                if (Regex.IsMatch(order.Signature, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(order.StatusDescription, RegExpr, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(order.SendDate.Value.ToShortDateString(), RegExpr, RegexOptions.IgnoreCase))
                {
                    orders.Add(order);
                }
            }

            OnWorkCompleted(orders);
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

        private void OnWorkCompleted(List<Order> orders)
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, orders);
            }
        }
    }
}
