using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class ArchivedMachineModule : PropertyChangedNotifier
    {
        public ArchivedMachineModule()
            : this(null)
        {
        }

        public ArchivedMachineModule(MachineModule m)
        {
            Parts = new HashSet<ArchivedMachineModuleSetElement>();

            if (m == null)
            {
                return;
            }

            _oldId = m.Id;
            _name = m.Name;

            foreach (var element in m.MachineModulePartSet)
            {
                var e = new ArchivedMachineModuleSetElement(element);
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

        public virtual ArchivedOrder Order
        {
            get { return _order; }
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ICollection<ArchivedMachineModuleSetElement> Parts
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
        private string _name;

        private ICollection<ArchivedMachineModuleSetElement> _parts;

        private ArchivedOrder _order;
    }
}
