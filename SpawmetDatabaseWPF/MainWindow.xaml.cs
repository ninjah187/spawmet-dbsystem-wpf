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
        public ObservableCollection<Part> DataGridItemsSource { get; set; }

        //trzymać jeden context przez cały czas trwania programu, czy tworzyć do każdorazowego działania?

        public MainWindow()
        {
            InitializeComponent();

            DataGridItemsSource = new ObservableCollection<Part>();

            this.DataContext = this;

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;
            
            switch (menuItem.Header.ToString())
            {
                case "Części":
                {
                    using (var context = new SpawmetDBContext())
                    {
                        foreach (var part in context.Parts)
                        {
                            DataGridItemsSource.Add(part);
                        }

                        //var parts = context.Parts.Cast<IModelElement>().ToList();
                        //DataGridItemsSource = new ObservableCollection<IModelElement>(parts);
                    }
                }
                break;
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPartWindow(this);
            addWindow.Show();
        }
    }
}
