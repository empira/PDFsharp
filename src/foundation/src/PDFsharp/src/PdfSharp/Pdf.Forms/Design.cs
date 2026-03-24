// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

// v7.0.0 TODO review, Fields OTT

namespace PdfSharp.Pdf.Forms
{
    /// <summary>
    /// Base class for all interactive form (AcroForm) fields.
    /// Fields are always created as indirect objects.
    /// </summary>
    public abstract class PdfFormField : PdfDictionary // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.4  Field dictionaries / Page 530

        internal PdfFormField(PdfDocument document)
            : base(document, true)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormField" /> class.
        /// </summary>
        protected PdfFormField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the name of this field.
        /// </summary>
        /// <remarks>
        /// Returns the value of the /T key.
        /// </remarks>
        public string Name 
            => Elements.GetString(Keys.T);

        public string FullName
        {
            get
            {
                var fullName = Parent?.FullName;
                var name = Name;

                // The /T entry is required. Only a pure widget at a leaf doesn’t have one. It is recognized as a
                // PdfFormFieldWidget, as the widget may also contain field keys. If it doesn’t have a name, simply
                // return its parent’s FullName.
                if (String.IsNullOrEmpty(name))
                    return fullName ?? throw new NotImplementedException("Should not occur: fullName is null.");

                fullName = String.IsNullOrEmpty(fullName)
                    ? name
                    : fullName + "." + name;

                return fullName;
            }
        }

        public PdfFormField? Parent 
            => Elements.GetDictionary<PdfFormField>(Keys.Parent);

        /// <summary>
        /// Gets the actual type of the field with inheritance considered.
        /// </summary>
        public virtual Type FieldType => GetType();

        /// <summary>
        /// Gets or sets the field flags of this instance.
        /// </summary>
        /// <remarks>
        /// Returns the value of the /Ff key.
        /// </remarks>
        internal PdfFormFieldFlags Flags
        {
            get
            {
                if (Elements.TryGetEnum<PdfFormFieldFlags>(Keys.Ff, out var result))
                    return result;

                var parent = Parent;
                return parent?.Flags ?? 0;
            }
            set => Elements.SetIntegerFlag(Keys.Ff, (int)value);
        }

