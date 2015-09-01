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

namespace SpawmetDatabaseWPF.CommonWindows
{
    /// <summary>
    /// Interaction logic for ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        public event EventHandler Confirmed;
        public event EventHandler Declined;
        //opcjonalnie mozna zrobic metode bool Show(), która zwraca true,
        //jeśli użytkownik nacisnął tak

        public ConfirmWindow(string message, Window owner = null)
            : this(message, "Potwierdzenie", owner)
        {
        }

        public ConfirmWindow(string message, string title, Window owner = null)
        {
            InitializeComponent();

            this.Owner = owner;

            MessageTextBlock.Text = message;

            this.Title = title;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.KeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        OnConfirmed();
                        break;

                    case Key.Escape:
                        OnDeclined();
                        break;
                }
            };
        }

        public new void Show()
        {
            this.ShowDialog();
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnConfirmed();
        }

        private void DeclineButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnDeclined();
        }

        private void OnConfirmed()
        {
            Close();
            if (Confirmed != null)
            {
                Confirmed(this, EventArgs.Empty);
            }
        }

        private void OnDeclined()
        {
            Close();
            if (Declined != null)
            {
                Declined(this, EventArgs.Empty);
            }
        }
    }
}
