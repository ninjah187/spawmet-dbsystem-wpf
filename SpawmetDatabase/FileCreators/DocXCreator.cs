using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novacode;
using SpawmetDatabase.Model;

namespace SpawmetDatabase.FileCreators
{
    public class DocXCreator : Creator
    {
        public DocXCreator()
            : base()
        {
        }

        public override void Create(Machine machine, string savePath)
        {
            Create(machine.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Machine> machines, string savePath)
        {
            if (savePath == "")
            {
                throw new InvalidOperationException("Path must not be empty string.");
            }

            var doc = DocX.Create(savePath);

            string watermark = "SPAW-MET, " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            doc.AddHeaders();
            doc.Headers.even.InsertParagraph(watermark)
                .Alignment = Alignment.right;
            doc.Headers.odd.InsertParagraph(watermark)
                .Alignment = Alignment.right;

            doc.AddFooters();
            doc.Footers.even.InsertParagraph(watermark)
                .Alignment = Alignment.right;
            doc.Footers.odd.InsertParagraph(watermark)
                .Alignment = Alignment.right;

            foreach (var machine in machines)
            {
                //var footerParagraph = //doc.InsertParagraph(footer, false, footerFormatting);
                //footerParagraph.Alignment = Alignment.right};

                var headerParagraph = doc.InsertParagraph(machine.Name, false, _machineFormatting);
                headerParagraph.Alignment = Alignment.both;

                // Add empty paragraph for table spacing:
                doc.InsertParagraph("");

                // Create table containing info about parts:
                var table = doc.AddTable(machine.StandardPartSet.Count + 1, 2);

                // Set width for cells:
                foreach (var row in table.Rows)
                {
                    row.Cells[0].Width = 0.8d * doc.PageWidth;
                    row.Cells[1].Width = 0.2d * doc.PageWidth;
                }

                // Add table columns titles:
                {
                    var nameParagraph = table.Rows[0].Cells[0].Paragraphs.First();
                    nameParagraph.Append("Część");
                    nameParagraph.Bold();
                    nameParagraph.Alignment = Alignment.center;

                    var amountParagraph = table.Rows[0].Cells[1].Paragraphs.First();
                    amountParagraph.Append("Ilość");
                    amountParagraph.Bold();
                    amountParagraph.Alignment = Alignment.center;
                }

                // Fill table with content:
                int rowCount = 1;
                foreach (var element in machine.StandardPartSet)
                {
                    var nameParagraph = table.Rows[rowCount].Cells[0].Paragraphs.First();
                    nameParagraph.Append(element.Part.Name);
                    nameParagraph.Alignment = Alignment.left;

                    var amountParagraph = table.Rows[rowCount].Cells[1].Paragraphs.First();
                    amountParagraph.Append(element.Amount.ToString());
                    amountParagraph.Alignment = Alignment.right;

                    rowCount++;
                }

                doc.InsertTable(table);

                doc.InsertParagraph("");
                doc.InsertParagraph("");
            }

            doc.Save();
            doc.Dispose();
        }

    }
}
