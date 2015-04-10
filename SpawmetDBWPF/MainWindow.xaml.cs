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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDBWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpawmetDBContext _dbContext;
        private List<Part> _itemsSource;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            _dbContext = new SpawmetDBContext();
            
            _itemsSource = new List<Part>();


        }
    }
}
