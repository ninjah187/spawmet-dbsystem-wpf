﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabaseWPF.ViewModel;

namespace SpawmetDatabaseWPF
{
    public class DbContextMediator
    {
        public event EventHandler<SpawmetWindowViewModel> ReloadParts;

        public event EventHandler<IDbContextChangesNotifier> ContextChanged;

        public void NotifyPartsChange(SpawmetWindowViewModel notifier)
        {
            if (ReloadParts != null)
            {
                ReloadParts(this, notifier);
            }
        }

        public void NotifyContextChange(IDbContextChangesNotifier notifier)
        {
            if (ContextChanged != null)
            {
                ContextChanged(this, notifier);
            }
        }

        public void NotifyContextChange(IDbContextChangesNotifier notifier, params Type[] ignoredTypes)
        {
            // http://blogs.msdn.com/b/vancem/archive/2006/10/01/779503.aspx
            
        }
    }
}
