using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Word;

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

        public decimal InPrice
        {
            get { return _inPrice; }
            set
            {
                if (_inPrice != value)
                {
                    _inPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public decimal Discount
        {
            get { return _discount; }
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public decimal DiscountPercentage
        {
            get { return _discountPercentage; }
            set
            {
                if (_discountPercentage != value)
                {
                    _discountPercentage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public decimal OutPrice
        {
            get { return _outPrice; }
            set
            {
                if (_outPrice != value)
                {
                    _outPrice = value;
                    NotifyPropertyChanged();
                }
            }
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
        private decimal _inPrice;
        private decimal _discount;
        private decimal _discountPercentage;
        private decimal _outPrice;
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
