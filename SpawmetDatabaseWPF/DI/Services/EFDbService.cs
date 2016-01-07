using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.DI.Services
{
    public class EFDbService : IDatabaseService
    {
        private readonly SpawmetDBContext _dbContext;

        public ICollection<Machine> GetMachines()
        {
            throw new NotImplementedException();
        }

        public ICollection<Order> GetOrders()
        {
            throw new NotImplementedException();
        }
    }
}
