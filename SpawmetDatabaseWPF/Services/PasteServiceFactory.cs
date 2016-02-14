using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public class PasteServiceFactory
    {
        public static PasteService GetSingletonPasteService()
        {
            var service = new PasteService();
            service.CopyService = CopyServiceSingleton.Instance;

            return service;
        }
    }
}
