using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public class PasteServiceSingleton : SingletonBase<PasteService>
    {
        static PasteServiceSingleton()
        {
            //throw new Exception();

            //Instance = Instance ?? new PasteService();

            //Instance.CopyService = CopyServiceSingleton.Instance;
        }
    }
}
