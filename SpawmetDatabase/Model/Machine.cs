using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class Machine : EditableModelElement<Machine>, INotifyPropertyChanged
    {
        public Machine()
        {
            this.Orders = new HashSet<Order>();
            this.StandardPartSet = new HashSet<StandardPartSetElement>();
            this.Modules = new HashSet<MachineModule>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override int Id
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

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
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
                    NotifyPropertyChanged("Orders");
                }
            }
        }

        public virtual ICollection<StandardPartSetElement> StandardPartSet
        {
            get { return _standardPartSet; }
            set
            {
                if (_standardPartSet != value)
                {
                    _standardPartSet = value;
                    NotifyPropertyChanged("StandardPartSet");
                }
            }
        }

        private ICollection<MachineModule> _modules;
        public virtual ICollection<MachineModule> Modules
        {
            get { return _modules; }
            set
            {
                if (_modules != value)
                {
                    _modules = value;
                    NotifyPropertyChanged("Modules");
                }
            }
        }

        private int _id;
        private string _name;

        private ICollection<Order> _orders;
        private ICollection<StandardPartSetElement> _standardPartSet;
        private ICollection<MachineModule> _machineModules;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override void UndoChanges()
        {
            var backup = BackupElement;

            Id = backup.Id;
            Name = backup.Name;
        }
    }
}
