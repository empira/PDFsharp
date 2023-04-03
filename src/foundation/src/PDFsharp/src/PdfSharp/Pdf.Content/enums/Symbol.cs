// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Content
{
    /// <summary>
    /// Terminal symbols recognized by PDF content stream lexer.
    /// </summary>
    public enum CSymbol
    {
#pragma warning disable 1591
        None,
        Comment,
        Integer,
        Real,
        /*Boolean?,*/
        String,
        HexString,
        UnicodeString,
        UnicodeHexString,
        Name,
        Operator,
        BeginArray,
        EndArray,
        Dictionary,  // HACK: << ... >> is scanned as string literal.
        Eof,
        Error = -1,
    }
}
