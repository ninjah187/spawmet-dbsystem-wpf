﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Commands;
using SpawmetDatabaseWPF.CommonWindows;
using SpawmetDatabaseWPF.Config;
using SpawmetDatabaseWPF.Events;
using SpawmetDatabaseWPF.Windows.Searching;

namespace SpawmetDatabaseWPF.ViewModel
{
    public class MachinesWindowViewModel : SpawmetWindowViewModel
    {
        public event EventHandler PartSetStartLoading;
        public event EventHandler PartSetCompletedLoading;

        public event EventHandler OrdersStartLoading;
        public event EventHandler OrdersCompletedLoading;

        private BackgroundWorker _partsBackgroundWorker;
        private BackgroundWorker _ordersBackgroundWorker;

        private MachinesWindow _window;

        private ObservableCollection<Machine> _machines;
        public ObservableCollection<Machine> Machines
        {
            get { return _machines; }
            set
            {
                if (_machines != value)
                {
                    _machines = value;
                    OnPropertyChanged();
                }
            }
        }

        private Machine _selectedMachine;
        public Machine SelectedMachine
        {
            get
            {
                return _selectedMachine;
            }
            set
            {
                if (_selectedMachine != value)
                {
                    _selectedMachine = value;
                    OnPropertyChanged();
                    OnElementSelected(_selectedMachine);
                    LoadStandardPartSet();
                    LoadOrders();
                }
            }
        }

        private ObservableCollection<StandardPartSetElement> _standardPartSet;
        public ObservableCollection<StandardPartSetElement> StandardPartSet
        {
            get { return _standardPartSet; }
            set
            {
                if (_standardPartSet != value)
                {
                    _standardPartSet = value;
                    OnPropertyChanged();
                }
            }
        }

