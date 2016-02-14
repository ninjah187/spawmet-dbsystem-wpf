using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public class CopyServiceSingleton
    {
        public static ICopyService Instance { get; protected set; }

        static CopyServiceSingleton()
        {
            Instance = new CopyService();
        }
    }
}
