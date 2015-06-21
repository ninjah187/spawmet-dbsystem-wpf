using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
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
using System.Windows.Threading;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddMachinesFromDirectory.xaml
    /// </summary>
    public partial class AddMachinesFromDirectory : Window
    {
        public event EventHandler<Machine> MachineAdded;
        public event EventHandler<StandardPartSetElement> PartSetElementAdded;
        public event EventHandler WorkCompleted;

        public ObservableCollection<Machine> Machines { get; set; }
        public ObservableCollection<Part> Parts { get; set; }
        public ObservableCollection<StandardPartSetElement> PartSets { get; set; }

        private DispatcherTimer _timer;
        private static readonly TimeSpan _timerStep = new TimeSpan(0, 0, 0, 0, 100);
        private DateTime _startTime;
        private DateTime _currentTime;
        private const string CurrentTimePattern = "HH:mm:ss.f";

        private MachinePathParser _parser;

        public AddMachinesFromDirectory(string machineDirectoryPath)
        {
            InitializeComponent();

            DataContext = this;
            
            Machines = new ObservableCollection<Machine>();
            Parts = new ObservableCollection<Part>();
            PartSets = new ObservableCollection<StandardPartSetElement>();

            _currentTime = default(DateTime);

            TimeTextBlock.Text = _currentTime.ToLongTimeString();

            _timer = new DispatcherTimer();
            _timer.Interval = _timerStep;
            _timer.Tick += delegate
            {
                _currentTime = DateTime.Now;
                //TimeTextBlock.Text = _currentTime.ToString(CurrentTimePattern);
                var time = _currentTime - _startTime;
                TimeTextBlock.Text = time.ToString();
            };

            _parser = new MachinePathParser(machineDirectoryPath);
            _parser.DirectoryReached += (sender, dir) =>
            {
                STAExecute(() =>
                {
                    DirectoryTextBlock.Text = dir;
                });
            };
            _parser.FileReached += (sender, file) =>
            {
                STAExecute(() =>
                {
                    FileTextBlock.Text = file;
                });
            };
            _parser.MachineAdded += (sender, machine) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Machines.Add(machine);

                    MachinesListBox.ScrollIntoView(machine);

                    string count = "Dodano: " + Machines.Count.ToString();
                    MachinesCountTextBlock.Text = count;

                    OnMachineAdded(machine);
                });
            };
            _parser.PartAdded += (sender, part) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Parts.Add(part);

                    PartsListBox.ScrollIntoView(part);

                    string count = "Dodano: " + Parts.Count.ToString();
                    PartsCountTextBlock.Text = count;
                });
            };
            _parser.StandardPartSetElementAdded += (sender, element) =>
            {
                STAExecute(() =>
                {
                    PartSets.Add(element);

                    PartSetsDataGrid.ScrollIntoView(element);

                    string count = "Dodano: " + PartSets.Count.ToString();
                    PartSetsCountTextBlock.Text = count;

                    OnPartSetElementAdded(element);
                });
            };
            _parser.ParserRunCompleted += (sender, e) =>
            {
                _timer.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ParserProgressBar.IsIndeterminate = false;

                    //string txt = "Czas operacji: " + e.TimeElapsed;
                    var time = _currentTime - _startTime;
                    string txt = "Czas operacji: " + time;
                    //string txt = "Czas operacji: " + _currentTime.ToString(CurrentTimePattern);
                    MessageBox.Show(txt, "Zakończono dodawanie maszyn");

                    OnWorkCompleted();
                });
            };

            this.Loaded += delegate
            {
                _startTime = _currentTime = DateTime.Now;
                _timer.Start();
                _parser.ParseAsync();
            };

            this.Closed += delegate
            {
                _parser.Abort();
            };
        }

        private void STAExecute(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        private void OnMachineAdded(Machine machine)
        {
            if (MachineAdded != null)
            {
                MachineAdded(this, machine);
            }
        }

        private void OnPartSetElementAdded(StandardPartSetElement element)
        {
            if (PartSetElementAdded != null)
            {
                PartSetElementAdded(this, element);
            }
        }

        private void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }
    }
}
