using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase
{
    public class SpawmetDBInitializer : DropCreateDatabaseAlways<SpawmetDBContext>
    {
        private static readonly Random random = new Random();

        protected override void Seed(SpawmetDBContext context)
        {
            
        }
    }
}
