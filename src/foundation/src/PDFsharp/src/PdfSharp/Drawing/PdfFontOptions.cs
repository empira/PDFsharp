// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Pdf;

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Specifies details about how the font is used in PDF creation.
    /// </summary>
    public class XPdfFontOptions
    {
        internal XPdfFontOptions() { }

        //DELETE
        /// <summary>
        /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
        /// </summary>
        [Obsolete("Must not specify an embedding option anymore. Fonts get always embedded.")]
        public XPdfFontOptions(PdfFontEncoding encoding, PdfFontEmbedding embedding)
        {
            FontEncoding = encoding;
            FontEmbedding = PdfFontEmbedding.Automatic;
            FontEncoding = PdfFontEncoding.Automatic;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
        /// </summary>
        public XPdfFontOptions(PdfFontEncoding encoding)
        {
            FontEncoding = encoding;
        }

        //DELETE
        /// <summary>
        /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
        /// </summary>
        [Obsolete("Must not specify an embedding option anymore. Fonts get always embedded as subsets.")]
        public XPdfFontOptions(PdfFontEmbedding embedding)
        {
            FontEmbedding = PdfFontEmbedding.Automatic;
            FontEncoding = PdfFontEncoding.Automatic;
        }

        /// <summary>
        /// Gets a value indicating the font embedding.
        /// </summary>
        public PdfFontEmbedding FontEmbedding { get; }

        /// <summary>
        /// Gets a value indicating how the font is encoded.
        /// </summary>
        public PdfFontEncoding FontEncoding { get; }

        /// <summary>
        /// Gets the default options with WinAnsi encoding and always font embedding.
        /// </summary>
        public static XPdfFontOptions AutomaticEncoding => new(PdfFontEncoding.Automatic);

        /// <summary>
        /// Gets the default options with WinAnsi encoding and always font embedding.
        /// </summary>
        public static XPdfFontOptions WinAnsiDefault => new(PdfFontEncoding.WinAnsi);

        /// <summary>
        /// Gets the default options with Unicode encoding and always font embedding.
        /// </summary>
        public static XPdfFontOptions UnicodeDefault => new(PdfFontEncoding.Unicode);
    }
}
