using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabase
{
    public class MachinePathParser : IDisposable
    {
        private struct PartCreateArgs
        {
            public string Name { get; set; }
            public int Amount { get; set; }
        }

        private static readonly char[] TrimChars = { '.', ' ' };

        public event EventHandler<string> DirectoryReached;
        public event EventHandler<string> FileReached;

        public event EventHandler<Machine> MachineAdded;
        public event EventHandler<Part> PartAdded;
        public event EventHandler<StandardPartSetElement> StandardPartSetElementAdded;
        public event EventHandler<MachineModule> MachineModuleAdded;
        public event EventHandler<MachineModuleSetElement> MachineModuleSetElementAdded;
        public event EventHandler<ParserRunCompletedEventArgs> ParserRunCompleted;
        //public event EventHandler<ParserRunFailedEventArgs> ParserRunFailed;

        public string Path { get; private set; }
        public Machine Machine { get; private set; }
        public List<Part> Parts { get; private set; }

        private DirectoryCrawler _crawler;
        private string _machineName;

        private Stopwatch _stopwatch;

        private Thread _thread;

        public MachinePathParser(string path)
        {
            Path = path;
            Parts = new List<Part>();

            _machineName = path.Split('\\').Last().ToLower();
            _crawler = new DirectoryCrawler();

            _crawler.DirectoryReached += (sender, dir) =>
            {
                OnDirectoryReached(dir);
            };
            _crawler.FileReached += (sender, file) =>
            {
                OnFileReached(file);
            };

            _stopwatch = new Stopwatch();

            //Console.WriteLine(_machineName);
        }

        public void Dispose()
        {
            _crawler.Dispose();
        }

        public void ParseAsync()
        {
            _thread = new Thread(Parse);
            _thread.Start();
        }

        public void Abort()
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        public void Parse()
        {
            _stopwatch.Reset();
            _stopwatch.Start();

            foreach (var file in _crawler.EnumerateFilesFromDirectories(Path, "dxf"))
            {
                AddElements(file);
            }

            _stopwatch.Stop();

            OnParserRunCompleted(_stopwatch.Elapsed);
        }

        private void AddElements(string partPath)
        {
            partPath = partPath.ToLower();

            string[] separator = { "do wypalania" };
            string[] temp = partPath.Split(separator, StringSplitOptions.None);

            string machineNamePath = temp[0];
            string partNamePath = temp[1];

            string machineName = ExcludeMachineName(machineNamePath);

            char[] sep = { '\\' };
            temp = partNamePath.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string moduleName = "";
            PartCreateArgs partArgs;
            if (temp.Length == 1)
            {
                partArgs = ExcludePartArgs(temp[0]);
            } 
            else if (temp.Length == 2)
            {
                moduleName = temp[0];
                partArgs = ExcludePartArgs(temp[1]);
            } else throw new InvalidOperationException("Problem in modules and parts parsing.\n");

            UpdateDatabase(machineName, partArgs, moduleName);
        }

        // excludes machine name from path in e. g. format: "F:\spawmet\_machineName\more\words\of\name" => _machine name more words of name
        private string ExcludeMachineName(string machineNamePath)
        {
            string[] temp = machineNamePath.Split('\\');
            string s = temp[0];
            int i = 0;

            while (s != _machineName)
            {
                i++;
                s = temp[i];
            }

            string machineName = "";
            for (int j = i; j < temp.Length; j++)
            {
                if (j == temp.Length - 1)
                {
                    machineName += temp[j];
                }
                else
                {
                    machineName += temp[j] + " ";
                }
            }

            return machineName;
        }

        // excludes arguments need in Part object creation from path in e. g. format: "part name x2.dxf" => args.name = "part name"; args.amount = 2
        private PartCreateArgs ExcludePartArgs(string partNamePath)
        {
            var args = new PartCreateArgs();

            string partNameWithAmount = partNamePath.Split('.')[0];
            string[] temp = partNameWithAmount.Split(' ');

            string partName = "";
            int amount = 0;

            for (int i = 0; i < temp.Length; i++)
            {
                string s = temp[i];
                if (s.StartsWith("x"))
                {
                    string am = s.Substring(1, s.Length - 1);
                    amount = int.Parse(am);
                }
                else
                {
                    if (i == temp.Length - 1)
                    {
                        partName += temp[i];
                    }
                    else
                    {
                        partName += temp[i] + " ";
                    }
                }
            }

            args.Amount = amount;
            args.Name = partName;

            return args;
        }

        private void UpdateDatabase(string machineName, PartCreateArgs partArgs, string moduleName)
        {
            using (var context = new SpawmetDBContext())
            {
                Machine machine;
                Part part;

                // if (context.Machines.Where(m => m.Name == machineName).Count() == 0)
                // if (context.Machines.Count(m => m.Name == machineName).Count() == 0)
                // if (context.Machines.Where(m => m.Name == machineName).Any() == false)
                if (!context.Machines.Any(m => m.Name == machineName))
                {
                    context.Machines.Add(machine = new Machine()
                    {
                        Name = machineName,
                    });
                    context.SaveChanges();
                    OnMachineAdded(machine);
                }
                else
                {
                    machine = context.Machines.Single(m => m.Name == machineName);
                }

                if (!context.Parts.Any(p => p.Name == partArgs.Name))
                {
                    context.Parts.Add(part = new Part()
                    {
                        Name = partArgs.Name,
                        Amount = 0,
                        Origin = Origin.Production,
                    });
                    context.SaveChanges();
                    OnPartAdded(part);
                }
                else
                {
                    part = context.Parts.Single(p => p.Name == partArgs.Name);
                }

                if (moduleName == "")
                {
                    if (!context.StandardPartSets.Any(e => e.Machine.Id == machine.Id &&
                                                           e.Part.Id == part.Id))
                    {
                        StandardPartSetElement element;
                        context.StandardPartSets.Add(element = new StandardPartSetElement()
                        {
                            Machine = machine,
                            Part = part,
                            Amount = partArgs.Amount,
                        });
                        context.SaveChanges();
                        OnStandardPartSetElementAdded(element);
                    }
                }
                else
                {
                    MachineModule module;
                    if (!context.MachineModules.Any(m => m.Name == moduleName && m.Machine.Id == machine.Id))
                    {
                        context.MachineModules.Add(module = new MachineModule()
                        {
                            Machine = machine,
                            Name = moduleName,
                        });
                        context.SaveChanges();
                        OnMachineModuleAdded(module);
                    }
                    else
                    {
                        module = context.MachineModules.Single(m => m.Name == moduleName && m.Machine.Id == machine.Id);
                    }

                    if (!context.MachineModulePartSets.Any(e => e.MachineModule.Id == module.Id &&
                                                                e.Part.Id == part.Id))
                    {
                        MachineModuleSetElement element;
                        context.MachineModulePartSets.Add(element = new MachineModuleSetElement()
                        {
                            MachineModule = module,
                            Part = part,
                            Amount = partArgs.Amount
                        });
                        context.SaveChanges();
                        OnMachineModuleSetElementAdded(element);
                    }
                }
            }
        }

        //    using (var context = new SpawmetDBContext())
        //    {
        //        Machine machine;
        //        Part part;

        //        if (context.Machines.Where(m => m.Name == machineName).Count() == 0)
        //        {
        //            context.Machines.Add(machine = new Machine()
        //            {
        //                Name = machineName,
        //                //Price = 0,
        //            });
        //            OnMachineAdded(machine);
        //        }
        //        else
        //        {
        //            machine = context.Machines.Single(m => m.Name == machineName);
        //        }
        //        context.SaveChanges();

        //        if (context.Parts.Where(p => p.Name == partName).Count() == 0)
        //        {
        //            context.Parts.Add(part = new Part()
        //            {
        //                Name = partName,
        //                Amount = 0,
        //                Origin = Origin.Production,
        //            });
        //            OnPartAdded(part);
        //        }
        //        else
        //        {
        //            part = context.Parts.Single(p => p.Name == partName);
        //        }
        //        context.SaveChanges();

        //        if (context.StandardPartSets.
        //            Where(el => el.Machine.Id == machine.Id
        //                        && el.Part.Id == part.Id)
        //            .Count() == 0)
        //        {
        //            StandardPartSetElement element;
        //            context.StandardPartSets.Add(element = new StandardPartSetElement()
        //            {
        //                Machine = machine,
        //                Part = part,
        //                Amount = partAmount,
        //            });
        //            OnStandardPartSetElementAdded(element);
        //        }
        //        context.SaveChanges();
        //    }
        //}

        private Part GetPart(string path)
        {
            var pathWords = new List<string>();

            string[] temp = path.Split('\\');

            var part = new Part();

            int i = 0;
            for (i = temp.Length - 1; i >= 0; i--)
            {
                var s = temp[i];
                //if (s.StartsWith("x"))
                //{
                //    string amount = s.Substring(1, s.Length - 1);
                //    part.Amount = int.Parse(amount);
                //    continue;
                //}

                if (Regex.IsMatch(s, "do wypalania", RegexOptions.IgnoreCase) == false)
                {
                    pathWords.Add(s);
                }
            }

            string partName = "";
            pathWords.Reverse();
            foreach (var word in pathWords)
            {
                if (word == pathWords.First())
                {
                    partName += word;
                }
                else
                {
                    partName += " " + word;
                }
            }

            part.Name = partName;

            return part;
        }

        private void OnMachineAdded(Machine machine)
        {
            if (MachineAdded != null)
            {
                MachineAdded(this, machine);
            }
        }

        private void OnPartAdded(Part part)
        {
            if (PartAdded != null)
            {
                PartAdded(this, part);
            }
        }

        private void OnStandardPartSetElementAdded(StandardPartSetElement element)
        {
            if (StandardPartSetElementAdded != null)
            {
                StandardPartSetElementAdded(this, element);
            }
        }

        private void OnMachineModuleAdded(MachineModule module)
        {
            if (MachineModuleAdded != null)
            {
                MachineModuleAdded(this, module);
            }
        }

        private void OnMachineModuleSetElementAdded(MachineModuleSetElement element)
        {
            if (MachineModuleSetElementAdded != null)
            {
                MachineModuleSetElementAdded(this, element);
            }
        }

        private void OnParserRunCompleted(TimeSpan timeElapsed)
        {
            if (ParserRunCompleted != null)
            {
                ParserRunCompleted(this, new ParserRunCompletedEventArgs(timeElapsed));
            }
        }

        private void OnDirectoryReached(string dir)
        {
            if (DirectoryReached != null)
            {
                DirectoryReached(this, dir);
            }
        }

        private void OnFileReached(string file)
        {
            if (FileReached != null)
            {
                FileReached(this, file);
            }
        }
    }
}
