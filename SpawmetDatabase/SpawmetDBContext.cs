using System.Data.Common;
using System.Data.Entity;
using MySql.Data.Entity;
using SpawmetDatabase.Model;

namespace SpawmetDatabase
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class SpawmetDBContext : DbContext
    {
        public SpawmetDBContext()
            : base("name=SpawmetDBContext")
        {
        }

        public SpawmetDBContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<User>().MapToStoredProcedures();
            //modelBuilder.Entity<Part>().MapToStoredProcedures();
            //modelBuilder.Entity<Delivery>().MapToStoredProcedures();
            //modelBuilder.Entity<Client>().MapToStoredProcedures();
            //modelBuilder.Entity<Machine>().MapToStoredProcedures();
            //modelBuilder.Entity<Order>().MapToStoredProcedures();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<StandardPartSetElement> StandardPartSets { get; set; }
        public DbSet<AdditionalPartSetElement> AdditionalPartSets { get; set; }
        public DbSet<DeliveryPartSetElement> DeliveryPartSets { get; set; }
        public DbSet<MachineModuleSetElement> MachineModulePartSets { get; set; }
        public DbSet<MachineModule> MachineModules { get; set; }

        public DbSet<ArchivedOrder> ArchivedOrders { get; set; }
        public DbSet<ArchivedClient> ArchivedClients { get; set; }
        public DbSet<ArchivedMachine> ArchivedMachines { get; set; }
        public DbSet<ArchivedMachineModule> ArchivedModules { get; set; }
        public DbSet<ArchivedStandardPartSetElement> ArchivedStandardPartSets { get; set; }
        public DbSet<ArchivedAdditionalPartSetElement> ArchivedAdditionalPartSets { get; set; }
        public DbSet<ArchivedMachineModuleSetElement> ArchivedMachineModulePartSets { get; set; }
    }
}
