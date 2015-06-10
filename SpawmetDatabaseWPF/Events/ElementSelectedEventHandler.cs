using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Events
{
    public delegate void ElementSelectedEventHandler<T>(object sender, ElementSelectedEventArgs<T> e);
}
