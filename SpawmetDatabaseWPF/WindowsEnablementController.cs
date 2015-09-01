using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpawmetDatabaseWPF
{
    public class WindowsEnablementController
    {
        public void EnableWindows()
        {
            foreach (var win in Application.Current.Windows)
            {
                var window = (Window) win;
                window.IsEnabled = true;
            }
        }

        public void DisableWindows()
        {
            foreach (var win in Application.Current.Windows)
            {
                var window = (Window) win;
                window.IsEnabled = false;
            }
        }
    }
}
