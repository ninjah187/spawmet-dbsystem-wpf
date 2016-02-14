using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Services
{
    public class CopyService : ICopyService
    {
        public IEnumerable<int> Tray { get; protected set; }

        public Type StoredType { get; protected set; }

        public void Copy<T>(IEnumerable<int> elementsIds)
        {
            Tray = elementsIds;
            StoredType = typeof (T);
        }
    }
}
