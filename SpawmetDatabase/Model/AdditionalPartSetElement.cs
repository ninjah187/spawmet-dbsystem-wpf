using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class AdditionalPartSetElement : PartSetElement
    {
        public virtual Order Order { get; set; }
    }
}
