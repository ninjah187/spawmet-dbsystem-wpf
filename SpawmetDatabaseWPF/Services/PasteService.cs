using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SpawmetDatabase;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.Utilities;

namespace SpawmetDatabaseWPF.Services
{
    public class PasteService : IPasteService
    {
        // property injection here
        public ICopyService CopyService { get; set; }

        public IDbContextMediator DbContextMediator { get; set; }
        public DbContextChangedHandler ContextChangedHandler { get; set; }

        protected Dictionary<Type, Action<int>> ActionsDictionary { get; set; }

        public PasteService()
        {
            DbContextMediator = (DbContextMediator) Application.Current.Properties["DbContextMediator"];
            DbContextMediator.Subscribers.Add(this);

            Initialize();
        }

        ~PasteService()
        {
            DbContextMediator.Subscribers.Remove(this);
        }

        private void Initialize()
        {
            ActionsDictionary = new Dictionary<Type, Action<int>>
            {
                { typeof (MachineModule), PasteModules }
            };
        }

        public bool CanPaste<T>()
        {
            if (CopyService.Tray == null || CopyService.StoredType != typeof (T))
            {
                return false;
            }
            return true;
        }

        public void Paste<T>(int targetId)
        {
            if (CanPaste<T>())
            {
                ActionsDictionary[typeof (T)](targetId);
            }
        }

        public void PasteModules(int targetMachineId)
        {
            using (var context = new SpawmetDBContext())
            {
                var ids = CopyService.Tray;

                var modules = context.MachineModules
                    .Where(m => ids.Contains(m.Id))
                    .Include(m => m.MachineModulePartSet)
                    .ToList();

                var targetMachine = context.Machines.Single(m => m.Id == targetMachineId);

                foreach (var module in modules)
                {
                    var newModule = new MachineModule()
                    {
                        Name = module.Name,
                        Price = module.Price,
                        Machine = targetMachine
                    };

                    foreach (var element in module.MachineModulePartSet.ToList())
                    {
                        var newElement = new MachineModuleSetElement()
                        {
                            MachineModule = newModule,
                            Part = element.Part,
                            Amount = element.Amount
                        };

                        newModule.MachineModulePartSet.Add(newElement);
                    }

                    context.MachineModules.Add(newModule);
                }

                context.SaveChanges();
            }

            Application.Current.Dispatcher.Invoke(() => DbContextMediator.NotifyContextChanged(this));
        }

    }
}
