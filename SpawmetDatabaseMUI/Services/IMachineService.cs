using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseMUI.Services
{
    public interface IMachineService
    {
        List<Machine> Machines { get; }
        void Add(Machine machine);
        void Add(IEnumerable<Machine> machines);
        void Delete(Machine machine);
        void Delete(IEnumerable<Machine> machines);
        void Update(Machine oldData, Machine newData);
    }
}
