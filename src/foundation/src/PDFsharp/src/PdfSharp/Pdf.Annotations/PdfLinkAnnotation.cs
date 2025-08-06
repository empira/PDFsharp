﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Actions;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a link annotation.
    /// </summary>
    public sealed class PdfLinkAnnotation : PdfAnnotation
    {
        // Just a hack to make MigraDoc work with this code.
        enum LinkType
        {
            None, Document, NamedDestination, Web, File
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
        /// </summary>
        public PdfLinkAnnotation()
        {
            _linkType = LinkType.None;
            Elements.SetName(PdfAnnotation.Keys.Subtype, "/Link");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
        /// </summary>
        public PdfLinkAnnotation(PdfDocument document)
            : base(document)
        {
            _linkType = LinkType.None;
            Elements.SetName(PdfAnnotation.Keys.Subtype, "/Link");
        }

        /// <summary>
        /// Creates a link within the current document.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPage">The one-based destination page number.</param>
        /// <param name="point">The position in the destination page.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, int destinationPage, XPoint? point = null)
        {
            if (destinationPage < 1)
                throw new ArgumentException("Invalid destination page in call to CreateDocumentLink: page number is one-based and must be 1 or higher.", nameof(destinationPage));

            PdfLinkAnnotation link = new PdfLinkAnnotation();
            link._linkType = LinkType.Document;
            link.Rectangle = rect;
            link._destPage = destinationPage;
            link._point = point;
            return link;
        }

        int _destPage;
        LinkType _linkType;
        string _url = "";
        private XPoint? _point = null;

        /// <summary>
        /// Creates a link within the current document using a named destination.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationName">The named destination’s name.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, string destinationName)
        {
            var link = new PdfLinkAnnotation
            {
                _linkType = LinkType.NamedDestination,
                Rectangle = rect,
                _action = PdfGoToAction.CreateGoToAction(destinationName)
            };
            return link;
        }

        PdfAction _action = default!;

        /// <summary>
        /// Creates a link to an external PDF document using a named destination.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationName">The named destination’s name in the target document.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, string documentPath, string destinationName, bool? newWindow = null)
        {
            var link = new PdfLinkAnnotation
            {
                _linkType = LinkType.NamedDestination,
                Rectangle = rect,
                _action = PdfRemoteGoToAction.CreateRemoteGoToAction(documentPath, destinationName, newWindow)
            };
            return link;
        }

        /// <summary>
        /// Creates a link to an embedded document.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the current (root or embedded) document to the embedded document holding the destination.
        /// ".." references to the parent, other strings refer to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfRectangle rect, string destinationPath, bool? newWindow = null)
        {
            PdfLinkAnnotation link = new PdfLinkAnnotation();
            link._linkType = LinkType.NamedDestination;
            link.Rectangle = rect;
            link._action = PdfEmbeddedGoToAction.CreatePdfEmbeddedGoToAction(destinationPath, newWindow);
            return link;
        }

        /// <summary>
        /// Creates a link to an embedded document in another document.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents in the target document.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the root document to the embedded document.
        /// Each segment name refers to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public static PdfLinkAnnotation CreateEmbeddedDocumentLink(PdfRectangle rect, string documentPath, string destinationPath, bool? newWindow = null)
        {
            PdfLinkAnnotation link = new PdfLinkAnnotation();
            link._linkType = LinkType.NamedDestination;
            link.Rectangle = rect;
            link._action = PdfEmbeddedGoToAction.CreatePdfEmbeddedGoToAction(documentPath, destinationPath, newWindow);
            return link;
        }

        /// <summary>
        /// Creates a link to the web.
        /// </summary>
        public static PdfLinkAnnotation CreateWebLink(PdfRectangle rect, string url)
        {
            var link = new PdfLinkAnnotation
            {
                _linkType = PdfLinkAnnotation.LinkType.Web,
                Rectangle = rect,
                _url = url
            };
            return link;
        }

        /// <summary>
        /// Creates a link to a file.
        /// </summary>
        public static PdfLinkAnnotation CreateFileLink(PdfRectangle rect, string fileName)
        {
            var link = new PdfLinkAnnotation
            {
                _linkType = LinkType.File,
                // TODO_OLD: Adjust bleed box here (if possible)
                Rectangle = rect,
                _url = fileName
            };
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
            // "If neither the Border nor the BS entry is present, the border is drawn as a solid line with a width of 1 point."
            // After this issue was fixed in newer Reader versions older PDFsharp created documents show an ugly solid border.
            // The following hack fixes this by specifying a 0 width border.
            if (Elements[PdfAnnotation.Keys.BS] == null)
                Elements[PdfAnnotation.Keys.BS] = new PdfLiteral("<</Type/Border/W 0>>");

            // May be superfluous. See comment above.
            if (Elements[PdfAnnotation.Keys.Border] == null)
                Elements[PdfAnnotation.Keys.Border] = new PdfLiteral("[0 0 0]");

            switch (_linkType)
            {
                case LinkType.None:
                    break;

                case LinkType.Document:
                    // destIndex > Owner.PageCount can happen when rendering pages using PDFsharp directly.
                    int destIndex = _destPage;
                    if (destIndex > Owner.PageCount)
                        destIndex = Owner.PageCount;
                    destIndex--;
                    dest = Owner.Pages[destIndex];
                    ////pdf.AppendFormat("/Dest[{0} 0 R/XYZ null null 0]\n", dest.ObjectID);
                    //Elements[Keys.Dest] = new PdfLiteral("[{0} 0 R/XYZ null null 0]", dest.ObjectNumber);
                    if (_point.HasValue)
                    {
                        Elements[Keys.Dest] = new PdfLiteral(Invariant($"[{dest.ObjectNumber} 0 R /XYZ {_point.Value.X} {_point.Value.Y} 0]") /*, dest.ObjectNumber, _point.Value.X, _point.Value.Y*/);
                    }
                    else
                    {
                        Elements[Keys.Dest] = new PdfLiteral(Invariant($"[{dest.ObjectNumber} 0 R /XYZ null null 0]") /*, dest.ObjectNumber*/);
                    }
                    break;

                case LinkType.NamedDestination:
                    Elements[PdfAnnotation.Keys.A] = _action;
                    break;

                case LinkType.Web:
                    //pdf.AppendFormat("/A<</S/URI/URI{0}>>\n", PdfEncoders.EncodeAsLiteral(url));
                    Elements[PdfAnnotation.Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>", //PdfEncoders.EncodeAsLiteral(url));
                        PdfEncoders.ToStringLiteral(_url, PdfStringEncoding.WinAnsiEncoding, writer.EffectiveSecurityHandler));
                    break;

                case LinkType.File:
                    //pdf.AppendFormat("/A<</Type/Action/S/Launch/F<</Type/Filespec/F{0}>> >>\n", 
                    //  PdfEncoders.EncodeAsLiteral(url));
                    Elements[PdfAnnotation.Keys.A] = new PdfLiteral("<</Type/Action/S/Launch/F<</Type/Filespec/F{0}>> >>",
                        //PdfEncoders.EncodeAsLiteral(url));
                        PdfEncoders.ToStringLiteral(_url, PdfStringEncoding.WinAnsiEncoding, writer.EffectiveSecurityHandler));
                    break;
            }
            base.WriteObject(writer);
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal new class Keys : PdfAnnotation.Keys
        {
            //  /// <summary>
            //  /// (Required) The type of annotation that this dictionary describes;
            //  /// must be Link for a link annotation.
            //  /// </summary>
            // inherited from base class

            /// <summary>
            /// (Optional; not permitted if an A entry is present) A destination to be displayed
            /// when the annotation is activated.
            /// </summary>
            [KeyInfo(KeyType.ArrayOrNameOrString | KeyType.Optional)]
            public const string Dest = "/Dest";

            /// <summary>
            /// (Optional; PDF 1.2) The annotation’s highlighting mode, the visual effect to be
            /// used when the mouse button is pressed or held down inside its active area:
            /// N (None) No highlighting.
            /// I (Invert) Invert the contents of the annotation rectangle.
            /// O (Outline) Invert the annotation’s border.
            /// P (Push) Display the annotation as if it were being pushed below the surface of the page.
            /// Default value: I.
            /// Note: In PDF 1.1, highlighting is always done by inverting colors inside the annotation rectangle.
            /// </summary>
            [KeyInfo("1.2", KeyType.Name | KeyType.Optional)]
            public const string H = "/H";

            /// <summary>
            /// (Optional; PDF 1.3) A URI action formerly associated with this annotation. When Web 
            /// Capture changes and annotation from a URI to a go-to action, it uses this entry to save 
            /// the data from the original URI action so that it can be changed back in case the target page for 
            /// the go-to action is subsequently deleted.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string PA = "/PA";

            // QuadPoints

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
