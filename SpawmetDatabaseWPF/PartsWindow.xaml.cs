using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for PartsWindow.xaml
    /// </summary>
    public partial class PartsWindow : Window
    {
        public ObservableCollection<Part> DataGridItemsSource
        {
            get { return _dbContext.Parts.Local; }
        }

        private SpawmetDBContext _dbContext;

        private MasterWindow _parentWindow;

        public PartsWindow(MasterWindow parentWindow)
        {
            InitializeComponent();

            _parentWindow = parentWindow;

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                MessageBox.Show("Brak połączenia z serwerem");
                Application.Current.Shutdown();
            }
        }

        private void Initialize()
        {
            _dbContext = new SpawmetDBContext();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                Part part = null;
                try
                {
                    part = (Part) MainDataGrid.SelectedItem;
                }
                catch (InvalidCastException exc)
                {
                    DetailTextBlock.Text = "";
                    return;
                }
                if (part != null)
                {
                    FillDetailedInfo(part);
                }
            };

            this.Loaded += (sender, e) =>
            {
                LoadDataIntoSource();
                _parentWindow.PartsWindowButton.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
                _parentWindow.PartsWindowButton.IsEnabled = true;
            };
        }

        private void FillDetailedInfo(Part part)
        {
            //string basicInfo = "";
            //string originName = part.Origin == Origin.Production ? "Produkcja" : "Zewnątrz";

            //basicInfo += "ID: " + part.Id +
            //             "\nNazwa: " + part.Name +
            //             "\nIlość: " + part.Amount +
            //             "\nPochodzenie: " + originName;
            
            //BasicInfoTextBlock.Text = basicInfo;
            //MachinesListBox.ItemsSource = part.Machines;
            //DeliveriesListBox.ItemsSource = part.Deliveries;

            string info = "";

            string originName = part.Origin == Origin.Production ? "Produkcja" : "Zewnątrz";

            info += "ID: " + part.Id +
                    "\nNazwa: " + part.Name +
                    "\nIlość: " + part.Amount +
                    "\nPochodzenie: " + originName +
                    "\n";
            info += "Maszyny:\n";
            foreach (var machine in part.Machines)
            {
                info += "- " + machine.Name + "\n";
            }
            info += "Dostawy:\n";
            foreach (var delivery in part.Deliveries)
            {
                string txt = delivery.Name + ", " + delivery.Date.ToShortDateString();
                info += "- " + txt + "\n";
            }
            info += "Zamówienia:\n";
            foreach (var order in part.Orders)
            {
                string clientName = order.Client != null ? order.Client.Name : "";
                string machineName = order.Machine != null ? order.Machine.Name : "";

                string txt = clientName + ", " + machineName + ", " +
                             order.StartDate.ToShortDateString();
                info += "- " + txt + "\n";
            }

            DetailTextBlock.Text = info;
        }

        private void LoadDataIntoSource()
        {
            if (DataGridItemsSource.Any())
            {
                DataGridItemsSource.Clear();
            }

            foreach (var part in _dbContext.Parts)
            {
                DataGridItemsSource.Add(part);
            }
        }

        private void AddButton_OnClicklick(object sender, RoutedEventArgs e)
        {
            new AddPartWindow(this, _dbContext).Show();
        }

        private void DeleteButton_OnClickutton_OnClick(object sender, RoutedEventArgs e)
        {
            var items = MainDataGrid.SelectedItems;
            var toDelete = new List<Part>();
            foreach (var item in items)
            {
                toDelete.Add((Part)item);
            }
            foreach (var deleteItem in toDelete)
            {
                _dbContext.Parts.Remove(deleteItem);
            }
            _dbContext.SaveChanges();
        }

        private void SaveButton_OnClicknClick(object sender, RoutedEventArgs e)
        {
            _dbContext.SaveChanges();
        }
    }
}
