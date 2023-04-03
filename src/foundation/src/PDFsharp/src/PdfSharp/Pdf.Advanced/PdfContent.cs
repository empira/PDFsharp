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
            : base(document)
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
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        /// <param name="dict">The dict.</param>
        public PdfContent(PdfDictionary dict) // HACK PdfContent
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
                    var filter = Elements["/Filter"];
                    if (filter == null && Stream is not null)
                    {
                        byte[] bytes = Filtering.FlateDecode.Encode(Stream.Value, _document.Options.FlateEncodeMode);
                        Stream.Value = bytes;
                        Elements.SetInteger("/Length", Stream.Length);
                        Elements.SetName("/Filter", "/FlateDecode");
                    }
                }
            }
        }

        /// <summary>
        /// Unfilters the stream.
        /// </summary>
        void Decode()
        {
            //if (Stream != null && Stream.Value != null)
            if (Stream is { Value: { } })  // StL: Is this really more readable???
            {
                var item = Elements["/Filter"];
                if (item != null)
                {
                    var decodeParams = Elements[PdfStream.Keys.DecodeParms];
                    var bytes = Filtering.Decode(Stream.Value, item, decodeParams);
                    if (bytes != null)
                    {
                        Stream.Value = bytes;
                        Elements.Remove("/Filter");
                        Elements.SetInteger("/Length", Stream.Length);
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
            // Therefore we try to relieve the problem by surrounding the content stream with push/restore 
            // graphic state operation.
            if (Stream != null)
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
                _pdfRenderer.Close();
                Debug.Assert(_pdfRenderer == null);
            }

            if (Stream != null)
            {
                //if (Owner.Options.CompressContentStreams)
                if (Owner.Options.CompressContentStreams && Elements.GetName("/Filter").Length == 0)
                {
                    Stream.Value = Filtering.FlateDecode.Encode(Stream.Value, _document.Options.FlateEncodeMode);
                    //Elements["/Filter"] = new PdfName("/FlateDecode");
                    Elements.SetName("/Filter", "/FlateDecode");
                }
                Elements.SetInteger("/Length", Stream.Length);
            }

            base.WriteObject(writer);
        }

        internal XGraphicsPdfRenderer? _pdfRenderer;

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal sealed class Keys : PdfStream.Keys
        {
            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
