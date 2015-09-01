using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class MachineModule : EditableModelElement<MachineModule>
    {
        public MachineModule()
        {
            MachineModulePartSet = new HashSet<MachineModuleSetElement>();
        }

        public override int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual ICollection<MachineModuleSetElement> MachineModulePartSet
        {
            get { return _machineModulePartSet; }
            set
            {
                if (_machineModulePartSet != value)
                {
                    _machineModulePartSet = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                }
            }
        }

        private int _id;
        private string _name;

        private Machine _machine;
        private ICollection<MachineModuleSetElement> _machineModulePartSet;
        private ICollection<Order> _orders;

        public override void UndoChanges()
        {
            var backup = BackupElement;

            Id = backup.Id;
            Name = backup.Name;
            Machine = backup.Machine;
            MachineModulePartSet = backup.MachineModulePartSet;
        }
    }
}
