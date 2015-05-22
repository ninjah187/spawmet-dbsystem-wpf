using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Machine : IModelElement, INotifyPropertyChanged
    {
        public Machine()
        {
            this.Orders = new HashSet<Order>();
            this.StandardPartSet = new HashSet<StandardPartSetElement>();
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

        public decimal Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    NotifyPropertyChanged("Price");
                }
            }
        }

        public virtual ICollection<Order> Orders
        {
            get { return _orders; }
            set
            {
                if (_orders != value)
                {
                    _orders = value;
                    NotifyPropertyChanged("Orders");
                }
            }
        }

        public virtual ICollection<StandardPartSetElement> StandardPartSet
        {
            get { return _standardPartSet; }
            set
            {
                if (_standardPartSet != value)
                {
                    _standardPartSet = value;
                    NotifyPropertyChanged("StandardPartSet");
                }
            }
        }

        private int _id;
        private string _name;
        private decimal _price;

        private ICollection<Order> _orders;
        private ICollection<StandardPartSetElement> _standardPartSet;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
