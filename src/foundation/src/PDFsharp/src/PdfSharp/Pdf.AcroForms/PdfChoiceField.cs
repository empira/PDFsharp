// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the base class for all choice field dictionaries.
    /// </summary>
    public abstract class PdfChoiceField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfChoiceField"/> class.
        /// </summary>
        protected PdfChoiceField(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.FT, "Ch");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfChoiceField"/> class.
        /// </summary>
        protected PdfChoiceField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Sets the default appearance for this field.
        /// </summary>
        public void SetDefaultAppearance(XFont font, double fontSize, XColor textColor)
        {
            if (font is null)
                throw new ArgumentNullException(nameof(font));
            if (fontSize < 0.0)
                throw new ArgumentException("Font size must be greater or equal to zero", nameof(fontSize));
            if (Owner.AcroForm is null)
                throw new InvalidOperationException("AcroForm has to be created first");

            var formResources = Owner.AcroForm.GetOrCreateResources();
            var fontType = font.PdfOptions.FontEmbedding == PdfFontEmbedding.OmitStandardFont
                ? FontType.Type1StandardFont
                : font.PdfOptions.FontEncoding == PdfFontEncoding.Unicode
                    ? FontType.Type0Unicode
                    : FontType.TrueTypeWinAnsi;
            var docFont = _document.FontTable.GetOrCreateFont(font.GlyphTypeface, fontType);
            var fontName = formResources.AddFont(docFont);
            var da = string.Format(CultureInfo.InvariantCulture, "{0} {1:F2} Tf {2:F4} {3:F4} {4:F4} rg",
                fontName, fontSize, textColor.R / 255, textColor.G / 255, textColor.B / 255);
            Elements.SetString(PdfAcroField.Keys.DA, da);
        }

        /// <summary>
        /// Gets the index of the specified string in the /Opt array or -1, if no such string exists.
        /// </summary>
        /// <param name="value">Value, for which the index should be retrieved</param>
        /// <param name="useExportValue">true if value is the export value, false if value is the text shown in the UI</param>
        protected int IndexInOptArray(string value, bool useExportValue)
        {
            var ancestor = FindParentHavingKey(Keys.Opt);
            var opt = ancestor.Elements.GetArray(Keys.Opt);
            if (opt != null)
            {
                int count = opt.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfItem item = opt.Elements[idx];
                    if (item is PdfString pdfString)
                    {
                        if (pdfString.Value == value)
                            return idx;
                    }
                    else if (item is PdfArray array)
                    {
                        if (array.Elements.Count > 0)
                        {
                            // Pdf Reference 1.7, Section 12.7.4.4: Should be a 2-element Array.
                            // Second value is the text shown in the UI.
                            if ((!useExportValue && array.Elements.Count > 1 && array.Elements.GetString(1) == value) ||
                                (array.Elements.Count > 0 && array.Elements.GetString(0) == value))
                                return idx;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the value or the display text from the index in the /Opt array.
        /// </summary>
        /// <param name="index">Index of the value that should be retrieved</param>
        /// <param name="useExportValue">true to get the export value, false to get the text shown in the UI</param>
        internal string ValueInOptArray(int index, bool useExportValue)
        {
            var ancestor = FindParentHavingKey(Keys.Opt);
            var opt = ancestor.Elements.GetArray(Keys.Opt);
            if (opt != null)
            {
                int count = opt.Elements.Count;
                if (index < 0 || index >= count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                PdfItem item = opt.Elements[index];
                if (item is PdfString pdfString)
                    return pdfString.Value;
                else if (item is PdfArray array)
                {
                    return !useExportValue && array.Elements.Count > 1
                                  ? array.Elements.GetString(1)
                                  : array.Elements.GetString(0);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets or sets the Value for the Field.
        /// For fields supporting multiple values (e.g. ListBox) use <see cref="PdfListBoxField.SelectedIndices"/> instead
        /// </summary>
        public override PdfItem? Value
        {
            get
            {
                var item = base.Value;
                if (item is PdfArray pdfArray)
                {
                    if (pdfArray.Elements.Count > 0)
                        item = pdfArray.Elements[0];
                }
                if (item is PdfString pdfString)
                {
                    // First try the export value
                    var idx = IndexInOptArray(pdfString.Value, true);
                    // If that is not found, try the string shown in the UI
                    if (idx < 0)
                        idx = IndexInOptArray(pdfString.Value, false);
                    if (idx < 0)
                        return null;
                    // return the display text
                    return new PdfString(ValueInOptArray(idx, true));
                }
                return null;
            }
            set { base.Value = value; }
        }

        /// <summary>
        /// Gets or sets the Default value for the field
        /// </summary>
        public override PdfItem? DefaultValue
        {
            get
            {
                var item = base.DefaultValue;
                if (item is PdfArray pdfArray)
                {
                    if (pdfArray.Elements.Count > 0)
                        item = pdfArray.Elements[0];
                }
                if (item is PdfString pdfString)
                {
                    // First try the export value
                    var idx = IndexInOptArray(pdfString.Value, true);
                    // If that is not found, try the string shown in the UI
                    if (idx < 0)
                        idx = IndexInOptArray(pdfString.Value, false);
                    if (idx < 0)
                        return null;
                    // return the display text
                    return new PdfString(ValueInOptArray(idx, true));
                }
                return null;
            }
            set { base.DefaultValue = value; }
        }

        /// <summary>
        /// Gets or sets the List of options (entries) available for selection.
        /// This is the list of values shown in the UI.
        /// </summary>
        public ICollection<string> Options
        {
            get
            {
                var result = new List<string>();
                var ancestor = FindParentHavingKey(Keys.Opt);
                var options = ancestor.Elements.GetArray(Keys.Opt);
                if (options != null)
                {
                    foreach (var item in options)
                    {
                        if (item is PdfString s)
                            result.Add(s.Value);
                        else
                        {
                            if (item is PdfArray array)
                            {
                                // Pdf Reference 1.7, Section 12.7.4.4 : Value is the SECOND entry in the Array
                                // (the first value is the exported value)
                                var v = array.Elements.GetString(array.Elements.Count > 1 ? 1 : 0);
                                if (string.IsNullOrEmpty(v))
                                    v = string.Empty;
                                result.Add(v);
                            }
                        }
                    }
                }
                return result;
            }
            set
            {
                var ary = new PdfArray(_document);
                foreach (var item in value)
                    ary.Elements.Add(new PdfString(item));
                Elements.SetObject(Keys.Opt, ary);
            }
        }

        /// <summary>
        /// Gets the list of values for this Field.<br></br>
        /// May or may not be equal to <see cref="Options"/> but has always the same amount of items.
        /// </summary>
        public ICollection<string> Values
        {
            get
            {
                var result = new List<string>();
                var ancestor = FindParentHavingKey(Keys.Opt);
                var options = ancestor.Elements.GetArray(Keys.Opt);
                if (options != null)
                {
                    foreach (var item in options)
                    {
                        if (item is PdfString s)
                            result.Add(s.Value);
                        else
                        {
                            if (item is PdfArray array)
                            {
                                var ary = array;
                                var v = ary.Elements.GetString(0);
                                if (string.IsNullOrEmpty(v))
                                    v = string.Empty;
                                result.Add(v);
                            }
                        }
                    }
                }
                return result;
            }

        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Optional) An array of options that shall be presented to the user.
            /// Each element of the array is either a text string representing one of the available
            /// options or an array consisting of two text strings: the option’s export value
            /// and the text that shall be displayed as the name of the option.<br></br>
            /// If this entry is not present, no choices should be presented to the user. 
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

            /// <summary>
            /// (Optional) For scrollable list boxes, the top index (the index in the Opt array
            /// of the first option visible in the list). Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string TI = "/TI";

            /// <summary>
            ///  (Sometimes required, otherwise optional; PDF 1.4) For choice fields that allow 
            ///  multiple selection (MultiSelect flag set), an array of integers, sorted in ascending order,
            ///  representing the zero-based indices in the Opt array of the currently selected option items.<br></br>
            ///  This entry shall be used when two or more elements in the Opt array have different names
            ///  but the same export value or when the value of the choice field is an array.<br></br>
            ///  This entry should not be used for choice fields that do not allow multiple selection.<br></br>
            ///  If the items identified by this entry differ from those in the V entry of the
            ///  field dictionary (see discussion following this Table), the V entry shall be used.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string I = "/I";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
