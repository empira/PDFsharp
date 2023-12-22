// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using System.Text;

namespace MigraDoc.DocumentObjectModel.Visitors
{
    /// <summary>
    /// Extension methods for document objects.
    /// </summary>
    public static class ElementsExtensions
    {
        /// <summary>
        /// Gets the elements/sections/rows/cells of a DocumentObject.
        /// </summary>
        /// <param name="documentObject">The DocumentObject.</param>
        /// <param name="includeHeaderFooter">Shall headers and footers also be returned.
        /// No effect for DocumentObjects that neither are nor contain Sections.</param>
        public static IEnumerable<DocumentObject?> GetElements(this DocumentObject documentObject, bool includeHeaderFooter = false)
        {
            if (documentObject is Document document)
                return document.Sections.Cast<DocumentObject>();

            if (documentObject is Section section)
            {
                var elements = section.Elements.Cast<DocumentObject?>();

                if (includeHeaderFooter)
                {
                    // Access Header/Footer Primary/FirstPage/EvenPage creates a new HeaderFooter element if not existing. To avoid this, we check if it exists.
                    if (section.Headers.HasHeaderFooter(section.Headers.Values.Primary))
                        elements = elements.Concat(section.Headers.Primary.Elements);
                    if (section.Headers.HasHeaderFooter(section.Headers.Values.FirstPage))
                        elements = elements.Concat(section.Headers.FirstPage.Elements);
                    if (section.Headers.HasHeaderFooter(section.Headers.Values.EvenPage))
                        elements = elements.Concat(section.Headers.EvenPage.Elements);

                    if (section.Footers.HasHeaderFooter(section.Footers.Values.Primary))
                        elements = elements.Concat(section.Footers.Primary.Elements);
                    if (section.Footers.HasHeaderFooter(section.Footers.Values.FirstPage))
                        elements = elements.Concat(section.Footers.FirstPage.Elements);
                    if (section.Footers.HasHeaderFooter(section.Footers.Values.EvenPage))
                        elements = elements.Concat(section.Footers.EvenPage.Elements);
                    //if (section.Headers.HasHeaderFooter(HeaderFooterIndex.Primary))
                    //    elements = elements.Concat(section.Headers.Primary.Elements.Cast<DocumentObject>());
                    //if (section.Headers.HasHeaderFooter(HeaderFooterIndex.FirstPage))
                    //    elements = elements.Concat(section.Headers.FirstPage.Elements.Cast<DocumentObject>());
                    //if (section.Headers.HasHeaderFooter(HeaderFooterIndex.EvenPage))
                    //    elements = elements.Concat(section.Headers.EvenPage.Elements.Cast<DocumentObject>());

                    //if (section.Footers.HasHeaderFooter(HeaderFooterIndex.Primary))
                    //    elements = elements.Concat(section.Footers.Primary.Elements.Cast<DocumentObject>());
                    //if (section.Footers.HasHeaderFooter(HeaderFooterIndex.FirstPage))
                    //    elements = elements.Concat(section.Footers.FirstPage.Elements.Cast<DocumentObject>());
                    //if (section.Footers.HasHeaderFooter(HeaderFooterIndex.EvenPage))
                    //    elements = elements.Concat(section.Footers.EvenPage.Elements.Cast<DocumentObject>());
                }
                return elements;
            }

            if (documentObject is HeaderFooter headerFooter)
                return headerFooter.Elements.Cast<DocumentObject>();

            if (documentObject is Paragraph paragraph)
                return paragraph.Elements.Cast<DocumentObject>();

            if (documentObject is FormattedText formattedText)
                return formattedText.Elements.Cast<DocumentObject>();

            if (documentObject is TextFrame textFrame)
                return textFrame.Elements.Cast<DocumentObject>();

            if (documentObject is Hyperlink hyperlink)
                return hyperlink.Elements.Cast<DocumentObject>();

            if (documentObject is Table table)
                return table.Rows.Cast<DocumentObject>();

            if (documentObject is Row row)
                return row.Cells.Cast<DocumentObject>();

            if (documentObject is Cell cell)
                return cell.Elements.Cast<DocumentObject>();

            return new DocumentObject[] { };
        }

