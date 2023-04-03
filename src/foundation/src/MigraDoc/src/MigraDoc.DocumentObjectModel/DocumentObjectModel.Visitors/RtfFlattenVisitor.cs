// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Represents the visitor for flattening the DocumentObject to be used in the RtfRenderer.
    /// </summary>
    public class RtfFlattenVisitor : VisitorBase
    {
        internal override void VisitFormattedText(FormattedText formattedText)
        {
            var document = formattedText.Document;
            ParagraphFormat? format = null;

            //Style style = document.Styles[formattedText._style.Value];
            var style = document.Styles[formattedText.Values.Style /*?? ""*/];
            if (style != null)
                format = style.Values.ParagraphFormat;
            else if (!String.IsNullOrEmpty(formattedText.Values.Style))
                format = document.Styles[StyleNames.InvalidStyleName]!.Values.ParagraphFormat; // BUG: Clone here?

            if (format != null)
            {
                if (formattedText.Values.Font is null)
                    formattedText.Font = format.Values.Font?.Clone() ?? NRT.ThrowOnNull<Font>(); // BUG Throwing if format.Values.Font is null
                else if (format.Values.Font is not null)
                    FlattenFont(formattedText.Values.Font, format.Values.Font);
            }
        }

        internal override void VisitHyperlink(Hyperlink hyperlink)
        {
            var styleFont = hyperlink.Document.Styles[StyleNames.Hyperlink]!.Font ?? throw new InvalidOperationException("Style does not exist.");
            if (hyperlink.Values.Font is null)
                hyperlink.Font = styleFont.Clone();
            else
                FlattenFont(hyperlink.Values.Font, styleFont);
        }
    }
}
