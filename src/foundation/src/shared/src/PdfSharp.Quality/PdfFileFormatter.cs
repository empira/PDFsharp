using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Quality
{
    public static class PdfFileFormatter
    {
        public static void FormatDocument(string path)
        {
            try
            {
                var pdfDocument = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
                PdfObject[] objects = pdfDocument.Internals.GetAllObjects();
                for (int idx = 0; idx < objects.Length; idx++)
                {
                    if (objects[idx] is PdfDictionary dict && dict.Stream != null)
                    // ??? ReSharper creates the next line - 
                    //if (objects[idx] is PdfDictionary { Stream: { } } dict)
                    {
                        dict.Stream.TryUnfilter();
                    }
                }
                var name = Path.GetFileName(path);
                path = Path.GetDirectoryName(path)
                       ?? throw new InvalidOperationException($"'{path}' is not a valid path.");
                name = Path.GetFileNameWithoutExtension(name) + "_" + ".pdf";
                path = Path.Combine(path, name);

                pdfDocument.Save(path);
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
        }
    }
}
