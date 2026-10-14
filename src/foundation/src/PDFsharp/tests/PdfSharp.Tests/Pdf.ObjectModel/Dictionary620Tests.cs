// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Pdf.Advanced;
using Xunit;

namespace PdfSharp.Tests.Pdf.ObjectModel
{
    /// <summary>
    /// Ensure that 6.2 code compiles without any issues in 6.3 and later.
    /// </summary>
    [Collection("PDFsharp")]
    public class Dictionary620Tests
    {
        [Fact]
        public void Ensure_PdfDictionary_620_API()
        {
            // Ensure that all functions from 6.2 still compile.

            PdfDictionary dict = new PdfDictionary();
            _ = dict;
            if (TrueOrFalse.False)
            {
                // Just compile, but do not execute.

                dict = new PdfDictionary((PdfDocument)null!);
                dict = dict.Clone();
                PdfDictionary.DictionaryElements e = dict.Elements;
#pragma warning disable CS8619 // Nullability of reference types in value doesn’t match target type.
                using IEnumerator<KeyValuePair<string, PdfItem?>> @enum = dict.GetEnumerator(); // 4STLA: 6.2.0: PdfItem?; 6.3.0: PdfItem
#pragma warning restore CS8619
                string str = dict.ToString();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                // BREAKING: Cannot set property Stream anymore.
                PdfDictionary.PdfStream stream = dict.Stream; // 4STLA: 6.2.0: PdfStream; 6.3.0: PdfStream?
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                // ReSharper disable once UseCollectionExpression
                PdfDictionary.PdfStream stream2 = dict.CreateStream(new byte[0]);
            }
        }

        [Fact]
        public void Ensure_PdfDictionary_Elements_620_API()
        {
            // Ensure that all functions from 6.2 still compile.

            PdfDictionary dict = new PdfDictionary();
            PdfDictionary.DictionaryElements e = dict.Elements;
            if (TrueOrFalse.False)
            {
                // Just compile, but do not execute.

                e = e.Clone();
                bool b = e.GetBoolean("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                b = e.GetBoolean("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                e.SetBoolean("0", false);
                int i = e.GetInteger("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                i = e.GetInteger("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                uint ui = e.GetUnsignedInteger("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                ui = e.GetUnsignedInteger("0"); // ditto
                e.SetInteger("0", 0);
                double d = e.GetReal("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                d = e.GetReal("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                e.SetReal("0", 0d);
                string str = e.GetString("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                str = e.GetString("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                b = e.TryGetString("0", out str!);
                e.SetString("0", "");
                str = e.GetName("0");
                e.SetName("0", "");
                var rect = e.GetRectangle("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                rect = e.GetRectangle("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                e.SetRectangle("0", rect!);
                XMatrix xm = e.GetMatrix("0", false); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                xm = e.GetMatrix("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                e.SetMatrix("0", xm);
                // Breaking change
                var dtm = e.GetDateTime("0", DateTime.Now); // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                e.SetDateTime("0", dtm!.Value);
                PdfItem? nitem = e.GetValue("0", VCF.None);  // 4stla: 6.2.0: 2 parameters, 6.3.0: 3 parameters
                nitem = e.GetValue("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 3 parameters
                e.SetValue("0", nitem!);
                PdfObject? nobj = e.GetObject("0");
                PdfDictionary? ndict = e.GetDictionary("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 2 parameters
                PdfArray? narray = e.GetArray("0"); // 4stla: 6.2.0: 1 parameter, 6.3.0: 2 parameters
                PdfReference? nref = e.GetReference("0");
                e.SetObject("0", nobj!);
#pragma warning disable CS0618 // Type or member is obsolete but was part of the PDFsharp 6.2 API
                e.SetReference("0", nobj!);
#pragma warning restore CS0618 // Type or member is obsolete
                e.SetReference("0", nref!);

                #region IDictionary Members
                b = e.IsReadOnly;
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                using IEnumerator<KeyValuePair<string, PdfItem?>> @enum = e.GetEnumerator(); // Nullability changed.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                nitem = e["0"];
                nitem = e[new PdfName()];
                b = e.Remove("0");
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                b = e.Remove(new KeyValuePair<string, PdfItem?>("0", nitem)); // Nullability.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                b = e.ContainsKey("0");
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                b = e.Contains(new KeyValuePair<string, PdfItem?>("0", nitem)); // Nullability.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                e.Clear();
                e.Add("0", nitem!);
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                e.Add(new KeyValuePair<string, PdfItem?>("0", nitem)); // Nullability.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                PdfName[] names = e.KeyNames;
                ICollection<string> keys = e.Keys;
                b = e.TryGetValue("0", out nitem);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                ICollection<PdfItem?> coll = e.Values;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                b = e.IsFixedSize;

                #endregion

                #region ICollection Members

                b = e.IsSynchronized;
                i = e.Count;
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                // ReSharper disable once UseCollectionExpression
                e.CopyTo(new KeyValuePair<string, PdfItem?>[0], i); // Nullability.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                object obj = e.SyncRoot;

                #endregion

                ArrayOrSingleItemHelper item = e.ArrayOrSingleItem;
            }
        }
    }
}
