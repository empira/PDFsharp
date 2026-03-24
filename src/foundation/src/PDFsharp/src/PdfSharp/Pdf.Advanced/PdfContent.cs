// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing.Pdf;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the content of a page. PDFsharp supports only one content stream per page.
    /// If an imported page has an array of content streams, the streams are concatenated to
    /// one single stream.
    /// </summary>
    public sealed class PdfContent : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        public PdfContent(PdfDocument document)
            : base(document, true)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        internal PdfContent(PdfPage page)
            : base(page != null! ? page.Owner : null!)  // NRT can page be null?
        {
            //_pageContent = new PageContent(page);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfContent(PdfDictionary dict) // HACK_OLD PdfContent
            : base(dict)
        {
            // A PdfContent dictionary is always unfiltered.
            Decode();
        }

        /// <summary>
        /// Sets a value indicating whether the content is compressed with the ZIP algorithm.
        /// </summary>
        public bool Compressed
        {
            set
            {
                if (value)
                {
                    var filter = Elements.GetValue(PdfStream.Keys.Filter); // #US373
                    if (filter == null && Stream is not null)
                    {
                        byte[] bytes = Filtering.FlateDecode.Encode(Stream.Value, Document.Options.FlateEncodeMode);
                        Stream.Value = bytes;
                        Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                        Elements.SetInteger(PdfStream.Keys.Length, Stream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Unfilters the stream.
        /// </summary>
        void Decode()
        {
            if (Stream is not null)
            {
                var filter = Elements.GetValue(PdfStream.Keys.Filter);
                if (filter != null)
                {
                    var decodeParams = Elements.GetValue(PdfStream.Keys.DecodeParms);
                    var bytes = Filtering.Decode(Stream.Value, filter, decodeParams);
                    if (bytes != null!)
                    {
                        Stream.Value = bytes;
                        Elements.Remove(PdfStream.Keys.Filter);
                        Elements.SetInteger(PdfStream.Keys.Length, Stream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Surround content with q/Q operations if necessary.
        /// </summary>
        internal void PreserveGraphicsState()
        {
            // If a content stream is touched by PDFsharp it is typically because graphical operations are
            // prepended or appended. Some nasty PDF tools do not preserve the graphical state correctly.
            // Therefore, we try to relieve the problem by surrounding the content stream with push/restore 
            // graphic state operation.
            if (Stream != null!)
            {
                var value = Stream.Value;
                if (value != null!)  // NRT
                {
                    int length = value.Length;
                    if (length > 1 && ((value[0] != (byte)'q' || value[1] != (byte)'\n')))
                    {
                        var newValue = new byte[length + 2 + 3];
                        newValue[0] = (byte)'q';
                        newValue[1] = (byte)'\n';
                        Array.Copy(value, 0, newValue, 2, length);
                        newValue[length + 2] = (byte)' ';
                        newValue[length + 3] = (byte)'Q';
                        newValue[length + 4] = (byte)'\n';
                        Stream.Value = newValue;
                        Elements.SetInteger("/Length", Stream.Length);
                    }
                }
            }
        }

        internal override void WriteObject(PdfWriter writer)
        {
            if (_pdfRenderer != null)
            {
                // GetContent also disposes the underlying XGraphics object, if one exists
                //Stream = new PdfStream(PdfEncoders.RawEncoding.GetBytes(pdfRenderer.GetContent()), this);
                if (_pdfRenderer is XGraphicsPdfRenderer xgfxRenderer)
                {
                    xgfxRenderer.Close();
                }
                else
                {
                    // Case: Renderer is a PDFsharp Graphics DrawingContext for PDF.
                    // No automatic close, throw.
                    throw new InvalidOperationException(
                        $"A renderer of type {_pdfRenderer.GetType().FullName} is still open for this content stream.");
                }
                Debug.Assert(_pdfRenderer == null);
            }

            if (Stream != null!)
            {
                // Acrobat crashes if a PDF file contains an empty stream that is compressed and 
                // therefore about 2 to 4 bytes long. So we do not compress very short streams
                // at all.
                const int streamLengthCompressionThreshold = 32;

                // Compress the stream if it has a minimum length and has no filter set yet.
                // Short streams are smaller without compression.
                if (Owner.Options.CompressContentStreams && !Elements.HasValue(PdfStream.Keys.Filter) &&
                    Stream.Value.Length > streamLengthCompressionThreshold)
                {
                    Stream.Value = Filtering.FlateDecode.Encode(Stream.Value, Document.Options.FlateEncodeMode);
                    Elements.SetName(PdfStream.Keys.Filter, "/FlateDecode");
                }
                Elements.SetInteger(PdfStream.Keys.Length, Stream.Length);
            }

            base.WriteObject(writer);
        }

        // Sets the renderer that currently renders this content stream.
        internal void SetRenderer(IPageContentRenderer? renderer) => _pdfRenderer = renderer;

        internal IPageContentRenderer? Renderer => _pdfRenderer;

        IPageContentRenderer? _pdfRenderer;

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : PdfStream.Keys
        {
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
