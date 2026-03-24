// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.PdfDictionaryExtensions;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the border effect of all annotations.
    /// </summary>
    public sealed class PdfAnnotationAppearance : PdfDictionary
    {
        // Reference 2.0: 12.5.5  Appearance streams / Page 474

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationAppearance"/> class.
        /// </summary>
        public PdfAnnotationAppearance(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfAnnotationAppearance(PdfDictionary dict)
            : base(dict)
        { }

        public Dictionary<string, PdfFormXObject>? GetNValues()
        {
            return GetValues("/N");
        }

        public Dictionary<string, PdfFormXObject>? GetRValues()
        {
            return GetValues("/R");
        }

        public Dictionary<string, PdfFormXObject>? GetDValues()
        {
            return GetValues("/D");
        }

        Dictionary<string, PdfFormXObject>? GetValues(string type)
        {
            var dict = Elements.GetDictionary(type);
            if (dict == null)
                return null;

            Dictionary<string, PdfFormXObject> result = new();

            if (PdfFormXObject.IsFormXObject(dict))
            {
                Transform("/", dict);
            }
            else
            {
                var keys = dict.Elements.Keys;
                foreach (var key in keys)
                {
                    //if (dict.Elements[key] is PdfDictionary stream && PdfFormXObject.IsFormXObject(stream))
                    if (dict.Elements.GetValue(key) is PdfDictionary stream && PdfFormXObject.IsFormXObject(stream)) // #US373
                    {
                        Transform(key, stream);
                    }
                }
            }
            return result;

            void Transform(string key, PdfDictionary d)
            {
                var form = (PdfFormXObject)dict.Transform(typeof(PdfFormXObject));
                result.Add(key, form);
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 170 — Entries in an appearance dictionary / Page 475

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) The annotation’s normal appearance.
            /// </summary>
            [KeyInfo(KeyType.StreamOrDictionary | KeyType.Required)]
            public const string N = "/N";

            /// <summary>
            /// (Optional) The annotation’s rollover appearance. Default value: the value of the N entry.
            /// </summary>
            [KeyInfo(KeyType.StreamOrDictionary | KeyType.Optional)]
            public const string R = "/R";

            /// <summary>
            /// (Optional) The annotation’s down appearance. Default value: the value of the N entry.
            /// </summary>
            [KeyInfo(KeyType.StreamOrDictionary | KeyType.Optional)]
            public const string D = "/D";

            // ReSharper restore InconsistentNaming

            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
