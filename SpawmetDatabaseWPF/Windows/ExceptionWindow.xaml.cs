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

namespace SpawmetDatabaseWPF.Windows.Common
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        private ExceptionWindow(Exception exception)
        {
            InitializeComponent();

            var exc = exception;
            bool firstRun = true;
            while (exc != null)
            {
                //string msg = "";
                //msg = "Exception type: " + exc.GetType().ToString() + "\n";
                //msg += "Exception message: " + exc.Message + "\n";
                //msg += "To string: " + exc.ToString() + "\n";
                //msg += "\n";

                var msg = "";
                if (firstRun == false)
                {
                    msg += "Inner exception:\n";
                }
                else
                {
                    firstRun = false;
                }
                msg += exc.ToString();

                ExceptionInfoTextBox.Text = msg + "\n";

                exc = exc.InnerException;
            }
        }

        public static void Show(Exception exception, Window owner = null)
        {
            var win = new ExceptionWindow(exception);
            win.Owner = owner;
            win.Focus();
            win.ShowDialog();
        }

        private void FillExceptionInfo(Exception exception)
        {
            
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
