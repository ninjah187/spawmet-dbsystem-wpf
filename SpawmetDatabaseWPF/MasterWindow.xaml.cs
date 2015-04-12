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

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for MasterWindow.xaml
    /// </summary>
    public partial class MasterWindow : Window
    {
        private List<Window> _childrenWindows;

        public MasterWindow()
        {
            InitializeComponent();

            _childrenWindows = new List<Window>();

            this.Closed += (sender, e) =>
            {
                foreach (var window in _childrenWindows)
                {
                    window.Close();
                }
            };
        }

        private void PartsWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new PartsWindow(this);
            _childrenWindows.Add(window);
            window.Show();
        }

        private void MachinesWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new MachinesWindow(this);
            _childrenWindows.Add(window);
            window.Show();
        }
    }
}
