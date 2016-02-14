using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public abstract class SingletonBase<T> where T : class, new()
    {
        public static T Instance { get; protected set; }

        protected static Dictionary<Type, Type> FactoriesDictionary { get; set; }

        static SingletonBase()
        {
            InitializeFactoriesInfo();

            var targetType = typeof (T);

            foreach (var keyValue in FactoriesDictionary)
            {
                if (keyValue.Key == targetType)
                {
                    var factoryType = keyValue.Value;

                    var factory = (IFactory<T>) Activator.CreateInstance(factoryType);

                    Instance = factory.GetItem();

                    return;
                }
            }

            Instance = new T();
        }

        private static void InitializeFactoriesInfo()
        {
            if (FactoriesDictionary != null)
            {
                return;
            }

            FactoriesDictionary = new Dictionary<Type, Type>()
            {
                // { typeof (CopyService), typeof (CopyServiceFactory) },
                { typeof (PasteService), typeof (PasteServiceSingletonFactory) }
            };
        }
    }
}
