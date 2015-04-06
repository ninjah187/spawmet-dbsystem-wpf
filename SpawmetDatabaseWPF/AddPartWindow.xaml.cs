﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddPartWindow.xaml
    /// </summary>
    public partial class AddPartWindow : Window
    {
        private Window _parentWindow;

        public AddPartWindow(Window parentWindow)
        {
            InitializeComponent();

            _parentWindow = parentWindow;

            this.Loaded += (sender, e) =>
            {
                _parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _parentWindow.IsEnabled = true;
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            var amount = int.Parse(AmountTextBox.Text);

            using (var context = new SpawmetDBContext())
            {
                Part p;
                context.Parts.Add(p = new Part()
                {
                    Name = name,
                    Amount = amount,
                    Origin = null,
                });
                ((MainWindow) _parentWindow).DataGridItemsSource.Add(p);
                context.SaveChanges();
            }

            this.Close();
        }
    }
}