// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// This class is intended for empira internal use only and may change or drop in future releases.
    /// </summary>
    public class PdfCustomValues : PdfDictionary
    {
        internal PdfCustomValues()
        { }

        internal PdfCustomValues(PdfDocument document)
            : base(document)
        { }

        internal PdfCustomValues(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public PdfCustomValueCompressionMode CompressionMode
        {
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public bool Contains(string key)
        {
            return Elements.ContainsKey(key);
        }

        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public PdfCustomValue? this[string key]
        {
            get
            {
                var dict = Elements.GetDictionary(key);
                if (dict == null)
                    return null;
                if (dict is not PdfCustomValue cust)
                    cust = new PdfCustomValue(dict);
                return cust;
            }
            set
            {
                if (value == null)
                {
                    Elements.Remove(key);
                }
                else
                {
                    Owner.Internals.AddObject(value);
                    Elements.SetReference(key, value);
                }
            }
#if old
            get
            {
                PdfDictionary dict = Elements.GetDictionary(key);
                if (dict == null)
                    return null;
                if (!(dict is PdfCustomValue))
                    dict = new PdfCustomValue(dict);
                return dict.Stream.Value;
            }
            set
            {
                PdfCustomValue cust;
                PdfDictionary dict = Elements.GetDictionary(key);
                if (dict == null)
                {
                    cust = new PdfCustomValue();
                    Owner.Internals.AddObject(cust);
                    Elements.Add(key, cust);
                }
                else
                {
                    cust = dict as PdfCustomValue;
                    if (cust == null)
                        cust = new PdfCustomValue(dict);
                }
                cust.Value = value;
            }
#endif
        }

        /// <summary>
        /// This function is intended for empira internal use only.
        /// </summary>
        public static void ClearAllCustomValues(PdfDocument document)
        {
            document.CustomValues = null;
            foreach (var page in document.Pages)
                page.CustomValues = null!;
        }

        //public static string Key = "/PdfSharp.CustomValue";

        internal static PdfCustomValues Get(DictionaryElements elem)
        {
            string key = elem.Owner.Owner.Internals.CustomValueKey;
            PdfCustomValues? customValues;
            var dict = elem.GetDictionary(key);
            if (dict == null)
            {
                customValues = new PdfCustomValues();
                elem.Owner.Owner.Internals.AddObject(customValues);
                elem.Add(key, customValues);
            }
            else
            {
                customValues = dict as PdfCustomValues;
                if (customValues == null)
                    customValues = new PdfCustomValues(dict);
            }
            return customValues;
        }

        internal static void Remove(DictionaryElements elem)
        {
            elem.Remove(elem.Owner.Owner.Internals.CustomValueKey);
        }
    }
}
