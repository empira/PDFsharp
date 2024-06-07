// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Reads a PDF document, unpacks all its streams, and save it under a new name.
    /// </summary>
    public static class PdfFileFormatter
    {
        /// <summary>
        /// Reads a PDF file, formats the content and saves the new document.
        /// </summary>
        /// <param name="path">The path.</param>
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
            catch (Exception)
            {
                _ = typeof(int);
            }
        }
    }
}
