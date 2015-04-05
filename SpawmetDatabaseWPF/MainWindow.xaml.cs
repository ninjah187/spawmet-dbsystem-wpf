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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (var context = new SpawmetDBContext())
            {
                MainDataGrid.ItemsSource = context.Orders.ToList();
            }

            //using (var context = new SpawmetDBContext())
            //{
            //    MainTextBlock.Text = "";

            //    foreach (var order in context.Orders.ToList())
            //    {
            //        var clientName = order.Client != null ? order.Client.Name : "";
            //        var machineName = order.Machine != null ? order.Machine.Name : "";

            //        MainTextBlock.Text +=
            //            "Id zamówienia: " + order.Id +
            //            "\nNazwa klienta: " + clientName +
            //            "\nData złożenia: " + order.StartDate +
            //            "\nData wysyłki: " + order.SendDate +
            //            "\nUwagi: " + order.Remarks +
            //            "\nNazwa maszyny: " + machineName +
            //            "\nPodstawowe części: " + "\n";
            //        if (order.Machine != null)
            //        {
            //            foreach (var part in order.Machine.StandardPartSet)
            //            {
            //                MainTextBlock.Text += "- " + part.Name + "\n";
            //            }
            //        }
            //        MainTextBlock.Text += "Dodatkowe części: " + "\n";
            //        foreach (var part in order.AdditionalPartSet)
            //        {
            //            MainTextBlock.Text += "- " + part.Name + "\n";
            //        }
            //        MainTextBlock.Text += "\n";
            //    }
            //}

        }

    }
}
