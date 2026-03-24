// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Actions;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

// v7.0.0 TODO review creator functions

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    // Just a hack to make MigraDoc work with this code.
    enum PdfLinkAnnotationTypes
    {
        None,
        Document,
        NamedDestination,
        Web,
        File // TODO: un-nest
    }
}

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF link annotation.
    /// </summary>
    public sealed class PdfLinkAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.5  Link annotations / Page 482

        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        public PdfLinkAnnotation()
            => throw new NotImplementedException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
        /// </summary>
        public PdfLinkAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfLinkAnnotation(PdfDictionary dict)
            : base(dict)
        {
            Initialize();
        }

        void Initialize()
        {
            _linkType = PdfLinkAnnotationTypes.None;
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Link);
        }

        const string ObsoleteMessage = "PDFsharp 6.4: Parameter requires a PDF page as first parameter.";

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, int destinationPage, XPoint? point = null)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link within the current document.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPage">The one-based destination page number.</param>
        /// <param name="point">The position in the destination page.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfPage page, PdfRectangle rect, int destinationPage, XPoint? point = null)
        {
            EnsurePageHasDocument(page);

            if (destinationPage < 1)
                throw new ArgumentException("Invalid destination page in call to CreateDocumentLink: page number is one-based and must be 1 or higher.", nameof(destinationPage));

            var link = new PdfLinkAnnotation(page.Document)
            {
                _linkType = PdfLinkAnnotationTypes.Document,
                Rectangle = rect,
                _destPage = destinationPage,
                _point = point
            };
            page.Annotations.Add(link);
            return link;
        }

        int _destPage;
        PdfLinkAnnotationTypes _linkType;
        string _url = "";
        XPoint? _point = null;

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, string destinationName)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link within the current document using a named destination.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationName">The named destination’s name.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfPage page, PdfRectangle rect, string destinationName)
        {
            EnsurePageHasDocument(page);

            var link = new PdfLinkAnnotation(page.Document)
            {
                _linkType = PdfLinkAnnotationTypes.NamedDestination,
                Rectangle = rect,
                _action = PdfGoToAction.CreateGoToAction(destinationName)
            };
            page.Annotations.Add(link);
            return link;
        }

        PdfAction _action = null!;

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, string documentPath, string destinationName, bool? newWindow = null)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link to an external PDF document using a named destination.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationName">The named destination’s name in the target document.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfPage page, PdfRectangle rect, string documentPath, string destinationName, bool? newWindow = null)
        {
            EnsurePageHasDocument(page);

            var link = new PdfLinkAnnotation(page.Document)
            {
                _linkType = PdfLinkAnnotationTypes.NamedDestination,
                Rectangle = rect,
                _action = PdfRemoteGoToAction.CreateRemoteGoToAction(documentPath, destinationName, newWindow)
            };
            page.Annotations.Add(link);
            return link;
        }

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfRectangle rect, string destinationPath, bool? newWindow = null)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link to an embedded document.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the current (root or embedded) document to the embedded document holding the destination.
        /// ".." references to the parent, other strings refer to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfPage page, PdfRectangle rect, string destinationPath, bool? newWindow = null)
        {
            EnsurePageHasDocument(page);

            //PdfLinkAnnotation link = new PdfLinkAnnotation(page);  // TODO: Dangerous bug. Page binds to PdfDictionary and creates a type transformation. 
            PdfLinkAnnotation link = new PdfLinkAnnotation(page.Document);
            link._linkType = PdfLinkAnnotationTypes.NamedDestination;
            link.Rectangle = rect;
            link._action = PdfEmbeddedGoToAction.CreatePdfEmbeddedGoToAction(destinationPath, newWindow);
            page.Annotations.Add(link);
            return link;
        }

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfRectangle rect, string documentPath, string destinationPath, bool? newWindow = null)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link to an embedded document in another document.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents in the target document.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the root document to the embedded document.
        /// Each segment name refers to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfPage page, PdfRectangle rect, string documentPath, string destinationPath, bool? newWindow = null)
        {
            EnsurePageHasDocument(page);

            PdfLinkAnnotation link = new PdfLinkAnnotation(page.Document);
            link._linkType = PdfLinkAnnotationTypes.NamedDestination;
            link.Rectangle = rect;
            link._action = PdfEmbeddedGoToAction.CreatePdfEmbeddedGoToAction(documentPath, destinationPath, newWindow);
            page.Annotations.Add(link);
            return link;
        }

        [Obsolete(ObsoleteMessage)]
        public static PdfLinkAnnotation CreateWebLink(PdfRectangle rect, string url)
            => throw new NotImplementedException(ObsoleteMessage);

        /// <summary>
        /// Creates a link to the web.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The rectangle using World coordinates.</param>
        /// <param name="url">The destination URL.</param>
        public static PdfLinkAnnotation CreateWebLink(PdfPage page, PdfRectangle rect, string url)
        {
            EnsurePageHasDocument(page);

            var link = new PdfLinkAnnotation(page.Document)
            {
                _linkType = PdfLinkAnnotationTypes.Web,
                Rectangle = rect,
                _url = url
            };
            page.Annotations.Add(link);
            return link;
        }

        [Obsolete("PDFsharp 6.4: Requires a PDF page as first parameter.")]
        public static PdfLinkAnnotation CreateFileLink(PdfRectangle rect, string fileName)
            => throw new NotImplementedException("");

        /// <summary>
        /// Creates a link to a file.
        /// </summary>
        /// <param name="page">The page the annotation belong to.</param>
        /// <param name="rect">The rectangle using World coordinates.</param>
        /// <param name="fileName">The destination file.</param>
        public static PdfLinkAnnotation CreateFileLink(PdfPage page, PdfRectangle rect, string fileName)
        {
            EnsurePageHasDocument(page);

            var link = new PdfLinkAnnotation(page.Document)
            {
                _linkType = PdfLinkAnnotationTypes.File,
                // TODO_OLD: Adjust bleed box here (if possible)
                Rectangle = rect,
                _url = fileName
            };
            page.Annotations.Add(link);
            return link;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            PdfPage? dest = null;
            //pdf.AppendFormat(CultureInfo.InvariantCulture,
            //  "{0} 0 obj\n<<\n/Type/Annot\n/Subtype/Link\n" +
            //  "/Rect[{1} {2} {3} {4}]\n/BS<</Type/Border>>\n/Border[0 0 0]\n/C[0 0 0]\n",
            //  ObjectID.ObjectNumber, rect.X1, rect.Y1, rect.X2, rect.Y2);

            // Older Adobe Reader versions uses a border width of 0 as default value if neither Border nor BS are present.
            // But the PDF Reference specifies:
            // “If neither the Border nor the BS entry is present, the border is drawn as a solid line with a width of 1 point.”
            // After this issue was fixed in newer Reader versions older PDFsharp created documents show an ugly solid border.
            // The following hack fixes this by specifying a 0 width border.
            //if (Elements[Keys.BS] == null) // TODO #US373 Just a null check.
            if (!Elements.HasValue(Keys.BS)) // #US373
                Elements[Keys.BS] = new PdfLiteral("<</Type/Border/W 0>>");

            // May be superfluous. See comment above.
            //if (Elements[PdfAnnotation.Keys.Border] == null) // TODO #US373 Just a null check.
            if (!Elements.HasValue(PdfAnnotation.Keys.Border)) // #US373
                Elements[PdfAnnotation.Keys.Border] = new PdfLiteral("[0 0 0]");

            switch (_linkType)
            {
                case PdfLinkAnnotationTypes.None:
                    break;

                case PdfLinkAnnotationTypes.Document:
                    // destIndex > Owner.PageCount can happen when rendering pages using PDFsharp directly.
                    int destIndex = _destPage;
                    if (destIndex > Owner.PageCount)
                        destIndex = Owner.PageCount;
                    destIndex--;
                    dest = Owner.Pages[destIndex];
                    // TODO: Use format strings for double.
                    if (_point.HasValue)
                    {
                        Elements[Keys.Dest] = new PdfLiteral(Invariant($"[{dest.ObjectNumber} 0 R /XYZ {_point.Value.X} {_point.Value.Y} 0]"));
                    }
                    else
                    {
                        Elements[Keys.Dest] = new PdfLiteral(Invariant($"[{dest.ObjectNumber} 0 R /XYZ null null 0]"));
                    }
                    break;

                case PdfLinkAnnotationTypes.NamedDestination:
                    Elements[Keys.A] = _action;
                    break;

                case PdfLinkAnnotationTypes.Web:
                    Elements[Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>",
                        PdfEncoders.ToStringLiteral(_url, PdfStringEncoding.WinAnsiEncoding, writer.EffectiveSecurityHandler));
                    break;

                case PdfLinkAnnotationTypes.File:
                    Elements[Keys.A] = new PdfLiteral("<</Type/Action/S/Launch/F<</Type/Filespec/F{0}>>>>",
                        PdfEncoders.ToStringLiteral(_url, PdfStringEncoding.WinAnsiEncoding, writer.EffectiveSecurityHandler));
                    break;
            }
            base.WriteObject(writer);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 176 — Additional entries specific to a link annotation / Page 483

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional; PDF 1.1) An action that shall be performed when the link annotation is activated.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required)]
            public const string A = "/A";

            /// <summary>
            /// (Optional; not permitted if an A entry is present) A destination that shall be displayed
            /// when the annotation is activated (12.3.2, "Destinations").
            /// </summary>
            [KeyInfo(KeyType.ArrayOrNameOrString | KeyType.Optional)]
            public const string Dest = "/Dest";

            /// <summary>
            /// (Optional; PDF 1.2) The annotation’s highlighting mode, the visual effect that shall be
            /// used when the mouse button is pressed or held down inside its active area:<br/>
            /// N (None) No highlighting.<br/>
            /// I (Invert) Invert the contents of the annotation rectangle.<br/>
            /// O (Outline) Invert the annotation’s border.<br/>
            /// P (Push) Display the annotation as if it were being pushed below the surface of the page.<br/>
            /// Default value: I.
            /// </summary>
            [KeyInfo("1.2", KeyType.Name | KeyType.Optional)]
            public const string H = "/H";

            /// <summary>
            /// (Optional; PDF 1.3) A URI action (see 12.6.4.8, "URI actions") formerly associated with this
            /// annotation. When a PDF processor changes an annotation from a URI to a go-to action, it may
            /// use this entry to save the data from the original URI action so that it can be changed back
            /// in case the target page for the go-to action is subsequently deleted.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string PA = "/PA";

            /// <summary>
            /// (Optional; PDF 1.6) An array of 8×𝑛 numbers specifying the coordinates of n quadrilaterals
            /// in default user space that comprise the region in which the link should be activated. The
            /// coordinates for each quadrilateral are given in the order:<br/>
            /// 𝑥1 𝑦1 𝑥2 𝑦2 𝑥3 𝑦3 𝑥4 𝑦4<br/>
            /// specifying the four vertices of the quadrilateral in counterclockwise order.For orientation
            /// purposes, such as when applying an underline border style, the bottom of a quadrilateral is
            /// the line formed by (x1, y1) and (x2, y2).<br/>
            /// If this entry is not present, or the PDF processor does not recognise it, or if any
            /// coordinates in the QuadPoints array lie outside the region specified by Rect then the
            /// activation region for the link annotation shall be defined by its Rect entry.<br/>
            /// NOTE<br/>
            /// The last paragraph above was clarified in this document (2020).
            /// </summary>
            [KeyInfo("1.6", KeyType.Array | KeyType.Optional)]
            public const string QuadPoints = "/QuadPoints";


            /// <summary>
            /// (Optional; PDF 1.6) A border style dictionary specifying the line width and dash pattern
            /// that shall be used in drawing the annotation’s border.
            /// </summary>
            [KeyInfo("1.6", KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            // ReSharper restore InconsistentNaming

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
