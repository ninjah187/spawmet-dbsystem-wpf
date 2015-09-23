using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SpawmetDatabase.Model
{
    public class Order : IModelElement, INotifyPropertyChanged
    {
        public Order()
        {
            this.AdditionalPartSet = new HashSet<AdditionalPartSetElement>();
            MachineModules = new HashSet<MachineModule>();
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

        public OrderStatus? Status
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

        public string StatusDescription
        {
            get { return _orderStatus.Value.GetDescription(); }
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

        public DateTime? StartDate
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

        public DateTime? SendDate
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

        public bool ConfirmationSent
        {
            get { return _confirmationSent; }
            set
            {
                if (_confirmationSent != value)
                {
                    _confirmationSent = value;
                    NotifyPropertyChanged("ConfirmationSent");
                }
            }
        }

        public decimal Price
        {
            get
            {
                //decimal sum = 0;
                //sum += Machine.Price;
                //sum += MachineModules.ToList().Sum(module => module.Price);
                //return sum;
                return 0;
            }
            //set
            //{
            //    if (_price != value)
            //    {
            //        _price = value;
            //        NotifyPropertyChanged("Price");
            //    }
            //}
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                if (_serialNumber != value)
                {
                    _serialNumber = value;
                    NotifyPropertyChanged("SerialNumber");
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

        public virtual ICollection<MachineModule> MachineModules
        {
            get { return _machineModules; }
            set
            {
                if (_machineModules != value)
                {
                    _machineModules = value;
                    NotifyPropertyChanged("MachineModules");
                }
            }
        }

        public virtual Period Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Signature
        {
            //get
            //{
            //    string clientName = Client != null ? Client.Name : "";
            //    string machineName = Machine != null ? Machine.Name : "";
            //    return clientName + ", " + machineName + ", " + StartDate.Value.ToShortDateString();
            //}
            get
            {
                string clientName = Client != null ? Client.Name : "";
                string machineName = Machine != null ? Machine.Name : "";
                string startDate = StartDate != null ? StartDate.Value.ToString("yyyy-MM-dd") : "";

                string signature = "Klient: " + clientName
                                   + "\nMaszyna: " + machineName
                                   + "\nData przyjęcia: " + startDate;

                return signature;
            }
        }

        private int _id;
        private OrderStatus? _orderStatus;
        private string _remarks;
        private DateTime? _startDate;
        private DateTime? _sendDate;
        private bool _confirmationSent;
        private decimal _price;
        private string _serialNumber;

        private Client _client;
        private Machine _machine;
        private ICollection<MachineModule> _machineModules;
        private ICollection<AdditionalPartSetElement> _additionalPartSet;
        private Period _period;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
