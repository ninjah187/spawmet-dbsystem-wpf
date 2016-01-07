using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class Period : EditableModelElement<Period>
    {
        public Period()
        {
            _orders = new HashSet<Order>();
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
        public DateTime Start
        {
            get
            {
                return _start;
            }
            set
            {
                if (_start != value)
                {
                    _start = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DateTime End
        {
            get
            {
                return _end;
            }
            set
            {
                if (_end != value)
                {
                    _end = value;
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
        private DateTime _start;
        private DateTime _end;

        private ICollection<Order> _orders;

        public override void UndoChanges()
        {
            var backup = BackupElement;

            Id = backup.Id;
            Start = backup.Start;
            End = backup.End;
        }
    }
}
