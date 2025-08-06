// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

// ReSharper disable StringLiteralTypo

namespace MigraDoc.DocumentObjectModel.IO
{
    class KeyWords
    {
        static KeyWords()
        {
            EnumToName.Add(Symbol.True, "true");
            EnumToName.Add(Symbol.False, "false");
            EnumToName.Add(Symbol.Null, "null");

            EnumToName.Add(Symbol.Styles, @"\styles");
            EnumToName.Add(Symbol.Document, @"\document");
            EnumToName.Add(Symbol.EmbeddedFile, @"\EmbeddedFile");
            EnumToName.Add(Symbol.Section, @"\section");
            EnumToName.Add(Symbol.Paragraph, @"\paragraph");
            EnumToName.Add(Symbol.Header, @"\header");
            EnumToName.Add(Symbol.Footer, @"\footer");
            EnumToName.Add(Symbol.PrimaryHeader, @"\primaryheader");
            EnumToName.Add(Symbol.PrimaryFooter, @"\primaryfooter");
            EnumToName.Add(Symbol.FirstPageHeader, @"\firstpageheader");
            EnumToName.Add(Symbol.FirstPageFooter, @"\firstpagefooter");
            EnumToName.Add(Symbol.EvenPageHeader, @"\evenpageheader");
            EnumToName.Add(Symbol.EvenPageFooter, @"\evenpagefooter");
            EnumToName.Add(Symbol.Table, @"\table");
            EnumToName.Add(Symbol.Columns, @"\columns");
            EnumToName.Add(Symbol.Column, @"\column");
            EnumToName.Add(Symbol.Rows, @"\rows");
            EnumToName.Add(Symbol.Row, @"\row");
            EnumToName.Add(Symbol.Cell, @"\cell");
            EnumToName.Add(Symbol.Image, @"\image");
            EnumToName.Add(Symbol.TextFrame, @"\textframe");
            EnumToName.Add(Symbol.PageBreak, @"\pagebreak");
            EnumToName.Add(Symbol.Barcode, @"\barcode");
            EnumToName.Add(Symbol.Chart, @"\chart");
            EnumToName.Add(Symbol.HeaderArea, @"\headerarea");
            EnumToName.Add(Symbol.FooterArea, @"\footerarea");
            EnumToName.Add(Symbol.TopArea, @"\toparea");
            EnumToName.Add(Symbol.BottomArea, @"\bottomarea");
            EnumToName.Add(Symbol.LeftArea, @"\leftarea");
            EnumToName.Add(Symbol.RightArea, @"\rightarea");
            EnumToName.Add(Symbol.PlotArea, @"\plotarea");
            EnumToName.Add(Symbol.Legend, @"\legend");
            EnumToName.Add(Symbol.XAxis, @"\xaxis");
            EnumToName.Add(Symbol.YAxis, @"\yaxis");
            EnumToName.Add(Symbol.ZAxis, @"\zaxis");
            EnumToName.Add(Symbol.Series, @"\series");
            EnumToName.Add(Symbol.XValues, @"\xvalues");
            EnumToName.Add(Symbol.Point, @"\point");

            EnumToName.Add(Symbol.Bold, @"\bold");
            EnumToName.Add(Symbol.Italic, @"\italic");
            EnumToName.Add(Symbol.Underline, @"\underline");
            EnumToName.Add(Symbol.FontSize, @"\fontsize");
            EnumToName.Add(Symbol.FontColor, @"\fontcolor");
            EnumToName.Add(Symbol.Font, @"\font");
            //
            EnumToName.Add(Symbol.Field, @"\field");
            EnumToName.Add(Symbol.Symbol, @"\symbol");
            EnumToName.Add(Symbol.Chr, @"\chr");
            //
            EnumToName.Add(Symbol.Footnote, @"\footnote");
            EnumToName.Add(Symbol.Hyperlink, @"\hyperlink");
            //
            EnumToName.Add(Symbol.SoftHyphen, @"\-");
            EnumToName.Add(Symbol.Tab, @"\tab");
            EnumToName.Add(Symbol.LineBreak, @"\linebreak");
            EnumToName.Add(Symbol.Space, @"\space");
            EnumToName.Add(Symbol.NoSpace, @"\nospace");

            //
            //
            EnumToName.Add(Symbol.BraceLeft, "{");
            EnumToName.Add(Symbol.BraceRight, "}");
            EnumToName.Add(Symbol.BracketLeft, "[");
            EnumToName.Add(Symbol.BracketRight, "]");
            EnumToName.Add(Symbol.ParenLeft, "(");
            EnumToName.Add(Symbol.ParenRight, ")");
            EnumToName.Add(Symbol.Colon, ":");
            EnumToName.Add(Symbol.Semicolon, ";");  //??? id DDL?
            EnumToName.Add(Symbol.Dot, ".");
            EnumToName.Add(Symbol.Comma, ",");
            EnumToName.Add(Symbol.Percent, "%");  //??? id DDL?
            EnumToName.Add(Symbol.Dollar, "$");  //??? id DDL?
            //enumToName.Add(Symbol.At,                "@");
            EnumToName.Add(Symbol.Hash, "#");  //??? id DDL?
            //enumToName.Add(Symbol.Question,          "?");  //??? id DDL?
            //enumToName.Add(Symbol.Bar,               "|");  //??? id DDL?
            EnumToName.Add(Symbol.Assign, "=");
            EnumToName.Add(Symbol.Slash, "/");  //??? id DDL?
            EnumToName.Add(Symbol.BackSlash, "\\");
            EnumToName.Add(Symbol.Plus, "+");  //??? id DDL?
            EnumToName.Add(Symbol.PlusAssign, "+=");
            EnumToName.Add(Symbol.Minus, "-");  //??? id DDL?
            EnumToName.Add(Symbol.MinusAssign, "-=");
            EnumToName.Add(Symbol.Blank, " ");

            //---------------------------------------------------------------
            //---------------------------------------------------------------
            //---------------------------------------------------------------

            NameToEnum.Add("true", Symbol.True);
            NameToEnum.Add("false", Symbol.False);
            NameToEnum.Add("null", Symbol.Null);
            //
            NameToEnum.Add(@"\styles", Symbol.Styles);
            NameToEnum.Add(@"\document", Symbol.Document);
            NameToEnum.Add(@"\EmbeddedFile", Symbol.EmbeddedFile);
            NameToEnum.Add(@"\section", Symbol.Section);
            NameToEnum.Add(@"\paragraph", Symbol.Paragraph);
            NameToEnum.Add(@"\header", Symbol.Header);
            NameToEnum.Add(@"\footer", Symbol.Footer);
            NameToEnum.Add(@"\primaryheader", Symbol.PrimaryHeader);
            NameToEnum.Add(@"\primaryfooter", Symbol.PrimaryFooter);
            NameToEnum.Add(@"\firstpageheader", Symbol.FirstPageHeader);
            NameToEnum.Add(@"\firstpagefooter", Symbol.FirstPageFooter);
            NameToEnum.Add(@"\evenpageheader", Symbol.EvenPageHeader);
            NameToEnum.Add(@"\evenpagefooter", Symbol.EvenPageFooter);
            NameToEnum.Add(@"\table", Symbol.Table);
            NameToEnum.Add(@"\columns", Symbol.Columns);
            NameToEnum.Add(@"\column", Symbol.Column);
            NameToEnum.Add(@"\rows", Symbol.Rows);
            NameToEnum.Add(@"\row", Symbol.Row);
            NameToEnum.Add(@"\cell", Symbol.Cell);
            NameToEnum.Add(@"\image", Symbol.Image);
            NameToEnum.Add(@"\textframe", Symbol.TextFrame);
            NameToEnum.Add(@"\pagebreak", Symbol.PageBreak);
            NameToEnum.Add(@"\barcode", Symbol.Barcode);
            NameToEnum.Add(@"\chart", Symbol.Chart);
            NameToEnum.Add(@"\headerarea", Symbol.HeaderArea);
            NameToEnum.Add(@"\footerarea", Symbol.FooterArea);
            NameToEnum.Add(@"\toparea", Symbol.TopArea);
            NameToEnum.Add(@"\bottomarea", Symbol.BottomArea);
            NameToEnum.Add(@"\leftarea", Symbol.LeftArea);
            NameToEnum.Add(@"\rightarea", Symbol.RightArea);
            NameToEnum.Add(@"\plotarea", Symbol.PlotArea);
            NameToEnum.Add(@"\legend", Symbol.Legend);
            NameToEnum.Add(@"\xaxis", Symbol.XAxis);
            NameToEnum.Add(@"\yaxis", Symbol.YAxis);
            NameToEnum.Add(@"\zaxis", Symbol.ZAxis);
            NameToEnum.Add(@"\series", Symbol.Series);
            NameToEnum.Add(@"\xvalues", Symbol.XValues);
            NameToEnum.Add(@"\point", Symbol.Point);
            NameToEnum.Add(@"\bold", Symbol.Bold);
            NameToEnum.Add(@"\italic", Symbol.Italic);
            NameToEnum.Add(@"\underline", Symbol.Underline);
            NameToEnum.Add(@"\fontsize", Symbol.FontSize);
            NameToEnum.Add(@"\fontcolor", Symbol.FontColor);
            NameToEnum.Add(@"\font", Symbol.Font);
            //
            NameToEnum.Add(@"\field", Symbol.Field);
            NameToEnum.Add(@"\symbol", Symbol.Symbol);
            NameToEnum.Add(@"\chr", Symbol.Chr);
            //
            NameToEnum.Add(@"\footnote", Symbol.Footnote);
            NameToEnum.Add(@"\hyperlink", Symbol.Hyperlink);
            //
            NameToEnum.Add(@"\-", Symbol.SoftHyphen); //??? \( is another special case.
            NameToEnum.Add(@"\tab", Symbol.Tab);
            NameToEnum.Add(@"\linebreak", Symbol.LineBreak);
            NameToEnum.Add(@"\space", Symbol.Space);
            NameToEnum.Add(@"\nospace", Symbol.NoSpace);
        }

        /// <summary>
        /// Returns Symbol value from name, or Symbol.None if no such Symbol exists.
        /// </summary>
        internal static Symbol SymbolFromName(string name)
        {
            Symbol symbol;
            if (!NameToEnum.TryGetValue(name, out symbol))
            {
                // Check for case-sensitive keywords. Allow first character upper case only.
                if (string.Compare(name, "True", StringComparison.OrdinalIgnoreCase) == 0)
                    symbol = Symbol.True;
                else if (string.Compare(name, "False", StringComparison.OrdinalIgnoreCase) == 0)
                    symbol = Symbol.False;
                else if (string.Compare(name, "Null", StringComparison.OrdinalIgnoreCase) == 0)
                    symbol = Symbol.Null;
                else
                    symbol = Symbol.None;
            }
            return symbol;
        }

        /// <summary>
        /// Returns string from Symbol value.
        /// </summary>
        internal static string NameFromSymbol(Symbol symbol) => EnumToName[symbol];

        static readonly Dictionary<Symbol, string> EnumToName = [];
        static readonly Dictionary<string, Symbol> NameToEnum = [];
    }
}
