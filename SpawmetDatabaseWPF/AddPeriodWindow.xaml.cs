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
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Utilities;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddPeriodWindow.xaml
    /// </summary>
    public partial class AddPeriodWindow : Window, IDbContextChangesNotifier
    {
        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }
        private readonly Type[] _contextChangeInfluencedTypes = { typeof(OrdersWindow) };

        public AddPeriodWindow()
        {
            InitializeComponent();

            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];

            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AddPeriodAsync();
        }

        private async Task AddPeriodAsync()
        {
            var start = StartDatePicker.SelectedDate;
            var end = EndDatePicker.SelectedDate;

            if (start == null || end == null)
            {
                MessageWindow.Show("Musisz wybrać daty.", "Błąd");
                return;
            }

            IsEnabled = false;
            await Task.Run(() =>
            {
                var period = new Period()
                {
                    Start = start.Value,
                    End = end.Value
                };

                using (var context = new SpawmetDBContext())
                {
                    context.Periods.Add(period);
                    context.SaveChanges();
                }
            });

            DbContextMediator.NotifyContextChanged(this, _contextChangeInfluencedTypes);

            Close();
        }
    }
}
