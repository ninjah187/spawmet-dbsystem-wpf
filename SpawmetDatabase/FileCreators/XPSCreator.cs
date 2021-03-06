﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using SpawmetDatabase.Model;

namespace SpawmetDatabase.FileCreators
{
    public class XPSCreator : DocXCreator
    {
        public override void Create(Machine machine, string savePath)
        {
            Create(machine.AsEnumerable(), savePath);
        }

        // Creates a XPS by converting it from temporary created docx file.
        public override void Create(IEnumerable<Machine> machines, string savePath)
        {
            base.Create(machines, @".\temp.docx");

            string sourcePath = "";

            var sourceFile = File.Open(@".\temp.docx", FileMode.Open);
            sourcePath = sourceFile.Name;
            sourceFile.Close();
            sourceFile.Dispose();

            SaveXPS(sourcePath, savePath);
        }

        public override void Create(Order order, string savePath)
        {
            Create(order.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Order> orders, string savePath)
        {
            base.Create(orders, @".\temp.docx");

            string sourcePath = "";

            var sourceFile = File.Open(@".\temp.docx", FileMode.Open);
            sourcePath = sourceFile.Name;
            sourceFile.Close();
            sourceFile.Dispose();

            SaveXPS(sourcePath, savePath);
        }

        public override void Create(Part part, string savePath)
        {
            Create(part.AsEnumerable(), savePath);
        }

        public override void Create(IEnumerable<Part> parts, string savePath)
        {
            base.Create(parts, @".\temp.docx");

            string sourcePath = "";

            var sourceFile = File.Open(@".\temp.docx", FileMode.Open);
            sourcePath = sourceFile.Name;
            sourceFile.Close();
            sourceFile.Dispose();

            SaveXPS(sourcePath, savePath);
        }

        protected void SaveXPS(string sourcePath, string savePath)
        {
            // Create an instance of the Word ApplicationClass object:
            var wordApplication = new Application();
            Document wordDocument = null;

            object paramSourceDocPath = sourcePath;
            object paramMissing = Type.Missing;

            var paramExportFilePath = savePath;
            var paramExportFormat = WdExportFormat.wdExportFormatXPS;
            var paramOpenAfterExport = false;
            var paramExportOptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;
            var paramExportRange = WdExportRange.wdExportAllDocument;
            int paramStartPage = 0;
            int paramEndPage = 0;
            var paramExportItem = WdExportItem.wdExportDocumentContent;
            var paramIncludeDocProps = true;
            var paramKeepIRM = true;
            var paramCreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;
            var paramDoStructureTags = true;
            var paramBitmapMissingFonts = true;
            var paramUseISO19005_1 = false;

            try
            {
                // Open the source document.
                wordDocument = wordApplication.Documents.Open(
                    ref paramSourceDocPath, ref paramMissing, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing,
                    ref paramMissing, ref paramMissing, ref paramMissing,
                    ref paramMissing);

                // Export it in the specified format:
                if (wordDocument != null)
                {
                    wordDocument.ExportAsFixedFormat(paramExportFilePath,
                        paramExportFormat, paramOpenAfterExport,
                        paramExportOptimizeFor, paramExportRange, paramStartPage,
                        paramEndPage, paramExportItem, paramIncludeDocProps,
                        paramKeepIRM, paramCreateBookmarks, paramDoStructureTags,
                        paramBitmapMissingFonts, paramUseISO19005_1,
                        ref paramMissing);
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                // Close and release the Document object:
                if (wordDocument != null)
                {
                    wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordDocument = null;
                }

                // Quit Word and release the ApplicationClass object:
                if (wordApplication != null)
                {
                    //wordApplication.NormalTemplate.Save();
                    wordApplication.NormalTemplate.Saved = true;
                    wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                    wordApplication = null;
                }
            }

            File.Delete(@".\temp.docx");
        }
    }
}
