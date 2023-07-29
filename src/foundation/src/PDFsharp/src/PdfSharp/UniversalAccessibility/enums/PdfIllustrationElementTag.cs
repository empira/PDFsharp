// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// PDF Illustration Element tags for Universal Accessibility.
    /// </summary>
    public enum PdfIllustrationElementTag
    {
        /// <summary>
        /// (Figure) An item of graphical content. Its placement may be specified with the Placementlayout attribute.
        /// </summary>
        Figure = 0,

        /// <summary>
        /// (Formula) A mathematical formula.
        /// </summary>
        Formula = 1,

        /// <summary>
        /// (Form) A widget annotation representing an interactive form field.
        /// If the element contains a Role attribute, it may contain content items that represent
        /// the value of the (non-interactive) form field. If the element omits a Role attribute,
        ///  its only child is an object reference identifying the widget annotation. 
        /// The annotations’ appearance stream defines the rendering of the form element.
        /// </summary>
        Form = 2
    }
}
