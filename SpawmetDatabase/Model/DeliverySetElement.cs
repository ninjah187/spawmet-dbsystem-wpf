using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class DeliveryPartSetElement : IModelElement
    {
        public int Id { get; set; }
        public int Amount { get; set; }

        public virtual Part Part { get; set; }
        public virtual Delivery Delivery { get; set; }
    }
}
