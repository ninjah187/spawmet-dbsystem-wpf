using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Exceptions
{
    public class ConnectionLostException : Exception
    {
        public ConnectionLostException()
            : this("")
        {
        }

        public ConnectionLostException(string message)
            : this(message, null)
        {
        }

        public ConnectionLostException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
