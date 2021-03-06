﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SpawmetDatabaseWPF
{
    public interface ISpawmetWindow
    {
        event CancelEventHandler Closing;
        event EventHandler Closed;

        double Left { get; set; }
        double Top { get; set; }
        double Width { get; set; }
        double Height { get; set; }

        WindowState WindowState { get; set; }

        DataGrid DataGrid { get; }

        void CommitEdit();
    }
}
