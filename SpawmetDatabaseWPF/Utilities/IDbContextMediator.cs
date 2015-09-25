using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Utilities
{
    public interface IDbContextMediator
    {
        ICollection<IDbContextChangesNotifier> Subscribers { get; }

        void NotifyContextChanged(IDbContextChangesNotifier notifier);
        void NotifyContextChanged(IDbContextChangesNotifier notifier, params Type[] targetTypes);
    }
}
