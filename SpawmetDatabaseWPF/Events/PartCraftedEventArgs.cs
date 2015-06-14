using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Events
{
    public class PartCraftedEventArgs : EventArgs
    {
        public Part Part { get; private set; }
        public int Amount { get; private set; }

        public PartCraftedEventArgs(Part part, int amount)
        {
            Part = part;
            Amount = amount;
        }
    }
}
