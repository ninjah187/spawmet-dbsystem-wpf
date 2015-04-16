using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AddPartToMachine.xaml
    /// </summary>
    public partial class AddPartToMachine : Window
    {
        public ObservableCollection<Part> ListBoxItemsSource { get; set; }

        private readonly MachinesWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly Machine _machine;

        public AddPartToMachine(MachinesWindow parentWindow, SpawmetDBContext dbContext, Machine machine)
        {
            InitializeComponent();

            _parentWindow = parentWindow;
            _dbContext = dbContext;
            _machine = machine;

            #region
            //MainListBox.ItemsSource = _dbContext.Parts.Local;
            //using (var context = new SpawmetDBContext())
            //{
            //    Func<Part, bool> predicate = (Part p) =>
            //    {
            //        foreach (var mach in p.Machines.ToList())
            //        {
            //            if (mach.Id == machine.Id)
            //            {
            //                return false;
            //            }
            //        }
            //        return true;
            //        //return p.Machines.All(mach => mach.Id != machine.Id);
            //    };
            //    var parts = context.Parts.Where(predicate).OrderBy(p => p.Id).ToList();
            //    MainListBox.ItemsSource = parts;
            //}
            //Func<Part, bool> wherePredicate = (Part part) =>
            //{
            //    return part.Machines.ToList().All(mach => mach.Id != machine.Id);
            //};
            //var parts = _dbContext.Parts.Where(wherePredicate).OrderBy(part => part.Id).ToList();
            #endregion
            var parts = _dbContext.Parts.ToList();
            foreach (var standardPartSetElement in _machine.StandardPartSet)
            {
                parts.Remove(standardPartSetElement.Part);
            }
            //parts = parts.OrderBy(part => part.Id).ToList();
            MainListBox.ItemsSource = parts.OrderBy(part => part.Id);

            this.Loaded += (sender, e) =>
            {
                _parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _parentWindow.IsEnabled = true;
            };
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var part = (Part) MainListBox.SelectedItem;
            int amount;
            try
            {
                amount = int.Parse(AmountTextBox.Text);
            }
            catch (FormatException exc)
            {
                AmountTextBox.Text = "Wpisz wartość liczbową.";
                return;
            }

            var partSetElement = new StandardPartSetElement()
            {
                Part = part,
                Amount = amount,
            };

            _machine.StandardPartSet.Add(partSetElement);
            _dbContext.SaveChanges();

            _parentWindow.StandardPartSetDataGrid.ItemsSource = _machine.StandardPartSet.OrderBy(element => element.Part.Id);

            this.Close();
        }
    }
}
