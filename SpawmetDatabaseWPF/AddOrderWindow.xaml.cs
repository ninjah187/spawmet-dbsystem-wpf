﻿using System;
using System.Collections.Generic;
using System.Data;
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
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    /// <summary>
    /// Interaction logic for AddOrderWindow.xaml
    /// </summary>
    public partial class AddOrderWindow : Window
    {
        private OrdersWindow _parentWindow;
        private SpawmetDBContext _dbContext;

        public AddOrderWindow()
        {
            InitializeComponent();
        }

        public AddOrderWindow(OrdersWindow parentWindow, SpawmetDBContext dbContext)
        {
            InitializeComponent();

            _parentWindow = parentWindow;
            _dbContext = dbContext;

            ClientComboBox.ItemsSource = _dbContext.Clients.OrderBy(c => c.Name).ToList();
            MachineComboBox.ItemsSource = _dbContext.Machines.OrderBy(m => m.Name).ToList();

            this.Loaded += (sender, e) =>
            {
                _parentWindow.IsEnabled = false;
            };
            this.Closed += (sender, e) =>
            {
                _parentWindow.IsEnabled = true;
            };

            RemarksTextBox.GotFocus += TextBox_GotFocus;

            StartDatePicker.SelectedDate = DateTime.Today;
            SendDatePicker.SelectedDate = DateTime.Today;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var client = (Client) ClientComboBox.SelectedItem;
            var machine = (Machine) MachineComboBox.SelectedItem;
            if (machine == null)
            {
                MessageBox.Show("Wybierz maszynę.", "Błąd");
                return;
            }
            var startDate = StartDatePicker.SelectedDate;
            var sendDate = SendDatePicker.SelectedDate;
            var status = (OrderStatus) StatusComboBox.SelectedIndex;
            var remarks = RemarksTextBox.Text;

            var order = new Order()
            {
                //Id = FindSmallestFreeId(),
                Client = client,
                Machine = machine,
                StartDate = startDate,
                SendDate = sendDate,
                Status = status,
                Remarks = remarks
            };
            _dbContext.Orders.Add(order);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (EntityException exc)
            {
                Disconnected();
            }

            this.Close();
        }

        private int FindSmallestFreeId()
        {
            var ids = _dbContext.Orders.Select(o => o.Id).ToList();
            int id = -1;
            for (int i = 0; i < ids.Count - 1; i++)
            {
                if (ids[i] == ids[i + 1] - 1)
                {
                    continue;
                }

                id = ids[i] + 1;
                return id;
            }
            return id;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Disconnected()
        {
            _parentWindow.MainDataGrid.IsEnabled = false;
            _parentWindow.DetailsStackPanel.IsEnabled = false;
            _parentWindow.MachinesMenuItem.IsEnabled = false;
            _parentWindow.FillDetailedInfo(null);
            MessageBox.Show("Brak połączenia z serwerem.", "Błąd");
            _parentWindow.ConnectMenuItem.IsEnabled = true;
        }
    }
}