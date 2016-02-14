using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public abstract class FactoryBase<T> : IFactory<T> where T : new()
    {
        public virtual T GetItem()
        {
            return new T();
        }
    }
}
