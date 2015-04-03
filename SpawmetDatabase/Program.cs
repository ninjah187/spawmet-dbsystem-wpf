using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
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
                        foreach (var part in order.Machine.StandardPartSet)
                        {
                            Console.WriteLine("-- " + part.Name);
                        }
                    }
                    Console.WriteLine("Dodatkowe części: ");
                    foreach (var part in order.AdditionalPartSet)
                    {
                        Console.WriteLine("-- " + part.Name);
                    }
                    Console.WriteLine();
                }
            }

            Console.ReadKey();
        }
    }
}
