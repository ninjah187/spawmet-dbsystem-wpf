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
    /// Interaction logic for SearchPartsWindow.xaml
    /// </summary>
    public partial class SearchPartsWindow : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler<List<Part>> WorkCompleted;

        private SpawmetDBContext _dbContext;
        public string RegExpr { get; private set; }

        public SearchPartsWindow(Window owner, SpawmetDBContext dbContext)
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

            var parts = new List<Part>();

            foreach (var part in _dbContext.Parts)
            {
                if (Regex.IsMatch(part.Name, RegExpr, RegexOptions.IgnoreCase))
                {
                    parts.Add(part);
                }
            }

            OnWorkCompleted(parts);
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

        private void OnWorkCompleted(List<Part> parts)
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, parts);
            }
        }
    }
}
