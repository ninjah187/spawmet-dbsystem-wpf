using System.Collections.Generic;

namespace SpawmetDatabase.Model
{
    public class Machine : IModelElement
    {
        public Machine()
        {
            this.Orders = new HashSet<Order>();
            this.StandardPartSet = new HashSet<Part>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Part> StandardPartSet { get; set; }
    }
}
