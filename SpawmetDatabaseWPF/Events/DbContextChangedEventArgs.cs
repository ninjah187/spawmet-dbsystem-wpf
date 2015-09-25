using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Events
{
    public class DbContextChangedEventArgs : EventArgs
    {
        public IDbContextChangesNotifier Notifier { get; set; }
        public Type[] 
    }
}
