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


        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            var price = Decimal.Parse(PriceTextBox.Text);

            var machine = new Machine()
            {
                Name = name,
                Price = price,
            };

            _dbContext.Machines.Add(machine);
            _dbContext.SaveChanges();
        }
    }
}
