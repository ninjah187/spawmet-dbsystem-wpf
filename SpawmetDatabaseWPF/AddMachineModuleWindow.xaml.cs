using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
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
    /// Interaction logic for AddMachineModuleWindow.xaml
    /// </summary>
    public partial class AddMachineModuleWindow : Window, IDbContextChangesNotifier
    {
        public DbContextMediator Mediator { get; set; }

        private int _targetId;

        public AddMachineModuleWindow(int machineId)
        {
            InitializeComponent();

            _targetId = machineId;

            Mediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];

            NameTextBox.Focus();

            this.KeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        AddModuleAsync();
                        break;

                    case Key.Escape:
                        Close();
                        break;
                }
            };
        }

        private async void AddButtonOnClick(object sender, RoutedEventArgs e)
        {
            await AddModuleAsync();
        }

        private async Task AddModuleAsync()
        {
            var name = NameTextBox.Text;
            if (name.Length == 0)
            {
                MessageWindow.Show("Brak nazwy.", "Błąd");
                return;
            }

            var task = Task.Run(() =>
            {
                var module = new MachineModule()
                {
                    Name = name,
                };

                using (var context = new SpawmetDBContext())
                {
                    //try
                    //{
                        var machine = context.Machines.Single(m => m.Id == _targetId);

                        module.Machine = machine;

                        context.MachineModules.Add(module);
                        context.SaveChanges();
                    //}
                    //catch (EntityException exc)
                    //{
                    //    MessageWindow.Show("Błąd połączenia." + exc.Message + "\ninner exception: " + exc.InnerException.Message, "Błąd");
                    //}
                }
            });

            IsEnabled = false;

            await task;

            Mediator.NotifyContextChange(this);

            Close();
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox) sender).SelectAll();
        }
    }
}
