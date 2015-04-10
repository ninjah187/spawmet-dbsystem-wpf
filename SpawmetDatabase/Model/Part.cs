using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Part : IModelElement, INotifyPropertyChanged
    {
        public Part()
        {
            this.Deliveries = new HashSet<Delivery>();
            this.Machines = new HashSet<Machine>();
            this.Orders = new HashSet<Order>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get { return _id; }
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

        public Nullable<Origin> Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value)
                {
                    _origin = value;
                    NotifyPropertyChanged("Origin");
                }
            }
        }

        public virtual ICollection<Delivery> Deliveries { get; set; }
        public virtual ICollection<Machine> Machines { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        private int _id;
        private string _name;
        private int _amount;
        private Origin? _origin;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
