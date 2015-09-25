using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Utilities
{
    public interface IDbContextChangesNotifier
    {
        IDbContextMediator DbContextMediator { get; set; }

        DbContextChangedHandler ContextChangedHandler { get; set; }
    }
}
