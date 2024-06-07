// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF group XObject.
    /// </summary>
    public abstract class PdfGroupAttributes : PdfDictionary
    {
        internal PdfGroupAttributes(PdfDocument thisDocument)
            : base(thisDocument)
        {
            Elements.SetName(Keys.Type, "/Group");
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            /// <summary>
            ///(Optional) The type of PDF object that this dictionary describes;
            ///if present, must be Group for a group attributes dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The group subtype, which identifies the type of group whose
            /// attributes this dictionary describes and determines the format and meaning
            /// of the dictionary’s remaining entries. The only group subtype defined in
            /// PDF 1.4 is Transparency. Other group subtypes may be added in the future.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string S = "/S";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}