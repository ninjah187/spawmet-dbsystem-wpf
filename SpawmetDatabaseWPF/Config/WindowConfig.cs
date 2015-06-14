using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Config
{
    public class WindowConfig
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public WindowState WindowState { get; set; }

        public IModelElement SelectedElement { get; set; }

        public WindowConfig()
        {
            Left = 40;
            Top = 40;
            Width = 800;
            Height = 480;

            WindowState = WindowState.Normal;

            SelectedElement = null;
        }
    }
}
