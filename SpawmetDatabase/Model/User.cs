using System.ComponentModel;

namespace SpawmetDatabase.Model
{
    public class User : IModelElement, INotifyPropertyChanged
    {
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

        public string Login
        {
            get { return _login; }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    NotifyPropertyChanged("Login");
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    NotifyPropertyChanged("Password");
                }
            }
        }

        public UserGroup Group
        {
            get { return _group; }
            set
            {
                if (_group != value)
                {
                    _group = value;
                    NotifyPropertyChanged("Group");
                }
            }
        }

        private int _id;
        private string _login;
        private string _password;
        private UserGroup _group;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
