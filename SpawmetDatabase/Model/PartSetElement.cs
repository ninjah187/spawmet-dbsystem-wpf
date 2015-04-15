using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public abstract class PartSetElement : IModelElement
    {
        public int Id { get; set; }
        public int Amount { get; set; }

        public virtual Part Part { get; set; }
    }
}
