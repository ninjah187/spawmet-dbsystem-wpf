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
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddPartToMachine.xaml
    /// </summary>
    public partial class AddPartToMachine : Window, IDbContextChangesNotifier
    {
        public event EventHandler<StandardPartSetElement> PartAdded;

        public ObservableCollection<Part> ListBoxItemsSource { get; set; }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        //private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        //private readonly MachinesWindow _parentWindow;
        private readonly SpawmetDBContext _dbContext;
        private readonly Machine _machine;

        public AddPartToMachine(/*MachinesWindow parentWindow, */SpawmetDBContext dbContext, Machine machine)
        {
            InitializeComponent();

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];

            //_parentWindow = parentWindow;
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
            MainListBox.ItemsSource = parts.OrderBy(part => part.Name);

            this.Loaded += (sender, e) =>
            {
                //_parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                //_parentWindow.IsEnabled = true;
            };

            AmountTextBox.GotFocus += TextBox_GotFocus;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            MainListBox.SelectedIndex = 0;
            MainListBox.Focus();
        }

        private async void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var part = (Part) MainListBox.SelectedItem;
            if (part == null)
            {
                MessageBox.Show("Brak części.", "Błąd");
                return;
            }
            int amount;
            try
            {
                amount = int.Parse(AmountTextBox.Text);
            }
            catch (FormatException exc)
            {
                MessageBox.Show("Ilość musi być liczbą.", "Błąd");
                return;
            }

            var partSetElement = new StandardPartSetElement()
            {
                Part = part,
                Amount = amount,
            };

            var waitWin = new WaitWindow("Proszę czekać, trwa aktualizacja stanu magazynu...");
            waitWin.Show();

            await Task.Run(() =>
            {
                _machine.StandardPartSet.Add(partSetElement);
                foreach (var order in _machine.Orders)
                {
                    if (order.Status == OrderStatus.InProgress ||
                        order.Status == OrderStatus.Done)
                    {
                        partSetElement.Part.Amount -= partSetElement.Amount;
                    }
                }
                _dbContext.SaveChanges();
            });

            waitWin.Close();

            //_machine.StandardPartSet.Add(partSetElement);
            //try
            //{
            //    _dbContext.SaveChanges();
            //}
            //catch (System.Data.Entity.Core.EntityException exc)
            //{
            //    Disconnected();
            //    return;
            //}

            //_parentWindow.StandardPartSetDataGrid.ItemsSource = _machine.StandardPartSet
            //    .OrderBy(el => el.Part.Name)
            //    .ToList();

            OnPartAdded(partSetElement);

            DbContextMediator.NotifyContextChanged(this);

            //foreach (var order in _machine.Orders)
            //{
                
            //}

            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox) sender).SelectAll();
        }

        private void Disconnected()
        {
            //_parentWindow.MainDataGrid.IsEnabled = false;
            //_parentWindow.DetailsStackPanel.IsEnabled = false;
            //_parentWindow.PartsMenuItem.IsEnabled = false;
            //_parentWindow.FillDetailedInfo(null);
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
            //_parentWindow.ConnectMenuItem.IsEnabled = true;
        }

        private void OnPartAdded(StandardPartSetElement part)
        {
            if (PartAdded != null)
            {
                PartAdded(this, part);
            }
        }
    }
}
