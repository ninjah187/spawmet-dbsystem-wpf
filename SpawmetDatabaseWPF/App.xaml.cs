using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SpawmetDatabaseWPF.Utilities;
using SpawmetDatabaseWPF.Windows.Common;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Properties["DbContextMediator"] = new DbContextMediator();
            //Properties["WindowsEnablementController"] = new WindowsEnablementController();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                ExceptionWindow.Show((Exception)e.ExceptionObject);
            };
            //DispatcherUnhandledException += (sender, e) =>
            //{
            //    ExceptionWindow.Show(e.Exception);
            //};
        }

        public void EnableWindows()
        {
            foreach (var win in Application.Current.Windows)
            {
                var window = (Window) win;
                window.IsEnabled = true;
            }
        }

        public void DisableWindows()
        {
            foreach (var win in Application.Current.Windows)
            {
                var window = (Window)win;
                window.IsEnabled = false;
            }
        }
    }
}
