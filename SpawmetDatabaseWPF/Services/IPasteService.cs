using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase;
using SpawmetDatabaseWPF.Utilities;

namespace SpawmetDatabaseWPF.Services
{
    public interface IPasteService : IDbContextChangesNotifier
    {
        //SpawmetDBContext DbContext { get; }

        ICopyService CopyService { get; }

        void PasteModules(int targetMachineId);
        void Paste<T>(int targetId);

        bool CanPaste<T>();
    }
}
