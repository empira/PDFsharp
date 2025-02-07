// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents a generic field. Used for AcroForm dictionaries unknown to PDFsharp.
    /// </summary>
    public sealed class PdfGenericField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of PdfGenericField.
        /// </summary>
        internal PdfGenericField(PdfDocument document)
            : base(document)
        { }

        internal PdfGenericField(PdfDictionary dict)
            : base(dict)
        { }

        internal override void Flatten()
        {
            base.Flatten();

            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget.Page != null)
                {
                    var appearances = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    var normalAppearance = appearances?.Elements.GetDictionary("/N");
                    var activeAppearance = widget.Elements.GetString(PdfAnnotation.Keys.AS);
                    if (!String.IsNullOrEmpty(activeAppearance) && normalAppearance != null && normalAppearance.Elements.ContainsKey(activeAppearance))
                        RenderContentStream(widget.Page, normalAppearance.Elements.GetDictionary(activeAppearance)!, widget.Rectangle);
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
