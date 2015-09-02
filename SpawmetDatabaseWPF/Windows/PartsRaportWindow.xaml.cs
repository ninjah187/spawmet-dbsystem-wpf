using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Windows
{
    /// <summary>
    /// Interaction logic for PartsRaportWindow.xaml
    /// </summary>
    public partial class PartsRaportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _withNewOrders;
        public bool WithNewOrders
        {
            get { return _withNewOrders; }
            set
            {
                if (_withNewOrders != value)
                {
                    _withNewOrders = value;
                    OnPropertyChanged();
                    LoadAsync();
                }
            }
        }

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

        //public PartsRaportWindow(List<Part> parts)
        //{
        //    InitializeComponent();

        //    DataContext = this;

        //    Parts = parts;
        //}

        private SpawmetDBContext _dbContext;

        public PartsRaportWindow()
        {
            InitializeComponent();

            DataContext = this;

            WithNewOrders = true;

            Loaded += async delegate
            {
                await LoadAsync();
            };

            Closed += delegate
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                }
            };
        }

        public async Task LoadAsync()
        {
            IsEnabled = false;
            await Task.Run(() =>
            {
                // WARNING: 
                //  Tools -> Options
                //  Debugging
                //  Uncheck "Enable property evaluation and other implicit function calls"
                // either there's entitycommandexecutionexception function evaluation is disabled because function timeout ...
                using (_dbContext = new SpawmetDBContext())
                {
                    var parts = _dbContext.Parts.ToList();
                    if (WithNewOrders)
                    {
                        var newOrders = new List<Order>();
                        foreach (var part in parts)
                        {
                            var orders = part.AdditionalPartSets
                                .Select(e => e.Order)
                                .Where(o => o.Status == OrderStatus.New);
                            newOrders.AddRange(orders.Except(newOrders));
                            //newOrders = newOrders.Distinct().ToList();
                        }
                        foreach (var order in newOrders)
                        {
                            foreach (var element in order.AdditionalPartSet)
                            {
                                element.Part.Amount -= element.Amount;
                            }
                            foreach (var element in order.Machine.StandardPartSet)
                            {
                                element.Part.Amount -= element.Amount;
                            }
                            foreach (var module in order.Machine.Modules)
                            {
                                foreach (var element in module.MachineModulePartSet)
                                {
                                    element.Part.Amount -= element.Amount;
                                }
                            }
                        }
                    }

                    Parts = parts.Where(p => p.Amount <= 0).ToList();
                }
            });
            IsEnabled = true;
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
