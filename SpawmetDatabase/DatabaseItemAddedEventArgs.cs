using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase
{
    public class DatabaseItemAddedEventArgs : EventArgs
    {
        public string ItemName { get; set; }

        public DatabaseItemAddedEventArgs(string itemName)
        {
            ItemName = itemName;
        }
    }
}
