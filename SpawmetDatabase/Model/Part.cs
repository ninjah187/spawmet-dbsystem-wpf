using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Part : IModelElement
    {
        public Part()
        {
            this.DeliveryPartSets = new HashSet<DeliveryPartSetElement>();
            this.StandardPartSets = new HashSet<StandardPartSetElement>();
            this.AdditionalPartSets = new HashSet<AdditionalPartSetElement>();
        }

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    //NotifyPropertyChanged("Name");
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
                    //NotifyPropertyChanged("Amount");
                }
            }
        }

        public Nullable<Origin> Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value)
                {
                    _origin = value;
                    //NotifyPropertyChanged("Origin");
                }
            }
        }

        public virtual ICollection<DeliveryPartSetElement> DeliveryPartSets {
            get
            {
                return _deliveryPartSets;
            }
            set
            {
                if (_deliveryPartSets != value)
                {
                    _deliveryPartSets = value;
                    //NotifyPropertyChanged("Deliveries");
                }
            }
        }

        public virtual ICollection<StandardPartSetElement> StandardPartSets
        {
            get
            {
                return _standardPartSets;
            }
            set
            {
                if (_standardPartSets != value)
                {
                    _standardPartSets = value;
                    //NotifyPropertyChanged("StandardPartSets");
                }
            }
        }

        public virtual ICollection<AdditionalPartSetElement> AdditionalPartSets
        {
            get
            {
                return _additionalPartSets;
            }
            set
            {
                if (_additionalPartSets != value)
                {
                    _additionalPartSets = value;
                    //NotifyPropertyChanged("AdditionalPartSets");
                }
            }
        }

        private string _name;
        private int _amount;
        private Origin? _origin;

        private ICollection<DeliveryPartSetElement> _deliveryPartSets;
        private ICollection<StandardPartSetElement> _standardPartSets;
        private ICollection<AdditionalPartSetElement> _additionalPartSets;
    }
}
