using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Events;
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.ViewModel;
using Application = System.Windows.Application;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for CraftPartWindow.xaml
    /// </summary>
    public partial class CraftPartWindow : Window, IDbContextChangesNotifier
    {
        public event EventHandler WorkStarted;
        public event EventHandler WorkCompleted;

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private Type[] _contextChangeInfluencedTypes =
        {
            typeof(MachinesWindowViewModel), typeof(PartsWindowViewModel),
            typeof(OrdersWindowViewModel), typeof(MachineModuleDetailsWindow)
        };

        private SpawmetDBContext _dbContext;
        private Part _part;

        private WindowsEnablementController _winEnablementController;

        public CraftPartWindow(Part part)
        {
            InitializeComponent();

            _dbContext = new SpawmetDBContext();
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

            _winEnablementController = new WindowsEnablementController(this);
            _winEnablementController.DisableWindows();

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);
            Closed += delegate
            {
                _dbContext.Dispose();

                DbContextMediator.Subscribers.Remove(this);

                _winEnablementController.EnableWindows();
            };
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
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

            OnWorkStarted();
            await AddPartAsync(amount);
            OnWorkCompleted();
            
            DbContextMediator.NotifyContextChanged(this, _contextChangeInfluencedTypes);

            Close();

            //string txt = "Wypalono: " + element.Part.Name + "\nIlość: " + element.Amount;
            //MessageWindow.Show(txt, "Wypalono część", _window);
        }

        private async Task AddPartAsync(int amount)
        {
            await Task.Run(() =>
            {
                var part = _dbContext.Parts.Single(p => p.Id == _part.Id);

                part.Amount += amount;
                _dbContext.SaveChanges();
            });
        }

        private void OnWorkStarted()
        {
            if (WorkStarted != null)
            {
                WorkStarted(this, EventArgs.Empty);
            }
        }

        private void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }
    }
}
