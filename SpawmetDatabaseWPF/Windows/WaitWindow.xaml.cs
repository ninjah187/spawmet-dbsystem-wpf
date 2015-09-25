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

namespace SpawmetDatabaseWPF.Windows
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window
    {
        private readonly WindowsEnablementController _winController;

        public WaitWindow(string message)
            : this(message, "Wykonywanie operacji")
        {
        }

        public WaitWindow(string message, string title)
        {
            InitializeComponent();

            MessageTextBlock.Text = message;
            Title = title;

            _winController = new WindowsEnablementController();

            Loaded += delegate
            {
                _winController.DisableWindows();
            };

            Closed += delegate
            {
                _winController.EnableWindows();
            };
        }
    }
}
