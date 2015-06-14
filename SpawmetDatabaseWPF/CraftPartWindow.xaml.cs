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
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Events;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for CraftPartWindow.xaml
    /// </summary>
    public partial class CraftPartWindow : Window
    {
        public event PartCraftedEventHandler PartCrafted;

        private SpawmetDBContext _dbContext;
        private Part _part;

        public CraftPartWindow(SpawmetDBContext context, Part part)
        {
            InitializeComponent();

            _dbContext = context;
            _part = part;

            PartTextBlock.Text = _part.Name;

            this.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    ButtonBase_OnClick(this, null);
                }
            };

            AmountTextBox.Focus();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var amountStr = AmountTextBox.Text;

            if (amountStr.Length == 0)
            {
                AmountTextBox.Text = "Ilość musi być liczbą.";
                return;
            }

            int amount = -1;
            try
            {
                amount = int.Parse(amountStr);
            }
            catch (FormatException)
            {
                AmountTextBox.Text = "Ilość musi być liczbą.";
                return;
            }

            _part.Amount += amount;
            _dbContext.SaveChanges();
            
            Close();

            OnPartCrafted(_part, amount);
        }

        private void OnPartCrafted(Part part, int amount)
        {
            if (PartCrafted != null)
            {
                var args = new PartCraftedEventArgs(part, amount);
                PartCrafted(this, args);
            }
        }
    }
}
