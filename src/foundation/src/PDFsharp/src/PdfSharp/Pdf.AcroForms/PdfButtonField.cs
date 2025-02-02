// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the base class for all button fields.
    /// </summary>
    public abstract class PdfButtonField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfButtonField"/> class.
        /// </summary>
        protected PdfButtonField(PdfDocument document)
            : base(document)
        {
            Elements.SetName(PdfAcroField.Keys.FT, "Btn");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfButtonField"/> class.
        /// </summary>
        protected PdfButtonField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets the name which represents the opposite of Off.
        /// </summary>
        public string? GetNonOffValue()
        {
            return GetNonOffValueInternal().TrimStart('/');
        }

        /// <summary>
        /// Gets the name which represents the opposite of /Off.
        /// </summary>
        protected string GetNonOffValueInternal()
        {
            // Try to get the information from the appearance dictionary.
            // Just return the first key that is not /Off.
            // I'm not sure what is the right solution to get this value.
            if (Annotations.Elements.Count > 0)
            {
                var widget = Annotations.Elements[0];
                if (widget != null)
                {
                    var ap = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    if (ap != null)
                    {
                        var n = ap.Elements.GetDictionary("/N");
                        if (n != null)
                        {
                            foreach (string name in n.Elements.Keys)
                                if (name != "/Off")
                                    return name;
                        }
                    }
                }
            }
            return "/Yes";
        }

        /// <summary>
        /// Gets the name which represents the opposite of /Off for the specified widget.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        protected static string? GetNonOffValue(PdfWidgetAnnotation widget)
        {
            if (widget != null)
            {
                var ap = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                if (ap != null)
                {
                    var n = ap.Elements.GetDictionary("/N");
                    if (n != null)
                    {
                        return n.Elements.Keys.FirstOrDefault(name => name != "/Off");
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to determine the visual appearance for this AcroField
        /// </summary>
        protected override void DetermineAppearance()
        {
            base.DetermineAppearance();
            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget.Page != null)
                {
                    var appearance = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    if (appearance != null)
                    {
                        // /N -> Normal appearance, /R -> Rollover appearance, /D -> Down appearance
                        var normalAppearance = appearance.Elements.GetDictionary("/N");
                        if (normalAppearance != null)
                        {
                            var activeAppearance = normalAppearance.Elements.GetDictionary(GetNonOffValueInternal());
                            if (activeAppearance != null)
                            {
                                try
                                {
                                    DetermineFontFromContent(activeAppearance.Stream.UnfilteredValue);
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // Pushbuttons have no additional entries.
        }
    }
}
