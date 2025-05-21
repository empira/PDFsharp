// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the list box field.
    /// </summary>
    public sealed class PdfListBoxField : PdfChoiceField
    {
        /// <summary>
        /// Initializes a new instance of PdfListBoxField.
        /// </summary>
        internal PdfListBoxField(PdfDocument document)
            : base(document)
        { }

        internal PdfListBoxField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the background color for selected items of the field.
        /// </summary>
        public XColor HighlightColor
        {
            get { return this.highlightColor; }
            set { this.highlightColor = value; }
        }
        XColor highlightColor = XColors.DarkBlue;

        /// <summary>
        /// Gets or sets the text-color for selected items of the field.
        /// </summary>
        public XColor HighlightTextColor
        {
            get { return this.highlightTextColor; }
            set { this.highlightTextColor = value; }
        }
        XColor highlightTextColor = XColors.White;

        /// <summary>
        /// Gets or sets the value for this ListBox<br></br>
        /// Note: As a <see cref="PdfListBoxField"/> may have multiple values selected,
        /// this is an enumerable instead of a single value
        /// </summary>
        public new IEnumerable<string> Value
        {
            get
            {
                var ancestor = FindParentHavingKey(PdfAcroField.Keys.V);
                if (ancestor.Elements.ContainsKey(PdfAcroField.Keys.V))
                {
                    var val = ancestor.Elements[PdfAcroField.Keys.V];
                    if (val is PdfString valString)
                        return [valString.Value];
                    if (val is PdfArray valArray)
                    {
                        return valArray.Elements.Select(v => (v?.ToString() ?? "").TrimStart('(').TrimEnd(')'));
                    }
                }
                return [];
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException("The field is read only.");
                if (value == null || !value.Any())
                {
                    Elements.Remove(PdfAcroField.Keys.V);
                    Elements.Remove(PdfChoiceField.Keys.I);
                    if (value == null)
                        return;
                }
                var indices = new List<int>();
                foreach (var v in value)
                {
                    var index = IndexInOptArray(v, true);
                    if (index < 0)
                        throw new ArgumentException($"'{v}' is not a valid value for field '{FullyQualifiedName}'. Valid values are: [{string.Join(",", Options)}]");

                    indices.Add(index);
                }
                SelectedIndices = indices;
            }
        }

        /// <summary>
        /// Gets or sets the Indices of the selected items of this Field
        /// </summary>
        public IEnumerable<int> SelectedIndices
        {
            get
            {
                var result = new List<int>();
                var ancestor = FindParentHavingKey(PdfAcroField.Keys.V);
                var value = ancestor.Elements[PdfAcroField.Keys.V];
                if (value is PdfString valString)
                {
                    result.Add(IndexInOptArray(valString.Value, true));
                }
                else
                {
                    var ary = ancestor.Elements.GetArray(PdfAcroField.Keys.V);       // /V takes precedence over /I
                    if (ary != null)
                    {
                        for (var i = 0; i < ary.Elements.Count; i++)
                        {
                            int idx;
                            var val = ary.Elements.GetString(i);
                            if (val != null && (idx = IndexInOptArray(val, true)) >= 0)
                                result.Add(idx);
                        }
                    }

                    if (result.Count > 0)
                        return result;

                    ancestor = FindParentHavingKey(PdfChoiceField.Keys.I);
                    ary = ancestor.Elements.GetArray(PdfChoiceField.Keys.I);
                    if (ary != null)
                    {
                        foreach (var item in ary.Elements)
                        {
                            if (item is PdfInteger pdfInt)
                                result.Add(pdfInt.Value);
                        }
                    }
                }
                return result;
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException("The field is read only.");
                if (value == null)
                {
                    Elements.Remove(PdfChoiceField.Keys.I);
                    Elements.Remove(PdfAcroField.Keys.V);
                    return;
                }
                if (value.Any(v => v < 0 || v >= Options.Count))
                    throw new ArgumentOutOfRangeException($"At least one of the indices [{string.Join(",", value)}] is out of range. Valid values are in the range 0..{Options.Count - 1}");

                Elements.Remove(PdfChoiceField.Keys.I);
                Elements.Remove(PdfAcroField.Keys.V);

                var indexList = new List<int>(value.Distinct());
                if (indexList.Count > 0)
                {
                    indexList.Sort();
                    var indices = new PdfArray(_document);
                    var values = new PdfArray(_document);
                    foreach (var index in indexList)
                    {
                        indices.Elements.Add(new PdfInteger(index));
                        values.Elements.Add(new PdfString(ValueInOptArray(index, true)));
                    }
                    if (indexList.Count > 1)
                    {
                        Elements.SetObject(PdfChoiceField.Keys.I, indices);
                        Elements.SetObject(PdfAcroField.Keys.V, values);
                    }
                    else
                        Elements.SetString(PdfAcroField.Keys.V, ValueInOptArray(indexList[0], true));
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of the first visible item in the ListBox
        /// </summary>
        public int TopIndex
        {
            get
            {
                var ancestor = FindParentHavingKey(PdfChoiceField.Keys.TI);
                return ancestor.Elements.GetInteger(PdfChoiceField.Keys.TI);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("TopIndex must not be less than zero");
                Elements.SetInteger(PdfChoiceField.Keys.TI, value);
            }
        }

        /// <summary>
        /// Renders the appearance of this field
        /// </summary>
        protected override void RenderAppearance()
        {
            if (Font is null)
                return;

            for (var idx = 0; idx < Annotations.Elements.Count; idx++)
            {
                var widget = Annotations.Elements[idx];
                if (widget == null)
                    continue;

                var rect = widget.Rectangle;
                var width = Math.Abs(rect.Width);
                var height = Math.Abs(rect.Height);
                // ensure a minimum size of 1x1, otherwise an exception is thrown
                if (width < 1.0 || height < 1.0)
                    continue;

                var preferredFontType = Options.All(s => AnsiEncoding.IsAnsi(s)) ? FontType.TrueTypeWinAnsi : FontType.Type0Unicode;
                SetFontType(preferredFontType);

                var xRect = new XRect(0, 0, width, height);
                var form = new XForm(_document, xRect);
                using (var gfx = XGraphics.FromForm(form))
                {
                    gfx.IntersectClip(xRect);
                    Owner.AcroForm?.FieldRenderer.ListBoxFieldRenderer.Render(this, widget, gfx, xRect);
                }
                form.DrawingFinished();
                SetXFormFont(form);

                var ap = new PdfDictionary(Owner);
                widget.Elements[PdfAnnotation.Keys.AP] = ap;
                ap.Elements["/N"] = form.PdfForm.Reference;

                var xobj = form.PdfForm;
                var s = xobj.Stream.ToString();
                s = "/Tx BMC\n" + s + "\nEMC";
                xobj.Stream.Value = new RawEncoding().GetBytes(s);
            }
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            RenderAppearance();
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // List boxes have no additional entries.

            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
