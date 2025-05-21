// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the base class for all interactive field dictionaries.
    /// </summary>
    public abstract class PdfAcroField : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of PdfAcroField.
        /// </summary>
        internal PdfAcroField(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAcroField"/> class. Used for type transformation.
        /// </summary>
        protected PdfAcroField(PdfDictionary dict)
            : base(dict)
        {
            DetermineAppearance();
        }

        /// <summary>
        /// Gets the name of this field.
        /// </summary>
        public string Name
        {
            get
            {
                string name = Elements.GetString(Keys.T);
                return name;
            }
            set
            {
                //if (value.Contains('.'))
                //    throw new ArgumentException("Field-names should not contain dots (.)", nameof(value));
                Debug.Assert(!value.Contains('.'), "Field-names should not contain dots (.)");
                Elements.SetString(Keys.T, value);
            }
        }

        /// <summary>
        /// An alternate field name, to be used in place of the actual
        /// field name wherever the field must be identified in the user interface (such as
        /// in error or status messages referring to the field). This text is also useful
        /// when extracting the document’s contents in support of accessibility to disabled
        /// users or for other purposes.
        /// </summary>
        public string AlternateName
        {
            get { return Elements.GetString(Keys.TU); }
            set { Elements.SetString(Keys.TU, value); }
        }

        /// <summary>
        /// The mapping name to be used when exporting interactive form field data from the document.
        /// </summary>
        public string MappingName
        {
            get { return Elements.GetString(Keys.TM); }
            set { Elements.SetString(Keys.TM, value); }
        }

        /// <summary>
        /// Gets the fully qualified name of this field, that is: "parent-name.field-name"
        /// <para>If the field has no parent, this is equal to <see cref="Name"/></para>
        /// </summary>
        /// <remarks>
        /// Note: These names are not required to be unique for a given document.<br></br>
        /// The spec states (12.7.3.2):<br></br>
        /// It is possible for different field dictionaries to have the same fully qualified field name if they are descendants of 
        /// a common ancestor with that name and have no partial field names of their own.
        /// Such field dictionaries are different representations of the same underlying field;
        /// they should differ only in properties that specify their visual appearance.
        /// </remarks>
        public string FullyQualifiedName
        {
            get
            {
                var fqn = Name;
                var parent = Elements.GetObject(Keys.Parent) as PdfDictionary;
                while (parent != null)
                {
                    var parentName = parent.Elements.GetString(Keys.T);
                    if (!string.IsNullOrEmpty(parentName))
                        fqn = parentName + "." + fqn;
                    parent = parent.Elements.GetObject(Keys.Parent) as PdfDictionary;
                }
                return fqn;
            }
        }

        /// <summary>
        /// Gets the Parent of this field or null, if the field has no parent
        /// </summary>
        public PdfAcroField? Parent
        {
            get
            {
                var parentRef = Elements.GetReference(Keys.Parent);
                if (parentRef != null && parentRef.Value is PdfDictionary pDict)
                    return PdfAcroFieldCollection.CreateAcroField(pDict);
                return null;
            }
            internal set
            {
                if (value != null)
                    Elements.SetReference(Keys.Parent, value);
                else
                    Elements.Remove(Keys.Parent);
            }
        }

        /// <summary>
        /// Gets the field flags of this instance.
        /// </summary>
        public PdfAcroFieldFlags Flags //=> (PdfAcroFieldFlags)Elements.GetInteger(Keys.Ff);
        {
            get
            {
                var ancestor = FindParentHavingKey(Keys.Ff);
                return (PdfAcroFieldFlags)ancestor.Elements.GetInteger(Keys.Ff);
            }
            set => Elements.SetInteger(Keys.Ff, (int)value);
        }

        internal PdfAcroFieldFlags SetFlags
        {
            get => (PdfAcroFieldFlags)Elements.GetInteger(Keys.Ff);
            set => Elements.SetInteger(Keys.Ff, (int)value);
        }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public virtual PdfItem? Value
        {
            get
            {
                var ancestor = FindParentHavingKey(Keys.V);
                return ancestor.Elements[Keys.V];
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException("The field is read only.");
                if (value is PdfString or PdfName)
                    Elements[Keys.V] = value;
                else
                    throw new NotImplementedException("Values other than string cannot be set.");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is read only.
        /// </summary>
        public bool ReadOnly
        {
            get => (Flags & PdfAcroFieldFlags.ReadOnly) != 0;
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.ReadOnly;
                else
                    SetFlags &= ~PdfAcroFieldFlags.ReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the font used to draw the text of the field.<br></br>
        /// The font-size may be adjusted during rendering when <see cref="AutomaticFontSize"/> is set to true.<br></br>
        /// In this case, the size of the field's widget determines the actual font-size.
        /// </summary>
        public XFont? Font
        {
            get { return font; }
            set { font = value; }
        }
        XFont? font;

        /// <summary>
        /// Gets the font size that was obtained by analyzing the Fields' content-stream.<br></br>
        /// May be zero. This means, during rendering, the font-size should be calculated based on the height of the field's widget.<br></br>
        /// PdfReference 2.0 states in chapter 12.7.4.3:<br></br>
        /// A zero value for size means that the font shall be auto-sized:
        /// its size shall be computed as an implementation dependent function.
        /// </summary>
        public double? FontSize
        {
            get { return _fontSize; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "FontSize must not be smaller than or equal to zero");
                _fontSize = value;
            }
        }
        private double? _fontSize;

        /// <summary>
        /// Gets or sets a value that determines whether the font should be auto-sized when rendered.<br></br>
        /// Note: It is not specified, how exactly the font-size should be calculated,
        /// but it would typically be a function of the height of the field's widget.<br></br>
        /// Unless you have a very specific reason to do so, you should not set this to true for new fields.<br></br>
        /// (support for this seems to be very poor in most common PDF-viewers)
        /// </summary>
        public bool AutomaticFontSize { get; set; }

        /// <summary>
        /// Gets or sets the foreground (i.e. text-) color of the field.
        /// </summary>
        /// <remarks>
        /// Note: This is not a real property of an AcroField, but a property of an AcroField's annotation.<br></br>
        /// If is included here for convenience so is doesn't has to be set for every annotation separately.
        /// </remarks>
        public XColor ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }
        XColor foreColor = XColors.Black;

        /// <summary>
        /// Gets or sets the default value of the field.
        /// </summary>
        public virtual PdfItem? DefaultValue
        {
            get
            {
                var ancestor = FindParentHavingKey(Keys.DV);
                return ancestor.Elements.ContainsKey(Keys.DV) ? ancestor.Elements.GetValue(Keys.DV) : new PdfString("");
            }
            set { Elements[Keys.DV] = value; }
        }

        /// <summary>
        /// Gets or sets the alignment for the text of this field.
        /// </summary>
        public virtual PdfAcroFieldTextAlignment TextAlign
        {
            get
            {
                var alignment = PdfAcroFieldTextAlignment.Left; // default
                var ancestor = FindParentHavingKey(Keys.Q);
                if (ancestor.Elements.ContainsKey(Keys.Q))
                    alignment = (PdfAcroFieldTextAlignment)ancestor.Elements.GetInteger(Keys.Q);
                else if (_document.AcroForm != null && _document.AcroForm.Elements.ContainsKey(Keys.Q))
                    alignment = (PdfAcroFieldTextAlignment)_document.AcroForm.Elements.GetInteger(Keys.Q);
                return alignment;
            }
            set { Elements[Keys.Q] = new PdfInteger((int)value); }
        }

        /// <summary>
        /// Gets the field with the specified name.
        /// </summary>
        public PdfAcroField? this[string name] => GetValue(name);

        /// <summary>
        /// Gets a child field by name.
        /// </summary>
        protected virtual PdfAcroField? GetValue(string name)
        {
            if (String.IsNullOrEmpty(name))
                return this;
            if (HasChildFields)
                return Fields.GetValue(name);
            return null;
        }

        /// <summary>
        /// Indicates whether the field has annotations, i.e. visible representations of the field.<br></br>
        /// Not all fields have visible representations of their own, e.g. when a field acts only as a container for other fields.
        /// </summary>
        public bool HasAnnotations => Annotations.Elements.Count > 0;

        /// <summary>
        /// Indicates whether the field has child fields.
        /// </summary>
        public bool HasChildFields
        {
            get
            {
                var kidsArray = Elements.GetArray(Keys.Kids);
                if (kidsArray != null)
                {
                    for (var i = 0; i < kidsArray.Elements.Count; i++)
                    {
                        if (kidsArray.Elements.GetObject(i) is PdfDictionary kid && IsField(kid))
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the collection of fields within this field.
        /// </summary>
        public PdfAcroFieldCollection Fields
        {
            get
            {
                if (_fields == null)
                {
                    // owner may be a widget annotation, we have to make sure, the owner is correct,
                    // otherwise an exception occurs (/Kids is not a valid key for Annotations)
                    if (Elements.Owner != this)
                        Elements.ChangeOwner(this);
                    object o = Elements.GetValue(Keys.Kids, VCF.CreateIndirect)!;
                    _fields = (PdfAcroFieldCollection)o;
                }
                // TODO: It would be nice if the FieldCollection contains only "real" fields.
                // Currently, the items are a mix of fields and annotations...
                return _fields;
            }
        }
        PdfAcroFieldCollection? _fields;

        /// <summary>
        /// Gets the annotations for this field.
        /// The elements in this list are of type <see cref="PdfWidgetAnnotation"/>.
        /// </summary>
        public PdfAnnotationArray Annotations
        {
            get
            {
                if (_annotations == null)
                {
                    _annotations = new PdfAnnotationArray();
                    var childs = Elements.GetArray(Keys.Kids);
                    if (childs != null && childs.Elements.Count > 0)
                    {
                        for (var i = 0; i < childs.Elements.Count; i++)
                        {
                            var obj = childs.Elements.GetDictionary(i);
                            if (obj is PdfWidgetAnnotation annotation)
                                _annotations.Elements.Add(annotation);
                            else if (obj != null && "/Widget".Equals(obj.Elements.GetString(PdfAnnotation.Keys.Subtype), StringComparison.OrdinalIgnoreCase))
                            {
                                var annot = new PdfWidgetAnnotation(obj);
                                if (annot.Page != null)
                                    annot.Parent = annot.Page.Annotations;
                                _annotations.Elements.Add(annot);
                                // must reset the value in the reference after type-transformation so a reference to this field points to the field, not the widget
                                obj.Reference!.Value = obj;
                            }
                        }
                    }
                    // if the dictionaries are merged (no childs), use current field as Widget
                    if ("/Widget".Equals(Elements.GetString(PdfAnnotation.Keys.Subtype), StringComparison.OrdinalIgnoreCase))
                    {
                        var annot = new PdfWidgetAnnotation(this);
                        if (annot.Page != null)
                            annot.Parent = annot.Page.Annotations;
                        _annotations.Elements.Add(annot);
                        // must reset the value in the reference after type-transformation
                        Reference!.Value = this;
                    }
                }
                return _annotations;
            }
        }
        PdfAnnotationArray? _annotations;

        /// <summary>
        /// Adds a new Annotation to this field.
        /// </summary>
        /// <param name="configure">A method that is used to configure the Annotation</param>
        /// <returns>The created and configured Annotation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual PdfWidgetAnnotation AddAnnotation(Action<PdfWidgetAnnotation> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var annotation = new PdfWidgetAnnotation(_document)
            {
                ParentField = this
            };
            // must add to iref-table here because we need a valid Reference for the field's Kids-array
            Owner.IrefTable.Add(annotation);

            configure(annotation);
            if (!Elements.ContainsKey(Keys.Kids))
                Elements.GetValue(Keys.Kids, VCF.CreateIndirect);
            var childs = Elements.GetArray(Keys.Kids)!;
            childs.Elements.Add(annotation.Reference!);
            // re-create updated annotations the next time the "Annotations"-Property is accessed
            _annotations = null;
            return annotation;
        }

        /// <summary>
        /// Removes the specified annotation from this field
        /// </summary>
        /// <param name="annotation">The annotation to remove</param>
        /// <returns>true, if the annotation was removed, false otherwise</returns>
        public bool RemoveAnnotation(PdfWidgetAnnotation annotation)
        {
            if (annotation == null)
                return false;
            Debug.Assert(Annotations.Elements.IndexOf(annotation) >= 0, "Annotation is not part of this field");

            var kids = Elements.GetArray(Keys.Kids);
            if (kids != null)
            {
                for (var i = 0; i < kids.Elements.Count; i++)
                {
                    var kid = kids.Elements.GetObject(i);
                    if (kid != null && kid.ObjectID == annotation.ObjectID)
                    {
                        kids.Elements.RemoveAt(i--);
                    }
                }
            }
            var removed = Annotations.Elements.Remove(annotation);
            if (removed)
                annotation.Page?.Annotations.Remove(annotation);
            // re-create updated annotations the next time the "Annotations"-Property is accessed
            _annotations = null;

            return removed;
        }

        /// <summary>
        /// Determines whether the specified field is an actual AcroField.<br></br>
        /// Used to tell apart actual fields from  WidgetAnnotations.<br></br>
        /// PdfReference 1.7, Chapter 12.7.1:
        /// As a convenience, when a field has only a single associated widget annotation, the
        /// contents of the field dictionary and the annotation dictionary(12.5.2, 'Annotation Dictionaries')
        /// may be merged into a single dictionary containing entries that pertain to both a field and an annotation.<br></br>
        /// This means, a PdfObject may be a PdfAcroField AND a PdfWidgetAnnotation at the same time.
        /// We consider a dictionary to be a Field, if one of the following is true:<br></br>
        /// - the /Subtype is missing (which is required for Annotations),<br></br>
        /// - it has an /T or /FT entry (which is required for terminal fields) -> Chapter 12.7.3.1 in PdfReference<br></br>
        /// - it has a /Kids entry (which is invalid for Annotations) -> indicates a container for other fields
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static bool IsField(PdfDictionary field)
        {
            return !field.Elements.ContainsKey(PdfAnnotation.Keys.Subtype)
                || field.Elements.ContainsKey(Keys.FT)
                || field.Elements.ContainsKey(Keys.T)
                || field.Elements.ContainsKey(Keys.Kids);
        }

        /// <summary>
        /// Used to retrieve inherited field-properties from parent-fields.<br></br>
        /// If no parent is found having the specified key, <b>this</b> is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected PdfAcroField FindParentHavingKey(string key)
        {
            var field = this;
            if (!field.Elements.ContainsKey(key))
                field = Parent?.FindParentHavingKey(key);
            return field ?? this;
        }

        /// <summary>
        /// Adds the specified <see cref="PdfAcroField"/> to the list of child-fields of this field
        /// </summary>
        /// <param name="childField"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddChild(PdfAcroField childField)
        {
            var existingField = Fields.GetValue(childField.Name);
            if (existingField != null)
                throw new InvalidOperationException($"Field '{Name}' already has a child-field named '{childField.Name}'");
            Fields.Elements.Add(childField);
            childField.Parent = this;
        }

        /// <summary>
        /// Removes this field, all child-fields and associated annotations from the document
        /// </summary>
        public void Remove()
        {
            var annots = Annotations.Elements.ToArray();
            foreach (var annot in annots)
                RemoveAnnotation(annot);

            // delete childs
            for (var i = 0; i < Fields.Count; i++)
            {
                var child = Fields[i];
                child.Remove();
            }
            Fields.Elements.Clear();

            var fieldsList = new[] { Parent?.Fields, Owner.AcroForm?.Fields };
            foreach (var fields in fieldsList)
            {
                for (var i = 0; i < fields?.Elements.Count; i++)
                {
                    var kid = fields.Elements.GetObject(i);
                    if (kid != null && kid.ObjectID == ObjectID)
                    {
                        fields.Elements.RemoveAt(i--);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to determine the Appearance of the Field by checking elements of its dictionary
        /// </summary>
        protected virtual void DetermineAppearance()
        {
            try
            {
                var ancestor = FindParentHavingKey(Keys.DA);
                var da = ancestor.Elements.GetString(Keys.DA);     // 12.7.3.3
                if (string.IsNullOrEmpty(da) && _document.AcroForm != null)
                {
                    // if Field does not contain appearance dictionary, check AcroForm
                    da = _document.AcroForm.Elements.GetString(Keys.DA);
                    if (da == null)
                    {
                        // no appearance found, use some default
                        font = new XFont("Helvetica", 10, XFontStyleEx.Regular);
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(da))
                    DetermineFontFromContent(PdfEncoders.RawEncoding.GetBytes(da));
            }
            catch
            {
                font = new XFont("Helvetica", 10, XFontStyleEx.Regular);
            }
        }

        /// <summary>
        /// Attempts to determine the font, font-size and fore-color of this AcroField
        /// </summary>
        /// <param name="contentBytes"></param>
        protected void DetermineFontFromContent(byte[] contentBytes)
        {
            string? fontName = null;
            double fontSize = 10.0;
            var content = ContentReader.ReadContent(contentBytes);
            for (var i = 0; i < content.Count; i++)
            {
                if (content[i] is COperator op)
                {
                    switch (op.OpCode.OpCodeName)
                    {
                        case OpCodeName.Tf:
                            fontName = op.Operands[0].ToString();
                            fontSize = double.Parse(op.Operands[1].ToString()!, CultureInfo.InvariantCulture);
                            break;
                        case OpCodeName.g:          // gray value (0.0 = black, 1.0 = white)
                            if (op.Operands.Count > 0)
                                ForeColor = XColor.FromGrayScale(double.Parse(op.Operands[0].ToString()!, CultureInfo.InvariantCulture));
                            break;
                        case OpCodeName.rg:         // rgb color (Chapter 8.6.8)
                            if (op.Operands.Count > 2)
                            {
                                var r = double.Parse(op.Operands[0].ToString()!, CultureInfo.InvariantCulture);
                                var g = double.Parse(op.Operands[1].ToString()!, CultureInfo.InvariantCulture);
                                var b = double.Parse(op.Operands[2].ToString()!, CultureInfo.InvariantCulture);
                                ForeColor = XColor.FromArgb((int)Math.Round(r * 255.0), (int)Math.Round(g * 255.0), (int)Math.Round(b * 255.0));
                            }
                            break;
                        case OpCodeName.k:
                            if (op.Operands.Count > 3)
                            {
                                var c = double.Parse(op.Operands[0].ToString()!, CultureInfo.InvariantCulture);
                                var m = double.Parse(op.Operands[1].ToString()!, CultureInfo.InvariantCulture);
                                var y = double.Parse(op.Operands[2].ToString()!, CultureInfo.InvariantCulture);
                                var k = double.Parse(op.Operands[3].ToString()!, CultureInfo.InvariantCulture);
                                ForeColor = XColor.FromCmyk((int)Math.Round(c * 255.0), (int)Math.Round(m * 255.0), (int)Math.Round(y * 255.0), (int)Math.Round(k * 255.0));
                            }
                            break;
                    }
                }
            }
            AutomaticFontSize = fontSize == 0;
            _fontSize = fontSize;
            if (!string.IsNullOrEmpty(fontName))
            {
                if (!TryGetFont(fontName, out font))
                {
                    // if not found or not supported (e.g. not a TrueTypeFont) create new font
                    try
                    {
                        // try to create font (may use a custom FontResolver)
                        font = new XFont(fontName, AutomaticFontSize ? 10 : fontSize, XFontStyleEx.Regular,
                            new XPdfFontOptions(PdfFontEncoding.Automatic, PdfFontEmbedding.EmbedCompleteFontFile));
                    }
                    catch
                    {
                        // unable to resolve font, use one of the Standard-Fonts
                        var defaultFontName = StandardFontNames.Helvetica;
                        // manually cache font-data so it can be found even without a FontResolver
                        var typefaceKey = XGlyphTypeface.ComputeGtfKey(defaultFontName, false, false);
                        if (!Globals.Global.Fonts.GlyphTypefacesByKey.ContainsKey(typefaceKey))
                        {
                            var fontData = StandardFontData.GetFontData(defaultFontName)!;
                            var fontSource = XFontSource.GetOrCreateFrom(fontData);
                            Globals.Global.Fonts.GlyphTypefacesByKey[typefaceKey] = new XGlyphTypeface(fontSource);
                        }
                        font = new XFont(defaultFontName,
                            AutomaticFontSize ? 10 : fontSize,
                            XFontStyleEx.Regular,   // should match the options for the typefaceKey
                            new XPdfFontOptions(PdfFontEncoding.Automatic, PdfFontEmbedding.EmbedCompleteFontFile));
                    }
                }
            }
        }

        /// <summary>
        /// Tries to create an XFont from the information stored in the AcroForm's Font-Resources
        /// </summary>
        /// <param name="resourceKey">The key in the AcroForms Font-Resources that identifies the font</param>
        /// <param name="xFont">Contains the font used by this field or null if the font could not be resolved</param>
        /// <returns></returns>
        internal bool TryGetFont(string resourceKey, [MaybeNullWhen(false)] out XFont? xFont)
        {
            xFont = null;
            if (Owner.AcroForm != null && Owner.AcroForm.Resources != null)
            {
                var fontDict = Owner.AcroForm.Resources.Fonts.Elements.GetDictionary(resourceKey);
                if (fontDict != null)
                {
                    var subType = fontDict.Elements.GetName(PdfFont.Keys.Subtype);
                    var fontName = fontDict.Elements.GetName(PdfFont.Keys.BaseFont);
                    var fontDescriptor = fontDict.Elements.GetDictionary(PdfFont.Keys.FontDescriptor);
                    var descendantFonts = fontDict.Elements.GetArray(PdfType0Font.Keys.DescendantFonts);
                    byte[]? fontData = null;
                    FontType fontType = FontType.TrueTypeWinAnsi;
                    // Standard-font that is not embedded
                    if (subType == "/Type1" && fontDescriptor == null && !string.IsNullOrEmpty(fontName))
                    {
                        fontName = fontName.TrimStart('/');
                        if (StandardFontData.IsStandardFont(fontName))
                        {
                            fontData = StandardFontData.GetFontData(fontName)!;
                            fontType = FontType.Type1StandardFont;
                        }
                    }
                    // embedded true-type font
                    else if (subType == "/TrueType" && fontDescriptor != null)
                    {
                        var fileRef = fontDescriptor.Elements.GetDictionary(PdfFontDescriptor.Keys.FontFile2);
                        if (fileRef != null)
                        {
                            fontData = fileRef?.Stream.UnfilteredValue;
                        }
                    }
                    else if (subType == "/Type0" && descendantFonts != null)
                    {
                        // entries like those generated by PDFsharp itself
                        var descFont = descendantFonts.Elements.GetDictionary(0);
                        if (descFont != null) 
                        {
                            fontDescriptor = descFont.Elements.GetDictionary(PdfFont.Keys.FontDescriptor);
                            if (fontDescriptor != null)
                            {
                                var fileRef = fontDescriptor.Elements.GetDictionary(PdfFontDescriptor.Keys.FontFile2);
                                if (fileRef != null)
                                {
                                    fontData = fileRef?.Stream.UnfilteredValue;
                                }
                            }
                        }
                    }
                    if (fontData != null)
                    {
                        var fontSource = XFontSource.GetOrCreateFrom(fontData);
                        var typeFace = new XGlyphTypeface(fontSource);
                        // cache the typeFace
                        if (!GlyphTypefaceCache.TryGetGlyphTypeface(typeFace.Key, out _))
                            GlyphTypefaceCache.AddGlyphTypeface(typeFace);
                        // cache the font
                        Owner.FontTable.CacheExistingFont(fontDict, typeFace, fontType);
                        var style = XFontStyleEx.Regular;
                        if (typeFace.IsBold)
                            style |= XFontStyleEx.Bold;
                        if (typeFace.IsItalic)
                            style |= XFontStyleEx.Italic;
                        if (string.IsNullOrEmpty(fontName) || fontType != FontType.Type1StandardFont)
                            fontName = typeFace.FamilyName;
                        xFont = new XFont(fontName, Math.Max(_fontSize ?? 0, 10.0), style,
                            new XPdfFontOptions(PdfFontEncoding.Automatic,
                            fontType == FontType.Type1StandardFont ? PdfFontEmbedding.OmitStandardFont : PdfFontEmbedding.EmbedCompleteFontFile));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Calculates the optimal font-size based on the height of the specified widget.<br></br>
        /// If a font-size greater than 0 is specified in the field's appearance or default appearance, this size is returned.<br></br>
        /// Otherwise the returned font-size is set to be 80% of the widget's height for single-line fields and a fixed value of 10 for multi-line fields.<br></br>
        /// Always returns a value greater than 0
        /// </summary>
        /// <param name="widget">The widget, the font-size should be based on</param>
        /// <returns></returns>
        internal double DetermineFontSize(PdfWidgetAnnotation? widget)
        {
            if (FontSize > 0.0)
                return FontSize.Value;

            var fontSize = 10.0;
            if (widget != null && !widget.Rectangle.IsZero)
            {
                var refValue = widget.Rotation == 0
                    || widget.Rotation == 180
                    || (widget.Flags & PdfAnnotationFlags.NoRotate) != 0
                        ? widget.Rectangle.Height : widget.Rectangle.Width;

                if (this is not PdfTextField field || !field.MultiLine)
                    // Rects were spotted with negative height
                    fontSize = Math.Abs(refValue * 0.80);  // set font size to 80% of the widget height
                return Math.Max(1.0, fontSize);
            }
            return fontSize;
        }

        /// <summary>
        /// This may switch the encoding for the current font to Unicode from the default WinAnsi.
        /// </summary>
        /// <param name="fontType"></param>
        internal void SetFontType(FontType fontType)
        {
            if (Font is null)
                return;

            // do not change the encoding for standard-fonts
            if (Font.PdfOptions.FontEmbedding == PdfFontEmbedding.OmitStandardFont)
                return;

            if (Font.PdfOptions.FontEncoding != PdfFontEncoding.Unicode && fontType == FontType.Type0Unicode)
                Font = new XFont(Font.GlyphTypeface, Font.Size,
                    new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.EmbedCompleteFontFile));
        }

        /// <summary>
        /// Adds the font of the current AcroField to the specified XForm object
        /// </summary>
        /// <param name="form"></param>
        internal void SetXFormFont(XForm form)
        {
            if (Font is null)
                return;

            var fontType = Font.PdfOptions.FontEmbedding == PdfFontEmbedding.OmitStandardFont
                ? FontType.Type1StandardFont
                : Font.PdfOptions.FontEncoding == PdfFontEncoding.Unicode
                    ? FontType.Type0Unicode
                    : FontType.TrueTypeWinAnsi;
            var docFont = _document.FontTable.GetOrCreateFont(Font.GlyphTypeface, fontType);
            form.PdfForm.Resources.AddFont(docFont);
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            // add the font to the AcroForm's resources
            if (Font != null && _document.AcroForm != null)
            {
                var formResources = _document.AcroForm.GetOrCreateResources();
                var fontType = Font.PdfOptions.FontEmbedding == PdfFontEmbedding.OmitStandardFont
                    ? FontType.Type1StandardFont
                    : Font.PdfOptions.FontEncoding == PdfFontEncoding.Unicode
                        ? FontType.Type0Unicode
                        : FontType.TrueTypeWinAnsi;
                var pdfFont = _document.FontTable.GetOrCreateFont(Font.GlyphTypeface, fontType);
                formResources.AddFont(pdfFont);
            }
            // TODO: as a small optimization, we may merge field and annotation if there is only a single annotation
            // -> leave this for a future version
            if (HasChildFields)
            {
                for (var i = 0; i < Fields.Elements.Count; i++)
                {
                    var field = Fields[i];
                    field.PrepareForSave();
                }
            }
            // accessing the Fields-property may have created a new empty array, remove that
            if (Fields.Elements.Count == 0)
                Elements.Remove(Keys.Kids);
            // handle annotations
            foreach (var annot in Annotations.Elements)
                annot.PrepareForSave();
        }

        internal virtual void Flatten()
        {
            // Copy Font-Resources to the Page
            // This is neccessary, because Fonts used by AcroFields may be referenced only by the AcroForm, which is deleted after flattening
            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if ((widget.Flags & PdfAnnotationFlags.Hidden) != 0 || (widget.Flags & PdfAnnotationFlags.NoView) != 0)
                {
                    RemoveAnnotation(widget);
                    i--;
                    continue;
                }
                if (widget.Page != null)
                {
                    var acroResources = _document.AcroForm?.Elements.GetDictionary(PdfAcroForm.Keys.DR);
                    var pageResources = widget.Page.Elements.GetDictionary(PdfPage.Keys.Resources);
                    if (acroResources != null && pageResources != null)
                    {
                        var acroFontList = acroResources.Elements.GetDictionary(PdfResources.Keys.Font);
                        var pageFontList = pageResources.Elements.GetDictionary(PdfResources.Keys.Font);
                        if (acroFontList != null)
                        {
                            pageFontList ??= new PdfDictionary(Owner);
                            pageResources.Elements.SetObject(PdfResources.Keys.Font, pageFontList);
                            foreach (var fontKey in acroFontList.Elements.Keys)
                            {
                                if (!pageFontList.Elements.ContainsKey(fontKey))
                                    pageFontList.Elements.Add(fontKey, acroFontList.Elements[fontKey]);
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                // Remove annotation
                widget.Parent?.Remove(widget);
                widget.Page?.Annotations.Remove(widget);
            }

            if (HasChildFields)
            {
                for (var i = 0; i < Fields.Elements.Count; i++)
                {
                    var field = Fields[i];
                    field.Flatten();
                }
            }

            if (Reference != null)
                _document.IrefTable.Remove(Reference);

            RenderAppearance();
            RenderAppearanceToPage();
        }

        /// <summary>
        /// Must be overridden by subclasses
        /// </summary>
        protected virtual void RenderAppearance()
        {
        }

        /// <summary>
        /// Renders the widget-appearances of this field directly onto the page.<br></br>
        /// Used by the <see cref="Flatten"/> method.
        /// </summary>
        protected void RenderAppearanceToPage()
        {
            // /N -> Normal appearance, /R -> Rollover appearance, /D -> Down appearance
            const string normalName = "/N";

            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget.Page != null)
                {
                    var appearances = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    if (appearances != null)
                    {
                        var normalAppearance = appearances.Elements.GetDictionary(normalName);
                        var appeareanceState = widget.Elements.GetName(PdfAnnotation.Keys.AS);
                        // if state is unset, treat normal appearance as the appearance itself
                        if (normalAppearance != null && string.IsNullOrEmpty(appeareanceState))
                            RenderContentStream(widget.Page, normalAppearance, widget.Rectangle);
                        else if (normalAppearance != null)
                        {
                            // the state is used by radio-buttons and checkboxes, which have a checked and an unchecked state
                            var selectedAppearance = normalAppearance.Elements.GetDictionary(appeareanceState);
                            if (selectedAppearance != null)
                                RenderContentStream(widget.Page, selectedAppearance, widget.Rectangle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders the contents of the supplied Stream to the Page at the position specified by the provided Rectangle
        /// </summary>
        /// <param name="page">Page to render the content onto</param>
        /// <param name="streamDict">A <see cref="PdfDictionary"/> containing a stream with drawing-operators and associated resources</param>
        /// <param name="rect"></param>
        protected virtual void RenderContentStream(PdfPage page, PdfDictionary streamDict, PdfRectangle rect)
        {
            if (streamDict == null || streamDict.Stream == null || rect.IsZero)
                return;
            var stream = streamDict.Stream;
            var content = ContentReader.ReadContent(stream.UnfilteredValue);
            // check for graphical objects and copy them to the pages resources
            foreach (var obj in content)
            {
                if (obj is COperator op)
                {
                    if (op.OpCode.OpCodeName == OpCodeName.Do)
                    {
                        var arg = op.Operands[0].ToString()!;
                        var resources = streamDict.Elements.GetDictionary("/Resources");
                        if (resources != null)
                        {
                            var xobjDict = resources.Elements.GetDictionary("/XObject");
                            if (xobjDict != null && xobjDict.Elements.ContainsKey(arg))
                            {
                                var objDict = xobjDict.Elements.GetDictionary(arg)!;
                                if (!page.Resources.Elements.ContainsKey("/XObject"))
                                    page.Resources.Elements.Add("/XObject", new PdfDictionary());
                                xobjDict = page.Resources.Elements.GetDictionary("/XObject")!;
                                // create new unique name for the xobject
                                var objKey = arg + Guid.NewGuid().ToString("N");
                                objDict.Elements.SetName("/Name", objKey);
                                xobjDict.Elements[objKey] = objDict;
                                op.Operands[0] = new CName(objKey);
                            }
                        }
                    }
                }
            }
            // TODO: use W or W* operator for clipping
            var matrix = new XMatrix();
            matrix.TranslateAppend(rect.X1, rect.Y1);
            var matElements = matrix.GetElements();
            var matrixOp = OpCodes.OperatorFromName("cm");
            foreach (var el in matElements)
                matrixOp.Operands.Add(new CReal { Value = el });
            content.Insert(0, matrixOp);

            // Save and restore Graphics state
            var appendedContent = page.Contents.AppendContent();
            appendedContent.CreateStream(content.ToContent());
            appendedContent.PreserveGraphicsState();    // wrap in q/Q
        }

        /// <summary>
        /// Holds the collection of WidgetAnnotations for a field
        /// </summary>
        public sealed class PdfAnnotationArray
        {
            private readonly List<PdfWidgetAnnotation> elements = [];

            /// <summary>
            /// Gets the list of <see cref="PdfWidgetAnnotation"/> of the array
            /// </summary>
            public List<PdfWidgetAnnotation> Elements
            {
                get { return elements; }
            }
        }

        /// <summary>
        /// Holds a collection of interactive fields.
        /// </summary>
        public sealed class PdfAcroFieldCollection : PdfArray
        {
            internal PdfAcroFieldCollection(PdfDocument document)
                : base(document)
            { }

            internal PdfAcroFieldCollection(PdfArray array)
                : base(array)
            { }

            /// <summary>  
            /// Gets the number of elements in the array.  
            /// </summary>  
            public int Count => Elements.Count;

            /// <summary>
            /// Gets the names of all fields in the collection.
            /// </summary>
            public string[] Names
            {
                get
                {
                    int count = Elements.Count;
                    var names = new List<string>(count);
                    for (int idx = 0; idx < count; idx++)
                    {
                        var dict = Elements.GetDictionary(idx);
                        // the element may be a WidgetAnnotation lacking the /T key, skip these
                        if (dict != null && dict.Elements.ContainsKey(Keys.T))
                        {
                            var name = dict.Elements.GetString(Keys.T);
                            if (!string.IsNullOrEmpty(name))
                                names.Add(name);
                        }
                    }
                    return names.ToArray();
                }
            }

            /// <summary>
            /// Gets a field from the collection. For your convenience an instance of a derived class like
            /// PdfTextField or PdfCheckBox is returned if PDFsharp can guess the actual type of the dictionary.
            /// If the actual type cannot be guessed by PDFsharp the function returns an instance
            /// of PdfGenericField.
            /// </summary>
            public PdfAcroField this[int index]
            {
                get
                {
                    PdfItem item = Elements[index];
                    Debug.Assert(item is PdfReference);
                    PdfDictionary? dict = ((PdfReference)item).Value as PdfDictionary;
                    Debug.Assert(dict != null);
                    PdfAcroField? field = dict as PdfAcroField;
                    if (field == null && dict != null!)
                    {
                        // Do type transformation
                        field = CreateAcroField(dict);
                        //Elements[index] = field.XRef;
                    }
                    return field!; // NRT
                }
            }

            /// <summary>
            /// Gets the field with the specified name.
            /// </summary>
            public PdfAcroField? this[string name] => GetValue(name);

            internal PdfAcroField? GetValue(string name)
            {
                if (String.IsNullOrEmpty(name))
                    return null;

                int dot = name.IndexOf('.');
                string prefix = dot == -1 ? name : name.Substring(0, dot);
                string suffix = dot == -1 ? "" : name.Substring(dot + 1);

                int count = Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var field = this[idx];
                    if (field.Name == prefix)
                        return field.GetValue(suffix);
                }
                return null;
            }

            /// <summary>
            /// Create a derived type like PdfTextField or PdfCheckBox if possible.
            /// If the actual cannot be guessed by PDFsharp the function returns an instance
            /// of PdfGenericField.
            /// </summary>
            internal static PdfAcroField CreateAcroField(PdfDictionary dict)
            {
                string ft = dict.Elements.GetName(Keys.FT);
                PdfAcroFieldFlags flags = (PdfAcroFieldFlags)dict.Elements.GetInteger(Keys.Ff);
                switch (ft)
                {
                    case "/Btn":
                        if ((flags & PdfAcroFieldFlags.Pushbutton) != 0)
                            return new PdfPushButtonField(dict);

                        if ((flags & PdfAcroFieldFlags.Radio) != 0)
                            return new PdfRadioButtonField(dict);

                        return new PdfCheckBoxField(dict);

                    case "/Tx":
                        return new PdfTextField(dict);

                    case "/Ch":
                        if ((flags & PdfAcroFieldFlags.Combo) != 0)
                            return new PdfComboBoxField(dict);
                        else
                            return new PdfListBoxField(dict);

                    case "/Sig":
                        return new PdfSignatureField(dict);

                    default:
                        // this is either a non-terminal field or a WidgetAnnotation
                        return new PdfGenericField(dict);
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required for terminal fields; inheritable) The type of field that this dictionary
            /// describes:<br></br>
            ///   Btn           Button<br></br>
            ///   Tx            Text<br></br>
            ///   Ch            Choice<br></br>
            ///   Sig (PDF 1.3) Signature<br></br>
            /// Note: This entry may be present in a nonterminal field (one whose descendants
            /// are themselves fields) in order to provide an inheritable FT value. However, a
            /// nonterminal field does not logically have a type of its own; it is merely a container
            /// for inheritable attributes that are intended for descendant terminal fields of
            /// any type.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string FT = "/FT";

            /// <summary>
            /// (Required if this field is the child of another in the field hierarchy; absent otherwise)
            /// The field that is the immediate parent of this one (the field, if any, whose Kids array
            /// includes this field). A field can have at most one parent; that is, it can be included
            /// in the Kids array of at most one other field.
            /// </summary>
            [KeyInfo(KeyType.Dictionary)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Sometimes required, as described below) An array of indirect references to the immediate
            /// children of this field. In a non-terminal field, the Kids array shall refer to field dictionaries
            /// that are immediate descendants of this field. In a terminal field, the Kids array ordinarily
            /// shall refer to one or more separate widget annotations that are associated with this field.
            /// However, if there is only one associated widget annotation, and its contents have been merged
            /// into the field dictionary, Kids shall be omitted.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional, typeof(PdfAcroFieldCollection))]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Optional) The partial field name.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string T = "/T";

            /// <summary>
            /// (Optional; PDF 1.3) An alternate field name, to be used in place of the actual
            /// field name wherever the field must be identified in the user interface (such as
            /// in error or status messages referring to the field). This text is also useful
            /// when extracting the document’s contents in support of accessibility to disabled
            /// users or for other purposes.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
            public const string TU = "/TU";

            /// <summary>
            /// (Optional; PDF 1.3) The mapping name to be used when exporting interactive form field 
            /// data from the document.
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
            /// (Optional; inheritable) The field’s value, whose format varies depending on
            /// the field type; see the descriptions of individual field types for further information.
            /// </summary>
            [KeyInfo(KeyType.Various | KeyType.Optional)]
            public const string V = "/V";

            /// <summary>
            /// (Optional; inheritable) The default value to which the field reverts when a
            /// reset-form action is executed. The format of this value is the same as that of V.
            /// </summary>
            [KeyInfo(KeyType.Various | KeyType.Optional)]
            public const string DV = "/DV";

            /// <summary>
            /// (Optional; PDF 1.2) An additional-actions dictionary defining the field’s behavior
            /// in response to various trigger events. This entry has exactly the same meaning as
            /// the AA entry in an annotation dictionary.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string AA = "/AA";

            // ----- Additional entries to all fields containing variable text --------------------------

            /// <summary>
            /// (Required; inheritable) The default appearance string, containing a sequence of
            /// valid page-content graphics or text state operators defining such properties as
            /// the field’s text size and color.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Required)]
            public const string DA = "/DA";

            /// <summary>
            /// (Optional; inheritable) A code specifying the form of quadding (justification)
            /// to be used in displaying the text:
            ///   0 Left-justified
            ///   1 Centered
            ///   2 Right-justified
            /// Default value: 0 (left-justified).
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            // Keys specific to fields containing variable text

            /// <summary>
            /// (Optional; PDF 1.5) A default style string, as described in Adobe XML 
            /// Architecture, XML Forms Architecture(XFA) Specification, version 3.3.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string DS = "/DS";

            /// <summary>
            /// (Optional; PDF 1.5) A rich text string, as described in Adobe XML 
            /// Architecture, XML Forms Architecture(XFA) Specification, version 3.3. 
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string RV = "/RV";

            // ReSharper restore InconsistentNaming
        }
    }
}