        private StandardPartSetElement _selectedPartSetElement;
        public StandardPartSetElement SelectedPartSetElement
        {
            get { return _selectedPartSetElement; }
            set
            {
                if (_selectedPartSetElement != value)
                {
                    _selectedPartSetElement = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get { return _orders; }
            set
            {
                if (_orders != value)
                {
                    _orders = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddMachineCommand { get; private set; }

        public ICommand DeleteMachinesCommand { get; private set; }

        public ICommand PrintDialogCommand { get; private set; }

        public override ICommand RefreshCommand { get; protected set; }

        public ICommand SaveToFileCommand { get; private set; }

        public ICommand AddPartToMachineCommand { get; private set; }

        public ICommand CraftPartCommand { get; private set; }

        public ICommand DeletePartFromMachineCommand { get; private set; }

        public ICommand AddMachinesFromDirectoryCommand { get; private set; }

        public override ICommand NewSearchWindowCommand { get; protected set; }

        public MachinesWindowViewModel(MachinesWindow window)
            : this(window, null)
        {
        }

        public MachinesWindowViewModel(MachinesWindow window, WindowConfig config)
            : base(window, config)
        {
            _window = window;

            InitializeCommands();
            InitializeBackgroundWorkers();
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposeBackgroundWorkers();
        }

        private void DisposeBackgroundWorkers()
        {
            if (_partsBackgroundWorker != null)
            {
                _partsBackgroundWorker.Dispose();
            }
            if (_ordersBackgroundWorker != null)
            {
                _ordersBackgroundWorker.Dispose();
            }
        }

        private void InitializeBackgroundWorkers()
        {
            DisposeBackgroundWorkers();

            _partsBackgroundWorker = new BackgroundWorker();
            _partsBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int)e.Argument;
                List<StandardPartSetElement> result;
                lock (DbContextLock)
                {
                    result = DbContext.StandardPartSets
                        .Where(el => el.Machine.Id == machineId)
                        .Include(el => el.Part)
                        .OrderBy(el => el.Part.Name)
                        .ToList();
                }
                e.Result = result;
            };
            _partsBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (ICollection<StandardPartSetElement>)e.Result;
                StandardPartSet = new ObservableCollection<StandardPartSetElement>(source);

                OnPartSetCompletedLoading();
            };

            _ordersBackgroundWorker = new BackgroundWorker();
            _ordersBackgroundWorker.DoWork += (sender, e) =>
            {
                var machineId = (int)e.Argument;
                var context = new SpawmetDBContext();
                var result = context.Orders
                    .Where(m => m.Machine.Id == machineId)
                    .Include(o => o.Client)
                    .Include(o => o.Machine)
                    .OrderBy(o => o.Id)
                    .ToList();
                e.Result = result;
                context.Dispose();
            };
            _ordersBackgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                var source = (ICollection<Order>) e.Result;
                Orders = new ObservableCollection<Order>(source);

                OnOrdersCompletedLoading();
            };
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AddMachineCommand = new Command(() =>
            {
                var win = new AddMachineWindow(DbContext);
                win.MachineAdded += (sender, e) =>
                {
                    Machines.Add(e);
                };
                win.Show();
            });

            CraftPartCommand = new Command(() =>
            {
                var element = SelectedPartSetElement;
                var part = DbContext.Parts.Single(p => p.Id == element.Part.Id);
                part.Amount += element.Amount;
                
                DbContext.SaveChanges();

                string txt = "Wypalono: " + part.Name + "\nIlość: " + element.Amount;
                MessageBox.Show(txt, "Wypalono część");
            });

            DeleteMachinesCommand = new Command(() =>
            {
                var selected = GetSelectedMachines();
                if (selected == null)
                {
                    return;
                }

                string msg = selected.Count == 1
                    ? "Czy na pewno chcesz usunąć zaznaczoną maszynę?"
                    : "Czy na pewno chcesz usunąć zaznaczone maszyny?";

                var confirmWin = new ConfirmWindow(_window, msg);
                confirmWin.Confirmed += delegate
                {
                    var win = new DeleteMachineWindow(DbContext, selected);
                    win.MachinesDeleted += (sender, machines) =>
                    {
                        foreach (var machine in machines)
                        {
                            Machines.Remove(machine);
                        }
                    };
                    win.WorkCompleted += delegate
                    {
                        StandardPartSet = null;
                        Orders = null;

                        OnElementSelected(null);
                    };
                    win.Show();
                };
                confirmWin.Show();
            });

            PrintDialogCommand = new Command(() =>
            {
                var selected = GetSelectedMachines();
                if (selected == null)
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
                    string description = selected.Count == 1
                        ? selected.First().Name
                        : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                    var printWindow = new PrintWindow(selected, printDialog);
                    printWindow.Show();
                }
            });

            RefreshCommand = new Command(() =>
            {
                SaveDbStateCommand.Execute(null);

                var config = GetWindowConfig();
                var win = new MachinesWindow(config);
                win.Loaded += delegate
                {
                    _window.Close();
                };
                win.Show();
            });

            SaveToFileCommand = new Command(() =>
            {
                var selected = GetSelectedMachines();
                if (selected == null)
                {
                    return;
                }

                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Plik Word 2007 (*.docx)|*.docx|Plik PDF (*.pdf)|*.pdf";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = selected.Count == 1
                    ? selected.First().Name
                    : "Wykaz maszyn, " + DateTime.Now.ToString("yyyy-MM-dd HH_mm");

                if (saveFileDialog.ShowDialog() == true)
                {
                    new SaveFileWindow(selected, saveFileDialog.FileName).Show();
                }
            });

            AddPartToMachineCommand = new Command(() =>
            {
                var machine = SelectedMachine;

                if (machine == null)
                {
                    return;
                }

                var win = new AddPartToMachine(DbContext, machine);
                win.PartAdded += (sender, partSetElement) =>
                {
                    StandardPartSet.Add(partSetElement);
                };
                win.Show();
            });

            DeletePartFromMachineCommand = new Command(() =>
            {
                if (SelectedPartSetElement == null)
                {
                    return;
                }

                var element = DbContext.StandardPartSets
                    .Single(el => el.Part.Id == SelectedPartSetElement.Part.Id
                                                                      && el.Machine.Id == SelectedPartSetElement.Machine.Id);
                DbContext.StandardPartSets.Remove(element);
                DbContext.SaveChanges();

                LoadStandardPartSet();
            });

            AddMachinesFromDirectoryCommand = new Command(() =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();

                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var win = new AddMachinesFromDirectory(dialog.SelectedPath);
                    win.MachineAdded += (sender, machine) =>
                    {
                        Machines.Add(machine);
                    };
                    win.PartSetElementAdded += (sender, element) =>
                    {
                        if (SelectedMachine == null)
                        {
                            return;
                        }

                        if (SelectedMachine.Id == element.Machine.Id)
                        {
                            StandardPartSet.Add(element);
                        }
                    };
                    win.Show();
                }

                dialog.Dispose();
            });

