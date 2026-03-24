// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Forms;
using PdfSharp.Pdf.Advanced;

// v7.0.0 TODO review

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Represents a PDF widget annotation.
    /// </summary>
    public sealed class PdfWidgetAnnotation : PdfAnnotation
    {
        // Reference 2.0: 12.5.6.19  Widget annotations / Page 498

        [Obsolete("PDFsharp 6.4: Use a constructor with a PDF document parameter.")]
        public PdfWidgetAnnotation()
            => throw new InvalidOperationException("PDFsharp 6.4: Use a constructor with a PDF document parameter.");

        public PdfWidgetAnnotation(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfWidgetAnnotation(PdfDictionary dictionary)
            : base(dictionary)
        {
            Initialize();
        }

        internal PdfWidgetAnnotation(PdfFormField acroField)
        {
            Document = acroField.Owner;

            Debug.Assert(acroField.IsIndirect);

            var oldElementsFromBaseClasses = Elements;
            Elements = null!;
            Elements = acroField.Elements;
            foreach (var item in oldElementsFromBaseClasses)
                Elements.Add(item.Key, item.Value);

            // Create new reference. Same ObjectID, but different value.
            _actualIRef = acroField.Reference;
            var iref = new PdfReference(acroField, this);

            Initialize();
        }

        internal static PdfWidgetAnnotation CreateWidgetAnnotationAndField(PdfDocument document)
        {
            var field = new PdfFormFieldWidget(document);
            return new PdfWidgetAnnotation(field);
        }

        void Initialize()
        {
            Elements.SetName(PdfAnnotation.Keys.Subtype, PdfAnnotationTypeNames.Widget);
        }

        internal override PdfReference? ActualReference
        {
            get => _actualIRef ?? base.ActualReference;
        }
        readonly PdfReference? _actualIRef = null;

        internal static bool IsWidgetAnnotation(PdfFormField field)
        {
            return field.Elements.GetName(PdfAnnotation.Keys.Subtype) == PdfAnnotationTypeNames.Widget;
        }

        internal bool IsField()
        {
            return _actualIRef?.Value is PdfFormField;
        }

        internal PdfFormField? GetAsField()
        {
            if (_field != null)
                return _field;

            return _field ??= _actualIRef?.Value as PdfFormField;
        }
        PdfFormField? _field;

        /// <summary>
        /// Returns the shared field object or the parent field.
        /// The returned field may be a field, that shall not be considered a field but simply a widget.
        /// However, those fields still may have the /V entry set. Therefore, return that field instead of the bottom-most fully qualified field.
        /// Remember to get the bottom-most fully qualified field to access all the fields widgets, for example.
        /// </summary>
        public PdfFormField? GetField()
        {
            var field = GetAsField();
            if (field != null)
            {
                // This widget is also a field: return the referred field object.
                return field;
            }

            field = GetParent();

            if (field == null)
                PdfSharpLogHost.Logger.LogWarning($"No parent field could be found for the widget annotation with object ID '{Reference?.ObjectID}'.");

            return field;
        }


        PdfFormField? GetParent()
        {
            var parentFromKey = Elements.GetDictionary<PdfFormField>(Keys.Parent);

            var parentFromSearch = FindParent();
            Debug.Assert(parentFromKey == null || parentFromSearch == parentFromKey, "Check for the correct parent.");

            return parentFromSearch; // By now, we always return the parent from search.
        }

        PdfFormField? FindParent()
        {
            static IEnumerable<PdfFormField> GetAllFields(IEnumerable<PdfFormField> fields)
            {
                foreach (var field in fields)
                {
                    yield return field;

                    foreach (var kid in GetAllFields(field.GetKids()))
                        yield return kid;
                }
            }

            var acroForm = Document.Catalog.GetAcroForm();
            if (acroForm == null)
                return null;

            var allFields = GetAllFields(acroForm.Fields);


            return allFields.SingleOrDefault(f => f.GetKids().Any(field =>
            {
                // Currently, each kid is a field - for a pure widget a PdfFormFieldWidget is added.
                var widget = field.GetAsWidgetAnnotation();
                return widget == this;
            }));
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfAnnotation.Keys
        {
            // Reference 2.0: Table 191 — Additional entries specific to a widget annotation / Page 499

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The annotation’s highlighting mode, the visual effect to be used when
            /// the mouse button is pressed or held down inside its active area:<br/>
            ///   N (None) No highlighting.<br/>
            ///   I (Invert) Invert the contents of the annotation rectangle.<br/>
            ///   O (Outline) Invert the annotation’s border.<br/>
            ///   P (Push) Display the annotation’s down appearance, if any. If no down appearance is defined,
            ///     offset the contents of the annotation rectangle to appear as if it were being pushed below
            ///     the surface of the page.<br/>
            ///   T (Toggle) Same as P (which is preferred).<br/>
            /// A highlighting mode other than P overrides any down appearance defined for the annotation. <br/>
            /// Default value: I.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string H = "/H";

            /// <summary>
            /// (Optional) An appearance characteristics dictionary that shall be used in constructing a
            /// dynamic appearance stream specifying the annotation’s visual presentation on the page.
            /// The name MK for this entry is of historical significance only and has no direct meaning.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string MK = "/MK";

            /// <summary>
            /// (Optional; PDF 1.1) An action that shall be performed when the annotation is activated.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string A = "/A";

            /// <summary>
            /// (Optional; PDF 1.2) An additional-actions dictionary defining the annotation’s behaviour
            /// in response to various trigger events.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            /// <summary>
            /// (Optional; PDF 1.2) A border style dictionary specifying the width and dash pattern that
            /// shall be used in drawing the annotation’s border.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string BS = "/BS";

            /// <summary>
            /// (Required if this widget annotation is one of multiple children in a field; optional otherwise)
            /// An indirect reference to the widget annotation’s parent field. A widget annotation may have
            /// at most one parent; that is, it can be included in the Kids array of at most one field.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Parent = "/Parent";

            // ReSharper restore InconsistentNaming

            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        internal static string[] WidgetKeys =
        [
            // Keys specific to all annotation.
            // "/Type", comes from AcroFiled
            "/Subtype",  // Can be /Widget
            "/Rect",
            "/Contents",
            "/P",
            "/NM",
            "/M",
            "/F",
            "/AP",
            "/AS",
            "/Border",
            "/C",
            "/StructParent",
            "/OC",

            // Keys specific to a widget annotation.
            "/H",
            "/MK",
            "/A",
            "/AA",
            "/BS",
            "/Parent"
        ];

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;

        //readonly PdfFormField? _acroField;
    }
}

namespace PdfSharp.Pdf.Annotations  // #FILE PdfWidgetAnnotationAppearanceCharacteristics.cs
{
    /// <summary>
    /// Represents a text annotation.
    /// </summary>
    public sealed class PdfWidgetAnnotationAppearanceCharacteristics : PdfDictionary
    {
        public PdfWidgetAnnotationAppearanceCharacteristics()
        { }

        public PdfWidgetAnnotationAppearanceCharacteristics(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfWidgetAnnotationAppearanceCharacteristics(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: 12.5.6.19  Widget annotations / Page 498
            // Reference 2.0: Table 192 — Entries in an appearance characteristics dictionary / Page 500

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) The number of degrees by which the widget annotation shall be rotated
            /// counterclockwise relative to the page. The value shall be a multiple of 90.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string R = "/R";

            /// <summary>
            /// (Optional) An array of numbers that shall be in the range 0.0 to 1.0 specifying the colour
            /// of the widget annotation’s border. The number of array elements determines the colour space
            /// in which the colour shall be defined:
            /// 0 No colour; transparent
            /// 1 DeviceGray
            /// 3 DeviceRGB
            /// 4 DeviceCMYK
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string BC = "/BC";

            /// <summary>
            /// (Optional) An array of numbers that shall be in the range 0.0 to 1.0 specifying the colour
            /// of the widget annotation’s background. The number of array elements shall determine the
            /// colour space, as described for BC.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string BG = "/BG";

            /// <summary>
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string XXX = "/XXX";

            /// <summary>
            /// (Optional; button fields only) The widget annotation’s normal caption, which shall be
            /// displayed when it is not interacting with the user.
            /// Unlike the remaining entries listed in this Table, which apply only to widget annotations
            /// associated with push-button fields, the CA entry may be used with any type of button field,
            /// including check boxes and radio buttons.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string CA = "/CA";

            /// <summary>
            /// (Optional; push-button fields only) The widget annotation’s rollover caption, which shall
            /// be displayed when the user rolls the cursor into its active area without pressing the
            /// mouse button.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string RC = "/RC";

            /// <summary>
            /// (Optional; push-button fields only) The widget annotation’s alternate (down) caption,
            /// which shall be displayed when the mouse button is pressed within its active area.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string AC = "/AC";

            /// <summary>
            /// (Optional; push-button fields only; shall be an indirect reference) A form XObject
            /// defining the widget annotation’s normal icon, which shall be displayed when it is not
            /// interacting with the user.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string I = "/I";

            /// <summary>
            /// (Optional; push-button fields only; shall be an indirect reference) A form XObject defining
            /// the widget annotation’s rollover icon, which shall be displayed when the user rolls the
            /// cursor into its active area without pressing the mouse button.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string RI = "/RI";

            /// <summary>
            /// (Optional; push-button fields only; shall be an indirect reference) A form XObject defining
            /// the widget annotation’s alternate (down) icon, which shall be displayed when the mouse button
            /// is pressed within its active area.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string IX = "/IX";

            /// <summary>
            /// (Optional; push-button fields only) An icon fit dictionary specifying how the widget
            /// annotation’s icon shall be displayed within its annotation rectangle. If present, the icon
            /// fit dictionary shall apply to all of the annotation’s icons (normal, rollover, and alternate).
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string IF = "/IF";

            /// <summary>
            /// (Optional; push-button fields only) A code indicating where to position the text of the widget
            /// annotation’s caption relative to its icon:<br/>
            /// 0 No icon; caption only<br/>
            /// 1 No caption; icon only<br/>
            /// 2 Caption below the icon<br/>
            /// 3 Caption above the icon<br/>
            /// 4 Caption to the right of the icon<br/>
            /// 5 Caption to the left of the icon<br/>
            /// 6 Caption overlaid directly on the icon<br/>
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string TP = "/TP";

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
