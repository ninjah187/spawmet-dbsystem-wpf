using System.Collections.Generic;

namespace SpawmetDatabase.Model
{
    public class Client
    {
        public Client()
        {
            this.Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Nip { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
