using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public abstract class SingletonBase<T> where T : class, new()
    {
        public static T Instance { get; protected set; }

        static SingletonBase()
        {
            if (Instance == null)
            {
                Instance = new T();
            }
        } 
    }
}
