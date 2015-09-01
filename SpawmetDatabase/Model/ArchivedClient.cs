using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class ArchivedClient : PropertyChangedNotifier
    {
        public ArchivedClient()
        {
            
        }

        public ArchivedClient(Client c)
        {
            _oldId = c.Id;
            _name = c.Name;
            _phone = c.Phone;
            _email = c.Email;
            _nip = c.Nip;
            _address = c.Address;
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

        public int OldId
        {
            get { return _oldId; }
            set
            {
                if (_oldId != value)
                {
                    _oldId = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged("Name");
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
                    OnPropertyChanged("Phone");
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
                    OnPropertyChanged("Email");
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
                    OnPropertyChanged("Nip");
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
                    OnPropertyChanged("Address");
                }
            }
        }

        //public virtual ArchivedOrder Order
        //{
        //    get { return _order; }
        //    set
        //    {
        //        if (_order != value)
        //        {
        //            _order = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        private int _id;
        private int _oldId;
        private string _name;
        private string _phone;
        private string _email;
        private string _nip;
        private string _address;

        //private ArchivedOrder _order;
    }
}
