using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    // ATTENTION: this class is using SHALLOW COPING
    // for now it seems that its good enough, but beware of troubles in future
    // maybe deep coping will be needed
    public abstract class EditableModelElement<T> : IModelElement, IEditableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract int Id { get; set; }

        protected T BackupElement;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit)
            {
                return;
            }
            _inEdit = true;
            BackupElement = (T) MemberwiseClone();
        }

        public void CancelEdit()
        {
            if (_inEdit == false)
            {
                return;
            }
            _inEdit = false;
            UndoChanges();
        }

        public void EndEdit()
        {
            if (_inEdit == false)
            {
                return;
            }
            _inEdit = false;
            BackupElement = default(T);
        }

        public abstract void UndoChanges();

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
