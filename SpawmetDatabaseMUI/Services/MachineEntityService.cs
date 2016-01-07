using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseMUI.Services
{
    public class MachineEntityService : SingletonBase<MachineEntityService>, IMachineService
    {
        private IEntityService _entityService;

        private MachineEntityService(IEntityService entityService)
        {
            Machines = new List<Machine>();

            _entityService = entityService;

            lock (_entityService.ContextLock)
            {
                Machines = _entityService.Context.Machines.ToList();
            }
        }

        static MachineEntityService()
        {
            Instance = new MachineEntityService(EntityService.Instance);
        }

        public List<Machine> Machines { get; private set; }

        public void Add(Machine machine)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<Machine> machines)
        {
            throw new NotImplementedException();
        }

        public void Delete(Machine machine)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<Machine> machines)
        {
            throw new NotImplementedException();
        }

        public void Update(Machine oldData, Machine newData)
        {
            throw new NotImplementedException();
        }
    }
}
