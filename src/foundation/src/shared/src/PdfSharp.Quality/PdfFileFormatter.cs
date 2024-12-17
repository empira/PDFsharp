// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
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
                    if (objects[idx] is PdfDictionary { Stream: not null } dict)
                    {
                        dict.Stream.TryUncompress();
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
#if DEBUG
                _ = typeof(int);
#endif
            }
        }
    }
}
