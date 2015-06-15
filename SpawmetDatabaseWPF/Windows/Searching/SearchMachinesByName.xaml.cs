using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// Interaction logic for SearchMachinesByName.xaml
    /// </summary>
    public partial class SearchMachinesByName : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler<List<Machine>> WorkCompleted;

        private SpawmetDBContext _dbContext;
        private object _dbContextLock;
        public string RegExpr { get; private set; }

        public SearchMachinesByName(Window owner, SpawmetDBContext dbContext)
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

            var machines = new List<Machine>();

            foreach (var machine in _dbContext.Machines)
            {
                if (Regex.IsMatch(machine.Name, RegExpr, RegexOptions.IgnoreCase))
                {
                    machines.Add(machine);
                }
            }

            OnWorkCompleted(machines);
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

        private void OnWorkCompleted(List<Machine> machines)
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, machines);
            }
        }
    }
}