        /// <summary>
        /// Gets the elements/sections/rows/cells of Type T of a DocumentObject.
        /// </summary>
        /// <param name="documentObject">The DocumentObject.</param>
        /// <param name="includeHeaderFooter">Shall headers and footers also be returned.
        /// No effect for DocumentObjects that neither are nor contain Sections.</param>
        public static IEnumerable<T> GetElements<T>(this DocumentObject documentObject, bool includeHeaderFooter = false) where T : DocumentObject
        {
            var elements = documentObject.GetElements(includeHeaderFooter);
            return elements.Filter<T>();
        }

        /// <summary>
        /// Gets the elements/sections/rows/cells of a DocumentObject recursively.
        /// </summary>
        /// <param name="documentObject">The DocumentObject.</param>
        /// <param name="includeHeaderFooter">Shall headers and footers also be returned.
        /// No effect for DocumentObjects that neither are nor contain Sections.</param>
        public static IEnumerable<DocumentObject?> GetElementsRecursively(this DocumentObject documentObject, bool includeHeaderFooter = false)
        {
            var elements = documentObject.GetElements(includeHeaderFooter);
            foreach (var element in elements)
            {
                yield return element;

                if (element == null) // Null elements have no children.
                    continue;

                var children = element.GetElementsRecursively(includeHeaderFooter);
                foreach (var child in children)
                    yield return child;
            }
        }

        /// <summary>
        /// Gets the elements/sections/rows/cells of a DocumentObject recursively, stopping recursion at elements of the Types listed in stopAtItems.
        /// </summary>
        /// <param name="documentObject">The DocumentObject.</param>
        /// <param name="stopAtElements">A list with types the recursion shall stop at.</param>
        /// <param name="includeStoppingElements">If false, the stopping element and its children are not returned. If true, the stopping element is returned, but its children are not.</param>
        /// <param name="includeHeaderFooter">Shall headers and footers also be returned.
        /// No effect for DocumentObjects that neither are nor contain Sections.</param>
        public static IEnumerable<DocumentObject?> GetElementsRecursively(this DocumentObject documentObject, 
            List<Type> stopAtElements, bool includeStoppingElements = false, bool includeHeaderFooter = false)
        {
            var elements = documentObject.GetElements(includeHeaderFooter);
            foreach (var element in elements)
            {
                var type = element?.GetType();
                var stop = type != null && stopAtElements.Contains(type);

                if (stop && !includeStoppingElements)
                    yield break;

                yield return element;

                if (stop)
                    yield break;

                if (element == null) // Null elements have no children.
                    continue;

                var children = element.GetElementsRecursively(includeHeaderFooter);
                foreach (var child in children)
                    yield return child;
            }
        }

        /// <summary>
        /// Gets the elements/sections/rows/cells of Type T of a DocumentObject recursively.
        /// </summary>
        /// <param name="documentObject">The DocumentObject.</param>
        /// <param name="includeHeaderFooter">Shall headers and footers also be returned.
        /// No effect for DocumentObjects that neither are nor contain Sections.</param>
        public static IEnumerable<T> GetElementsRecursively<T>(this DocumentObject documentObject, bool includeHeaderFooter = false)
            where T : DocumentObject
        {
            var elements = documentObject.GetElementsRecursively(includeHeaderFooter);
            return elements.Filter<T>();
        }

        /// <summary>
        /// Filters the document objects of a specific type.
        /// </summary>
        /// <typeparam name="T">The type to be filtered.</typeparam>
        /// <param name="elements">The document objects.</param>
        /// <returns></returns>
        public static IEnumerable<T> Filter<T>(this IEnumerable<DocumentObject?> elements) 
            where T : DocumentObject
        {
            foreach (var element in elements)
            {
                if (element is T t)
                    yield return t;
            }
        }

