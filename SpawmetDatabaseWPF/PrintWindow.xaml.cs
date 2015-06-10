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
        private readonly BackgroundWorker _backgroundWorker;

        private readonly string _xpsPath;

        public PrintWindow(IEnumerable<Machine> machines, PrintDialog printDialog/*,
            Window parentWindow*/)
        {
            InitializeComponent();

            if (Directory.Exists(@".\temp") == false)
            {
                Directory.CreateDirectory(@".\temp");
            }

            // Create stream to get full path of .\temp.xps (it's needed in XpsDocument constructor).
            var stream = File.Open(@".\temp\print.xps", FileMode.Create);
            _xpsPath = stream.Name;
            stream.Close();
            stream.Dispose();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += (sender, e) =>
            {
                var xpsCreator = new XPSCreator();
                xpsCreator.Create(machines, _xpsPath);
            };
            _backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var xpsDocument = new XpsDocument(_xpsPath, FileAccess.ReadWrite);
                var fixedDocumentSequence = xpsDocument.GetFixedDocumentSequence();

                string description = machines.Count() == 1
                ? machines.First().Name
                : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                printDialog.PrintDocument(fixedDocumentSequence.DocumentPaginator, description);
                
                xpsDocument.Close();
                this.Close();
            };

            this.Loaded += (sender, e) =>
            {
                //parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _backgroundWorker.Dispose();
                //parentWindow.IsEnabled = true;
            };

            _backgroundWorker.RunWorkerAsync();
        }
    }
}
