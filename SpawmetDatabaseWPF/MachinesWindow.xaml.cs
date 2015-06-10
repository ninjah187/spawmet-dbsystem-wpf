﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using SpawmetDatabase;
using SpawmetDatabase.FileCreators;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    /***********************************************************************************/
    /*** Main rule for loading data to UI from another threads:                      ***/
    /***   - readonly data (like OrdersListBox in MachinesWindow) can be loaded from ***/
    /***     another context (with using Include() to load all related data, in      ***/
    /***     order to display Signature).                                            ***/
    /***   - read/write data (like StandardPartSetGrid in MachinesWindow) MUST be    ***/
    /***     loaded from main _dbContext (with using lock and Include).              ***/
    /***********************************************************************************/

    public partial class MachinesWindow : Window, ISpawmetWindow
    {
        public MachinesWindow()
            : this(40, 40)
        {
        }

        public MachinesWindow(double x, double y)
        {
            InitializeComponent();

            Left = x;
            Top = y;

            var viewModel = new MachinesWindowViewModel(this);
            DataContext = viewModel;

            viewModel.ElementSelected += (sender, e) =>
            {
                var machine = (Machine) e.Element;

                IdTextBlock.Text = machine.Id.ToString();
                NameTextBlock.Text = machine.Name;
                PriceTextBlock.Text = machine.Price.ToString();
            };

            viewModel.PartSetStartLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = true;
            };
            viewModel.PartSetCompletedLoading += delegate
            {
                StandardPartSetProgressBar.IsIndeterminate = false;
            };

            viewModel.OrdersStartLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = true;
            };
            viewModel.OrdersCompletedLoading += delegate
            {
                OrdersProgressBar.IsIndeterminate = false;
            };

            this.Closed += delegate
            {
                viewModel.Dispose();
            };
        }
    }
}
