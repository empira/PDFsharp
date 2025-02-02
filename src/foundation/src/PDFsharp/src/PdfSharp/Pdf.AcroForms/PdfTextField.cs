// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the text field.
    /// </summary>
    public sealed class PdfTextField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of PdfTextField.
        /// </summary>
        internal PdfTextField(PdfDocument document)
            : base(document)
        {
            Elements.SetName(PdfAcroField.Keys.FT, "Tx");
        }

        internal PdfTextField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Same as <see cref="Text"/> (which should be used instead)
        /// </summary>
        public new string Value
        {
            get => Text;
            set => Text = value;
        }

        /// <summary>
        /// Gets or sets the text value of the text field.
        /// </summary>
        public string Text
        {
            get
            {
                var ancestor = FindParentHavingKey(PdfAcroField.Keys.V);
                return ancestor.Elements.GetString(PdfAcroField.Keys.V);
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException("The field is read only.");
                // TODO: check MaxLength ? (risky -> potential data-loss)
                Elements.SetString(PdfAcroField.Keys.V, value ?? string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the maximum length of the field.
        /// </summary>
        /// <value>The length of the max.</value>
        public int MaxLength
        {
            get
            {
                var ancestor = FindParentHavingKey(Keys.MaxLen);
                return ancestor.Elements.GetInteger(Keys.MaxLen);
            }
            set { Elements.SetInteger(Keys.MaxLen, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has multiple lines.
        /// </summary>
        public bool MultiLine
        {
            get { return (Flags & PdfAcroFieldFlags.Multiline) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.Multiline;
                else
                    SetFlags &= ~PdfAcroFieldFlags.Multiline;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is used for passwords.
        /// </summary>
        public bool Password
        {
            get { return (Flags & PdfAcroFieldFlags.Password) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.Password;
                else
                    SetFlags &= ~PdfAcroFieldFlags.Password;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is a combined field.
        /// A combined field is a text field made up of multiple "combs" of equal width. The number of combs is determined by <see cref="MaxLength"/>.
        /// </summary>
        public bool Combined
        {
            get { return (Flags & PdfAcroFieldFlags.CombTextField) != 0; }
            set
            {
                if (value)
                    SetFlags |= PdfAcroFieldFlags.CombTextField;
                else
                    SetFlags &= ~PdfAcroFieldFlags.CombTextField;
            }
        }

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
            var fontType = font.PdfOptions.FontEncoding == PdfFontEncoding.Unicode
                ? FontType.Type0Unicode
                : FontType.TrueTypeWinAnsi;
            var docFont = _document.FontTable.GetOrCreateFont(font.GlyphTypeface, fontType);
            var fontName = formResources.AddFont(docFont);
            var da = string.Format(CultureInfo.InvariantCulture, "{0} {1:F2} Tf {2:F4} {3:F4} {4:F4} rg",
                fontName, fontSize, textColor.R / 255, textColor.G / 255, textColor.B / 255);
            Elements.SetString(PdfAcroField.Keys.DA, da);
        }

        /// <summary>
        /// Creates the normal appearance form X object for the annotation that represents
        /// this acro form text field.
        /// </summary>
        protected override void RenderAppearance()
        {
            if (Font == null || string.IsNullOrEmpty(Text))
                return;

            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget == null)
                    continue;

                var rect = widget.Rectangle;
                var width = Math.Abs(rect.Width);
                var height = Math.Abs(rect.Height);
                // ensure a minimum size of 1x1, otherwise an exception is thrown
                if (width < 1.0 || height < 1.0)
                    continue;

                var xRect = new XRect(0, 0, width, height);
                var form = (widget.Rotation == 90 || widget.Rotation == 270) && (widget.Flags & PdfAnnotationFlags.NoRotate) == 0
                    ? new XForm(_document, XUnit.FromPoint(rect.Height), XUnit.FromPoint(rect.Width))
                    : new XForm(_document, xRect);

                if (widget.Rotation != 0 && (widget.Flags & PdfAnnotationFlags.NoRotate) == 0)
                {
                    // I could not get this to work using gfx.Rotate/Translate Methods...
                    const double deg2Rad = 0.01745329251994329576923690768489;  // PI/180
                    var sr = Math.Sin(widget.Rotation * deg2Rad);
                    var cr = Math.Cos(widget.Rotation * deg2Rad);
                    // see PdfReference 1.7, Chapter 8.3.3 (Common Transformations)
                    // TODO: Is this always correct ? I had only the chance to test this with a 90 degree rotation...
                    form.PdfForm.Elements.SetMatrix(PdfFormXObject.Keys.Matrix, new XMatrix(cr, sr, -sr, cr, xRect.Width, 0));
                    if (widget.Rotation == 90 || widget.Rotation == 270)
                        xRect = new XRect(0, 0, rect.Height, rect.Width);
                }

                var preferredFontType = AnsiEncoding.IsAnsi(Text) ? FontType.TrueTypeWinAnsi : FontType.Type0Unicode;
                SetFontType(preferredFontType);

                using (var gfx = XGraphics.FromForm(form))
                {
                    gfx.IntersectClip(xRect);
                    Owner.AcroForm?.FieldRenderer.TextFieldRenderer.Render(this, widget, gfx, xRect);
                }
                form.DrawingFinished();
                SetXFormFont(form);

                // Get existing or create new appearance dictionary.
                if (widget.Elements[PdfAnnotation.Keys.AP] is not PdfDictionary ap)
                {
                    ap = new PdfDictionary(_document);
                    widget.Elements[PdfAnnotation.Keys.AP] = ap;
                }

                ap.Elements["/N"] = form.PdfForm.Reference;

                var xobj = form.PdfForm;
                var s = xobj.Stream.ToString();
                // Thank you Adobe: Without putting the content in 'EMC brackets'
                // the text is not rendered by PDF Reader 9 or higher.
                s = "/Tx BMC\n" + s + "\nEMC";
                xobj.Stream.Value = new RawEncoding().GetBytes(s);
            }
            // create DefaultAppearance for newly created fields (required according to the spec)
            if (!Elements.ContainsKey(PdfAcroField.Keys.DA) && _document.AcroForm != null)
            {
                var pdfFont = _document.FontTable.GetOrCreateFont(Font.GlyphTypeface, FontType.Type0Unicode);
                var formResources = _document.AcroForm.GetOrCreateResources();
                var fontName = formResources.AddFont(pdfFont);
                Elements.Add(PdfAcroField.Keys.DA, new PdfString(string.Format(
                    CultureInfo.InvariantCulture, "{0} {1:0.###} Tf {2:0.###} {3:0.###} {4:0.###} rg",
                    fontName, FontSize ?? Font.Size, ForeColor.R / 255.0, ForeColor.G / 255.0, ForeColor.B / 255.0)));
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
            /// <summary>
            /// (Optional; inheritable) The maximum length of the field’s text, in characters.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string MaxLen = "/MaxLen";

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
