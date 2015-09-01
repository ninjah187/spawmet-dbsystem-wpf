using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SpawmetDatabase.Model;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Windows.Xps.Packaging;
using SpawmetDatabase.FileCreators;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintWindow : Window
    {
        private string _path;

        public PrintWindow()
        {
            InitializeComponent();
        }

        protected void PrepareXpsFile()
        {
            if (Directory.Exists(@".\temp") == false)
            {
                Directory.CreateDirectory(@".\temp");
            }

            var stream = File.Open(@".\temp\print.xps", FileMode.Create);
            _path = stream.Name;
            stream.Close();
            stream.Dispose();
        }

        public async Task PrintAsync(IEnumerable<Machine> machines, PrintDialog printDialog)
        {
            await Task.Run(() =>
            {
                PrepareXpsFile();

                var xpsCreator = new XPSCreator();
                xpsCreator.Create(machines, _path);
            });

            var xpsDocument = new XpsDocument(_path, FileAccess.ReadWrite);
            var fixedDocumentSequence = xpsDocument.GetFixedDocumentSequence();

            string description = machines.Count() == 1
                ? machines.First().Name
                : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

            printDialog.PrintDocument(fixedDocumentSequence.DocumentPaginator, description);

            xpsDocument.Close();

            this.Close();
        }

        public async Task PrintAsync(IEnumerable<Order> orders, PrintDialog printDialog)
        {
            await Task.Run(() =>
            {
                PrepareXpsFile();

                var xpsCreator = new XPSCreator();
                xpsCreator.Create(orders, _path);
            });

            var xpsDocument = new XpsDocument(_path, FileAccess.ReadWrite);
            var fixedDocumentSequence = xpsDocument.GetFixedDocumentSequence();

            string description = orders.Count() == 1
                ? orders.First().Machine.Name
                : "Wykaz zamówień, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

            printDialog.PrintDocument(fixedDocumentSequence.DocumentPaginator, description);

            xpsDocument.Close();

            this.Close();
        }

        public async Task PrintAsync(IEnumerable<Part> parts, PrintDialog printDialog)
        {
            await Task.Run(() =>
            {
                PrepareXpsFile();

                var xpsCreator = new XPSCreator();
                xpsCreator.Create(parts, _path);
            });

            var xpsDocument = new XpsDocument(_path, FileAccess.ReadWrite);
            var fixedDocumentSequence = xpsDocument.GetFixedDocumentSequence();

            string description = "Raport z magazynu";

            printDialog.PrintDocument(fixedDocumentSequence.DocumentPaginator, description);

            xpsDocument.Close();

            this.Close();
        }
    }
}
