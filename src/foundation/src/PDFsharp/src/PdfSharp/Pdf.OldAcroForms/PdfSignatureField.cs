// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.OldAcroForms
{
    /// <summary>
    /// Represents the signature field.
    /// </summary>
    public sealed class PdfSignatureField : PdfFormField
    {
        /// <summary>
        /// Initializes a new instance of PdfSignatureField.
        /// </summary>
        internal PdfSignatureField(PdfDocument document)
            : base(document)
        {
            CustomAppearanceHandler = null!;
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfSignatureField(PdfDictionary dict)
            : base(dict)
        {
            CustomAppearanceHandler = null!;
        }

        /// <summary>
        /// Handler that creates the visual representation of the digital signature in PDF.
        /// </summary>
        public IAnnotationAppearanceHandler CustomAppearanceHandler { get; internal set; }

        /// <summary>
        /// Creates the custom appearance form X object for the annotation that represents
        /// this acro form text field.
        /// </summary>
        void RenderCustomAppearance()
        {
            var rect = Elements.GetRectangle(PdfAnnotation.Keys.Rect);
            if (rect == null)
                return;

            var visible = rect.X1 + rect.X2 + rect.Y1 + rect.Y2 != 0;
            if (!visible)
                return;

            if (CustomAppearanceHandler == null)
                throw new Exception("AppearanceHandler is not set.");

            var form = new XForm(Document, rect.Size);
            var gfx = XGraphics.FromForm(form);

            CustomAppearanceHandler.DrawAppearance(gfx, rect.ToXRect());

            form.DrawingFinished();

            // Get existing or create new appearance dictionary
            if (Elements[PdfAnnotation.Keys.AP] is not PdfDictionary ap)
            {
                ap = new PdfDictionary(Document);
                Elements[PdfAnnotation.Keys.AP] = ap;
            }

            // Set XRef to normal state
            ap.Elements["/N"] = form.PdfForm.RequiredReference;

            // PdfRenderer can be null.
            form.PdfRenderer?.Close();
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            if (CustomAppearanceHandler != null!)
                RenderCustomAppearance();
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfFormField.Keys
        {
            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes; if present,
            /// must be Sig for a signature dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

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
