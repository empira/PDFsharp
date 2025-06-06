// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Globalization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;

#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
#endif

namespace PdfSharp.Internal
{
    enum NotSupportedBehavior
    {
        /// <summary>
        /// Function invocation has no effect.
        /// Returns a default value if necessary.
        /// </summary>
        DoNothing,

        /// <summary>
        /// Logs a warning.
        /// </summary>
        Log,

        /// <summary>
        /// Logs an error.
        /// </summary>
        LogError,

        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        Throw
    }

    ///// <summary>
    ///// A bunch of internal helper functions.
    ///// </summary>
    //static class Diagnostics
    //{
    //    // ReSharper disable once ConvertToAutoProperty
    //    public static NotImplementedBehavior NotImplementedBehavior
    //    {
    //        get => _notImplementedBehavior;
    //        set => _notImplementedBehavior = value;
    //    }
    //    static NotImplementedBehavior _notImplementedBehavior;
    //}

    static class ParserDiagnostics  // #CLEANUP 
    {
        public static void ThrowParserException(string message)
        {
            throw new PdfReaderException(message);
        }

        public static void ThrowParserException(string message, Exception innerException)
        {
            throw new PdfReaderException(message, innerException);
        }

        public static void HandleUnexpectedCharacter(char ch, string dump)
        {
            // Hex formatting does not work with type Char. It must be cast to integer.
            string message =
                Invariant($"Unexpected character '0x{(uint)ch:x4}' in PDF stream. The file may be corrupted. ") +
                Invariant($"If you think this is a bug in PDFsharp, please visit {UrlLiterals.LinkToCannotOpenPdfFile} for further information.\n") +
                dump;
            ThrowParserException(message);
        }
        public static void HandleUnexpectedToken(string token, string dump)
        {
            string message =
                Invariant($"Unexpected token '{token}' in PDF stream. The file may be corrupted. ") +
                Invariant($"If you think this is a bug in PDFsharp, please visit {UrlLiterals.LinkToCannotOpenPdfFile} for further information.\n") +
                dump;
            ThrowParserException(message);
        }

        public static void CannotFindEndOfStream(PdfDictionary dict)
        {
            string message = $"Cannot find end of stream in object '{dict.ObjectID}'.";
            ThrowParserException(message);
        }
    }

    static class ContentReaderDiagnostics  // #CLEANUP 
    {
        public static void ThrowContentReaderException(string message)
        {
            throw new ContentReaderException(message);
        }

        public static void ThrowContentReaderException(string message, Exception innerException)
        {
            throw new ContentReaderException(message, innerException);
        }

        public static void ThrowNumberOutOfIntegerRange(long value)
        {
            string message = String.Format(CultureInfo.InvariantCulture, "Number '{0}' out of integer range.", value);
            ThrowContentReaderException(message);
        }

        public static void HandleUnexpectedCharacter(char ch)
        {
            string message = String.Format(CultureInfo.InvariantCulture,
                "Unexpected character '0x{0:x4}' in content stream. The stream may be corrupted or the feature is not implemented. " +
                $"If you think this is a bug in PDFsharp, please visit {UrlLiterals.LinkToCannotOpenPdfFile} for further information.\n", (int)ch);
            ThrowContentReaderException(message);
        }
    }
}
