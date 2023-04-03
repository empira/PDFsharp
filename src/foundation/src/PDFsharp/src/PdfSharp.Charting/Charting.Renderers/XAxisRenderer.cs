// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base class for all X axis renderer.
    /// </summary>
    abstract class XAxisRenderer : AxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the XAxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal XAxisRenderer(RendererParameters parameters) : base(parameters)
        { }

        /// <summary>
        /// Returns the default tick labels format string.
        /// </summary>
        protected override string GetDefaultTickLabelsFormat() 
            => "0";
    }
}
