using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Client : IModelElement, INotifyPropertyChanged
    {
        public Client()
        {
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

        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    NotifyPropertyChanged("Phone");
                }
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    NotifyPropertyChanged("Email");
                }
            }
        }

        public string Nip
        {
            get { return _nip; }
            set
            {
                if (_nip != value)
                {
                    _nip = value;
                    NotifyPropertyChanged("Nip");
                }
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                if (_address != value)
                {
                    _address = value;
                    NotifyPropertyChanged("Address");
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

        private int _id;
        private string _name;
        private string _phone;
        private string _email;
        private string _nip;
        private string _address;

        private ICollection<Order> _orders;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