            NewSearchWindowCommand = new Command(() =>
            {
                var win = new SearchMachinesByName(_window, DbContext);
                win.WorkCompleted += (sender, machines) =>
                {
                    Machines = new ObservableCollection<Machine>(machines);
                    
                    StandardPartSet = null;
                    Orders = null;

                    OnElementSelected(null);

                    SearchExpression = win.RegExpr;

                    MessageWindow.Show("Zakończono wyszukiwanie", win);
                };
                win.Show();
            });
        }

        public override void Load()
        {
            LoadMachines();

            // if some element were previously selected; needed in refreshing window
            if (WindowConfig.SelectedElement != null)
            {
                var machine = Machines.Single(m => m.Id == WindowConfig.SelectedElement.Id);

                SelectedMachine = machine;

                _window.DataGrid.SelectedItem = SelectedMachine;
                _window.DataGrid.ScrollIntoView(SelectedMachine);
            }
        }

        private void LoadMachines()
        {
            var machines = DbContext.Machines.ToList();

            Machines = new ObservableCollection<Machine>(machines);
        }

        private void LoadStandardPartSet()
        {
            var machine = SelectedMachine;

            if (_partsBackgroundWorker.IsBusy == false)
            {
                _partsBackgroundWorker.RunWorkerAsync(machine.Id);

                OnPartSetStartLoading();
            }
        }

        private void LoadOrders()
        {
            var machine = SelectedMachine;

            if (_ordersBackgroundWorker.IsBusy == false)
            {
                _ordersBackgroundWorker.RunWorkerAsync(machine.Id);

                OnOrdersStartLoading();
            }
        }

        private List<Machine> GetSelectedMachines() // bind instead of GetSelectedMachines()
        {
            if (_window.MainDataGrid.SelectedItems.Count == 0)
            {
                return null;
            }

            var selected = new List<Machine>();
            foreach (var item in _window.MainDataGrid.SelectedItems)
            {
                selected.Add((Machine)item);
            }

            return selected;
        }

        protected override WindowConfig GetWindowConfig()
        {
            var config = base.GetWindowConfig();
            config.SelectedElement = SelectedMachine;

            return config;
        }

        //private IEnumerable<Machine> GetSelectedMachines()
        //{
        //    if (_window.MainDataGrid.SelectedItems.Count == 0)
        //    {
        //        yield break;
        //    }
        //    else
        //    {
        //        foreach (var item in _window.MainDataGrid.SelectedItems)
        //        {
        //            yield return (Machine) item;
        //        }
        //    }
        //}

        #region Event invokers.

        private void OnPartSetStartLoading()
        {
            if (PartSetStartLoading != null)
            {
                PartSetStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnPartSetCompletedLoading()
        {
            if (PartSetCompletedLoading != null)
            {
                PartSetCompletedLoading(this, EventArgs.Empty);
            }
        }

        private void OnOrdersStartLoading()
        {
            if (OrdersStartLoading != null)
            {
                OrdersStartLoading(this, EventArgs.Empty);
            }
        }

        private void OnOrdersCompletedLoading()
        {
            if (OrdersCompletedLoading != null)
            {
                OrdersCompletedLoading(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
