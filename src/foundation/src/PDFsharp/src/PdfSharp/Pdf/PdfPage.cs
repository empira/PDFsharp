// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
#if CORE
using PdfSharp.Internal.Imaging;
#endif
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using System.ComponentModel;

// v7.0.0 TODO review, TODO Keys 2.0

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a page in a PDF document.
    /// </summary>
    public sealed class PdfPage : PdfPageTreeBase, IContentStream
    {
        // 7.7.3.3 Page objects
        /// <summary>
        /// Initializes a new page. The page must be added to a document before it can be used.
        /// Depending on the IsMetric property of the current region, the page size is set to 
        /// A4 or Letter, respectively. If this size is not appropriate it should be changed before
        /// any drawing operations are performed on the page.
        /// </summary>
        public PdfPage()
        {
            Elements.SetName(Keys.Type, "/Page");
            Initialize(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPage"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public PdfPage(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Page");
            Elements[Keys.Parent] = document.Pages.RequiredReference;
            Initialize(false);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPage(PdfDictionary dict)
            : base(dict)
        {
            Initialize(true);
        }

        internal void Initialize(bool setupSizeFromMediaBox)
        {
            if (setupSizeFromMediaBox)
            {
                // Setup page size from MediaBox.
                var rectangle = Elements.GetRectangle(InheritablePageKeys.MediaBox, false);
                if (rectangle == null)
                    throw new InvalidOperationException("Page has no MediaBox.");

                _width = XUnit.FromPoint(rectangle.X2 - rectangle.X1);
                _height = XUnit.FromPoint(rectangle.Y2 - rectangle.Y1);
                UpdateOrientation();
            }
            else
            {
                // Setting the MediaBox is done in Size.
                Size = RegionInfo.CurrentRegion.IsMetric ? PageSize.A4 : PageSize.Letter;
            }
        }

        /// <summary>
        /// Gets or sets a user-defined object that contains arbitrary information associated with this PDF page.
        /// The tag is not used by PDFsharp.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Closes the page. A closed page cannot be modified anymore, and it is not possible to
        /// get an XGraphics object for a closed page. Closing a page is not required, but may save
        /// resources if the document has many pages.
        /// </summary>
        public void Close()
        {
            _closed = true;
        }
        bool _closed;

        /// <summary>
        /// Gets a value indicating whether the page is closed.
        /// </summary>
        internal bool IsClosed => _closed;

        /// <summary>
        /// Gets or sets the PdfDocument this page belongs to.
        /// </summary>
        public override PdfDocument Document
        {
            internal set
            {
                if (!ReferenceEquals(base.Document, value))
                {
                    base.Document = value;
                    Elements[Keys.Parent] = value.Pages.RequiredReference;
                }
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the page. The default value is PageOrientation.Portrait.
        /// If the page width is less than or equal to page height, the orientation is Portrait;
        /// otherwise Landscape.
        /// </summary>
        public PageOrientation Orientation
        {
            get => _orientation;
            set
            {
                // In case that _width == _height orientation is already Portrait and cannot be changed.
                switch (value)
                {
                    case PageOrientation.Portrait:
                        // Is it currently Landscape?
                        if (_width > _height)
                        {
                            Debug.Assert(_orientation == PageOrientation.Landscape);
                            (_width, _height) = (_height, _width);
                            _orientation = value;
                            MediaBox = new PdfRectangle(0, 0, _width.Point, _height.Point);
                        }
                        else
                        {
                            Debug.Assert(_orientation == PageOrientation.Portrait);
                        }
                        break;

                    case PageOrientation.Landscape:
                        // Is it currently Portrait?
                        if (_width < _height)
                        {
                            Debug.Assert(_orientation == PageOrientation.Portrait);
                            (_width, _height) = (_height, _width);
                            _orientation = value;
                            MediaBox = new PdfRectangle(0, 0, _width.Point, _height.Point);
                        }
                        else
                        {
                            // Quadratic page is considered to be Portrait.
                            Debug.Assert(_orientation == PageOrientation.Landscape || _width == _height);
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }
        PageOrientation _orientation;

        void UpdateOrientation()
        {
            // Quadratic page is considered to be Portrait.
            _orientation = _width <= _height
                ? PageOrientation.Portrait
                : PageOrientation.Landscape;
        }

        /// <summary>
        /// Sets one of the predefined standard sizes.
        /// </summary>
        public PageSize Size
        {
            [Obsolete("Use Width and Height to get the absolute size of the page.")]
            get => throw new InvalidOperationException("Cannot get PageSize. Use Width and Height instead.");
            set
            {
                if (!Enum.IsDefined(typeof(PageSize), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(PageSize));

                var size = PageSizeConverter.ToSize(value);
                MediaBox = new(0, 0, size.Width, size.Height);
            }
        }

        /// <summary>
        /// Gets or sets the trim margins.
        /// </summary>
        public TrimMargins TrimMargins
        {
            get
            {
                _trimMargins ??= new();
                return _trimMargins;
            }
            set
            {
                _trimMargins ??= new();
                if (value != null!)
                {
                    _trimMargins.Left = value.Left;
                    _trimMargins.Right = value.Right;
                    _trimMargins.Top = value.Top;
                    _trimMargins.Bottom = value.Bottom;
                }
                else
                    _trimMargins.All = XUnit.Zero;
            }
        }
        TrimMargins? _trimMargins;

        /// <summary>
        /// Gets or sets the media box directly. XGraphics is not prepared to work with a media box
        /// with an origin other than (0,0).
        /// </summary>
        public PdfRectangle MediaBox
        {
            get => Elements.GetRequiredRectangle(InheritablePageKeys.MediaBox);
            set
            {
                // Can be different from 0 in case of imported PDF.
                _width = XUnit.FromPoint(value.X2 - value.X1);
                _height = XUnit.FromPoint(value.Y2 - value.Y1);
                UpdateOrientation();
                Elements.SetRectangle(InheritablePageKeys.MediaBox, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a media box is set.
        /// </summary>
        public bool HasMediaBox => Elements.HasValue(InheritablePageKeys.MediaBox);

        /// <summary>
        /// Gets a copy of the media box if it exists, or an empty PdfRectangle if no media box is set.
        /// </summary>
        [Obsolete("Use properties HasMediaBox and MediaBox.")]
        public PdfRectangle MediaBoxReadOnly => Elements.GetRequiredRectangle(InheritablePageKeys.MediaBox, false, new());

        /// <summary>
        /// Gets or sets the crop box.
        /// </summary>
        public PdfRectangle CropBox
        {
            get => Elements.GetRequiredRectangle(InheritablePageKeys.CropBox);
            set => Elements.SetRectangle(InheritablePageKeys.CropBox, value);
        }

        /// <summary>
        /// Gets a value indicating whether a crop box is set.
        /// </summary>
        public bool HasCropBox => Elements.HasValue(InheritablePageKeys.CropBox);

        /// <summary>
        /// Gets a copy of the crop box if it exists, or an empty PdfRectangle if no crop box is set.
        /// </summary>
        [Obsolete("Use properties HasCropBox and CropBox.")]
        public PdfRectangle CropBoxReadOnly => Elements.GetRequiredRectangle(InheritablePageKeys.CropBox, false, new());

        /// <summary>
        /// Gets a copy of the effective crop box if it exists, or an empty PdfRectangle if neither crop box nor media box are set.
        /// </summary>
        public PdfRectangle EffectiveCropBoxReadOnly
        {
            get => HasCropBox ? CropBox : MediaBox;
        }

        /// <summary>
        /// Gets or sets the bleed box.
        /// </summary>
        public PdfRectangle BleedBox
        {
            get => Elements.GetRequiredRectangle(Keys.BleedBox);
            set => Elements.SetRectangle(Keys.BleedBox, value);
        }

        /// <summary>
        /// Gets a value indicating whether a bleed box is set.
        /// </summary>
        public bool HasBleedBox => Elements.HasValue(Keys.BleedBox);

        /// <summary>
        /// Gets a copy of the bleed box if it exists, or an empty PdfRectangle if no bleed box is set.
        /// </summary>
        [Obsolete("Use properties HasBleedBox and BleedBox.")]

        public PdfRectangle BleedBoxReadOnly => Elements.GetRequiredRectangle(Keys.BleedBox, false, new());

        /// <summary>
        /// Gets a copy of the effective bleed box if it exists, or an empty PdfRectangle if neither bleed box nor crop box nor media box are set.
        /// </summary>
        public PdfRectangle EffectiveBleedBoxReadOnly
        {
            get
            {
                if (HasBleedBox)
                    return BleedBox;
                return EffectiveCropBoxReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the art box.
        /// </summary>
        public PdfRectangle ArtBox
        {
            get => Elements.GetRequiredRectangle(Keys.ArtBox);
            set => Elements.SetRectangle(Keys.ArtBox, value);
        }

        /// <summary>
        /// Gets a value indicating whether an art box is set.
        /// </summary>
        public bool HasArtBox => Elements.HasValue(Keys.ArtBox);

        /// <summary>
        /// Gets a copy of the art box if it exists, or an empty PdfRectangle if no art box is set.
        /// </summary>
        [Obsolete("Use properties HasArtBox and ArtBox.")]
        public PdfRectangle ArtBoxReadOnly => Elements.GetRequiredRectangle(Keys.ArtBox, false, new());

        /// <summary>
        /// Gets a copy of the effective art box if it exists, or an empty PdfRectangle if neither art box nor crop box nor media box are set.
        /// </summary>
        public PdfRectangle EffectiveArtBoxReadOnly
        {
            get
            {
                if (HasArtBox)
                    return ArtBox;
                return EffectiveCropBoxReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the trim box.
        /// </summary>
        public PdfRectangle TrimBox
        {
            get => Elements.GetRequiredRectangle(Keys.TrimBox);
            set => Elements.SetRectangle(Keys.TrimBox, value);
        }

        /// <summary>
        /// Gets a value indicating whether a trim box is set.
        /// </summary>
        public bool HasTrimBox => Elements.HasValue(Keys.TrimBox);

        /// <summary>
        /// Gets a copy of the trim box if it exists, or an empty PdfRectangle if no trim box is set.
        /// </summary>
        [Obsolete("Use properties HasTrimBox and TrimBox.")]

        public PdfRectangle TrimBoxReadOnly => Elements.GetRequiredRectangle(Keys.TrimBox, false, new());

        /// <summary>
        /// Gets a copy of the effective trim box if it exists, or an empty PdfRectangle if neither trim box nor crop box nor media box are set.
        /// </summary>
        public PdfRectangle EffectiveTrimBoxReadOnly
        {
            get
            {
                if (HasTrimBox)
                    return TrimBox;
                return EffectiveCropBoxReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the height of the page.
        /// If the page width is less than or equal to page height, the orientation is Portrait;
        /// otherwise Landscape.
        /// </summary>
        public XUnit Height
        {
#if DEBUG
            get
            {
                var rectangle = Elements.GetRequiredRectangle(InheritablePageKeys.MediaBox);
                var height = XUnit.FromPoint(rectangle.Y2 - rectangle.Y1);
                Debug.Assert(height == _height);
                return _height;
            }
#else
            get => _height;
#endif
            set
            {
                // #PageOrientation
                _height = value;
                var rect = MediaBox;
                MediaBox = new PdfRectangle(rect.X1, 0, rect.X2, _height.Point);
                UpdateOrientation();
            }
        }
        XUnit _height;

        /// <summary>
        /// Gets or sets the width of the page.
        /// If the page width is less than or equal to page height, the orientation is Portrait;
        /// otherwise Landscape.
        /// </summary>
        public XUnit Width
        {
#if DEBUG
            get
            {
                var rectangle = Elements.GetRequiredRectangle(InheritablePageKeys.MediaBox);
                var width = XUnit.FromPoint(rectangle.X2 - rectangle.X1);
                Debug.Assert(width == _width);
                return _width;
            }
#else
            get => _width;
#endif
            set
            {
                // #PageOrientation
                _width = value;
                var rect = MediaBox;
                MediaBox = new PdfRectangle(0, rect.Y1, _width.Point, rect.Y2);
                UpdateOrientation();
            }
        }
        XUnit _width;

        /// <summary>
        /// Gets or sets the height of the page in point.
        /// If the page width is less than or equal to page height, the orientation is Portrait;
        /// otherwise Landscape.
        /// </summary>
        public double PointHeight
        {
            get => _height.Point;
            set => Height = XUnit.FromPoint(value);  // Use property to update orientation.
        }

        /// <summary>
        /// Gets or sets the width of the page in point.
        /// If the page width is less than or equal to page height, the orientation is Portrait;
        /// otherwise Landscape.
        /// </summary>
        public double PointWidth
        {
            get => _width.Point;
            set => Width = XUnit.FromPoint(value);  // Use property to update orientation.
        }

        /// <summary>
        /// Gets or sets the /Rotate entry of the PDF page. The value is the number of degrees by which the page 
        /// should be rotated clockwise when displayed or printed. The value must be a multiple of 90.
        /// This property does the same as the Rotation property, but uses an integer value.
        /// </summary>
        // #PageOrientation
        // PDFsharp does not set this value, but for imported pages this value can be set and must be taken
        // into account when adding graphic to such a page.
        public int Rotate
        {
            get => Elements.GetInteger(InheritablePageKeys.Rotate);
            set
            {
                if (value >= 360)
                {
                    // An imported PDF may contain a value larger than or equal to 360.
                    var message = Invariant($"Illegal /Rotate value {value}.");
                    PdfSharpLogHost.Logger.LogError(message);
                    while (value >= 360)
                        value -= 360;
                }

                if (value % 90 != 0 || value > 270)
                    throw new ArgumentException("Value must be 0, 90, 180, or 270.");
                Elements.SetInteger(InheritablePageKeys.Rotate, value);
            }
        }

        /// <summary>
        /// Gets or sets a value how a PDF viewer application should rotate this page.
        /// This property does the same as the Rotate property, but uses an enum value.
        /// </summary>
        public PageRotation Rotation
        {
            get
            {
                var value = Elements.GetInteger(InheritablePageKeys.Rotate);
                return value switch
                {
                    0 => PageRotation.None,
                    90 => PageRotation.Rotate90DegreesRight,
                    180 => PageRotation.RotateUpsideDown,
                    270 => PageRotation.Rotate90DegreesLeft,
                    _ => throw new InvalidEnumArgumentException(nameof(value), value, typeof(PageRotation))
                };
            }
            set
            {
                if (value != PageRotation.None &&
                    value != PageRotation.Rotate90DegreesRight &&
                    value != PageRotation.RotateUpsideDown &&
                    value != PageRotation.Rotate90DegreesLeft)
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(PageRotation));
                Elements.SetInteger(InheritablePageKeys.Rotate, (int)value);
            }
        }

        /// <summary>
        /// The content stream currently used by an XGraphics object for rendering.
        /// </summary>
        internal PdfContent? RenderContent;

        /// <summary>
        /// Gets the array of content streams of the page.
        /// </summary>
        public PdfContents Contents
        {
            get
            {
                if (_contents == null)
                {
                    if (true) // || Document.IsImported)
                    {
                        //var item = Elements[Keys.Contents];
                        var item = Elements.GetValue(Keys.Contents); // #US373
                        if (item == null)
                        {
                            _contents = new PdfContents(Owner);
                            //Owner.irefTable.Add(_contents);
                        }
                        else
                        {
                            //if (item is PdfReference reference) // TODO #US373
                            //    item = reference.Value;

                            if (item is PdfArray array)
                            {
                                // It is already an array of content streams.
                                if (array.IsIndirect)
                                {
                                    // Make it a direct array.
                                    array = array.Clone();
                                    array.Document = Owner;
                                }
                                // TODO_OLD 4STLA: Causes Exception "Object type transformation must not be done with direct objects" in "protected PdfObject(PdfObject obj)"
                                _contents = new PdfContents(array);
                            }
                            else
                            {
                                // Only one content stream -> create array.
                                _contents = new PdfContents(Owner);
                                //Owner.irefTable.Add(_contents);
                                var content = new PdfContent((PdfDictionary)item);
                                _contents.Elements.Add(content.Reference ?? NRT.ThrowOnNull<PdfReference>());
                            }
                        }
                    }
                    //else
                    //{
                    //  _content = new PdfContent(Document);
                    //  Document.xrefTable.Add(_content);
                    //}
                    Elements[Keys.Contents] = _contents;
                }
                return _contents;
            }
        }
        PdfContents? _contents;

        #region Annotations

        /// <summary>
        /// Gets the annotations array of this page.
        /// </summary>
        public bool HasAnnotations
        {
            get
            {
                _hasAnnotations ??= (PdfAnnotations?)Elements.GetValue(Keys.Annots) != null;
                return _hasAnnotations.Value;
            }
        }
        bool? _hasAnnotations;

        /// <summary>
        /// Gets the annotations array of this page.
        /// </summary>
        public PdfAnnotations Annotations
        {
            get
            {
                // Get or create annotations array.
                var annots = Elements.GetRequiredArray<PdfAnnotations>(Keys.Annots, VCF.Create);
                annots.Page = this;
                return annots;
                //if (_annotations == null)
                //{
                //    // Get or create annotations array.
                //    _annotations = (PdfAnnotations?)Elements.GetValue(Keys.Annots, VCF.Create) ?? NRT.ThrowOnNull<PdfAnnotations>();
                //    _annotations.Page = this;
                //    _hasAnnotations = false;
                //}
                //return _annotations;
            }
        }
        //PdfAnnotations? _annotations;

        /// <summary>
        /// Adds an internal document link.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPage">The destination page.</param>
        /// <param name="point">The position in the destination page.</param>
        public PdfLinkAnnotation AddDocumentLink(PdfRectangle rect, int destinationPage, XPoint? point = null)
        {
            var annotation = PdfLinkAnnotation.CreateDocumentLink(this, rect, destinationPage, point);
            return annotation;
        }

        /// <summary>
        /// Adds an internal document link.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationName">The Named Destination’s name.</param>
        public PdfLinkAnnotation AddDocumentLink(PdfRectangle rect, string destinationName)
        {
            var annotation = PdfLinkAnnotation.CreateDocumentLink(this, rect, destinationName);
            return annotation;
        }

        /// <summary>
        /// Adds an external document link.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationName">The Named Destination’s name in the target document.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window. If not set, the viewer application should behave in accordance with the current user preference.</param>
        public PdfLinkAnnotation AddDocumentLink(PdfRectangle rect, string documentPath, string destinationName, bool? newWindow = null)
        {
            var annotation = PdfLinkAnnotation.CreateDocumentLink(this, rect, documentPath, destinationName, newWindow);
            return annotation;
        }

        /// <summary>
        /// Adds an embedded document link.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the current (root or embedded) document to the embedded document holding the destination.
        /// ".." references the parent, other strings refer to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public PdfLinkAnnotation AddEmbeddedDocumentLink(PdfRectangle rect, string destinationPath, bool? newWindow = null)
        {
            var annotation = PdfLinkAnnotation.CreateEmbeddedDocumentLink(this, rect, destinationPath, newWindow);
            return annotation;
        }

        /// <summary>
        /// Adds an external embedded document link.
        /// </summary>
        /// <param name="rect">The link area in default page coordinates.</param>
        /// <param name="documentPath">The path to the target document.</param>
        /// <param name="destinationPath">The path to the named destination through the embedded documents in the target document.
        /// The path is separated by '\' and the last segment is the name of the named destination.
        /// The other segments describe the route from the root document to the embedded document.
        /// Each segment name refers to a child with this name in the EmbeddedFiles name dictionary.</param>
        /// <param name="newWindow">True, if the destination document shall be opened in a new window.
        /// If not set, the viewer application should behave in accordance with the current user preference.</param>
        public PdfLinkAnnotation AddEmbeddedDocumentLink(PdfRectangle rect, string documentPath, string destinationPath, bool? newWindow = null)
        {
            var annotation = PdfLinkAnnotation.CreateEmbeddedDocumentLink(this, rect, documentPath, destinationPath, newWindow);
            return annotation;
        }

        /// <summary>
        /// Adds a link to the Web.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="url">The URL.</param>
        public PdfLinkAnnotation AddWebLink(PdfRectangle rect, string url)
        {
            var annotation = PdfLinkAnnotation.CreateWebLink(this, rect, url);
            return annotation;
        }

        /// <summary>
        /// Adds a link to a file.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="fileName">Name of the file.</param>
        public PdfLinkAnnotation AddFileLink(PdfRectangle rect, string fileName)
        {
            var annotation = PdfLinkAnnotation.CreateFileLink(this, rect, fileName);
            return annotation;
        }

        #endregion

        // TODO_OLD: PdfActions

        // TODO_OLD: PdfPageTransition

        /// <summary>
        /// Gets or sets the custom values.
        /// </summary>
        public PdfCustomValues CustomValues
        {
            get
            {
                if (_customValues == null)
                    _customValues = PdfCustomValues.Get(Elements);
                return _customValues;
            }
            set
            {
                if (value != null)
                    throw new ArgumentException("Only null is allowed to clear all custom values.");
                PdfCustomValues.Remove(Elements);
                _customValues = null;
            }
        }
        PdfCustomValues? _customValues;

        /// <summary>
        /// Gets the PdfResources object of this page.
        /// </summary>
        public PdfResources Resources
        {
            get
            {
#if true
                return field ??= Elements.GetRequiredDictionary<PdfResources>(InheritablePageKeys.Resources, VCF.Create);
#else
                return _resources ??= (PdfResources?)Elements.GetValue(InheritablePageKeys.Resources, VCF.Create)
                                      ?? NRT.ThrowOnNull<PdfResources>();
#endif
            }
        }

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        PdfResources IContentStream.Resources => Resources;

        /// <summary>
        /// Gets the resource name of the specified font within this page.
        /// </summary>
        internal string GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont)
        {
            pdfFont = Document.FontTable.GetOrCreateFont(glyphTypeface, fontType);
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont)
            => GetFontName(glyphTypeface, fontType, out pdfFont);

        /// <summary>
        /// Tries to get the resource name of the specified font data within this page.
        /// Returns null if no such font exists.
        /// </summary>
        internal string? TryGetFontName(string idName, out PdfFont? pdfFont)
        {
            pdfFont = Document.FontTable.TryGetFont(idName);
            string? name = null;
            if (pdfFont != null)
                name = Resources.AddFont(pdfFont);
            return name;
        }

        /// <summary>
        /// Gets the resource name of the specified font data within this page.
        /// </summary>
        internal string GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
        {
            pdfFont = Document.FontTable.GetFont(idName, fontData);
            //pdfFont = new PdfType0Font(Owner, idName, fontData);
            //pdfFont.Document = _document;
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
            => GetFontName(idName, fontData, out pdfFont);

        /// <summary>
        /// Gets the resource name of the specified image within this page.
        /// </summary>
        internal string GetImageName(XImage image)
        {
            var pdfImage = Document.ImageTable.GetImage(image);
            Debug.Assert(pdfImage != null);
            string name = Resources.AddImage(pdfImage);
            return name;
        }

#if CORE
        internal string GetImageName(PdfSharp.Internal.Imaging.ImportedImage importedImage)
        {
            var pdfImage = Document.ImageTable.GetImage(importedImage);
            Debug.Assert(pdfImage != null);
            string name = Resources.AddImage(pdfImage);
            return name;
        }
#endif

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        string IContentStream.GetImageName(XImage image)
            => GetImageName(image);

        /// <summary>
        /// Gets the resource name of the specified form within this page.
        /// </summary>
        internal string GetFormName(XForm form)
        {
            var pdfForm = Document.FormTable.GetForm(form);
            Debug.Assert(pdfForm != null);
            string name = Resources.AddForm(pdfForm);
            return name;
        }

        /// <summary>
        /// Implements the interface because the primary function is internal.
        /// </summary>
        string IContentStream.GetFormName(XForm form)
            => GetFormName(form);

        internal override void WriteObject(PdfWriter writer)
        {
            // #PDF-A
            // Suppress transparency group if PDF-A is required.
            if (!Document.IsPdfA)
            {
                // Add transparency group to prevent rendering problems of Adobe viewer.
                // Update (PDFsharp 1.50 beta 3): Add transparency group only if ColorMode is defined.
                // Rgb is the default for the ColorMode, but if user sets it to Undefined then
                // we respect this and skip the transparency group.
                TransparencyUsed = true; // TODO_OLD: check XObjects
                if (TransparencyUsed && !Elements.ContainsKey(Keys.Group) &&
                    Document.Options.ColorMode != PdfColorMode.Undefined)
                {
                    var group = new PdfDictionary();
                    Elements["/Group"] = group;
                    if (Document.Options.ColorMode != PdfColorMode.Cmyk)
                        group.Elements.SetName("/CS", "/DeviceRGB");
                    else
                        group.Elements.SetName("/CS", "/DeviceCMYK");

                    // #PDF-A
                    group.Elements.SetName("/S", "/Transparency");

                    //False is default: group.Elements["/I"] = new PdfBoolean(false);
                    //False is default: group.Elements["/K"] = new PdfBoolean(false);
                }
            }
            base.WriteObject(writer);
        }

        /// <summary>
        /// HACK_OLD to indicate that a page-level transparency group must be created.
        /// </summary>
        internal bool TransparencyUsed { get; set; }

        /// <summary>
        /// Applies inherited values from the parent nodes to this page.
        /// </summary>
        internal void ApplyInheritedValues(ref readonly PdfPageTreeNode.InheritedValues inheritedValues)  // TODO review
        {
#if true
            if (inheritedValues.Resources != null)
            {
                var resources = Elements.GetDictionary(InheritablePageKeys.Resources);
                //if (res is PdfReference pdfReference)
                //{
                //    resources = (PdfDictionary)pdfReference.Value.Clone();
                //    resources.Document = Owner;
                //}
                //else
                //    resources = (PdfDictionary?)res;

                // The PDF 2.0 specifications states:
                // “All values shall be inherited as-is, without merging, even for composite data types such as arrays and dictionaries.”

                if (resources == null)
                {
                    //resources = values.Resources.Clone();
                    //resources.Document = Owner;
                    Elements.Add(InheritablePageKeys.Resources, inheritedValues.Resources);
                }
                else
                {

                    //foreach (var name in values.Resources.Elements.KeyNames)
                    //{
                    //    if (!resources.Elements.ContainsKey(name.Value))
                    //    {
                    //        var item = values.Resources.Elements[name];
                    //        Debug.Assert(item != null);
                    //        if (item is PdfObject)
                    //            item = item.Clone();
                    //        resources.Elements.Add(name.ToString(), item);
                    //    }
                    //}
                }
            }

            if (inheritedValues.MediaBox != null && Elements.GetValue(InheritablePageKeys.MediaBox) == null)
                Elements[InheritablePageKeys.MediaBox] = inheritedValues.MediaBox;

            if (inheritedValues.CropBox != null && Elements.GetValue(InheritablePageKeys.CropBox) == null)
                Elements[InheritablePageKeys.CropBox] = inheritedValues.CropBox;

            if (inheritedValues.Rotate != null && Elements.GetValue(InheritablePageKeys.Rotate) == null)
                Elements[InheritablePageKeys.Rotate] = inheritedValues.Rotate;
#else
            // Old code - DELETE
            if (values.Resources != null!)
            {
                PdfDictionary? resources;
                var res = page.Elements[InheritablePageKeys.Resources];
                if (res is PdfReference pdfReference)
                {
                    resources = (PdfDictionary)pdfReference.Value.Clone();
                    resources.Document = page.Owner;
                }
                else
                    resources = (PdfDictionary?)res;

                if (resources == null)
                {
                    resources = values.Resources.Clone();
                    resources.Document = page.Owner;
                    page.Elements.Add(InheritablePageKeys.Resources, resources);
                }
                else
                {
                    foreach (var name in values.Resources.Elements.KeyNames)
                    {
                        if (!resources.Elements.ContainsKey(name.Value))
                        {
                            var item = values.Resources.Elements[name];
                            Debug.Assert(item != null);
                            if (item is PdfObject)
                                item = item.Clone();
                            resources.Elements.Add(name.ToString(), item);
                        }
                    }
                }
            }

            if (values.MediaBox != null! && page.Elements[InheritablePageKeys.MediaBox] == null)
                page.Elements[InheritablePageKeys.MediaBox] = values.MediaBox;

            if (values.CropBox != null! && page.Elements[InheritablePageKeys.CropBox] == null)
                page.Elements[InheritablePageKeys.CropBox] = values.CropBox;

            if (values.Rotate != null! && page.Elements[InheritablePageKeys.Rotate] == null)
                page.Elements[InheritablePageKeys.Rotate] = values.Rotate;
#endif
        }

        internal override void PrepareForSave()
        {
            if (_trimMargins?.AreSet ?? false)
            {
                // These are the values InDesign set for an A4 page with 3mm crop margin at each edge.
                // (recall that PDF rectangles are two points and NOT a point and width and height)
                // /MediaBox[0.0 0.0 612.283 858.898]       # in millimeter: (0 0) (216 302.7)
                // /CropBox[0.0 0.0 612.283 858.898]        # -------------- " ---------------
                // /BleedBox[0.0 0.0 612.283 858.898]       # -------------- " ---------------
                // /ArtBox[8.50394 8.50394 603.78 850.394]  # in millimeter: (3 3) (213 300)
                // /TrimBox[8.50394 8.50394 603.78 850.394] # ------------- " --------------
                //
                // An A4 page has a size of 210 x 297 mm²

                double width = _trimMargins.Left.Point + Width.Point + _trimMargins.Right.Point;
                double height = _trimMargins.Top.Point + Height.Point + _trimMargins.Bottom.Point;

                MediaBox = new(0, 0, width, height);
                CropBox = new(0, 0, width, height);
                BleedBox = new(0, 0, width, height);

                var rect = new PdfRectangle(_trimMargins.Left.Point, _trimMargins.Top.Point,
                  width - _trimMargins.Right.Point, height - _trimMargins.Bottom.Point);
                TrimBox = rect;
                ArtBox = rect;
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : InheritablePageKeys
        {
            // IMPROVE Reference 2.0: Table 31
            // All entries except the inheritable page keys.

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be Page for a page object.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Page")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required; must be an indirect reference)
            /// The page tree node that is the immediate parent of this page object.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required | KeyType.MustBeIndirect)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Required if PieceInfo is present; optional otherwise; PDF 1.3) The date and time
            /// when the page’s contents were most recently modified. If a page-piece dictionary
            /// (PieceInfo) is present, the modification date is used to ascertain which of the 
            /// application data dictionaries that it contains correspond to the current content
            /// of the page.
            /// </summary>
            [KeyInfo(KeyType.Date)]
            public const string LastModified = "/LastModified";

            /// <summary>
            /// (Optional; PDF 1.3) A rectangle, expressed in default user space units, defining the 
            /// region to which the contents of the page should be clipped when output in a production
            /// environment. Default value: the value of CropBox.
            /// </summary>
            [KeyInfo("1.3", KeyType.Rectangle | KeyType.Optional)]
            public const string BleedBox = "/BleedBox";

            /// <summary>
            /// (Optional; PDF 1.3) A rectangle, expressed in default user space units, defining the
            /// intended dimensions of the finished page after trimming. Default value: the value of 
            /// CropBox.
            /// </summary>
            [KeyInfo("1.3", KeyType.Rectangle | KeyType.Optional)]
            public const string TrimBox = "/TrimBox";

            /// <summary>
            /// (Optional; PDF 1.3) A rectangle, expressed in default user space units, defining the
            /// extent of the page’s meaningful content (including potential white-space) as intended
            /// by the page’s creator. Default value: the value of CropBox.
            /// </summary>
            [KeyInfo("1.3", KeyType.Rectangle | KeyType.Optional)]
            public const string ArtBox = "/ArtBox";

            /// <summary>
            /// (Optional; PDF 1.4) A box color information dictionary specifying the colors and other 
            /// visual characteristics to be used in displaying guidelines on the screen for the various
            /// page boundaries. If this entry is absent, the application should use its own current 
            /// default settings.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional)]
            public const string BoxColorInfo = "/BoxColorInfo";

            /// <summary>
            /// (Optional) A content stream describing the contents of this page. If this entry is absent, 
            /// the page is empty. The value may be either a single stream or an array of streams. If the 
            /// value is an array, the effect is as if all of the streams in the array were concatenated,
            /// in order, to form a single stream. This allows PDF producers to create image objects and
            /// other resources as they occur, even though they interrupt the content stream. The division
            /// between streams may occur only at the boundaries between lexical tokens but is unrelated
            /// to the page’s logical content or organization. Applications that consume or produce PDF 
            /// files are not required to preserve the existing structure of the Contents array.
            /// </summary>
            //[KeyInfo(KeyType.Array | KeyType.Stream | KeyType.Optional)] // #US373 Note that you cannot use "|" with types.
            [KeyInfo(KeyType.StreamOrArray | KeyType.Optional)] // #US373
            public const string Contents = "/Contents";

            /// <summary>
            /// (Optional; PDF 1.4) A group attributes dictionary specifying the attributes of the page’s 
            /// page group for use in the transparent imaging model.
            /// </summary>
            [KeyInfo("1.4", KeyType.Dictionary | KeyType.Optional)]
            public const string Group = "/Group";

            /// <summary>
            /// (Optional) A stream object defining the page’s thumbnail image.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string Thumb = "/Thumb";

            /// <summary>
            /// (Optional; PDF 1.1; recommended if the page contains article beads) An array of indirect
            /// references to article beads appearing on the page. The beads are listed in the array in 
            /// natural reading order.
            /// </summary>
            [KeyInfo("1.1", KeyType.Array | KeyType.Optional)]
            public const string B = "/B";

            /// <summary>
            /// (Optional; PDF 1.1) The page’s display duration (also called its advance timing): the 
            /// maximum length of time, in seconds, that the page is displayed during presentations before
            /// the viewer application automatically advances to the next page. By default, the viewer does 
            /// not advance automatically.
            /// </summary>
            [KeyInfo("1.1", KeyType.Real | KeyType.Optional)]
            public const string Dur = "/Dur";

            /// <summary>
            /// (Optional; PDF 1.1) A transition dictionary describing the transition effect to be used 
            /// when displaying the page during presentations.
            /// </summary>
            [KeyInfo("1.1", KeyType.Dictionary | KeyType.Optional)]
            public const string Trans = "/Trans";

            /// <summary>
            /// (Optional) An array of annotation dictionaries representing annotations associated with 
            /// the page.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional, typeof(PdfAnnotations))]
            public const string Annots = "/Annots";

            /// <summary>
            /// (Optional; PDF 1.2) An additional-actions dictionary defining actions to be performed 
            /// when the page is opened or closed.
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            /// <summary>
            /// (Optional; PDF 1.4) A metadata stream containing metadata for the page.
            /// </summary>
            [KeyInfo("1.4", KeyType.Stream | KeyType.Optional)]
            public const string Metadata = "/Metadata";

            /// <summary>
            /// (Optional; PDF 1.3) A page-piece dictionary associated with the page.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string PieceInfo = "/PieceInfo";

            /// <summary>
            /// (Required if the page contains structural content items; PDF 1.3)
            /// The integer key of the page’s entry in the structural parent tree.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string StructParents = "/StructParents";

            /// <summary>
            /// (Optional; PDF 1.3; indirect reference preferred) The digital identifier of
            /// the page’s parent Web Capture content set.
            /// </summary>
            [KeyInfo("1.3", KeyType.String | KeyType.Optional)]
            public const string ID = "/ID";

            /// <summary>
            /// (Optional; PDF 1.3) The page’s preferred zoom (magnification) factor: the factor 
            /// by which it should be scaled to achieve the natural display magnification.
            /// </summary>
            [KeyInfo("1.3", KeyType.Real | KeyType.Optional)]
            public const string PZ = "/PZ";

            /// <summary>
            /// (Optional; PDF 1.3) A separation dictionary containing information needed
            /// to generate color separations for the page.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
            public const string SeparationInfo = "/SeparationInfo";

            /// <summary>
            /// (Optional; PDF 1.5) A name specifying the tab order to be used for annotations
            /// on the page. The possible values are R (row order), C (column order),
            /// and S (structure order).
            /// </summary>
            [KeyInfo("1.5", KeyType.Name | KeyType.Optional)]
            public const string Tabs = "/Tabs";

            /// <summary>
            /// (Required if this page was created from a named page object; PDF 1.5)
            /// The name of the originating page object.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string TemplateInstantiated = "/TemplateInstantiated";

            /// <summary>
            /// (Optional; PDF 1.5) A navigation node dictionary representing the first node
            /// on the page.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string PresSteps = "/PresSteps";

            /// <summary>
            /// (Optional; PDF 1.6) A positive number giving the size of default user space units,
            /// in multiples of 1/72 inch. The range of supported values is implementation-dependent.
            /// </summary>
            [KeyInfo("1.6", KeyType.Real | KeyType.Optional)]
            public const string UserUnit = "/UserUnit";

            /// <summary>
            /// (Optional; PDF 1.6) An array of viewport dictionaries specifying rectangular regions 
            /// of the page.
            /// </summary>
            [KeyInfo("1.6", KeyType.Dictionary | KeyType.Optional)]
            public const string VP = "/VP";

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

        /// <summary>
        /// Predefined keys common to PdfPage and PdfPages.
        /// </summary>
        public class InheritablePageKeys : KeysBase
        {
            // TODO Reference 2.0: Table 31
            // These 4 entries are the inheritable page keys.

            /// <summary>
            /// (Required; inheritable) A dictionary containing any resources required by the page. 
            /// If the page requires no resources, the value of this entry should be an empty dictionary.
            /// Omitting the entry entirely indicates that the resources are to be inherited from an 
            /// ancestor node in the page tree.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required | KeyType.Inheritable, typeof(PdfResources))]
            public const string Resources = "/Resources";

            /// <summary>
            /// (Required; inheritable) A rectangle, expressed in default user space units, defining the 
            /// boundaries of the physical medium on which the page is intended to be displayed or printed.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Required | KeyType.Inheritable)]
            public const string MediaBox = "/MediaBox";

            /// <summary>
            /// (Optional; inheritable) A rectangle, expressed in default user space units, defining the 
            /// visible region of default user space. When the page is displayed or printed, its contents 
            /// are to be clipped (cropped) to this rectangle and then imposed on the output medium in some
            /// implementation defined manner. Default value: the value of MediaBox.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Optional | KeyType.Inheritable)]
            public const string CropBox = "/CropBox";

            /// <summary>
            /// (Optional; inheritable) The number of degrees by which the page should be rotated clockwise 
            /// when displayed or printed. The value must be a multiple of 90. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Rotate = "/Rotate";
        }
    }
}
