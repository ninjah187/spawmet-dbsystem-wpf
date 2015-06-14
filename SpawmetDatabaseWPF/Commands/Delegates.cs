using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Commands
{
    public delegate void ExecuteCommand();

    public delegate bool CanExecuteCommand();

    public delegate void ExecuteCommand<T>(T parameter);

    public delegate bool CanExecuteCommand<T>(T parameter);
}
