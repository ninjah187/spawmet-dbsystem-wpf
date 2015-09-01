using System;
using System.CodeDom;
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
        //private BackgroundWorker _backgroundWorker;

        //public SaveFileWindow(IEnumerable<Machine> machines, string path)
        //{
        //    InitializeComponent();

        //    _backgroundWorker = new BackgroundWorker();
        //    _backgroundWorker.DoWork += (sender, e) =>
        //    {
        //        string extension = path.Split('.').Last();
        //        Creator creator;
        //        switch (extension)
        //        {
        //            case "docx":
        //                creator = new DocXCreator();
        //                break;

        //            case "pdf":
        //                creator = new PDFCreator();
        //                break;

        //            default:
        //                throw new InvalidOperationException("Incorrect file extension.");
        //        }
        //        creator.Create(machines, path);
        //    };
        //    _backgroundWorker.RunWorkerCompleted += (sender, e) =>
        //    {
        //        this.Close();
        //    };
            
        //    _backgroundWorker.RunWorkerAsync();
        //}

        public SaveFileWindow(IEnumerable<Machine> machines, string path)
        {
            InitializeComponent();

            SaveAsync(machines, path);
        }

        public SaveFileWindow(IEnumerable<Order> orders, string path)
        {
            InitializeComponent();

            SaveAsync(orders, path);
        }

        public SaveFileWindow(IEnumerable<Part> parts, string path)
        {
            InitializeComponent();

            ///try
            //{
                SaveAsync(parts, path);
            //}
            //catch (Exception exc)
            //{
            //    throw exc;
            //}
        }

        public async void SaveAsync(IEnumerable<Machine> machines, string path)
        {
            await Task.Run(() =>
            {
                var creator = PrepareCreator(path);
                creator.Create(machines, path);
            });

            this.Close();
        }

        public async void SaveAsync(IEnumerable<Order> orders, string path)
        {
            await Task.Run(() =>
            {
                var creator = PrepareCreator(path);
                creator.Create(orders, path);
            });

            this.Close();
        }

        public async void SaveAsync(IEnumerable<Part> parts, string path)
        {
            await Task.Run(() =>
            {
                var creator = PrepareCreator(path);
                creator.Create(parts, path);
            });

            this.Close();
        }

        private Creator PrepareCreator(string path)
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

            return creator;
        }
    }
}
