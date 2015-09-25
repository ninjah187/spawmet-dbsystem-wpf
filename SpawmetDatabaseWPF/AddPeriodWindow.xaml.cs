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

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddPeriodWindow.xaml
    /// </summary>
    public partial class AddPeriodWindow : Window, IDbContextChangesNotifier
    {
        public DbContextMediator Mediator { get; set; }

        public AddPeriodWindow()
        {
            InitializeComponent();

            Mediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];

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

            Mediator.NotifyContextChange(this);

            Close();
        }
    }
}
