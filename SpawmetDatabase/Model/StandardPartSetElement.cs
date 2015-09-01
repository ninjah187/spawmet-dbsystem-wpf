using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class StandardPartSetElement : IModelElement, INotifyPropertyChanged
    {
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
                    _part = value;
                    NotifyPropertyChanged("Part");
                }
            }
        }

        public virtual Machine Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    NotifyPropertyChanged("Machine");
                }
            }
        }

        private int _id;
        private int _amount;

        private Part _part;
        private Machine _machine;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
