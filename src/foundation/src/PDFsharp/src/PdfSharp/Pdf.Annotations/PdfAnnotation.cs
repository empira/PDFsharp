// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.PdfDictionaryExtensions;

// v7.0.0 TODO review

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents the base class of all annotations.
    /// </summary>
    public abstract class PdfAnnotation : PdfDictionary
    {
        // Reference 2.0: 12.5.2  Annotation dictionaries / Page 467

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotation"/> class.
        /// </summary>
        protected PdfAnnotation()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotation"/> class.
        /// The annotation is created as an indirect object.
        /// </summary>
        protected PdfAnnotation(PdfDocument document)
            : base(document, true)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfAnnotation(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(Keys.Type, "/Annot");
            //Elements.SetString(Keys.NM, Guid.NewGuid().ToString("D"));
            Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
        }

        ///// <summary>
        ///// Removes an annotation from the document
        ///// <seealso cref="PdfAnnotations.Remove(PdfAnnotation)"/>
        ///// </summary>
        //[Obsolete("Use 'Parent.Remove(this)'")]
        //public void Delete()
        //{
        //    Parent_.Remove(this);
        //}

        /// <summary>
        /// Gets or sets the annotation flags of this instance.
        /// </summary>
        public PdfAnnotationFlags Flags
        {
            get => (PdfAnnotationFlags)Elements.GetInteger(Keys.F);
            set
            {
                Elements.SetIntegerFlag(Keys.F, (int)value);
                Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        public bool TestFlag(PdfAnnotationFlags flag)
        {
            var result = (Flags & flag) == flag;
            return result;
        }

        public void SetFlag(PdfAnnotationFlags flag)
        {
            Flags |= flag;
        }

        public void ClearFlag(PdfAnnotationFlags flag)
        {
            Flags &= ~flag;
        }

        /// <summary>
        /// Gets or sets the PdfAnnotations object that this annotation belongs to.
        /// </summary>
        public PdfAnnotations? Parent
        {
            get => _parent ?? NRT.ThrowOnNull<PdfAnnotations>();
            set => _parent = value;
        }
        PdfAnnotations? _parent;

        /// <summary>
        /// Gets or sets the page this annotation belongs to.
        /// </summary>
        public PdfPage? Page
        {
            get => Elements.GetObject<PdfPage>(Keys.P);
            set
            {
                if (value != null)
                    Elements.SetObject(Keys.P, value);
                else
                    Elements.Remove(Keys.P);
            }
        }

        /// <summary>
        /// Gets or sets the annotation rectangle, defining the location of the annotation
        /// on the page in default user space units.
        /// </summary>
        public PdfRectangle Rectangle
        {
            get => Elements.GetRequiredRectangle(Keys.Rect);
            set
            {
                Elements.SetRectangle(Keys.Rect, value);
                // TODO: Check if update /M is correct.
                //Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        // TODO Define the space names
        public XRect PageRectangle
        {
            get
            {
                if (Page == null)
                    throw new InvalidOperationException("The property PageRectangle requires the Page property to be defined.");

                var pdfRect = Elements.GetRectangle(Keys.Rect);
                if (pdfRect == null)
                    return XRect.Empty; // TODO Mist!

                return new(pdfRect.X1, Page.Height.Point - pdfRect.Y1 - pdfRect.Height, pdfRect.Width, pdfRect.Height);
            }
            set
            {
                if (Page == null)
                    throw new InvalidOperationException("The property PageRectangle requires the Page property to be defined.");

                double y = Page.Height.Point - value.Y - value.Height;
                var pdfRect = new PdfRectangle(
                    value.X, y,
                    value.X + value.Width, y + value.Height);
                Elements.SetRectangle(Keys.Rect, pdfRect);
                Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed for the annotation or, if this type of
        /// annotation does not display text, an alternate description of the annotation’s
        /// contents in human-readable form.
        /// </summary>
        public string Contents
        {
            get => Elements.GetString(Keys.Contents, true);
            set
            {
                Elements.SetString(Keys.Contents, value);
                Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        /// <summary>
        /// Gets or sets the color representing the components of the annotation. If the color
        /// has an alpha value other than 1, it is ignored. Use property Opacity to get or set the
        /// opacity of an annotation.
        /// </summary>
        public XColor Color
        {
            get
            {
                var item = Elements.GetValue(Keys.C); // #US373
                //var item = Elements[Keys.C];
                //if (item is PdfReference reference)
                //    item = reference.Value;
                if (item is PdfArray array)
                {
                    if (array.Elements.Count == 3)
                    {
                        // TODO_OLD: an array.GetColor() function may be useful here
                        return XColor.FromArgb(
                            (int)(array.Elements.GetReal(0) * 255),
                            (int)(array.Elements.GetReal(1) * 255),
                            (int)(array.Elements.GetReal(2) * 255));
                    }
                }
                return XColors.Black;
            }
            set
            {
                var array = new PdfArray(Owner, new PdfReal(value.R / 255.0),
                    new PdfReal(value.G / 255.0), new PdfReal(value.B / 255.0));
                Elements[Keys.C] = array;
                Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        /// <summary>
        /// Gets or sets the constant opacity value to be used in painting the annotation.
        /// This value applies to all visible elements of the annotation in its closed state
        /// (including its background and border) but not to the popup window that appears when
        /// the annotation is opened.
        /// </summary>
        public double Opacity
        {
            get
            {
                if (!Elements.ContainsKey(Keys.CA))
                    return 1;
                return Elements.GetReal(Keys.CA, true);
            }
            set
            {
                if (value is < 0 or > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Opacity must be a value in the range from 0 to 1.");
                Elements.SetReal(Keys.CA, value);
                Elements.SetDateTime(Keys.M, DateTimeOffset.Now);
            }
        }

        internal static PdfAnnotation CreateAnnotation(PdfDictionary dict, PdfPage? page = null)
        {
            if (dict is PdfAnnotation annotation)
                return annotation;

            var subtype = dict.Elements.GetName(Keys.Subtype);
#if true_
            switch (subtype)
            {
                case "/" + nameof(PdfAnnotationTypes.Text):
                    annotation = new PdfTextAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Link):
                    annotation = new PdfLinkAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.FreeText):

                case "/" + nameof(PdfAnnotationTypes.Line):
                    annotation = new PdfLineAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Square):
                    annotation = new PdfLineAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Circle):
                    annotation = new PdfCircleAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Polygon):
                case "/" + nameof(PdfAnnotationTypes.PolyLine):
                case "/" + nameof(PdfAnnotationTypes.Highlight):
                case "/" + nameof(PdfAnnotationTypes.Underline):
                case "/" + nameof(PdfAnnotationTypes.Squiggly):
                case "/" + nameof(PdfAnnotationTypes.StrikeOut):
                case "/" + nameof(PdfAnnotationTypes.Caret):
                    annotation = new PdfGenericAnnotation(dict);
                    break;

                Old code, does not compile.
                case "/" + nameof(PdfAnnotationTypes.RubberStamp):
                    annotation = new PdfRubberStampAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Ink):
                case "/" + nameof(PdfAnnotationTypes.Popup):
                case "/" + nameof(PdfAnnotationTypes.FileAttachment):
                case "/" + nameof(PdfAnnotationTypes.Sound):
                case "/" + nameof(PdfAnnotationTypes.Movie):
                case "/" + nameof(PdfAnnotationTypes.Screen):
                    annotation = new PdfGenericAnnotation(dict);
                    break;

                case "/" + nameof(PdfAnnotationTypes.Widget):
                    annotation = CreateWidget();
                    break;

                case "/" + nameof(PdfAnnotationTypes.PrinterMark):
                case "/" + nameof(PdfAnnotationTypes.TrapNet):
                case "/" + nameof(PdfAnnotationTypes.Watermark):
                case "/3D" /*+ nameof(PdfAnnotationTypes.ThreeD)*/:
                case "/" + nameof(PdfAnnotationTypes.Redact):
                case "/" + nameof(PdfAnnotationTypes.Projection):
                case "/" + nameof(PdfAnnotationTypes.RichMedia):
                    annotation = new PdfGenericAnnotation(dict);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid annotation subtype '{subtype}'.");
            }
#else
            annotation = subtype switch
            {
                PdfAnnotationTypeNames.Text
                    => new PdfTextAnnotation(dict),

                PdfAnnotationTypeNames.Link
                    => new PdfLinkAnnotation(dict),

                PdfAnnotationTypeNames.FreeText
                    => new PdfFreeTextAnnotation(dict),

                PdfAnnotationTypeNames.Line
                    => new PdfLineAnnotation(dict),

                PdfAnnotationTypeNames.Square
                    => new PdfSquareAnnotation(dict),

                PdfAnnotationTypeNames.Circle
                    => new PdfCircleAnnotation(dict),

                PdfAnnotationTypeNames.Polygon
                    => new PdfPolygonAnnotation(dict),

                PdfAnnotationTypeNames.PolyLine
                    => new PdfPolyLineAnnotation(dict),

                PdfAnnotationTypeNames.Highlight
                    => new PdfHighlightAnnotation(dict),

                PdfAnnotationTypeNames.Underline
                    => new PdfUnderlineAnnotation(dict),

                PdfAnnotationTypeNames.Squiggly
                    => new PdfSquigglyAnnotation(dict),

                PdfAnnotationTypeNames.StrikeOut
                    => new PdfStrikeOutAnnotation(dict),

                PdfAnnotationTypeNames.Caret
                    => new PdfCaretAnnotation(dict),

                PdfAnnotationTypeNames.Stamp
                    => new PdfStampAnnotation(dict),

                PdfAnnotationTypeNames.Ink
                    => new PdfInkAnnotation(dict),

                PdfAnnotationTypeNames.Popup
                    => new PdfPopupAnnotation(dict),

                PdfAnnotationTypeNames.FileAttachment
                    => new PdfFileAttachmentAnnotation(dict),

                PdfAnnotationTypeNames.Sound
                    => new PdfSoundAnnotation(dict),

                PdfAnnotationTypeNames.Movie
                    => new PdfMovieAnnotation(dict),

                PdfAnnotationTypeNames.Screen
                    => new PdfScreenAnnotation(dict),

                PdfAnnotationTypeNames.Widget
                    => new PdfWidgetAnnotation(dict),

                PdfAnnotationTypeNames.PrinterMark
                    => new PdfPrinterMarkAnnotation(dict),

                PdfAnnotationTypeNames.TrapNet
                    => new PdfTrapNetAnnotation(dict),

                PdfAnnotationTypeNames.Watermark
                    => new PdfWatermarkAnnotation(dict),

                PdfAnnotationTypeNames.ThreeD
                    => new Pdf3DAnnotation(dict),

                PdfAnnotationTypeNames.Redact
                    => new PdfRedactAnnotation(dict),

                PdfAnnotationTypeNames.Projection
                    => new PdfProjectionAnnotation(dict),

                PdfAnnotationTypeNames.RichMedia
                    => new PdfRichMediaAnnotation(dict),

                _ => throw new InvalidOperationException($"Invalid annotation subtype '{subtype}'.")
            };
#endif
            if (page != null)
                annotation.Page = page;

            return annotation;

            //PdfWidgetAnnotation CreateWidget()
            //{
            //    if (dict is PdfWidgetAnnotation widget)
            //        return widget;
            //    if (dict is PdfFormField field)
            //        return field.GetAsWidgetAnnotation();
            //    return new PdfWidgetAnnotation(dict);
            //}
        }

        protected internal static void EnsurePageHasDocument(PdfPage page)
        {
            if (page.Document == null)
            {
                throw new InvalidOperationException(
                    "You cannot create a PDF annotation for a page that does not yet belong to a document.");
            }
        }

        internal string? AppearanceState
        {
            get
            {
                if (Elements.TryGetName(Keys.AS, out var result))
                    return result;
                return null;
            }
            set
            {
                if (value != null)
                    Elements.SetValue(Keys.AS, new PdfName(value));
                else
                    Elements.Remove(Keys.AS);
            }
        }

        /// <summary>
        /// Gets all appearance states for this widget, that are stored in the /N (normal) entry.
        /// </summary>
        internal List<string> GetAppearanceStateNames()
        {
            var subDict = GetAppearanceStreamSubDictionary(PdfAnnotationAppearance.Keys.N);
            return subDict?.Elements.Keys.ToList() ?? [];
        }

        /// <summary>
        /// Gets the appearance stream for the given state of this widget, that is stored in the /N (normal) entry.
        /// </summary>
        /// <param name="appearanceState">The name of the appearance state.</param>
        internal PdfFormXObject? GetNormalAppearanceStream(string appearanceState)
        {
            return GetAppearanceStream(PdfAnnotationAppearance.Keys.N, appearanceState);
        }

        /// <summary>
        /// Gets the appearance stream for the given state of this widget, that are stored in the appearanceKey entry.
        /// </summary>
        /// <param name="appearanceKey">The key of the appearance subdictionary: /N (normal), /R (rollover) or /D (down).</param>
        /// <param name="appearanceState">The name of the appearance state.</param>
        internal PdfFormXObject? GetAppearanceStream(string appearanceKey, string appearanceState)
        {
            var subDict = GetAppearanceStreamSubDictionary(appearanceKey);

            return subDict?.Elements.GetDictionary<PdfFormXObject>(appearanceState);
        }

        /// <summary>
        /// Gets the appearance stream subdictionary for this widget, that is stored in the appearanceKey entry.
        /// </summary>
        /// <param name="appearanceKey">The key of the appearance subdictionary: /N (normal), /R (rollover) or /D (down).</param>
        PdfDictionary? GetAppearanceStreamSubDictionary(string appearanceKey)
        {
            var appearanceDict = Elements.GetDictionary(Keys.AP);
            var appearanceStreamOrSubDict = appearanceDict?.Elements.GetObject(appearanceKey);
            
            if (appearanceStreamOrSubDict is PdfDictionary subDict)
                return subDict;

            return null;
        }

        /// <summary>
        /// Gets the appearance stream for this widget, that is stored directly in the /N (normal) entry.
        /// </summary>
        internal PdfFormXObject? GetNormalAppearanceStream()
        {
            return GetAppearanceStream(PdfAnnotationAppearance.Keys.N);
        }

        /// <summary>
        /// Gets the appearance stream for this widget, that is stored directly in the appearanceKey entry.
        /// </summary>
        /// <param name="appearanceKey">The key of the appearance stream: /N (normal), /R (rollover) or /D (down).</param>
        internal PdfFormXObject? GetAppearanceStream(string appearanceKey)
        {
            var appearanceDict = Elements.GetDictionary(Keys.AP);
            var appearanceStreamOrSubDict = appearanceDict?.Elements.GetObject(appearanceKey);

            if (appearanceStreamOrSubDict is PdfFormXObject stream)
                return stream;
            
            return null;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Updated to:
            // Reference 2.0: 12.5.2  Annotation dictionaries / Page 467
            // Reference 2.0: Table 166 — Entries common to all annotation dictionaries / Page 467

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes;
            /// if present, shall be Annot for an annotation dictionary.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Annot")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of annotation that this dictionary describes.
            /// See PdfAnnotationTypes.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Subtype = "/Subtype";

            /// <summary>
            /// (Required) The annotation rectangle, defining the location of the annotation
            /// on the page in default user space units.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Required)]
            public const string Rect = "/Rect";

            /// <summary>
            /// (Optional) Text that shall be displayed for the annotation or, if this type of
            /// annotation does not display text, an alternative description of the annotation’s
            /// contents in human-readable form. In either case, this text is useful when extracting
            /// the document’s contents in support of accessibility to users with disabilities or
            /// for other purposes.
            /// See PdfAnnotation types for more details on the meaning of this entry for each
            /// annotation type.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string Contents = "/Contents";

            /// <summary>
            /// (Optional except as noted below; PDF 1.3; not used in FDF files) An indirect reference
            /// to the page object with which this annotation is associated. This entry shall be present
            /// in screen annotations associated with rendition actions.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string P = "/P";

            /// <summary>
            /// (Optional; PDF 1.4) The annotation name, a text string uniquely identifying it
            /// among all the annotations on its page.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string NM = "/NM";

            /// <summary>
            /// (Optional; PDF 1.1) The date and time when the annotation was most recently modified.
            /// The format should be a date string.
            /// </summary>
            [KeyInfo(KeyType.Date | KeyType.Optional)]
            public const string M = "/M";

            /// <summary>
            /// (Optional; PDF 1.1) A set of flags specifying various characteristics of the annotation.
            /// Default value: 0.
            /// </summary>
            [KeyInfo("1.1", KeyType.Integer | KeyType.Optional)]
            public const string F = "/F";

            /// <summary>
            /// (Optional; PDF 1.2) An appearance dictionary specifying how the annotation shall be presented
            /// visually on the page. A PDF writer shall include an appearance dictionary when writing or
            /// updating the PDF file except for the two cases listed below. Every annotation(including those
            /// whose Subtype value is Widget, as used for form fields), except for the two cases listed below,
            /// shall have at least one appearance dictionary.
            /// • Annotations where the value of the Rect key consists of an array where the value at index 1
            ///   is equal to the value at index 3 and the value at index 2 is equal to the value at index 4.
            ///   NOTE(2020)
            ///   The bullet point above was changed from “or” to “and” in this document to match requirements
            ///   in other published ISO PDF standards(such as PDF/A).
            /// • Annotations whose Subtype value is Popup, Projection or Link.
            /// </summary>
            [KeyInfo("1.2", KeyType.Dictionary | KeyType.Optional)]  // type is PdfAnnotationAppearance
            public const string AP = "/AP";

            /// <summary>
            /// (Required if the appearance dictionary AP contains one or more subdictionaries; PDF 1.2)
            /// The annotation’s appearance state, which selects the applicable appearance stream from an
            /// appearance subdictionary (see 12.5.5, "Appearance streams").
            /// </summary>
            [KeyInfo("1.2", KeyType.Name | KeyType.Optional)]
            public const string AS = "/AS";

            /// <summary>
            /// (Optional) An array specifying the characteristics of the annotation’s border, which
            /// shall be drawn as a rounded rectangle.
            /// (PDF 1.0) The array consists of three numbers defining the horizontal corner radius,
            /// vertical corner radius, and border width, all in default user space units.If the corner
            /// radii are 0, the border has square(not rounded) corners; if the border width is 0, no
            /// border is drawn.
            /// (PDF 1.1) The array may have a fourth element, an optional dash array defining a pattern
            /// of dashes and gaps that shall be used in drawing the border.The dash array shall be
            /// specified in the same format as in the line dash pattern parameter of the graphics
            /// state(see 8.4.3.6, "Line dash pattern"). The dash phase shall not be specified and
            /// shall be assumed to be 0.
            /// EXAMPLE A Border value of[0 0 1[3 2]] specifies a border 1 unit wide, with square corners,
            /// drawn with 3-unit dashes alternating with 2- unit gaps.
            /// NOTE
            /// (PDF 1.2) The dictionaries for some annotation types (such as free text and polygon
            /// annotations) can include the BS entry.That entry specifies a border style dictionary
            /// that has more settings than the array specified for the Border entry.If an annotation
            /// dictionary includes the BS entry, then the Border entry is ignored.
            /// Default value: [0 0 1].
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Border = "/Border";

            /// <summary>
            /// (Optional; PDF 1.1) An array of numbers in the range 0.0 to 1.0, representing a colour
            /// used for the following purposes:<br/>
            ///     The background of the annotation’s icon when closed<br/>
            ///     The title bar of the annotation’s popup window<br/>
            ///     The border of a link annotation<br/>
            /// The number of array elements determines the colour space in which the colour shall be defined:<br/>
            /// 0 No colour; transparent<br/>
            /// 1 DeviceGray<br/>
            /// 3 DeviceRGB<br/>
            /// 4 DeviceCMYK
            /// </summary>
            [KeyInfo("1.1", KeyType.Array | KeyType.Optional)]
            public const string C = "/C";

            /// <summary>
            /// (Required if the annotation is a structural content item; PDF 1.3)
            /// The integer key of the annotation’s entry in the structural parent tree.
            /// </summary>
            // #PDF-UA
            [KeyInfo("1.3", KeyType.Integer | KeyType.Optional)]
            public const string StructParent = "/StructParent";

            /// <summary>
            /// (Optional; PDF 1.5) An optional content group or optional content membership dictionary
            /// specifying the optional content properties for the annotation. Before the annotation
            /// is drawn, its visibility shall be determined based on this entry as well as the annotation
            /// flags specified in the F entry. If it is determined to be invisible, the annotation shall
            /// not be drawn.
            /// </summary>
            [KeyInfo("1.5", KeyType.Dictionary | KeyType.Optional)]
            public const string OC = "/OC";

            /// <summary>
            /// (Optional; PDF 2.0) An array of one or more file specification dictionaries which
            /// denote the associated files for this annotation.
            /// </summary>
            [KeyInfo("2.0", KeyType.ArrayOfDictionaries | KeyType.Optional)]
            public const string AF = "/AF";

            /// <summary>
            /// (Optional; PDF 2.0) When regenerating the annotation's appearance stream,
            /// this is the opacity value (11.2, "Overview of transparency") that shall be used
            /// for all nonstroking operations on all visible elements of the annotation in its
            /// closed state (including its background and border) but not the popup window that
            /// appears when the annotation is opened.
            /// Default value: 1.0
            /// The specified value shall not be used if the annotation has an appearance stream;
            /// in that case, the appearance stream shall specify any transparency.
            /// If no explicit appearance stream is defined for the annotation, and the processor is not
            /// able to regenerate the appearance, the annotation may be painted by implementation-dependent
            /// means that do not necessarily conform to the PDF imaging
            /// </summary>
            [KeyInfo("2.0", KeyType.Integer | KeyType.Optional)]
            public const string ca = "/ca";

            /// <summary>
            /// (Optional; PDF 1.4, PDF 2.0 for non-markup annotations) When regenerating the annotation's
            /// appearance stream, this is the opacity value (11.2, "Overview of transparency") that shall
            /// be used for stroking all visible elements of the annotation in its closed state, including
            /// its background and border, but not the popup window that appears when the annotation is
            /// opened.
            /// If a ca entry is not present in this dictionary, then the value of this CA entry shall also
            /// be used for nonstroking operations as well.
            /// Default Value: 1.0
            /// The specified value shall not be used if the annotation has an appearance stream;
            /// in that case, the appearance stream shall specify any transparency.
            /// If no explicit appearance stream is defined for the annotation, and the processor is not
            /// able to regenerate the appearance, the annotation may be painted by implementation-dependent
            /// means that do not necessarily conform to the PDF imaging model; in this case, the effect of
            /// this entry is implementation-dependent as well.
            /// </summary>
            [KeyInfo("1.4", KeyType.Integer | KeyType.Optional)]
            public const string CA = "/CA";

            /// <summary>
            /// (Optional; PDF 2.0) The blend mode that shall be used when painting the annotation onto the
            /// page. If this key is not present, blending shall take place using the Normal blend mode. The
            /// value shall be a name object, designating one of the standard blend modes listed in
            /// "Table 134 — Standard separable blend modes" and
            /// "Table 135 — Standard non-separable blend modes".
            /// </summary>
            [KeyInfo("2.0", KeyType.Name | KeyType.Optional)]
            public const string BM = "/BM";

            /// <summary>
            /// (Optional; PDF 2.0) A language identifier overriding the document’s language identifier to
            /// specify the natural language for all text in the annotation except where overridden by other
            /// explicit language specifications.
            /// </summary>
            [KeyInfo("2.0", KeyType.TextString | KeyType.Optional)]
            public const string Lang = "/Lang";

            /* Excerpt from the specs:
             * A PDF reader shall render the appearance dictionary without regard to any other keys and values in
             * the annotation dictionary and shall ignore the values of the 
             * C, IC, Border, BS, BE, BM, CA, ca, H, DA, Q, DS, LE, LL, LLE, and Sy keys.
             */

            // ----- Excerpt of entries specific to markup annotations ----------------------------------

#if true_ // TODO Remove this. See PdfMarkupAnnotation.
            /// <summary>
            /// (Optional; PDF 1.1) The text label to be displayed in the title bar of the annotation’s
            /// pop-up window when open and active. By convention, this entry identifies
            /// the user who added the annotation.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Optional; PDF 1.3) An indirect reference to a pop-up annotation for entering or
            /// editing the text associated with this annotation.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Popup = "/Popup";

            ///// <summary>
            ///// (Optional; PDF 1.4) The constant opacity value to be used in painting the annotation.
            ///// This value applies to all visible elements of the annotation in its closed state
            ///// (including its background and border) but not to the popup window that appears when
            ///// the annotation is opened.
            ///// The specified value is not used if the annotation has an appearance stream; in that
            ///// case, the appearance stream must specify any transparency. (However, if the viewer
            ///// regenerates the annotation’s appearance stream, it may incorporate the CA value
            ///// into the stream’s content.)
            ///// The implicit blend mode is Normal.
            ///// Default value: 1.0.
            ///// </summary>
            //[KeyInfo(KeyType.Real | KeyType.Optional)]
            //public const string CA = "/CA";

            //RC
            //CreationDate
            //IRT

            /// <summary>
            /// (Optional; PDF 1.5) Text representing a short description of the subject being
            /// addressed by the annotation.
            /// </summary>
            [KeyInfo("1.5", KeyType.TextString | KeyType.Optional)]
            public const string Subj = "/Subj";

            //RT
            //IT
#endif
            // ReSharper restore InconsistentNaming

            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
