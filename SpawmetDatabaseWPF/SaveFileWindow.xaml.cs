using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
using SpawmetDatabase.FileCreators;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for SaveFileWindow.xaml
    /// </summary>
    public partial class SaveFileWindow : Window
    {
        private BackgroundWorker _backgroundWorker;

        public SaveFileWindow(IEnumerable<Machine> machines, string path, Window parentWindow)
        {
            InitializeComponent();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += (sender, e) =>
            {
                string extension = path.Split('.').Last();
                Creator creator;
                switch (extension)
                {
                    case "docx":
                        creator = new DocXCreator();
                        break;

                    case "pdf":
                        creator = new PDFCreator();
                        break;

                    default:
                        throw new InvalidOperationException("Incorrect file extension.");
                }
                creator.Create(machines, path);
            };
            _backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                this.Close();
            };

            this.Loaded += (sender, e) =>
            {
                parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _backgroundWorker.Dispose();
                parentWindow.IsEnabled = true;
            };

            _backgroundWorker.RunWorkerAsync();
        }
    }
}
