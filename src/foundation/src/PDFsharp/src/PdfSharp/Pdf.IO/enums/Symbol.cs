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
        Comment, Null, Integer, LongInteger, Real, Boolean, String, HexString, UnicodeString, UnicodeHexString,
        Name, Keyword,
        BeginStream, EndStream,
        BeginArray, EndArray,
        BeginDictionary, EndDictionary,
        Obj, EndObj,
        R, // Is replaced by ObjRef.
        XRef, Trailer, StartXRef, Eof,
        
        // The lexer now can parse references in the form "nnn ggg R"
        // as a symbol in one step.
        ObjRef
#pragma warning restore 1591
    }
}
