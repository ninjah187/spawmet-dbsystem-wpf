using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Microsoft.Win32;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Windows
{
    /// <summary>
    /// Interaction logic for PartsRaportWindow.xaml
    /// </summary>
    public partial class PartsRaportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Part> _parts;
        public List<Part> Parts
        {
            get { return _parts; }
            set
            {
                if (_parts != value)
                {
                    _parts = value;
                    OnPropertyChanged();
                }
            }
        }

        public PartsRaportWindow(List<Part> parts)
        {
            InitializeComponent();

            DataContext = this;

            Parts = parts;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SaveToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var parts = Parts;
            if (parts == null)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Plik Word 2007 (*.docx)|*.docx|Plik PDF (*.pdf)|*.pdf";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "Raport z magazynu, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

            if (saveFileDialog.ShowDialog() == true)
            {
                new SaveFileWindow(parts, saveFileDialog.FileName).Show();
            }
        }

        private void PrintButton_OnClick(object sender, RoutedEventArgs e)
        {
            var parts = Parts;
            if (parts == null)
            {
                return;
            }

            var printDialog = new PrintDialog();
            printDialog.PageRangeSelection = PageRangeSelection.AllPages;
            printDialog.UserPageRangeEnabled = false;
            printDialog.SelectedPagesEnabled = false;

            bool? print = printDialog.ShowDialog();
            if (print == true)
            {
                //var printWindow = new PrintWindow(selected, printDialog);
                var printWindow = new PrintWindow();
                printWindow.PrintAsync(parts, printDialog);
                printWindow.Show();
            }
        }
    }
}
