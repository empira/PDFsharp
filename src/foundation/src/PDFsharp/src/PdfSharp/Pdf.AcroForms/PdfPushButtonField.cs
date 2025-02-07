// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the push button field.
    /// </summary>
    public sealed class PdfPushButtonField : PdfButtonField
    {
        /// <summary>
        /// Initializes a new instance of PdfPushButtonField.
        /// </summary>
        internal PdfPushButtonField(PdfDocument document)
            : base(document)
        {
            _document = document;
            SetFlags |= PdfAcroFieldFlags.Pushbutton;
        }

        internal PdfPushButtonField(PdfDictionary dict)
            : base(dict)
        { }

        private string? caption;
        /// <summary>
        /// Gets or sets the Caption of this Button
        /// </summary>
        public string? Caption
        {
            get
            {
                if (caption == null)
                {
                    foreach (var widget in Annotations.Elements)
                    {
                        if (!string.IsNullOrWhiteSpace(widget.NormalCaption))
                        {
                            caption = widget.NormalCaption;
                            break;
                        }
                    }
                }
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    foreach (var widget in Annotations.Elements)
                    {
                        widget.NormalCaption = value;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new Annotation to this field.
        /// </summary>
        /// <param name="configure">A method that is used to configure the Annotation</param>
        /// <returns>The created and configured Annotation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override PdfWidgetAnnotation AddAnnotation(Action<PdfWidgetAnnotation> configure)
        {
            var annot = base.AddAnnotation(configure);
            if (Caption != null)
                annot.NormalCaption = Caption;
            return annot;
        }

        /// <summary>
        /// Determines the visual appearance of this field, i.e. font and text-color
        /// </summary>
        protected override void DetermineAppearance()
        {
            var dict = this;
            base.DetermineAppearance();
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
                // if widget already has an appearance, use that (imported field)
                if (widget == null || widget.Elements.ContainsKey(PdfAnnotation.Keys.AP))
                    continue;

                var rect = widget.Rectangle;
                var width = Math.Abs(rect.Width);
                var height = Math.Abs(rect.Height);
                // ensure a minimum size of 1x1, otherwise an exception is thrown
                if (width < 1.0 || height < 1.0)
                    continue;

                var preferredFontType = AnsiEncoding.IsAnsi(Caption ?? string.Empty) ? FontType.TrueTypeWinAnsi : FontType.Type0Unicode;
                SetFontType(preferredFontType);

                var xRect = new XRect(0, 0, width, height);

                var formNormal = new XForm(_document, xRect);
                using (var gfx = XGraphics.FromForm(formNormal))
                {
                    gfx.IntersectClip(xRect);
                    Owner.AcroForm?.FieldRenderer.PushButtonFieldRenderer.RenderNormalState(this, widget, gfx, xRect);
                }
                formNormal.DrawingFinished();
                SetXFormFont(formNormal);

                // Note: implementing RenderRolloverState and RenderDownState is optional for the PushButtonFieldRenderer
                // the current implementation throws a NotImplementedException for these methods
                var formRollover = new XForm(_document, xRect);
                try
                {
                    using (var gfx = XGraphics.FromForm(formRollover))
                    {
                        gfx.IntersectClip(xRect);
                        Owner.AcroForm?.FieldRenderer.PushButtonFieldRenderer.RenderRolloverState(this, widget, gfx, xRect);
                    }
                    formRollover.DrawingFinished();
                    SetXFormFont(formRollover);
                }
                catch (NotImplementedException)
                {
                    formRollover = null;
                }

                var formDown = new XForm(_document, xRect);
                try
                {
                    using (var gfx = XGraphics.FromForm(formDown))
                    {
                        gfx.IntersectClip(xRect);
                        Owner.AcroForm?.FieldRenderer.PushButtonFieldRenderer.RenderDownState(this, widget, gfx, xRect);
                    }
                    formDown.DrawingFinished();
                    SetXFormFont(formDown);
                }
                catch (NotImplementedException)
                {
                    formDown = null;
                }

                var ap = new PdfDictionary(Owner);
                widget.Elements[PdfAnnotation.Keys.AP] = ap;
                ap.Elements["/N"] = formNormal.PdfForm.Reference;
                if (formRollover != null)
                    ap.Elements["/R"] = formRollover.PdfForm.Reference;
                if (formDown != null)
                    ap.Elements["/D"] = formDown.PdfForm.Reference;

                foreach (var form in new[] { formNormal, formRollover, formDown })
                {
                    if (form == null)
                        continue;
                    var xobj = form.PdfForm;
                    var s = xobj.Stream.ToString();
                    s = "/Tx BMC\n" + s + "\nEMC";
                    xobj.Stream.Value = new RawEncoding().GetBytes(s);
                }
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
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
