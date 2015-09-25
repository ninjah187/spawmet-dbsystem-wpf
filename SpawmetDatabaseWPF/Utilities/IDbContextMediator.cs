using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Utilities
{
    interface IDbContextMediator
    {
        List<IDbContextChangesNotifier> Subscribers { get; set; }

        void NotifyContextChanged(IDbContextChangesNotifier notifier);
        void NotifyContextChanged(IDbContextChangesNotifier notifier, params Type[] targetTypes);
    }
}
