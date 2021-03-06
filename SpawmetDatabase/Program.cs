﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using SpawmetDatabase.Scripts;
using System.Drawing.Printing;
using System.IO;
using Microsoft.Office.Interop.Word;
using SpawmetDatabase.FileCreators;

namespace SpawmetDatabase
{
    class Program
    {
        private static readonly Random random = new Random();

        private const int dbSize = 10;

        static void Main(string[] args)
        {
            Console.WriteLine(@"To start program, please run SpawmetDatabaseWPF.exe.");
            //CreateDb();

            //Console.Write("Podaj ścieżkę dostępu: ");
            //string path = Console.ReadLine();
            //PCDatabaseConnection.AddMachineBetter(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Pługi śnieżne");
            //var crawler = new DirectoryCrawler(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Widły z krokodylem");

            //// włóka łąkowa
            //using (var parser = new MachinePathParser(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Widły z krokodylem"))
            //{
            //    parser.MachineAdded += (sender, e) =>
            //    {
            //        Console.Clear();
            //        Console.WriteLine("Dodano maszynę:\n" + e.Name);
            //        Console.WriteLine();
            //    };
            //    parser.PartAdded += (sender, e) =>
            //    {
            //        Console.WriteLine("\tDodano część:\n\t" + e.Name);
            //        Console.WriteLine();
            //    };
            //    parser.StandardPartSetElementAdded += (sender, e) =>
            //    {
            //        string txt = e.Machine.Name + " <- " + e.Part.Name;
            //        Console.WriteLine("\t\tDodano zestaw:\n\t\t" + txt);
            //        Console.WriteLine();
            //    };
            //    parser.ParserRunCompleted += (sender, e) =>
            //    {
            //        Console.Clear();
            //        Console.WriteLine("Zakończono. Czas operacji: " + e.TimeElapsed);
            //    };

            //    /////////////////////////////////////////////////////
            //    /// zmień w parserze wybieranie indeksu "do wypalania"
            //    /// D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Szuflokrokodyle\....S\szuflokrokodyl HD\odkręcane boczki\1,0m; 4 zębów\Do wypalania 1,0 sob
            //    /// /////////////////////////////////////////////////

            //    parser.Parse();
            //}

            //var sw = new Stopwatch();
            //sw.Start();
            //foreach (var dir in crawler.EnumerateSubdirectories(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Pługi śnieżne", "do wypalania"))
            //{
            //    Console.WriteLine(dir);
            //}
            //sw.Stop();
            //Console.WriteLine("Get speed: " + sw.Elapsed);

            //crawler.GetFilesFromDirectories(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Pługi śnieżne\PS72, PSS75\strzałka\standard\2,6m\do wypalania");
            //crawler.GetFilesFromDirectories(@"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Widły z krokodylem\.....Mini\0,78m; 1 siłownik; 5 zębów\Do wypalania");

            //foreach (
            //    var file in
            //        crawler.GetFilesFromDirectories(
            //            @"D:\2015.05.14 - 14 maja 2015\SPAW-MET new\Pługi śnieżne\PS72, PSS75\strzałka\standard\2,6m\do wypalania"))
            //{
            //    Console.WriteLine(file);
            //}

            //crawler.Dispose();

            //PCDatabaseConnection.AddMachine(@"D:\Widły z krokodylem");

            //Console.WriteLine(OrderStatus.Done.GetDescription());

            //var yearBegin = new DateTime(DateTime.Now.Year, 1, 1);

            //var step = new TimeSpan(1, 0, 0, 0);

            //while (yearBegin.DayOfWeek != DayOfWeek.Monday)
            //{
            //    yearBegin += step;
            //}

            //var yearEnd = new DateTime(DateTime.Now.Year, 12, 31);
            //var timeSpan = new TimeSpan(7, 0, 0, 0);

            //var temp = yearBegin;
            //int i = 1;
            //while (temp.Year != DateTime.Now.Year + 1)
            //{
            //    Console.WriteLine("Tydzień " + i++ + ": " + temp.ToShortDateString());
            //    temp += timeSpan;
            //}

            //Console.ReadKey();

            //var mainProcess = Process.Start(@".\SpawmetDatabaseWPF.exe");



            //Process.GetCurrentProcess().Close();

            //CreateDb();
            ////Database.SetInitializer<SpawmetDBContext>(null);

            //using (var context = new SpawmetDBContext())
            //{
            //    context.Database.ExecuteSqlCommand("DELETE FROM Parts WHERE id=1");
            //}

            //using (var context = new SpawmetDBContext())
            //{
            //    foreach (var arch in context.ArchivedOrders.ToList())
            //    {
            //        Console.WriteLine("Części:\nMaszyna:");
            //        foreach (var p in arch.Machine.Parts.ToList())
            //        {
            //            Console.WriteLine(" - " + p.PartName + ", x" + p.Amount);
            //        }
            //        Console.WriteLine("Moduły:");
            //        foreach (var m in arch.Modules)
            //        {
            //            Console.WriteLine(" - " + m.Name);
            //            foreach (var p in m.Parts)
            //            {
            //                Console.WriteLine("   - " + p.PartName + ", x" + p.Amount);
            //            }
            //        }
            //        Console.WriteLine("Dodatkowe części:");
            //        foreach (var p in arch.Parts)
            //        {
            //            Console.WriteLine(" - " + p.PartName + ", x" + p.Amount);
            //        }
            //        Console.WriteLine();
            //    }
            //}

            //var sw = new Stopwatch();
            //Console.WriteLine("Rozpoczęcie inicjalizacji.");
            //Console.WriteLine("T: " + DateTime.Now);
            //sw.Start();
            //InitializeDB();
            //sw.Stop();
            //Console.WriteLine("Zakończono.");
            //Console.WriteLine("T: " + DateTime.Now);
            //Console.WriteLine("Czas operacji: " + sw.Elapsed);

            //using (var context = new SpawmetDBContext())
            //{
            //    var part = context.Parts.First();

            //    Console.WriteLine("ID: " + part.Id);
            //    Console.WriteLine("Nazwa: " + part.Name);
            //    Console.WriteLine("Ilość: " + part.Amount);

            //    foreach (var standardPartSetElement in part.StandardPartSets)
            //    {
            //        Console.WriteLine(standardPartSetElement.Machine.Name);
            //    }

            //foreach (var order in context.Orders.ToList())
            //{
            //    Console.WriteLine("ID zamówienia: " + order.Id);
            //    Console.WriteLine("Data złożenia: " + order.StartDate);
            //    Console.WriteLine("Data wysyłki: " + order.SendDate);
            //    Console.WriteLine("Status: " + order.Status);

            //    var clientName = order.Client != null ? order.Client.Name : "";
            //    Console.WriteLine("Nazwa klienta: " + clientName);

            //    var machineName = order.Machine != null ? order.Machine.Name : "";
            //    Console.WriteLine("Nazwa maszyny: " + machineName);

            //    Console.WriteLine("Podstawowe części maszyny: ");
            //    if (order.Machine != null)
            //    {
            //        foreach (var partSet in order.Machine.StandardPartSet)
            //        {
            //            Console.WriteLine("-- " + partSet.Part.Name);
            //        }
            //    }
            //    Console.WriteLine("Dodatkowe części: ");
            //    foreach (var partSet in order.AdditionalPartSet)
            //    {
            //        Console.WriteLine("-- " + partSet.Part.Name);
            //    }
            //    Console.WriteLine();
            //}

            //foreach (var user in context.Users.ToList())
            //{
            //    Console.WriteLine("Id: " + user.Id);
            //    Console.WriteLine("Login: " + user.Login);
            //    Console.WriteLine("Password sha512: " + user.Password);
            //    Console.WriteLine();
            //}
            //Console.WriteLine();
            //}

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
                    for (int i = 0; i < dbSize; i++)
                    {
                        var city = "miasto " + i;
                        var street = "ulica " + i;
                        var postalCode = "89-32" + i;
                        var address = city + ", " + street + ", " + postalCode;
                        clients.Add(new Client()
                        {
                            Name = "klient " + i,
                            Email = "email@poczta" + i + ".pl",
                            Nip = i.ToString() + i + i + i + i,
                            Phone = "0123215" + i,
                            Address = address,
                        });
                    }
                    context.Clients.AddRange(clients);
                    context.SaveChanges();
                }
                if (context.Parts.Count() == 0)
                {
                    var parts = new List<Part>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        var test = random.Next(2);
                        var origin = test % 2 == 0 ? Origin.Production : Origin.Outside;
                        parts.Add(new Part()
                        {
                            Name = "część " + i,
                            Amount = random.Next(10001),
                            Origin = origin,
                        });
                    }
                    context.Parts.AddRange(parts);
                    context.SaveChanges();
                }
                if (context.Deliveries.Count() == 0)
                {
                    var deliveries = new List<Delivery>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        deliveries.Add(new Delivery()
                        {
                            Name = "dostawa " + i,
                            Date = DateTime.Now
                        });
                    }
                    context.Deliveries.AddRange(deliveries);
                    context.SaveChanges();
                }
                if (context.DeliveryPartSets.Count() == 0)
                {
                    var deliveryPartSets = new List<DeliveryPartSetElement>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        var parts = context.Parts.ToList();
                        int partsCount = random.Next(parts.Count);
                        var delivery = context.Deliveries.Find(i + 1);
                        for (int j = 0; j < partsCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            var partSetElement = new DeliveryPartSetElement()
                            {
                                Delivery = delivery,
                                Part = part,
                                Amount = random.Next(1001)
                            };
                            deliveryPartSets.Add(partSetElement);
                            parts.RemoveAt(index);
                        }
                    }
                    context.DeliveryPartSets.AddRange(deliveryPartSets);
                    context.SaveChanges();
                }
                if (context.Machines.Count() == 0)
                {
                    var machines = new List<Machine>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        machines.Add(new Machine()
                        {
                            Name = "maszyna " + i,
                            //Price = random.Next(10000),
                            //StandardPartSet = standardPartSet,
                        });
                    }
                    context.Machines.AddRange(machines);
                    context.SaveChanges();
                }
                if (context.StandardPartSets.Count() == 0)
                {
                    var standardPartSets = new List<StandardPartSetElement>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        var parts = context.Parts.ToList();
                        int partsCount = random.Next(parts.Count);
                        var machine = context.Machines.Find(i + 1);
                        for (int j = 0; j < partsCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            var partSetElement = new StandardPartSetElement()
                            {
                                Machine = machine,
                                Part = part,
                                Amount = random.Next(1001)
                            };
                            standardPartSets.Add(partSetElement);
                            parts.RemoveAt(index);
                        }
                    }
                    context.StandardPartSets.AddRange(standardPartSets);
                    context.SaveChanges();
                }
                if (context.Orders.Count() == 0)
                {
                    var orders = new List<Order>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        var parts = context.Parts.ToList();
                        var randomClient = context.Clients.Find(random.Next(context.Clients.Count()) + 1);
                        var randomMachine = context.Machines.Find(random.Next(context.Machines.Count()) + 1);

                        var status = random.Next(3);

                        orders.Add(new Order()
                        {
                            Remarks = "Kilka uwag na temat zamówienia numer " + i,
                            StartDate = DateTime.Now,
                            SendDate = DateTime.Now,
                            Status = (OrderStatus) status,
                            Client = randomClient,
                            Machine = randomMachine,
                            //AdditionalPartSet = additionalPartSet,
                        });
                    }
                    context.Orders.AddRange(orders);
                    context.SaveChanges();
                }
                if (context.AdditionalPartSets.Count() == 0)
                {
                    var additionalPartSets = new List<AdditionalPartSetElement>();
                    for (int i = 0; i < dbSize; i++)
                    {
                        var parts = context.Parts.ToList();
                        int partsCount = random.Next(parts.Count);
                        var order = context.Orders.Find(i + 1);
                        for (int j = 0; j < partsCount; j++)
                        {
                            int index = random.Next(parts.Count);
                            var part = parts[index];
                            var partSetElement = new AdditionalPartSetElement()
                            {
                                Order = order,
                                Part = part,
                                Amount = random.Next(1001)
                            };
                            additionalPartSets.Add(partSetElement);
                            parts.RemoveAt(index);
                        }
                    }
                    context.AdditionalPartSets.AddRange(additionalPartSets);
                    context.SaveChanges();
                }

            }
        }

        private static void CreateDb()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<SpawmetDBContext>());
            using (var context = new SpawmetDBContext())
            {
                Console.WriteLine(context.Parts.Count());
            }
        }

    }
}
