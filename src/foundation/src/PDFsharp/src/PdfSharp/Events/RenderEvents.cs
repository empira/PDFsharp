// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace PdfSharp.Events
{
    /// <summary>
    /// EventArgs for PrepareTextEvent.
    /// </summary>
    public class PrepareTextEventArgs(PdfObject source, XFont font, string text) : PdfSharpEventArgs(source)
    {
        /// <summary>
        /// Gets the font used to draw the text.
        /// The font cannot be changed in an event handler.
        /// </summary>
        public XFont Font { get; init; } = font;

        /// <summary>
        /// Gets or sets the text to be processed.
        /// </summary>
        public string Text { get; set; } = text;
    }

    /// <summary>
    /// EventHandler for DrawString and MeasureString.
    /// Gives a document the opportunity to inspect or modify the string before it is used for drawing or measuring text.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The RenderTextEventHandler of the event.</param>
    public delegate void PrepareTextEventHandler(object sender, PrepareTextEventArgs e);

    /// <summary>
    /// EventArgs for RenderTextEvent.
    /// </summary>
    public class RenderTextEventArgs(PdfObject source, XFont font, CodePointGlyphIndexPair[] codePointGlyphIndexPair) : PdfSharpEventArgs(source)
    {
        /// <summary>
        /// Gets or sets a value indicating whether the determination of the glyph identifiers must be reevaluated.
        /// An event handler set this property to true after it changed code points but does not set
        /// the appropriate glyph identifier.
        /// </summary>
        public bool ReevaluateGlyphIndices { get; set; }

        /// <summary>
        /// Gets the font used to draw the text.
        /// The font cannot be changed in an event handler.
        /// </summary>
        public XFont Font { get; init; } = font;

        /// <summary>
        /// Gets or sets the array containing the code points and glyph indices.
        /// An event handler can modify or replace this array.
        /// </summary>
        public CodePointGlyphIndexPair[] CodePointGlyphIndexPairs { get; set; } = codePointGlyphIndexPair;
    }

    /// <summary>
    /// EventHandler for DrawString and MeasureString.
    /// Gives a document the opportunity to inspect or modify the UTF-32 code points with their corresponding
    /// glyph identifiers before they are used for drawing or measuring text.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The RenderTextEventHandler of the event.</param>
    public delegate void RenderTextEventHandler(object sender, RenderTextEventArgs e);

    /// <summary>
    /// A class encapsulating all render events of a PdfDocument.
    /// </summary>
    public class RenderEvents
    {
        /// <summary>
        /// An event raised whenever text is about to be drawn or measured in a PDF document.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The PrepareTextEventArgs of the event.</param>
        public void OnPrepareTextEvent(object sender, PrepareTextEventArgs args)
        {
            PrepareTextEvent?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for PrepareTextEvent.
        /// </summary>
        public event PrepareTextEventHandler? PrepareTextEvent;

        /// <summary>
        /// An event raised whenever text is drawn or measured in a PDF document.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The RenderTextEventArgs of the event.</param>
        public void OnRenderTextEvent(object sender, RenderTextEventArgs args)
        {
            RenderTextEvent?.Invoke(sender, args);
        }

        /// <summary>
        /// EventHandler for RenderTextEvent.
        /// </summary>
        public event RenderTextEventHandler? RenderTextEvent;
    }
}
