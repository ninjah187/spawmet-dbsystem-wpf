using FirstFloor.ModernUI.Windows.Controls;
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
using SpawmetDatabaseMUI.ViewModel;

namespace SpawmetDatabaseMUI
{
    /// <summary>
    /// Interaction logic for MachinesWindow.xaml
    /// </summary>
    public partial class MachinesWindow : ModernWindow
    {
        public MachinesWindow(MachinesViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
