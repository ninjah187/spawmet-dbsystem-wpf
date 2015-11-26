using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class ArchivedOrder : PropertyChangedNotifier, IModelElement
    {
        public ArchivedOrder()
            : this(null)
        {
        }

        public ArchivedOrder(Order o)
        {
            Modules = new HashSet<ArchivedMachineModule>();
            Parts = new HashSet<ArchivedAdditionalPartSetElement>();

            if (o == null)
            {
                return;
            }

            _oldId = o.Id;
            _remarks = o.Remarks;
            _startDate = o.StartDate.Value.ToShortDateString();
            _sendDate = o.SendDate.Value.ToShortDateString();
            _inPrice = o.InPrice.ToString();
            _discount = o.Discount.ToString();
            _discountPercentage = o.DiscountPercentage.ToString();
            _outPrice = o.OutPrice.ToString();
            _serialNumber = o.SerialNumber;

            if (o.Client != null)
            {
                _client = new ArchivedClient(o.Client);
            }
            
            _machine = new ArchivedMachine(o.Machine);

            foreach (var module in o.MachineModules)
            {
                _modules.Add(new ArchivedMachineModule(module));
            }

            foreach (var element in o.AdditionalPartSet)
            {
                var e = new ArchivedAdditionalPartSetElement(element);
                _parts.Add(e);
            }
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
                    OnPropertyChanged();
                }
            }
        }

        public string StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SendDate
        {
            get { return _sendDate; }
            set
            {
                if (_sendDate != value)
                {
                    _sendDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string InPrice
        {
            get { return _inPrice; }
            set
            {
                if (_inPrice != value)
                {
                    _inPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Discount
        {
            get { return _discount; }
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DiscountPercentage
        {
            get { return _discountPercentage; }
            set
            {
                if (_discountPercentage != value)
                {
                    _discountPercentage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OutPrice
        {
            get { return _outPrice; }
            set
            {
                if (_outPrice != value)
                {
                    _outPrice = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged("SerialNumber");
                }
            }
        }

        public virtual ArchivedClient Client
        {
            get { return _client; }
            set
            {
                if (_client != value)
                {
                    _client = value;
                    OnPropertyChanged("Client");
                }
            }
        }

        public virtual ArchivedMachine Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    OnPropertyChanged("Machine");
                }
            }
        }

        public virtual ICollection<ArchivedMachineModule> Modules
        {
            get { return _modules; }
            set
            {
                if (_modules != value)
                {
                    _modules = value;
                    OnPropertyChanged();
                }
            }
        }
        public virtual ICollection<ArchivedAdditionalPartSetElement> Parts
        {
            get { return _parts; }
            set
            {
                if (_parts != value)
                {
                    _parts = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _id;
        private int _oldId;
        private string _remarks;
        private string _startDate;
        private string _sendDate;
        private string _inPrice;
        private string _discount;
        private string _discountPercentage;
        private string _outPrice;
        private string _serialNumber;

        private ArchivedClient _client;
        private ArchivedMachine _machine;
        private ICollection<ArchivedMachineModule> _modules;
        private ICollection<ArchivedAdditionalPartSetElement> _parts;
    }
}
