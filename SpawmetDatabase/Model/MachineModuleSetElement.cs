using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class MachineModuleSetElement : EditableModelElement<MachineModuleSetElement>
    {
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

        public int Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual MachineModule MachineModule
        {
            get { return _machineModule; }
            set
            {
                if (_machineModule != value)
                {
                    _machineModule = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _id;
        private int _amount;

        private Part _part;
        private MachineModule _machineModule;

        public override void UndoChanges()
        {
            var backup = BackupElement;

            Id = backup.Id;
            Amount = backup.Amount;
            Part = backup.Part;
            MachineModule = backup.MachineModule;
        }
    }
}
