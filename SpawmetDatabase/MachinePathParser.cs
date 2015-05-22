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
        public event EventHandler<DatabaseItemAddedEventArgs> MachineAdded;
        public event EventHandler<DatabaseItemAddedEventArgs> PartAdded;
        public event EventHandler<DatabaseItemAddedEventArgs> StandardPartSetElementAdded;
        public event EventHandler<ParserRunCompletedEventArgs> ParserRunCompleted;

        public string Path { get; private set; }
        public Machine Machine { get; private set; }
        public List<Part> Parts { get; private set; }

        private DirectoryCrawler _crawler;
        private string _machineName;

        private static readonly char[] TrimChars = { '.', ' ' };

        private Stopwatch _stopwatch;

        public MachinePathParser(string path)
        {
            Path = path;
            Parts = new List<Part>();

            _machineName = path.Split('\\').Last();
            _crawler = new DirectoryCrawler();
            
            _stopwatch = new Stopwatch();
        }

        public void Dispose()
        {
            _crawler.Dispose();
        }

        public void ParseAsync()
        {
            var thread = new Thread(Parse);
            thread.Start();
        }

        public void Parse()
        {
            _stopwatch.Reset();
            _stopwatch.Start();

            foreach (var file in _crawler.EnumerateFilesFromDirectories(Path, "dxf"))
            {
                //Console.WriteLine(file);
                AddElements(file);
            }

            _stopwatch.Stop();

            OnParserRunCompleted(_stopwatch.Elapsed);
        }

        private void AddElements(string partPath)
        {
            var partNameWords = new List<string>();
            var machineVariantNameWords = new List<string>();

            string[] splitPath = partPath.Split('\\');

            var tempWords = new List<string>();
            for (int i = splitPath.Length - 1; i >= 0; i--)
            {
                var s = splitPath[i];

                tempWords.Add(s.ToLower());

                if (s == _machineName)
                {
                    break;
                }
            }
            tempWords.Reverse();

            int index = tempWords.IndexOf("do wypalania");

            for (int i = 0; i < tempWords.Count; i++)
            {
                if (i < index)
                {
                    string s = tempWords[i];
                    machineVariantNameWords.Add(s);
                }
                else if (i > index)
                {
                    string s = tempWords[i];
                    partNameWords.Add(s);
                }
            }

            //int i = 0;
            //for (i = temp.Length - 1; i >= 0; i--)
            //{
            //    var s = temp[i];

            //    if (s.StartsWith("x"))
            //    {
            //        string amount = s.Substring(1, s.Length - 1);
            //        partAmount = int.Parse(amount);
            //        continue;
            //    }

            //    if (Regex.IsMatch(s, "do wypalania", RegexOptions.IgnoreCase) == false)
            //    {
            //        partNameWords.Add(s);
            //        break;
            //    }
            //}

            //for (; i >= 0; i--)
            //{
            //    var s = temp[i];

            //    if (Regex.IsMatch(s, _machineName, RegexOptions.IgnoreCase) == false)
            //    {
            //        machineVariantNameWords.Add(s);
            //    }
            //}

            string partName = "";
            int partAmount = 0;
            //partNameWords.Reverse();
            foreach (var word in partNameWords)
            {
                if (word == partNameWords.First())
                {
                    partName += word;
                }
                else
                {
                    partName += " " + word;
                }
            }
            partName = partName.Split('.')[0]; // get rid of extension *.dxf

            string machineName = "";
            //machineVariantNameWords.Reverse();
            foreach (var word in machineVariantNameWords)
            {
                if (word == partNameWords.First())
                {
                    machineName += word;
                }
                else
                {
                    machineName += " " + word;
                }
            }

            // some cosmetics:
            string[] machineNameSplit = machineName.Split(' ');
            machineName = "";
            foreach (var word in machineNameSplit)
            {
                string s = word.Trim(TrimChars);
                if ((object)word == (object)machineNameSplit.First()) // (object) to compare references, not values
                {
                    machineName += s;
                }
                else
                {
                    machineName += " " + s;
                }
            }

            string[] partNameSplit = partName.Split(' ');
            partName = "";
            foreach (var word in partNameSplit)
            {
                string s = word.Trim(TrimChars);
                if (s.StartsWith("x"))
                {
                    partAmount = int.Parse(s.Substring(1, s.Length - 1));
                    continue;
                }
                
                if ((object)word == (object)partNameSplit.First()) // (object) to compare references, not values
                {
                    partName += s;
                }
                else
                {
                    partName += " " + s;
                }
            }

            using (var context = new SpawmetDBContext())
            {
                Machine machine;
                Part part;

                if (context.Machines.Where(m => m.Name == machineName).Count() == 0)
                {
                    context.Machines.Add(machine = new Machine()
                    {
                        Name = machineName,
                        Price = 0,
                    });
                    OnMachineAdded(machine.Name);
                }
                else
                {
                    machine = context.Machines.Single(m => m.Name == machineName);
                }
                context.SaveChanges();

                if (context.Parts.Where(p => p.Name == partName).Count() == 0)
                {
                    context.Parts.Add(part = new Part()
                    {
                        Name = partName,
                        Amount = 0,
                        Origin = Origin.Production,
                    });
                    OnPartAdded(part.Name);
                }
                else
                {
                    part = context.Parts.Single(p => p.Name == partName);
                }
                context.SaveChanges();

                if (context.StandardPartSets.
                    Where(el => el.Machine.Id == machine.Id
                                && el.Part.Id == part.Id)
                    .Count() == 0)
                {
                    StandardPartSetElement element;
                    context.StandardPartSets.Add(element = new StandardPartSetElement()
                    {
                        Machine = machine,
                        Part = part,
                        Amount = partAmount,
                    });
                    OnStandardPartSetElementAdded(element.Machine.Name + "\n \t\t-> " + element.Part.Name
                        + "\n \t\t-> x" + partAmount);
                }
                context.SaveChanges();
            }
        }

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

        private void OnMachineAdded(string itemName)
        {
            if (MachineAdded != null)
            {
                MachineAdded(this, new DatabaseItemAddedEventArgs(itemName));
            }
        }

        private void OnPartAdded(string itemName)
        {
            if (PartAdded != null)
            {
                PartAdded(this, new DatabaseItemAddedEventArgs(itemName));
            }
        }

        private void OnStandardPartSetElementAdded(string itemName)
        {
            if (StandardPartSetElementAdded != null)
            {
                StandardPartSetElementAdded(this, new DatabaseItemAddedEventArgs(itemName));
            }
        }

        private void OnParserRunCompleted(TimeSpan timeElapsed)
        {
            if (ParserRunCompleted != null)
            {
                ParserRunCompleted(this, new ParserRunCompletedEventArgs(timeElapsed));
            }
        }
    }
}