        /// <summary>
        /// Gets or sets the value of the field, considering inheritance.
        /// </summary>
        public PdfItem? Value
        {
            get => GetElementsValue<PdfItem>(Keys.V);
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("The field is read only.");

                if (value == null)
                {
                    Elements.Remove(Keys.V);
                    Parent?.Value = null;
                    return;
                }

                if (value is PdfString or PdfName)
                    Elements[Keys.V] = value;
                else
                    throw new NotImplementedException("Values other than string or name cannot be set.");
            }
        }

        /// <summary>
        /// Returns false, if the field shall not be considered a field but simply a widget annotation.
        /// </summary>
        public bool IsFullyQualifiedField()
        {
            // According to Reference 2.0: 12.7.4.2 Field names / Page 532,
            // a field may have no partial field name (/T). This shall than not be considered a field but simply a widget annotation.
            // Such a field shall only differ in properties specifying the visual appearance and is only one of possibly more representations of the same underlying field.
            return !String.IsNullOrEmpty(Name);
        }

        public bool TestFlag(PdfFormFieldFlags flag)
        {
            var result = (Flags & flag) == flag;
            return result;
        }

        public void SetFlag(PdfFormFieldFlags flag, bool value)
        {
            if (value)
                SetFlag(flag);
            else
                ClearFlag(flag);
        }

        public void SetFlag(PdfFormFieldFlags flag) => Flags |= flag;

        public void ClearFlag(PdfFormFieldFlags flag) => Flags &= ~flag;

        /// <summary>
        /// Gets or sets a value indicating whether the field flag ReadOnly is set.
        /// </summary>
        public bool IsReadOnly
        {
            get => TestFlag(PdfFormFieldFlags.ReadOnly);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.ReadOnly);
                else
                    ClearFlag(PdfFormFieldFlags.ReadOnly);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field flag NoExport is set.
        /// If set to true, the field shall not be exported.
        /// </summary>
        public bool IsNoExport
        {
            get => TestFlag(PdfFormFieldFlags.NoExport);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.NoExport);
                else
                    ClearFlag(PdfFormFieldFlags.NoExport);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field flag Required is set.
        /// If set to true, the field is required.
        /// </summary>
        public bool IsRequired
        {
            get => TestFlag(PdfFormFieldFlags.Required);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.Required);
                else
                    ClearFlag(PdfFormFieldFlags.Required);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field flag MultiSelect is set.
        /// If set to true, a choice field allows selection of multiple items.
        /// </summary>
        public bool IsMultiSelect
        {
            get => TestFlag(PdfFormFieldFlags.MultiSelect);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.MultiSelect);
                else
                    ClearFlag(PdfFormFieldFlags.MultiSelect);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field flag Multiline is set.
        /// If set to true, a text field may have multiple lines.
        /// </summary>
        public bool IsMultiLine
        {
            get => TestFlag(PdfFormFieldFlags.Multiline);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.Multiline);
                else
                    ClearFlag(PdfFormFieldFlags.Multiline);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field flag Password is set.
        /// If set to true, a text field doesn’t show its content as clear text.
        /// </summary>
        public bool IsPassword
        {
            get => TestFlag(PdfFormFieldFlags.Password);
            set
            {
                if (value)
                    SetFlag(PdfFormFieldFlags.Password);
                else
                    ClearFlag(PdfFormFieldFlags.Password);
            }
        }

        /// <summary>
        /// Indicates whether the field has children in the /Kids array.
        /// </summary>
        public bool HasKids
        {
            get
            {
                var fields = Elements.GetArray<PdfFormFields>(Keys.Kids);
                return fields?.Elements.Count > 0;
            }
        }

        /// <summary>
        /// Gets all fields within this field.
        /// </summary>
        /// <param name="onlyFullyQualifiedFields">Returns only fields that shall not be considered as widget annotations only.</param>
        public IEnumerable<PdfFormField> GetKids(bool onlyFullyQualifiedFields = false)
        {
            if (!HasKids)
                yield break;

            foreach (var kidField in Kids)
            {
                if (onlyFullyQualifiedFields && !kidField.IsFullyQualifiedField())
                    continue;

                yield return kidField;
            }
        }

        /// <summary>
        /// Gets all fields within this field recursively.
        /// </summary>
        /// <param name="onlyFullyQualifiedFields">Returns only fields that shall not be considered as widget annotations only.</param>
        public IEnumerable<PdfFormField> GetDescendants(bool onlyFullyQualifiedFields = false)
        {
            foreach (var kidField in GetKids(onlyFullyQualifiedFields))
            {
                yield return kidField;

                foreach (var descendant in kidField.GetDescendants(onlyFullyQualifiedFields))
                    yield return descendant;
            }
        }

        /// <summary>
        /// Gets the collection of fields within this field.
        /// </summary>
        public PdfFormFields Kids => Elements.GetRequiredArray<PdfFormFields>(Keys.Kids, VCF.Create);

        /// <summary>
        /// Gets a value of a field entry, considering inheritance.
        /// </summary>
        public T? GetElementsValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                                 DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
            (string key) where T : PdfItem
        {
            return GetElementsValue<T>(key, out _);
        }

        /// <summary>
        /// Gets a value of a field entry, considering inheritance.
        /// </summary>
        public T? GetElementsValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                               DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
            (string key, out PdfFormField? fieldHoldingValue) where T : PdfItem
        {
            // Due to inheritance, the value of an entry can be set at a field, that is not of the type that defines the key.
            // In some cases it is also important to know, at which field in the hierarchy the value is set.

            var value = Elements.GetValue<T>(key);
            // Option 1: Value is set at this field.
            if (value != null)
                fieldHoldingValue = this;
            // Option 2: Value may be set at parent field: Recursion.
            else if (Parent != null)
                value = Parent.GetElementsValue<T>(key, out fieldHoldingValue);
            // Option 3: Value is not set.
            else
                fieldHoldingValue = null;

            return value;
        }

        /// <summary>
        /// Sets the value of a field entry, checking the type with the given meta.
        /// </summary>
        internal void SetElementsValue<T>(DictionaryMeta meta, string key, T value) where T : PdfItem
        {
            // Due to inheritance, the value of an entry can be set at a field, that is not of the type that defines the key.

            var descriptor = meta[key];
            var expectedType = descriptor?.GetValueType();
            var actualType = typeof(T);

            if (expectedType != null && expectedType.IsAssignableFrom(actualType))
                throw new Exception($"Value of {key} must be of type {expectedType}. The value of type {actualType} can’t be assigned");

            Elements[key] = value;
        }

        /// <summary>
        /// Gets a value indicating whether Acro field contains keys of a PdfWidgetAnnotation.
        /// </summary>
        public bool ContainsWidgetPart // TODO: FormsCleanUp: Remove. Use GetAsWidgetAnnotation() or TryGetAsWidgetAnnotation() instead.
        {
            get
            {
                // TODO: Test may be too simple.
                if (Elements.TryGetName("/Subtype", out var value))
                {
                    return value == "/Widget";
                }
                return false;
            }
        }

        internal bool TryGetAsWidgetAnnotation([MaybeNullWhen(false)] out PdfWidgetAnnotation result)
        {
            if (!PdfWidgetAnnotation.IsWidgetAnnotation(this))
            {
                result = _widget = null;
                return false;
            }

            result = _widget ??= new(this);
            return true;
        }

        internal PdfWidgetAnnotation GetAsWidgetAnnotation()
        {
            if (!TryGetAsWidgetAnnotation(out var result))
            {
                throw new InvalidOperationException(
                    $"This Acro field is of subtype '{Elements.GetName(PdfAnnotation.Keys.Subtype, false, "/???")}', but it should be of subtype '/Widget'.");
            }
            return result;
        }
        internal PdfWidgetAnnotation? _widget;  // If the object ID of the field changes the widget must be adjusted too.

        internal override void PrepareForSave()
        {
            //base.PrepareForSave();
            if (Elements.TryGetValue<PdfInteger>(Keys.Ff, out var item))
                item.IsFlag = true;

            foreach (var field in GetKids())
            {
                field.PrepareForSave();
            }
        }

        internal override void WriteObject(PdfWriter writer)
        {
#if DEBUG
            if (writer.IsVerboseLayout)
            {
                if (PdfWidgetAnnotation.IsWidgetAnnotation(this))
                    writer.WriteComment("This field is also a widget");
            }
#endif
            base.WriteObject(writer);
        }

        /// <summary>
        /// Gets the derived type of PdfFormField.
        /// Returns field type and a flag that indicates if the field is also a widget.
        /// </summary>
        internal static (Type Type, bool IsWidget) GetAcroFieldType(PdfDictionary dict)
        {
            bool isWidget = dict.Elements.GetName(PdfAnnotation.Keys.Subtype) == "/Widget";
            string ft = dict.Elements.GetName(Keys.FT);
            PdfFormFieldFlags flags = (PdfFormFieldFlags)dict.Elements.GetInteger(Keys.Ff);
            switch (ft)
            {
                case PdfFormFieldType.ButtonLiteral:
                    if ((PdfFormFieldFlags.Pushbutton & flags) != 0)
                        return (typeof(PdfFormPushButtonField), isWidget);

                    if ((PdfFormFieldFlags.Radio & flags) != 0)
                        return (typeof(PdfFormRadioButtonField), isWidget);

                    return (typeof(PdfFormCheckBoxField), isWidget);

                case PdfFormFieldType.TextLiteral:
                    return (typeof(PdfFormTextField), isWidget);

                case PdfFormFieldType.ChoiceLiteral:
                    return (PdfFormFieldFlags.Combo & flags) == 0
                        ? (typeof(PdfFormListBoxField), isWidget)
                        : (typeof(PdfFormComboBoxField), isWidget);

                case PdfFormFieldType.SignatureLiteral:
                    return (typeof(PdfFormSignatureField), isWidget);

                default:
                    // The field has no field type.
                    if (isWidget)
                    {
                        // A field without field type but widget aspect is a PdfFormFieldWidget.
                        return (typeof(PdfFormFieldWidget), true);
                    }

                    // A field without field type and no widget aspect is a PdfFormFieldNode.
                    return (typeof(PdfFormFieldNode), false);
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public class Keys : KeysBase
        {
            // Reference 2.0: Table 226 — Entries common to all field dictionaries / Page 531

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required for terminal fields; inheritable) The type of field that this dictionary describes:<br/>
            /// Btn Button<br/>
            /// Tx Text<br/>
            /// Ch Choice<br/>
            /// Sig (PDF 1.3) Signature<br/>
            /// This entry may be present in a non-terminal field(one whose descendants are fields) to
            /// provide an inheritable FT value.However, a non-terminal field does not logically have a
            /// type of its own; it is merely a container for inheritable attributes that are intended
            /// for descendant terminal fields of any type.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string FT = "/FT";

            /// <summary>
            /// (Required if this field is the child of another in the field hierarchy; absent otherwise)
            /// The field that is the immediate parent of this one (the field, if any, whose Kids array
            /// includes this field). A field can have at most one parent; that is, it can be included in
            /// the Kids array of at most one other field.
            /// </summary>
            [KeyInfo(KeyType.Dictionary)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Sometimes required, as described below) An array of indirect references to the immediate
            /// children of this field.
            /// In a non-terminal field, the Kids array shall refer to field dictionaries that are immediate
            /// descendants of this field.In a terminal field, the Kids array ordinarily shall refer to one
            /// or more separate widget annotations that are associated with this field. However, if there is
            /// only one associated widget annotation, and its contents have been merged into the field
            /// dictionary, Kids shall be omitted.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional, typeof(PdfFormFields))]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Required) The partial field name.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Optional; PDF 1.3) An alternative field name that shall be used in place of the actual
            /// field name wherever the field shall be identified in the user interface (such as in error
            /// or status messages referring to the field). This text is also useful when extracting the
            /// document’s contents in support of accessibility to users with disabilities or for other
            /// purposes.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string TU = "/TU";

            /// <summary>
            /// (Optional; PDF 1.3) The mapping name that shall be used when exporting interactive form
            /// field data from the document.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string TM = "/TM";

            /// <summary>
            /// (Optional; inheritable) A set of flags specifying various characteristics of the field.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Ff = "/Ff";

            /// <summary>
            /// (Optional; inheritable) The field’s value, whose format varies depending on the field type.
            /// See the descriptions of individual field types for further information.
            /// </summary>
            [KeyInfo(KeyType.Various | KeyType.Optional)]
            public const string V = "/V";

            /// <summary>
            /// (Optional; inheritable) The default value to which the field reverts when a reset-form
            /// action is executed. The format of this value is the same as that of V.
            /// </summary>
            [KeyInfo(KeyType.Various | KeyType.Optional)]
            public const string DV = "/DV";

            /// <summary>
            /// (Optional; PDF 1.2) An additional-actions dictionary defining the field’s behaviour in
            /// response to various trigger events. This entry has exactly the same meaning as the AA
            /// entry in an annotation dictionary.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            // ReSharper restore InconsistentNaming

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

namespace PdfSharp.Pdf.Forms
{
    /// <summary>
    /// TODO MaOs4StLa What does that mean: Node interactive form (AcroForm) fields.
    /// </summary>
    public class PdfFormFieldWidget : PdfFormField // TODO: FormsCleanUp: Move to own file.
    {
        //   // Reference 2.0: 12.7.4  Field dictionaries / Page 530

        internal PdfFormFieldWidget(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormFieldWidget" /> class.
        /// </summary>
        protected PdfFormFieldWidget(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets the actual type of the field with inheritance considered.
        /// </summary>
        public override Type FieldType
        {
            get
            {
                // PdfFormFieldWidget doesn’t define the field type, so we ask the parent.
                var parent = Parent;
                if (parent == null)
                    throw new Exception("PdfFormFieldWidget must have a parent, which defines the type of the field.");
                return parent.FieldType;
            }
        }
    }
}

namespace PdfSharp.Pdf.Forms
{
    public abstract class PdfFormTextFieldBase : PdfFormField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.4.3  Variable text / Page 533

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormTextFieldBase" /> class.
        /// </summary>
        internal PdfFormTextFieldBase(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormTextFieldBase(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormField.Keys
        {
            // Reference 2.0: Table 228 — Additional entries common to all fields containing variable text / Page 533

            // ReSharper disable InconsistentNaming

            ///// <summary>
            ///// (Required; inheritable) A resource dictionary containing default resources
            ///// (such as fonts, patterns, or color spaces) to be used by the appearance stream.
            ///// At a minimum, this dictionary must contain a Font entry specifying the resource
            ///// name and font dictionary of the default font for displaying the field’s text.
            ///// </summary>
            //[KeyInfo(KeyType.Dictionary | KeyType.Required)]
            //public const string DR = "/DR";

            /// <summary>
            /// (Required; inheritable) The default appearance string containing a sequence of valid
            /// page-content graphics or text state operators that define such properties as the field’s
            /// text size and colour.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Required)]
            public const string DA = "/DA";

            /// <summary>
            /// (Optional; inheritable) A code specifying the form of quadding (justification) that shall
            /// be used in displaying the text:<br/>
            /// 0 Left-justified<br/>
            /// 1 Centred<br/>
            /// 2 Right-justified<br/>
            /// Default value: 0 (left-justified).
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            /// <summary>
            /// (Optional; PDF 1.5) A default style string, as described in Adobe XML Architecture,
            /// XML Forms Architecture (XFA) Specification, version 3.3.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string DS = "/DS";

            /// <summary>
            /// (Optional; PDF 1.5) A rich text string, as described in Adobe XML Architecture,
            /// XML Forms Architecture (XFA) Specification, version 3.3.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string RV = "/RV";

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

namespace PdfSharp.Pdf.Forms
{
    internal static class PdfFormFieldType // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5  Field types / Page 535
        public const string ButtonLiteral = "/Btn";
        public const string TextLiteral = "/Tx";
        public const string ChoiceLiteral = "/Ch";
        public const string SignatureLiteral = "/Sig";

        public static readonly Name Button = new(ButtonLiteral);
        public static readonly Name Text = new(TextLiteral);
        public static readonly Name Choice = new(ChoiceLiteral);
        public static readonly Name Signature = new(SignatureLiteral);
    }
}

namespace PdfSharp.Pdf.Forms
{
    public abstract class PdfFormButtonField : PdfFormField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.2  Button fields / Page 535

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormButtonField" /> class.
        /// </summary>
        internal PdfFormButtonField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormButtonField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfFormField.Keys.FT, PdfFormFieldType.Button.Value);
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormField.Keys
        {
            // Buttons have no additional entries.
        }
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormPushButtonField : PdfFormButtonField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.2.2  Push-buttons / Page 536

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormPushButtonField" /> class.
        /// </summary>
        internal PdfFormPushButtonField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormPushButtonField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            SetFlag(PdfFormFieldFlags.Pushbutton);
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormField.Keys
        {
            // Push-buttons have no additional entries.
        }
    }
}

namespace PdfSharp.Pdf.Forms
{
    public abstract class PdfFormCheckBoxOrRadioButtonField : PdfFormButtonField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.2.3 Check boxes / Page 536

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormCheckBoxOrRadioButtonField" /> class.
        /// </summary>
        internal PdfFormCheckBoxOrRadioButtonField(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormCheckBoxOrRadioButtonField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormButtonField.Keys
        {
            // Reference 2.0: Table 230 — Additional entry specific to check box and radio button fields / Page 537

            /// <summary>
            /// (Optional; inheritable; PDF 1.4) An array containing one entry for each widget annotation
            /// in the Kids array of the radio button or check box field. Each entry shall be a text string
            /// representing the on state of the corresponding widget annotation.
            /// When this entry is present, the names used to represent the on state in the AP dictionary of
            /// each annotation may use numerical position (starting with 0) of the annotation in the Kids
            /// array, encoded as a name object (for example: /0, /1). This allows distinguishing between
            /// the annotations even if two or more of them have the same value in the Opt array.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

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

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormCheckBoxField : PdfFormCheckBoxOrRadioButtonField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.2.3 Check boxes / Page 536

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormCheckBoxField" /> class.
        /// </summary>
        internal PdfFormCheckBoxField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormCheckBoxField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            // -- Reference 2.0: Table 229 — Field flags specific to button fields
            // No flag to set here.
        }
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormRadioButtonField : PdfFormCheckBoxOrRadioButtonField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.2.4 Radio buttons / Page 538

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormRadioButtonField" /> class.
        /// </summary>
        internal PdfFormRadioButtonField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormRadioButtonField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            // NoToggleToOff should be by default. When leaving it clear, clicking the selected radiobutton leaves no radiobutton of the group selected.
            SetFlag(PdfFormFieldFlags.Radio | PdfFormFieldFlags.NoToggleToOff);
        }

        // Check boxes have no additional key entries.
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormTextField : PdfFormTextFieldBase // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.3  Text fields / Page 539

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormTextField" /> class.
        /// </summary>
        internal PdfFormTextField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormTextField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfFormField.Keys.FT, PdfFormFieldType.Text.Value);

            // -- Reference 2.0: Table 231 — Field flags specific to text fields
            // No flag to set here.
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormField.Keys
        {
            // Reference 2.0: Table 232 — Additional entry specific to a text field / Page 541

            /// <summary>
            /// (Optional; inheritable) The maximum length of the field’s text, in characters.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string MaxLen = "/MaxLen";

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

namespace PdfSharp.Pdf.Forms
{
    public abstract class PdfFormChoiceField : PdfFormTextFieldBase // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.4  Choice fields / Page 541

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormChoiceField" /> class.
        /// </summary>
        internal PdfFormChoiceField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormChoiceField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            Elements.SetName(PdfFormField.Keys.FT, PdfFormFieldType.Choice.Value);
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// </summary>
        public new class Keys : PdfFormTextFieldBase.Keys
        {
            // Reference 2.0: Table 234 — Additional entries specific to a choice field / Page 542

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) An array of options that shall be presented to the user. Each element of the
            /// array is either a text string representing one of the available options or an array
            /// consisting of two text strings: the option’s export value and the text that shall be
            /// displayed as the name of the option.
            /// If this entry is not present, no choices should be presented to the user.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

            /// <summary>
            /// (Optional) For scrollable list boxes, the top index (the index in the Opt array of the
            /// first option visible in the list).
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string TI = "/TI";

            /// <summary>
            /// (Sometimes required, otherwise optional; PDF 1.4) For choice fields that allow multiple
            /// selection (MultiSelect flag set), an array of integers, sorted in ascending order,
            /// representing the zero-based indices in the Opt array of the currently selected option
            /// items. This entry shall be used when two or more elements in the Opt array have different
            /// names but the same export value or when the value of the choice field is an array. If the
            /// items identified by this entry differ from those in the V entry of the field dictionary
            /// (see discussion following this Table), the V entry shall be used.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string I = "/I";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal new static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));
            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormListBoxField : PdfFormChoiceField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.4  Choice fields / Page 541

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormListBoxField" /> class.
        /// </summary>
        internal PdfFormListBoxField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormListBoxField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            // -- Reference 2.0: Table 233 — Field flags specific to choice fields
            // No flag to set here.
        }

        // List boxes have no additional key entries.
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormComboBoxField : PdfFormChoiceField // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.4  Choice fields / Page 541

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormListBoxField" /> class.
        /// </summary>
        internal PdfFormComboBoxField(PdfDocument document)
            : base(document)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormComboBoxField(PdfDictionary dict)
            : base(dict)
        { }

        void Initialize()
        {
            //// -- Reference 2.0: Table 233 — Field flags specific to choice fields
            //AddFieldFlags(PdfFormFieldFlags.Combo);
            SetFlag(PdfFormFieldFlags.Combo);
        }

        // Combo boxes have no additional key entries.
    }
}

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormSignatureField : PdfFormTextFieldBase // TODO: FormsCleanUp: Move to own file.
    {
        // Reference 2.0: 12.7.5.5  Signature fields / Page 543

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormSignatureField" /> class.
        /// </summary>
        internal PdfFormSignatureField(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormSignatureField(PdfDictionary dict)
            : base(dict)
        { }

        // New code: Renders appearance when adding the field.
        internal void RenderAppearance()
        {
            if (CustomAppearanceHandler != null!)
                RenderCustomAppearance();
        }

        /// <summary>
        /// Handler that creates the visual representation of the digital signature in PDF.
        /// </summary>
        public IAnnotationAppearanceHandler? CustomAppearanceHandler { get; internal set; }

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
            if (!Elements.TryGetValue<PdfDictionary>(PdfAnnotation.Keys.AP, out var ap))
            {
                ap = new PdfDictionary(Document);
                Elements[PdfAnnotation.Keys.AP] = ap;
            }

            // Set XRef to normal state
            ap.Elements["/N"] = form.PdfForm.RequiredReference;

            // PdfRenderer can be null.
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            form.PdfRenderer?.Close();
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new class Keys : PdfFormTextFieldBase.Keys
        {
            // Reference 2.0: Table 235 — Additional entries specific to a signature field / Page 544

            /// <summary>
            /// (Optional; shall be an indirect reference; PDF 1.5) A signature field lock dictionary
            /// that specifies a set of form fields that shall be locked when this signature field is
            /// signed.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Lock = "/Lock";

            /// <summary>
            /// (Optional; shall be an indirect reference; PDF 1.5) A seed value dictionary containing
            /// information that constrains the properties of a signature that is applied to this field
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string SV = "/SV";

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

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormSignatureFieldLock : PdfDictionary // TODO: FormsCleanUp: NOT USED BY NOW! Move to own file.
    {
        // Reference 2.0: TODO

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormSignatureFieldLock" /> class.
        /// </summary>
        internal PdfFormSignatureFieldLock(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormSignatureFieldLock(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase  // TODO
        {
            // TODO Reference 2.0: Table 237 — Entries in a signature field seed value dictionary / Page 545

            /// <summary>
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

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormSignatureFieldSeedValue : PdfDictionary // TODO: FormsCleanUp: NOT USED BY NOW! Move to own file.
    {
        // Reference 2.0: TODO

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormSignatureFieldSeedValue" /> class.
        /// </summary>
        internal PdfFormSignatureFieldSeedValue(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormSignatureFieldSeedValue(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase  // TODO
        {
            // TODO Reference 2.0: Table 236 — Entries in a signature field lock dictionary / Page 544

            /// <summary>
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

namespace PdfSharp.Pdf.Forms
{
    public class PdfFormCertificateSeedValue : PdfDictionary // TODO: FormsCleanUp: NOT USED BY NOW! Move to own file.
    {
        // Reference 2.0: TODO

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFormSignatureFieldSeedValue" /> class.
        /// </summary>
        internal PdfFormCertificateSeedValue(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormCertificateSeedValue(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public class Keys : KeysBase  // TODO
        {
            // TODO Reference 2.0: Table 236 — Entries in a signature field lock dictionary / Page 544

            /// <summary>
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
