// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Internal.OpenType
{
    public static class KeyHelper
    {
        public static string CalcFontFaceKey(string familyName, string faceName, 
            OpenTypeFontStyle style, OpenTypeFontWeight weight, OpenTypeFontStretch stretch)
        {
            var key = Invariant($"{familyName}({faceName})|{(int)style}|{(int)weight}|{(int)stretch}");
            return key;
        }
        public static string CalcGlyphTypefaceKey(string familyName, string faceName,
            OpenTypeFontStyle style, OpenTypeFontWeight weight, OpenTypeFontStretch stretch,
            bool isBoldSimulated, bool isObliqueSimulated)
        {
            var key = Invariant($"{familyName}({faceName})|{(int)style}|{(int)weight}|{(int)stretch}|{
                (isBoldSimulated ? "B" : "")}{(isObliqueSimulated ? "O" : "")}");
            return key;
        }

        public static string CalcTypefaceKey(OpenTypeFontStyle style, OpenTypeFontWeight weight, OpenTypeFontStretch stretch)
        {
            var key = Invariant($"{(int)style}|{(int)weight}|{(int)stretch}");
            return key;
        }

        // Replace this code.
        public static string ComputeFdKey(string name, OTFontStyleHack style)
            => ComputeFdKey(name,
                (style & OTFontStyleHack.Bold) != 0,
                (style & OTFontStyleHack.Italic) != 0);

        // Replace this code.
        public static string ComputeFdKey(string name, bool isBold, bool isItalic)
        {
            name = name.ToLowerInvariant();
            var key = isBold switch
            {
                false when !isItalic => name + '/',
                true when !isItalic => name + "/b",
                false when isItalic => name + "/i",
                _ => name + "/bi"
            };
            return key;
        }
    }
}
