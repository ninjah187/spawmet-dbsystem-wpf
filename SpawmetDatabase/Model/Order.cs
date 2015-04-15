using System.Collections.Generic;

namespace SpawmetDatabase.Model
{
    public class Order : IModelElement
    {
        public Order()
        {
            this.AdditionalPartSet = new HashSet<AdditionalPartSetElement>();
        }

        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public string Remarks { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime SendDate { get; set; }

        public virtual Client Client { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual ICollection<AdditionalPartSetElement> AdditionalPartSet { get; set; }
    }
}
