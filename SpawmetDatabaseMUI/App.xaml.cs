using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SpawmetDatabase.Model;
using SpawmetDatabaseMUI.Services;
using SpawmetDatabaseMUI.ViewModel;

namespace SpawmetDatabaseMUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (EntityService.Instance.Context.Machines.Count() == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    EntityService.Instance.Context.Machines.Add(new Machine()
                    {
                        Name = "maszyna " + (i + 1)
                    });
                }
                EntityService.Instance.Context.SaveChanges();
            }

            var machinesWindow = new MachinesWindow(new MachinesViewModel(MachineEntityService.Instance));
            machinesWindow.Show();
        }
    }
}
