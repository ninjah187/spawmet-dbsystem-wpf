using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Novacode;

namespace SpawmetDatabase
{
    public class DocXExample
    {
        public static void CreateSampleDocument()
        {
            string fileName = @".\DocXExample.docx";

            var doc = DocX.Create(fileName);

            doc.InsertParagraph("This is paragraph.");

            doc.Save();

            doc.Dispose();
        }

        public static void CreateSampleFormattedDocument()
        {
            string fileName = @".\DocXExample.docx";
            string headlineText = "Constitution of the United States.";
            string paraOne = ""
                    + "We the People of the United States, in Order to form a more perfect Union, "
                    + "establish Justice, insure domestic Tranquility, provide for the common defence, "
                    + "promote the general Welfare, and secure the Blessings of Liberty to ourselves "
                    + "and our Posterity, do ordain and establish this Constitution for the United "
                    + "States of America.";
            
            // A formatting object for headline:
            var headLineFormat = new Formatting();
            headLineFormat.FontFamily = new FontFamily("Arial Black");
            headLineFormat.Size = 18d;
            headLineFormat.Position = 12;

            // A formatting object for normal paragraph text:
            var paraFormat = new Formatting();
            paraFormat.FontFamily = new FontFamily("Calibri");
            paraFormat.Size = 10d;

            // Create document in memory:
            var doc = DocX.Create(fileName);

            // Insert new text objects:
            doc.InsertParagraph(headlineText, false, headLineFormat);
            doc.InsertParagraph(paraOne, false, paraFormat);

            // Insert table:
            var table = doc.InsertTable(3, 2);

            table.Rows[0].Cells[0].Paragraphs.First().Append("A");

            // Save to the output directory:
            doc.Save();

            // Clean up:
            doc.Dispose();
        }
    }
}
