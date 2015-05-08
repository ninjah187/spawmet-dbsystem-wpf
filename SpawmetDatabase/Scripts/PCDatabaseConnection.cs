using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SpawmetDatabase.Model;
using System.Diagnostics;

namespace SpawmetDatabase.Scripts
{
    public static class PCDatabaseConnection
    {
        public static void AddMachine(string path)
        {
            /*** Show current time and start Stopwatch. ***/
            var sw = new Stopwatch();
            var startTime = DateTime.Now;
            sw.Start();
            /**********************************************/

            var directories = Directory.EnumerateDirectories(path);

            string machineName = "";

            string machine = "";

            {
                string[] temp = path.Split('\\');
                machine = temp.Last();
            }

            foreach (string directory in directories)
            {
                string size = "";
                {
                    string[] temp = directory.Split('\\');
                    size = temp.Last();
                }

                size = size.Trim('.');

                //Console.WriteLine("Wymiar: " + size);

                machineName = machine + " " + size;
                //Console.WriteLine("Nazwa maszyny: " + machineName);

                var machineVariantsDirectories = Directory.EnumerateDirectories(directory);

                foreach (string machineVariantDirectory in machineVariantsDirectories)
                {
                    Console.WriteLine(path);
                    Console.WriteLine("Czas rozpoczęcia: " + startTime);
                    Console.WriteLine();

                    if (machineVariantDirectory.Contains("OldVersions"))
                    {
                        continue;
                    }
                    //Console.WriteLine(machineVariantDirectory);

                    string machineVariantName = "";
                    {
                        string[] temp = machineVariantDirectory.Split('\\');
                        machineVariantName = temp.Last();
                    }

                    string machineFullName = machineName + " " + machineVariantName;
                    Console.WriteLine("Nazwa maszyny: " + machineFullName);

                    /*** Create Machine entity and insert it into database. ***/
                    /*** (if it's unique)                                   ***/
                    Machine machineEntity = null;
                    using (var context = new SpawmetDBContext())
                    {
                        if (context.Machines.Where(m => m.Name == machineFullName).Count() == 0)
                        {
                            machineEntity = new Machine()
                            {
                                Name = machineName + " " + machineVariantName,
                                Price = 0,
                            };
                            context.Machines.Add(machineEntity);
                            context.SaveChanges();

                            Console.WriteLine("    * Maszyna zapisana w bazie. *\n");
                        }
                    }
                    /**********************************************************/

                    Console.WriteLine("Części:");

                    string machinePartsDirectory = machineVariantDirectory + @"\Do wypalania";
                    var machinePartsPaths = Directory.EnumerateFiles(machinePartsDirectory);
                    foreach (string machinePartPath in machinePartsPaths)
                    {
                        string machinePartName = "";
                        {
                            string[] temp = machinePartPath.Split('\\');
                            machinePartName = temp.Last();
                        }
                        machinePartName = machinePartName.Substring(0, machinePartName.Length - 4);
                        int amount = 0;
                        
                        {
                            string[] temp = machinePartName.Split(' ');
                            //string amountStr = "";
                            List<string> machinePartNameWords = new List<string>();

                            foreach (string s in temp)
                            {
                                if (s.StartsWith("x"))
                                {
                                    string amountStr = s.Substring(1, s.Length - 1);
                                    try
                                    {
                                        amount = int.Parse(amountStr);
                                    }
                                    catch (FormatException exc)
                                    {
                                        Console.WriteLine("Błąd parsowania w elemencie: " + machinePartPath);
                                        return;
                                    }
                                    //amount = s;
                                }
                                else
                                {
                                    machinePartNameWords.Add(s);
                                }
                            }

                            /*** Get rid of useless white spaces from Part name words. ***/
                            for (int i = 0; i < machinePartNameWords.Count; i++)
                            {
                                machinePartNameWords[i] = machinePartNameWords[i].Trim(' ');
                                machinePartNameWords[i] = machinePartNameWords[i].ToLower();
                            }
                            /*************************************************************/

                            machinePartName = "";
                            foreach (string s in machinePartNameWords)
                            {
                                if (s == machinePartNameWords.First())
                                {
                                    machinePartName += s;
                                }
                                else
                                {
                                    machinePartName += " " + s;
                                }
                            }

                            /*** Create Part entity and insert it into database. ***/
                            /*** (if it's unique)                                ***/
                            using (var context = new SpawmetDBContext())
                            {
                                if (context.Parts.Where(p => p.Name == machinePartName).Count() == 0)
                                {
                                    var partEntity = new Part()
                                    {
                                        Name = machinePartName,
                                        Amount = 0,
                                        Origin = Origin.Production
                                    };
                                    context.Parts.Add(partEntity);
                                    context.SaveChanges();

                                    Console.WriteLine("    * Część zapisana w bazie. * ");
                                }
                            }
                            /*******************************************************/

                            /*** Create and add StandardPartSetElement.          ***/
                            using (var context = new SpawmetDBContext())
                            {
                                var mach = context.Machines.Single(m => m.Name == machineFullName);
                                var part = context.Parts.Single(p => p.Name == machinePartName);

                                if (context.StandardPartSets
                                    .Where(el => el.Machine.Id == mach.Id && el.Part.Id == part.Id)
                                    .Count() == 0)
                                {
                                    var element = new StandardPartSetElement()
                                    {
                                        Amount = amount,
                                        Machine = mach,
                                        Part = part,
                                    };
                                    context.StandardPartSets.Add(element);
                                    context.SaveChanges();

                                    Console.WriteLine("    * Zestaw zapisany w bazie. *");
                                }
                            }
                            /*******************************************************/
                        }

                        Console.WriteLine(" - " + machinePartName);
                        Console.WriteLine("   ilość: " + amount);
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                    //Console.ReadKey();
                    Console.Clear();
                }

                Console.WriteLine();
            }

            sw.Stop();
            Console.WriteLine("Czas zakończenia: " + DateTime.Now);
            Console.WriteLine("Całkowity czas operacji: " + sw.Elapsed);
        }
    }
}
