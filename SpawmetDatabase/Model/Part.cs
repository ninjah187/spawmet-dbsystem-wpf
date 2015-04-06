using System;
using System.Collections.Generic;

namespace SpawmetDatabase.Model
{
    public class Part : IModelElement
    {
        public Part()
        {
            this.Deliveries = new HashSet<Delivery>();
            this.Machines = new HashSet<Machine>();
            this.Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public Nullable<Origin> Origin { get; set; }

        public virtual ICollection<Delivery> Deliveries { get; set; }
        public virtual ICollection<Machine> Machines { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
