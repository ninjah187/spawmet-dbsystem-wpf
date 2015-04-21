using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Order : ModelElement
    {
        public Order()
        {
            this.AdditionalPartSet = new HashSet<AdditionalPartSetElement>();
        }

        public OrderStatus Status
        {
            get
            {
                return _orderStatus;
            }
            set
            {
                if (_orderStatus != value)
                {
                    _orderStatus = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

        public string Remarks
        {
            get
            {
                return _remarks;
            }
            set
            {
                if (_remarks != value)
                {
                    _remarks = value;
                    NotifyPropertyChanged("Remarks");
                }
            }
        }

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    NotifyPropertyChanged("StartDate");
                }
            }
        }

        public DateTime SendDate
        {
            get { return _sendDate; }
            set
            {
                if (_sendDate != value)
                {
                    _sendDate = value;
                    NotifyPropertyChanged("SendDate");
                }
            }
        }

        public virtual Client Client
        {
            get { return _client; }
            set
            {
                if (_client != value)
                {
                    _client = value;
                    NotifyPropertyChanged("Client");
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

        public virtual ICollection<AdditionalPartSetElement> AdditionalPartSet
        {
            get { return _additionalPartSet; }
            set
            {
                if (_additionalPartSet != value)
                {
                    _additionalPartSet = value;
                    NotifyPropertyChanged("AdditionalPartSet");
                }
            }
        }

        private OrderStatus _orderStatus;
        private string _remarks;
        private DateTime _startDate;
        private DateTime _sendDate;

        private Client _client;
        private Machine _machine;
        private ICollection<AdditionalPartSetElement> _additionalPartSet;

        public string Signature
        {
            get
            {
                string clientName = Client != null ? Client.Name : "";
                string machineName = Machine != null ? Machine.Name : "";
                return clientName + ", " + machineName + ", " + StartDate.ToShortDateString();
            }
        }
    }
}
