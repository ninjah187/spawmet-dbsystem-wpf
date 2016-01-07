using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;
using SpawmetDatabaseMUI.Services;

namespace SpawmetDatabaseMUI.ViewModel
{
    public class MachinesViewModel : BaseViewModel
    {
        private ObservableCollection<Machine> _machines;
        public ObservableCollection<Machine> Machines
        {
            get { return _machines; }
            set
            {
                if (_machines != value)
                {
                    _machines = value;
                    OnPropertyChanged();
                }
            }
        }

        private IMachineService _machineService;

        public MachinesViewModel(IMachineService machineService)
        {
            //_machineService = machineService;

            var machines = EntityService.Instance.Context.Machines.ToList();

            Machines = new ObservableCollection<Machine>(machines);
        }
    }
}
