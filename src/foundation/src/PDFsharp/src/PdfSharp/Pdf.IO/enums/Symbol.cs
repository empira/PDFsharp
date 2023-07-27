// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// PDF Terminal symbols recognized by lexer.
    /// </summary>
    public enum Symbol
    {
#pragma warning disable 1591
        None,
        Comment, Null, Integer, UInteger, Real, Boolean, String, HexString, UnicodeString, UnicodeHexString,
        Name, Keyword,
        BeginStream, EndStream,
        BeginArray, EndArray,
        BeginDictionary, EndDictionary,
        Obj, EndObj, R, XRef, Trailer, StartXRef, Eof,
#pragma warning restore 1591
    }
}
