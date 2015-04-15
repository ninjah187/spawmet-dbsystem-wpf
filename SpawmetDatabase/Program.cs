using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;
using System.Security.Cryptography;

namespace SpawmetDatabase
{
    class Program
    {
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            //using (var context = new SpawmetDBContext())
            //{
            //    context.Database.ExecuteSqlCommand("DELETE FROM Parts WHERE id=1");
            //}

            InitializeDB();

            using (var context = new SpawmetDBContext())
            {
                foreach (var order in context.Orders.ToList())
                {
                    Console.WriteLine("ID zamówienia: " + order.Id);
                    Console.WriteLine("Data złożenia: " + order.StartDate);
                    Console.WriteLine("Data wysyłki: " + order.SendDate);
                    Console.WriteLine("Status: " + order.Status);

                    var clientName = order.Client != null ? order.Client.Name : "";
                    Console.WriteLine("Nazwa klienta: " + clientName);

                    var machineName = order.Machine != null ? order.Machine.Name : "";
                    Console.WriteLine("Nazwa maszyny: " + machineName);

                    Console.WriteLine("Podstawowe części maszyny: ");
                    if (order.Machine != null)
                    {
                        foreach (var partSet in order.Machine.StandardPartSet)
                        {
                            Console.WriteLine("-- " + partSet.Part.Name);
                        }
                    }
                    Console.WriteLine("Dodatkowe części: ");
                    foreach (var partSet in order.AdditionalPartSet)
                    {
                        Console.WriteLine("-- " + partSet.Part.Name);
                    }
                    Console.WriteLine();
                }

                foreach (var user in context.Users.ToList())
                {
                    Console.WriteLine("Id: " + user.Id);
                    Console.WriteLine("Login: " + user.Login);
                    Console.WriteLine("Password sha512: " + user.Password);
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        private static void InitializeDB()
        {
            using (var context = new SpawmetDBContext())
            {
                context.Database.CreateIfNotExists();

                if (context.Users.Count() == 0)
                {
                    var admin = new User()
                    {
                        Group = UserGroup.Admin,
                        Login = "admin",
                        Password = Security.GetHash("pwd")
                    };
                    var user = new User()
                    {
                        Group = UserGroup.User,
                        Login = "user1",
                        Password = Security.GetHash("pwd2")
                    };
                    var user2 = new User()
                    {
                        Group = UserGroup.User,
                        Login = "ąćęóżźczęść",
                        Password = Security.GetHash("pwd3")
                    };
                    context.Users.Add(admin);
                    context.SaveChanges();
                    context.Users.Add(user);
                    context.Users.Add(user2);
                    context.SaveChanges();
                }

                if (context.Clients.Count() == 0)
                {
                    var clients = new List<Client>();
                    for (int i = 0; i < 100; i++)
                    {
                        clients.Add(new Client()
                        {
                            Name = "klient " + i,
                            City = "miasto " + i,
                            Email = "email@poczta" + i + ".pl",
                            Nip = i.ToString() + i + i + i + i,
                            Phone = "0123215" + i,
                            Street = "ulica " + i,
                            PostalCode = "89-32" + i,
                        });
                    }
                    context.Clients.AddRange(clients);
                    context.SaveChanges();
                }
                if (context.Parts.Count() == 0)
                {
                    var parts = new List<Part>();
                    for (int i = 0; i < 100; i++)
                    {
                        parts.Add(new Part()
                        {
                            Name = "część " + i,
                            Amount = i,
                            Origin = Origin.Production,
                        });
                    }
                    context.Parts.AddRange(parts);
                    context.SaveChanges();
                }
                if (context.Deliveries.Count() == 0)
                {
                    var deliveries = new List<Delivery>();
                    for (int i = 0; i < 100; i++)
                    {
                        var parts = context.Parts.ToList();
                        int partsCount = random.Next(parts.Count);
                        var deliveryParts = new List<Part>();
                        for (int j = 0; j < partsCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            deliveryParts.Add(part);
                            parts.RemoveAt(index);
                        }
                        deliveries.Add(new Delivery()
                        {
                            Name = "dostawa " + i,
                            Date = DateTime.Now,
                            Parts = deliveryParts,
                        });
                    }
                    context.Deliveries.AddRange(deliveries);
                    context.SaveChanges();
                }
                if (context.Machines.Count() == 0)
                {
                    var machines = new List<Machine>();
                    for (int i = 0; i < 100; i++)
                    {
                        var parts = context.Parts.ToList();
                        int partsCount = random.Next(parts.Count);
                        var standardPartSet = new List<StandardPartSetElement>();
                        for (int j = 0; j < partsCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            var partSetElement = new StandardPartSetElement()
                            {
                                Part = part,
                                Amount = random.Next(1001),
                            };
                            standardPartSet.Add(partSetElement);
                            parts.RemoveAt(index);
                        }

                        machines.Add(new Machine()
                        {
                            Name = "maszyna " + i,
                            Price = random.Next(10000),
                            StandardPartSet = standardPartSet,
                        });
                    }
                    context.Machines.AddRange(machines);
                    context.SaveChanges();
                }
                if (context.Orders.Count() == 0)
                {
                    var orders = new List<Order>();
                    for (int i = 0; i < 100; i++)
                    {
                        var parts = context.Parts.ToList();
                        var randomClient = context.Clients.Find(random.Next(context.Clients.Count()));
                        var randomMachine = context.Machines.Find(random.Next(context.Machines.Count()));

                        var additionalPartSet = new List<AdditionalPartSetElement>();
                        int partCount = random.Next(parts.Count);
                        for (int j = 0; j < partCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            var partSetElement = new AdditionalPartSetElement()
                            {
                                Part = part,
                                Amount = random.Next(1001),
                            };
                            additionalPartSet.Add(partSetElement);
                            parts.RemoveAt(index);
                        }

                        orders.Add(new Order()
                        {
                            Remarks = "Kilka uwag na temat zamówienia numer " + i,
                            StartDate = DateTime.Now,
                            SendDate = DateTime.Now,
                            Status = OrderStatus.Start,
                            Client = randomClient,
                            Machine = randomMachine,
                            AdditionalPartSet = additionalPartSet,
                        });
                    }
                    context.Orders.AddRange(orders);
                    context.SaveChanges();
                }

            }
        }

    }
}
