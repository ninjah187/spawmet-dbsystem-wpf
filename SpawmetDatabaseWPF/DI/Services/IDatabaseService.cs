using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.DI.Services
{
    public interface IDatabaseService
    {
        ICollection<Machine> GetMachines();
        ICollection<Order> GetOrders();
    }
}
