using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class AdditionalPartSetElement : IModelElement, INotifyPropertyChanged
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

        public virtual Order Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    NotifyPropertyChanged("Order");
                }
            }
        }

        private int _id;
        private int _amount;

        private Part _part;
        private Order _order;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
