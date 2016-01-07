using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
using System.Threading;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for ConnectingWindow.xaml
    /// </summary>
    public partial class ConnectingWindow : Window
    {
        public ConnectingWindow()
        {
            InitializeComponent();

            var thread = new Thread(() =>
            {
                try
                {
                    new MachinesWindow().Show();
                }
                catch (EntityException exc)
                {
                    Disconnected();
                    throw exc;
                }
            });
            thread.Start();
        }

        private void Disconnected()
        {
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
        }
    }
}
