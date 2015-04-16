using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabase.ViewModel
{
    public class PartViewModel
    {
        private Part _part;

        public int Id { get { return _part.Id; } }

        public string Name
        {
            get { return _part.Name; }
            set { _part.Name = value; }
        }

        public int Amount
        {
            get { return _part.Amount; }
            set { _part.Amount = value; }
        }

        public Origin? Origin
        {
            get { return _part.Origin; }
            set { _part.Origin = value; }
        }

        public ICollection<Delivery> Deliveries
        {
            get { return _part.Deliveries; }
        }

        public ICollection<Machine> Machines { get; private set; }

        public ICollection<Order> Orders { get; private set; }

        public PartViewModel(Part part)
        {
            _part = part;

            var machines = new List<Machine>();
            //foreach (var standardPartSet in )
        }
    }
}
