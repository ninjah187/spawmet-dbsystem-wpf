using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase;

namespace SpawmetDatabaseMUI.Services
{
    public interface IEntityService : IDisposable
    {
        SpawmetDBContext Context { get; }
        object ContextLock { get; }

        void SaveChanges();
        Task SaveChangesAsync();
    }
}
