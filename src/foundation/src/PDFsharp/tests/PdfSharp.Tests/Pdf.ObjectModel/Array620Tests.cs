// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using PdfSharp.TestHelper;
using Xunit;
// ReSharper disable UnusedVariable

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    /// <summary>
    /// Class ensures that 6.2 code compiles without any issues in 6.3 and later.
    /// </summary>
    [Collection("PDFsharp")]
    public class Array620Tests
    {
        [Fact]
        public void Ensure_PdfArray_620_API()
        {
            // Ensure that all functions from 6.2 still compile and later.

            PdfArray array = new PdfArray();
            if (TrueOrFalse.False)
            {
                // Just compile, but do not execute.
                PdfArray.ArrayElements e = array.Elements;

                array = new PdfArray((PdfDocument)null!);
                array = new PdfArray((PdfDocument)null!, new PdfItem[0]);
                //protected PdfArray(PdfArray array)
                array = array.Clone();
                using IEnumerator<PdfItem> @enum = array.GetEnumerator();
                string str = array.ToString();

            }
        }

        [Fact]
        public void Ensure_PdfArray_Elements_620_API()
        {
            // Ensure that all functions from 6.2 still compile and later.

            var array = new PdfArray();
            var elements = array.Elements;
            if (TrueOrFalse.False)
            {
                // Just compile, but do not execute.

                PdfArray.ArrayElements e = array.Elements;
                e = e.Clone();
                bool b = e.GetBoolean(0);
                int i = e.GetInteger(0);
                double d = e.GetReal(0);
                double? nd = e.GetNullableReal(0);
                string str = e.GetString(0);
                str = e.GetName(0);
                PdfObject? no = e.GetObject(0);
                PdfDictionary? ndict = e.GetDictionary(0);
                PdfArray? na = e.GetArray(0);
                PdfReference? nr = e.GetReference(0);
                PdfItem[] items = e.Items;

                #region IList Members

                b = e.IsReadOnly;
                PdfItem item = e[0];
                e.RemoveAt(0);
                e.Remove(item);
                e.Insert(0, item);
                b = e.Contains(item);
                e.Clear();
                i = e.IndexOf(item);
                e.Add(item);
                b = e.IsFixedSize;

                #endregion

                #region ICollection Members

                b = e.IsSynchronized;
                i = e.Count;
                // ReSharper disable once UseCollectionExpression
                e.CopyTo(new PdfItem[0], 0);
                object obj = e.SyncRoot;

                #endregion

                using IEnumerator<PdfItem> enum2 = e.GetEnumerator();
            }
        }
    }
}
