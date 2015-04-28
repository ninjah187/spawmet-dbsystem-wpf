using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for InitialWindow.xaml
    /// </summary>
    public partial class InitialWindow : Window
    {
        public InitialWindow()
        {
            InitializeComponent();

            //this.Loaded += (sender, e) =>
            //{
            //    try
            //    {
            //        new MachinesWindow(0, 0).Show();
            //    }
            //    catch (EntityException exc)
            //    {
            //        throw exc;
            //    }
            //};

            //using (var backgroundWorker = new BackgroundWorker())
            //{
            //    backgroundWorker.DoWork += (sender, e) =>
            //    {
            //        //var window = (MachinesWindow) e.Argument;
            //        var window = new MachinesWindow(0, 0);
            //        e.Result = window;
            //    };
            //    backgroundWorker.RunWorkerCompleted += (sender, e) =>
            //    {
            //        var window = (MachinesWindow) e.Result;
            //        window.Show();
            //    };
            //    //MachinesWindow window;
            //    backgroundWorker.RunWorkerAsync();
            //}

            var thread = new Thread(() =>
            {
                var window = new MachinesWindow(40, 40);
                window.Loaded += (sender, e) =>
                {
                    //Application.Current.Dispatcher.Invoke(() => Close());
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                };
                window.Show();

                window.Closed += (sender, e) =>
                {
                    window.Dispatcher.InvokeShutdown();
                };

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
