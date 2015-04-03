using System.Collections.Generic;

namespace SpawmetDatabase.Model
{
    public class Delivery
    {
        public Delivery()
        {
            this.Parts = new HashSet<Part>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime Date { get; set; }

        public virtual ICollection<Part> Parts { get; set; }
    }
}
