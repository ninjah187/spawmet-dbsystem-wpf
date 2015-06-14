using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace SpawmetDatabaseWPF.CommonWindows
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        private MessageWindow(string message, Window owner)
            : this(message, "", owner)
        {
        }

        private MessageWindow(string message, string title, Window owner)
        {
            InitializeComponent();
            
            MessageTextBlock.Text = message;

            if (title != "")
            {
                this.Title = title;
            }

            this.Owner = owner;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ConfirmButton.Click += delegate
            {
                this.Close();
            };
            
            this.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    this.Close();
                }
            };
        }

        public static void Show(string message, Window owner)
        {
            Show(message, "", owner);
        }

        public static void Show(string message, string title, Window owner)
        {
            var win = new MessageWindow(message, title, owner);
            win.Focus();
            win.ShowDialog();
        }
    }
}
