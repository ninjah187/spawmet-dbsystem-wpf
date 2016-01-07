using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class ArchivedAdditionalPartSetElement : PropertyChangedNotifier
    {
        public ArchivedAdditionalPartSetElement()
        {
        }

        public ArchivedAdditionalPartSetElement(AdditionalPartSetElement e)
        {
            if (e == null)
            {
                return;
            }

            _partName = e.Part.Name;
            _amount = e.Amount;
        }

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PartName
        {
            get { return _partName; }
            set
            {
                if (_partName != value)
                {
                    _partName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ArchivedOrder Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _id;
        private string _partName;
        private int _amount;

        private ArchivedOrder _order;
    }
}
