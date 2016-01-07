using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseMUI.Services
{
    public abstract class SingletonBase<T>
    {
        public static T Instance { get; protected set; }
    }
}
