using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SpawmetDatabase;

namespace SpawmetDatabaseMUI.Services
{
    public class EntityService : IEntityService
    {
        public static EntityService Instance { get; private set; }

        public SpawmetDBContext Context { get; private set; }
        public object ContextLock { get; private set; }

        private EntityService()
        {
            Context = new SpawmetDBContext();
            ContextLock = new object();
        }

        static EntityService()
        {
            Instance = new EntityService();
        }

        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
            }
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await Task.Run(() =>
            {
                lock (ContextLock)
                {
                    Context.SaveChanges();
                }
            });
        }
    }
}
