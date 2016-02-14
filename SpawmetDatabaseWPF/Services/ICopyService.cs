using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Services
{
    public interface ICopyService
    {
        IEnumerable<int> Tray { get; }
        Type StoredType { get; }

        void Copy<T>(IEnumerable<int> elementsIds);
    }
}