        /// <summary>
        /// Gets the text content of the cell's elements as a string.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>Text content as string.</returns>
        public static string GetText(this Cell cell)
        {
            return cell.Elements.GetText();
        }

        /// <summary>
        /// Gets the text content of the paragraph's elements as a string.
        /// </summary>
        /// <param name="paragraph">The paragraph.</param>
        /// <returns>Text content as string.</returns>
        public static string GetText(this Paragraph paragraph)
        {
            return paragraph.Elements.GetText();
        }

        /// <summary>
        /// Gets the text content of the hyperlinks's elements as a string.
        /// </summary>
        /// <param name="hyperlink">The hyperlink.</param>
        /// <returns>Text content as string.</returns>
        public static string GetText(this Hyperlink hyperlink)
        {
            return hyperlink.Elements.GetText();
        }

        /// <summary>
        /// Gets the text content of the elements as a string.
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <returns>Text content as string.</returns>
        public static string GetText(this IEnumerable<DocumentObject?> elements)
        {
            return GetTextAsStringBuilder(elements).ToString();
        }

        static StringBuilder GetTextAsStringBuilder(IEnumerable<DocumentObject?> elements)
        {
            var sb = new StringBuilder();
            Paragraph? lastAsParagraph = null;

            foreach (var element in elements)
            {
                if (element is null)
                    continue;

                var p = element as Paragraph;
                if (p != lastAsParagraph && sb.Length > 0)
                    sb.Append("\n\n");

                if (p != null)
                    sb.Append(GetTextAsStringBuilder(p.Elements));
                else if (element is Text t)
                    sb.Append(t.Content);
                else if (element is FormattedText ft)
                    sb.Append(GetTextAsStringBuilder(ft.Elements));
                else if (element is Character c)
                    sb.Append(c.GetText());

                lastAsParagraph = p;
            }

            return sb;
        }

        static string GetText(this Character character)
        {
            var symbol = character.GetSymbol();
            var result = "";

            for (var i = 0; i < character.Count; i++)
                result += symbol;

            return result;
        }

        static string GetSymbol(this Character character)
        {
            return character.SymbolName switch
            {
                SymbolName.Blank => " ",
                SymbolName.En => "\u2002",
                SymbolName.Em => "\u2003",
                SymbolName.EmQuarter => "\u2005",
                SymbolName.Tab => "\t",
                SymbolName.LineBreak => "\n",
                SymbolName.ParaBreak => "\n\n",
                SymbolName.Euro => "€",
                SymbolName.Copyright => "©",
                SymbolName.Trademark => "™",
                SymbolName.RegisteredTrademark => "®",
                SymbolName.Bullet => "•",
                SymbolName.Not => "¬",
                SymbolName.EmDash => "—",
                SymbolName.EnDash => "–",
                SymbolName.NonBreakableBlank => "\u0083",
                _ => character.Char.ToString()
            };
        }

        /// <summary>
        /// Gets the style of a DocumentObject.
        /// </summary>
        public static string? GetStyle(this DocumentObject documentObject)
        {
            if (documentObject is HeaderFooter headerFooter)
                return headerFooter.Style;

            if (documentObject is Paragraph paragraph)
                return paragraph.Style;

            if (documentObject is FormattedText formattedText)
                return formattedText.Style;

            return null;
        }

        /// <summary>
        /// Gets the ParagraphFormat of a DocumentObject.
        /// </summary>
        public static ParagraphFormat? GetFormat(this DocumentObject documentObject)
        {
            if (documentObject is HeaderFooter headerFooter)
                return headerFooter.Format;

            if (documentObject is Paragraph paragraph)
                return paragraph.Format;

            if (documentObject is Style style)
                return style.ParagraphFormat;

            return null;
        }

