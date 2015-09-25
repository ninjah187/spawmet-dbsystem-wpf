using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabaseWPF.CommonWindows;

namespace SpawmetDatabaseWPF.Utilities
{
    /// <summary>
    /// Class that handles passing info between IDbContextChangesNotifier instances about need of update DbContext.
    /// </summary>
    public class DbContextMediator : IDbContextMediator
    {
        public ICollection<IDbContextChangesNotifier> Subscribers { get; protected set; }

        public DbContextMediator()
        {
            Subscribers = new List<IDbContextChangesNotifier>();
        }

        /// <summary>
        /// Notifies all subscribers except notifier that DbContext has changed. 
        /// </summary>
        /// <param name="notifier">Object which notifies that it changed the context.</param>
        public void NotifyContextChanged(IDbContextChangesNotifier notifier)
        {
            foreach (var sub in Subscribers)
            {
                if (sub == notifier || sub.ContextChangedHandler == null)
                {
                    continue;
                }

                sub.ContextChangedHandler();
            }
        }

        /// <summary>
        /// Notifies only subscribers which are of types targetTypes.
        /// </summary>
        /// <param name="notifier">Object which notifies that it changed the context.</param>
        /// <param name="targetTypes">Types to be notified.</param>
        public void NotifyContextChanged(IDbContextChangesNotifier notifier, params Type[] targetTypes)
        {
            // http://blogs.msdn.com/b/vancem/archive/2006/10/01/779503.aspx - ways and performance of some of reflections mechanism

            foreach (var sub in Subscribers)
            {
                if (sub == notifier || sub.ContextChangedHandler == null)
                {
                    continue;
                }

                var subscriberType = sub.GetType();
                if (targetTypes.Any(type => type == subscriberType))
                {
                    sub.ContextChangedHandler();
                }

                //var subscriberTypeHandle = Type.GetTypeHandle(sub);
                //if (targetTypes.Any(type => subscriberTypeHandle.Equals(type.TypeHandle)))
                //{
                //    sub.ContextChangedHandler();
                //}
            }
        }

    }
}
