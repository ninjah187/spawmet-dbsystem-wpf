using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class StandardPartSetElement : PartSetElement
    {
        public virtual Machine Machine { get; set; }
    }
}
