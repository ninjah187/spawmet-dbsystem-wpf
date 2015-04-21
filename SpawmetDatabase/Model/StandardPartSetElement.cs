using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class StandardPartSetElement // : PartSetElement
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        
        public virtual Machine Machine { get; set; }
        public virtual Part Part { get; set; }

        //public virtual Machine Machine
        //{
        //    get { return _machine; }
        //    set
        //    {
        //        if (_machine != value)
        //        {
        //            _machine = value;
        //            NotifyPropertyChanged("Machine");
        //        }
        //    }
        //}

        //private Machine _machine;
    }
}
