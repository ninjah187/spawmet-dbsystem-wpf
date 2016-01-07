using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Delivery : IModelElement, INotifyPropertyChanged
    {
        public Delivery()
        {
            this.DeliveryPartSet = new HashSet<DeliveryPartSetElement>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }

        public virtual ICollection<DeliveryPartSetElement> DeliveryPartSet
        {
            get { return _deliveryPartSet; }
            set
            {
                if (_deliveryPartSet != value)
                {
                    _deliveryPartSet = value;
                    NotifyPropertyChanged("DeliveryPartSet");
                }
            }
        }

        public string Signature
        {
            get { return Name + ", " + Date.ToShortDateString(); }
        }

        private int _id;
        private string _name;
        private DateTime _date;

        private ICollection<DeliveryPartSetElement> _deliveryPartSet;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
