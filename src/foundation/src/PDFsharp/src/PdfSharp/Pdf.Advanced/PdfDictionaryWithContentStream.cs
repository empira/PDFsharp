// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a base class for dictionaries with a content stream.
    /// Implement IContentStream for use with a content writer.
    /// </summary>
    public abstract class PdfDictionaryWithContentStream : PdfDictionary, IContentStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionaryWithContentStream"/> class.
        /// </summary>
        protected PdfDictionaryWithContentStream()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionaryWithContentStream"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        protected PdfDictionaryWithContentStream(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance from an existing dictionary. Used for object type transformation.
        /// </summary>
        protected PdfDictionaryWithContentStream(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets the resources dictionary of this dictionary. If no such dictionary exists, it is created.
        /// </summary>
        internal PdfResources Resources
        {
            get => _resources ??= (PdfResources?)Elements.GetValue(Keys.Resources, VCF.Create) ?? NRT.ThrowOnNull<PdfResources>();
        }
        PdfResources? _resources;

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        PdfResources IContentStream.Resources => Resources;

        internal string GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont)
        {
            pdfFont = _document.FontTable.GetOrCreateFont(glyphTypeface, fontType);
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont) 
            => GetFontName(glyphTypeface, fontType, out pdfFont);

        internal string GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
        {
            pdfFont = _document.FontTable.GetFont(idName, fontData);
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
        {
            return GetFontName(idName, fontData, out pdfFont);
        }

        /// <summary>
        /// Gets the resource name of the specified image within this dictionary.
        /// </summary>
        internal string GetImageName(XImage image)
        {
            PdfImage pdfImage = _document.ImageTable.GetImage(image);
            Debug.Assert(pdfImage != null);
            string name = Resources.AddImage(pdfImage);
            return name;
        }

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        string IContentStream.GetImageName(XImage image)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the resource name of the specified form within this dictionary.
        /// </summary>
        internal string GetFormName(XForm form)
        {
            PdfFormXObject pdfForm = _document.FormTable.GetForm(form);
            Debug.Assert(pdfForm != null);
            string name = Resources.AddForm(pdfForm);
            return name;
        }

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        string IContentStream.GetFormName(XForm form)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : PdfDictionary.PdfStream.Keys
        {
            /// <summary>
            /// (Optional but strongly recommended; PDF 1.2) A dictionary specifying any
            /// resources (such as fonts and images) required by the form XObject.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResources))]
            public const string Resources = "/Resources";
        }
    }
}
