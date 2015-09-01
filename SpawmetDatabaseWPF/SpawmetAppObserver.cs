using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public class SpawmetAppObserver
    {
        public MachinesWindowViewModel MachinesVm { get; set; }
        public PartsWindowViewModel PartsVm { get; set; }
        public OrdersWindowViewModel OrdersVm { get; set; }
        public ClientsWindowViewModel ClientsVm { get; set; }
        public DeliveriesWindowViewModel DeliveriesVm { get; set; }

        public SpawmetAppObserver()
        {
        }

        public void NotifyPartsChange(SpawmetWindowViewModel notifier)
        {
            if (notifier != MachinesVm)
            {
                MachinesVm.LoadStandardPartSet();
            }

            if (notifier != PartsVm)
            {
                PartsVm.LoadParts();
            }
        }
    }
}
