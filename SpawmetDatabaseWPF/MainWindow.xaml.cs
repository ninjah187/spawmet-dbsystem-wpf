using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public ObservableCollection<Part> DataGridItemsSource { get; set; }
        public ObservableCollection<Part> DataGridItemsSource
        {
            get { return _dbContext.Parts.Local; }
        }

        private SpawmetDBContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Initialize();
            }
            catch (ProviderIncompatibleException e)
            {
                MessageBox.Show("Brak połączenia z serwerem.");
                Application.Current.Shutdown();
            }
        }

        private void Initialize()
        {
            _dbContext = new SpawmetDBContext();

            this.DataContext = this;

            MainDataGrid.SelectionChanged += (sender, e) =>
            {
                var part = (Part)MainDataGrid.SelectedItem;
                if (part != null)
                {
                    FillDetailedInfo(part);
                }
            };

            this.Loaded += (sender, e) =>
            {
                LoadDataIntoSource();
            };
            this.Closed += (sender, e) =>
            {
                _dbContext.Dispose();
            };
        }

        private void FillDetailedInfo(Part part)
        {
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
                string txt = delivery.Name + ", " + delivery.Date;
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;
            
            switch (menuItem.Header.ToString())
            {
                case "Części":
                {
                    LoadDataIntoSource();
                }
                break;
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPartWindow(this, _dbContext);
            addWindow.Show();

        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var items = MainDataGrid.SelectedItems;
            var toDelete = new List<Part>();
            foreach (var item in items)
            {
                var part = (Part)item;
                toDelete.Add(part);
            }
            foreach (var part in toDelete)
            {
                _dbContext.Parts.Remove(part);
            }
            _dbContext.SaveChanges();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            _dbContext.SaveChanges();
        }

        private void DetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var part = (Part)MainDataGrid.SelectedItem;
            if (part != null)
            {
                FillDetailedInfo(part);
            }
        }
    }
}
