// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Internal.OpenType
{
    static class OtMsgs
    {
        public static string ErrorReadingFontData
            => "Error while parsing an OpenType font.";
    }


    public struct OTColor
    {
        public OTColor(uint argb)
        {
            A = (byte)((argb >> 24) & 0xff);
            R = (byte)((argb >> 16) & 0xff);
            G = (byte)((argb >> 8) & 0xff);
            B = (byte)(argb & 0xff);
        }

        public static OTColor FromArgb(byte alpha, byte red, byte green, byte blue)
        {
            return new((uint)(alpha << 24) | (uint)(red << 16) | (uint)(green << 8) | blue);
        }

        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public uint Argb => (uint)((A << 24) | (R << 16) | (G << 8) | B);
    }


    /// <summary>
    /// Specifies style information applied to text.
    /// Note that this enum was named XFontStyle in PDFsharp versions prior to 6.0.
    /// </summary>
    [Flags]
    public enum OTFontStyleHack // Same values as System.Drawing.FontStyle.
    {
        // Will be renamed to XGdiFontStyle or XWinFontStyle.

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic text.
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Bold and italic text.
        /// </summary>
        BoldItalic = 3,
    }

    static class NRT
    {
        /// <summary>
        /// Throws an InvalidOperationException because an expression which must not be null is null.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        [DoesNotReturn]
        public static void ThrowOnNull(string? message = null)
            => throw new InvalidOperationException(message ?? "Expression must not be null here.");

        /// <summary>
        /// Throws InvalidOperationException. Use this function during the transition from older C# code
        /// to new code that uses nullable reference types.
        /// </summary>
        /// <typeparam name="TResult">The type this function must return to be compiled correctly.</typeparam>
        /// <param name="message">An optional message used for the exception.</param>
        /// <returns>Nothing, always throws.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        [DoesNotReturn]
        public static TResult ThrowOnNull<TResult>(string? message = null)
            => throw new InvalidOperationException(message ?? $"'{typeof(TResult).Name}' must not be null here.");
    }

    public enum OpenTypeFontEmbedding  // Keep in sync with PdfFontEmbedding.
    {
        /// <summary>
        /// OpenType font files with TrueType outline are embedded as a font subset.
        /// OpenType font files with PostScript outline are embedded as they are,
        /// because PDFsharp cannot compute subsets from this type of font files.
        /// </summary>
        TryComputeSubset = 0,

        /// <summary>
        /// The font file is completely embedded. No subset is computed.
        /// </summary>
        EmbedCompleteFontFile = 1,
    }
}
