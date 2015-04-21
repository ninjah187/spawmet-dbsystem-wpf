using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public abstract class PartSetElement : ModelElement
    {
        public int Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    NotifyPropertyChanged("Amount");
                }
            }
        }

        public virtual Part Part
        {
            get { return _part; }
            set
            {
                if (_part != value)
                {
                    NotifyPropertyChanged("Part");
                }
            }
        }

        private int _amount;

        private Part _part;
    }
}
