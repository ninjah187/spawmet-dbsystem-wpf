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
        private DocX _doc;

        public DocXCreator()
            : base()
        {
        }

        protected void PrepareFile(string savePath)
        {
            if (savePath == "")
            {
                throw new InvalidOperationException("Path must not be empty string.");
            }

            _doc = DocX.Create(savePath);

            string watermark = "SPAW-MET, " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            _doc.AddHeaders();
            _doc.Headers.even.InsertParagraph(watermark)
                .Alignment = Alignment.right;
            _doc.Headers.odd.InsertParagraph(watermark)
                .Alignment = Alignment.right;

            _doc.AddFooters();
            _doc.Footers.even.InsertParagraph(watermark)
                .Alignment = Alignment.right;
            _doc.Footers.odd.InsertParagraph(watermark)
                .Alignment = Alignment.right;
        }

        public override void Create(Machine machine, string savePath)
        {
            Create(machine.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Machine> machines, string savePath)
        {
            PrepareFile(savePath);

            foreach (var machine in machines)
            {
                //var footerParagraph = //doc.InsertParagraph(footer, false, footerFormatting);
                //footerParagraph.Alignment = Alignment.right};

                var headerParagraph = _doc.InsertParagraph(machine.Name, false, _machineFormatting);
                headerParagraph.Alignment = Alignment.both;

                // Add empty paragraph for table spacing:
                _doc.InsertParagraph("");

                // Create table containing info about parts:
                var table = _doc.AddTable(machine.StandardPartSet.Count + 1, 2);

                // Set width for cells:
                foreach (var row in table.Rows)
                {
                    row.Cells[0].Width = 0.8d * _doc.PageWidth;
                    row.Cells[1].Width = 0.2d * _doc.PageWidth;
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

                _doc.InsertTable(table);

                _doc.InsertParagraph("");
                _doc.InsertParagraph("");
            }

            _doc.Save();
            _doc.Dispose();
        }

        public override void Create(Order order, string savePath)
        {
            Create(order.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Order> orders, string savePath)
        {
            PrepareFile(savePath);

            foreach (var order in orders)
            {
                //string headerParagraphText = order.Client == null
                //    ? order.Machine.Name
                //    : order.Machine.Name + ", " + order.Client;

                string headerParagraphText = order.Machine.Name;

                var headerParagraph = _doc.InsertParagraph(headerParagraphText, false, _machineFormatting);
                headerParagraph.Alignment = Alignment.both;

                _doc.InsertParagraph("");

                var standardPartsTable = _doc.AddTable(order.Machine.StandardPartSet.Count + 1, 2);

                // Set width for cells:
                foreach (var row in standardPartsTable.Rows)
                {
                    row.Cells[0].Width = 0.8d * _doc.PageWidth;
                    row.Cells[1].Width = 0.2d * _doc.PageWidth;
                }

                // Add table columns titles:
                {
                    var nameParagraph = standardPartsTable.Rows[0].Cells[0].Paragraphs.First();
                    nameParagraph.Append("Część");
                    nameParagraph.Bold();
                    nameParagraph.Alignment = Alignment.center;

                    var amountParagraph = standardPartsTable.Rows[0].Cells[1].Paragraphs.First();
                    amountParagraph.Append("Ilość");
                    amountParagraph.Bold();
                    amountParagraph.Alignment = Alignment.center;
                }

                // Fill table with content:
                int rowCount = 1;
                foreach (var element in order.Machine.StandardPartSet)
                {
                    var nameParagraph = standardPartsTable.Rows[rowCount].Cells[0].Paragraphs.First();
                    nameParagraph.Append(element.Part.Name);
                    nameParagraph.Alignment = Alignment.left;

                    var amountParagraph = standardPartsTable.Rows[rowCount].Cells[1].Paragraphs.First();
                    amountParagraph.Append(element.Amount.ToString());
                    amountParagraph.Alignment = Alignment.right;

                    rowCount++;
                }

                _doc.InsertTable(standardPartsTable);

                _doc.InsertParagraph("");
                _doc.InsertParagraph("");

                // Insert additional parts table:
                var additionalPartsTable = _doc.AddTable(order.AdditionalPartSet.Count + 1, 2);

                // Set width for cells:
                foreach (var row in additionalPartsTable.Rows)
                {
                    row.Cells[0].Width = 0.8d * _doc.PageWidth;
                    row.Cells[1].Width = 0.2d * _doc.PageWidth;
                }

                // Add table columns titles:
                {
                    var nameParagraph = additionalPartsTable.Rows[0].Cells[0].Paragraphs.First();
                    nameParagraph.Append("Dodatkowa część");
                    nameParagraph.Bold();
                    nameParagraph.Alignment = Alignment.center;

                    var amountParagraph = additionalPartsTable.Rows[0].Cells[1].Paragraphs.First();
                    amountParagraph.Append("Ilość");
                    amountParagraph.Bold();
                    amountParagraph.Alignment = Alignment.center;
                }

                // Fill table with content:
                rowCount = 1;
                foreach (var element in order.AdditionalPartSet)
                {
                    var nameParagraph = additionalPartsTable.Rows[rowCount].Cells[0].Paragraphs.First();
                    nameParagraph.Append(element.Part.Name);
                    nameParagraph.Alignment = Alignment.left;

                    var amountParagraph = additionalPartsTable.Rows[rowCount].Cells[1].Paragraphs.First();
                    amountParagraph.Append(element.Amount.ToString());
                    amountParagraph.Alignment = Alignment.right;

                    rowCount++;
                }

                _doc.InsertTable(additionalPartsTable);

                _doc.InsertParagraph("");
                _doc.InsertParagraph("");
            }

            _doc.Save();
            _doc.Dispose();
        }

        public override void Create(Part part, string savePath)
        {
            Create(part.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Part> parts, string savePath)
        {
            PrepareFile(savePath);

            var headerParagraph = _doc.InsertParagraph("Raport z magazynu", false, _machineFormatting);
            headerParagraph.Alignment = Alignment.both;

            _doc.InsertParagraph("");

            var table = _doc.AddTable(parts.Count() + 1, 2);

            // Set width for cells:
            foreach (var row in table.Rows)
            {
                row.Cells[0].Width = 0.8d * _doc.PageWidth;
                row.Cells[1].Width = 0.2d * _doc.PageWidth;
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
            foreach (var part in parts)
            {
                var nameParagraph = table.Rows[rowCount].Cells[0].Paragraphs.First();
                nameParagraph.Append(part.Name);
                nameParagraph.Alignment = Alignment.left;

                var amountParagraph = table.Rows[rowCount].Cells[1].Paragraphs.First();
                amountParagraph.Append(part.Amount.ToString());
                amountParagraph.Alignment = Alignment.right;

                rowCount++;
            }

            _doc.InsertTable(table);

            _doc.InsertParagraph("");
            _doc.InsertParagraph("");

            _doc.Save();
            _doc.Dispose();
        }
    }
}