        /// <summary>
        /// Gets the font of a DocumentObject stored in Font or Format.Font.
        /// </summary>
        public static Font? GetFont(this DocumentObject documentObject)
        {
            if (documentObject is HeaderFooter headerFooter)
                return headerFooter.Format.Font;

            if (documentObject is Paragraph paragraph)
                return paragraph.Format.Font;

            if (documentObject is FormattedText formattedText)
                return formattedText.Font;

            if (documentObject is Hyperlink hyperlink)
                return hyperlink.Font;

            if (documentObject is Style style)
                return style.Font;

            return null;
        }

        /// <summary>
        /// Gets a directly or via style assigned ParagraphFormat sub-value of type Unit.
        /// </summary>
        public static Unit GetUsedFormatValue(this DocumentObject documentObject, Func<ParagraphFormat, Unit> getValue)
        {
            return documentObject.GetUsedFormatValue(getValue, value => value.IsEmpty, Unit.Zero);
        }

        /// <summary>
        /// Gets a directly or via style assigned ParagraphFormat sub-value.
        /// </summary>
        /// <param name="documentObject">The object to get the value for.</param>
        /// <param name="getValue">The function to get the value from ParagraphFormat.</param>
        /// <param name="isEmpty">The function to determine if the value is not set. If the directly assigned value is not set, we try to get it via style.</param>
        /// <param name="emptyValue">The empty value to return if the value is not assigned or could not be determined.</param>
        public static T GetUsedFormatValue<T>(this DocumentObject documentObject, Func<ParagraphFormat, T> getValue, Func<T, bool> isEmpty, T emptyValue)
        {
            // Won't work properly inside of tables, as the formats of table columns and rows are not considered as used value. Supplement this when needed.
            T result;

            var format = documentObject.GetFormat();
            if (format != null)
            {
                result = getValue(format);
                if (!isEmpty(result))
                    return result;
            }

            var style = documentObject.GetStyle();
            if (!String.IsNullOrEmpty(style))
            {
                var styleObj = documentObject.Document.Styles[style];
                while (styleObj != null)
                {
                    result = getValue(styleObj.ParagraphFormat);
                    if (!isEmpty(result))
                        return result;

                    styleObj = styleObj.GetBaseStyle();
                }
            }

            var parent = documentObject.Parent;
            if (parent != null)
                return GetUsedFormatValue(parent, getValue, isEmpty, emptyValue);

            Debug.Assert(false);
            return emptyValue;
        }

        /// <summary>
        /// Gets a directly or via style assigned Font sub-value of type Unit.
        /// </summary>
        public static Unit GetUsedFormatValue(this DocumentObject documentObject, Func<Font, Unit> getValue)
        {
            return documentObject.GetUsedFormatValue(getValue, value => value.IsEmpty, Unit.Zero);
        }

        /// <summary>
        /// Gets a directly or via style assigned Font sub-value.
        /// </summary>
        /// <param name="documentObject">The object to get the value for.</param>
        /// <param name="getValue">The function to get the value from Font.</param>
        /// <param name="isEmpty">The function to determine if the value is not set. If the directly assigned value is not set, we try to get it via style.</param>
        /// <param name="emptyValue">The empty value to return if the value is not assigned or could not be determined.</param>
        public static T GetUsedFormatValue<T>(this DocumentObject documentObject, Func<Font, T> getValue, Func<T, bool> isEmpty, T emptyValue)
        {
            // Won't work properly inside of tables, as the formats of table columns and rows are not considered as used value. Supplement this when needed.
            T result;

            var font = documentObject.GetFont();
            if (font != null)
            {
                result = getValue(font);
                if (!isEmpty(result))
                    return result;
            }

            var style = documentObject.GetStyle();
            if (!String.IsNullOrEmpty(style))
            {
                var styleObj = documentObject.Document.Styles[style];
                while (styleObj != null)
                {
                    result = getValue(styleObj.Font);
                    if (!isEmpty(result))
                        return result;

                    styleObj = styleObj.GetBaseStyle();
                }
            }

            var parent = documentObject.Parent;
            if (parent != null)
                return GetUsedFormatValue(parent, getValue, isEmpty, emptyValue);

            Debug.Assert(false);
            return emptyValue;
        }
    }
}
