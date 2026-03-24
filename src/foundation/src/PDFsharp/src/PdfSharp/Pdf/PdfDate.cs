// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.IO;

// v7.0.0 Ready

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct date value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfDate : PdfPrimitive
    {
        // Reference 2.0: 7.9.4 Dates / Page 717
        // See also: https://learn.microsoft.com/en-us/dotnet/standard/datetime/converting-between-datetime-and-offset

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// The Value property becomes null.
        /// </summary>
        public PdfDate()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// If the string is not a valid PDF date the Value property becomes null.
        /// </summary>
        public PdfDate(string date)
        {
            if (!Parser.TryParseDate(date, out var value))
            {
                var message = $"""The string "{date}" is not a valid PDF date.""";
                PdfSharpLogHost.PdfReadingLogger.LogError(message);
            }
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        [Obsolete("Use a DateTimeOffset for creating a PdfDate.")]
        public PdfDate(DateTime value)
        {
            // Let .NET do the conversion. We do not try to ‘optimize’ it.
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDate"/> class.
        /// </summary>
        public PdfDate(DateTimeOffset value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value as DateTimeOffset.
        /// Use ToString to get the PDF date string.
        /// The value is null if the class was not initialized with a valid PDF date.
        /// </summary>
        public DateTimeOffset? Value { get; }

        /// <summary>
        /// Returns the value as a PDF date formatted string,
        /// or the empty string if no value was set.
        /// The string looks like (D:YYYYMMDDHHmmSSOHH'mm').
        /// </summary>
        public override string ToString()
        {
            return ToPdfString(Value);

            // #DELETE 25-12-31
            //////#if old_code 
            ////            if (Value == null)
            ////                return "";

            ////            // TO/DO clean this all up #US270 / use DateTimeOffset instead of Date/Time
            ////#if DEBUG_
            ////            // TO/DO https://stackoverflow.com/questions/65004352/c-sharp-datetime-tostring-with-zzz-breaks-in-dotnet-framework-but-not-in-dotnet
            ////            var kind = Value.Kind;
            ////            // TO/DO Here we have a difference between .NET Framework and .NET
            ////            // See also SpecifyLocalDateTimeKindIfUnspecified
            ////            // Date/Time today = Date/Time.UtcNow;
            ////            // Console.WriteLine(String.Format("{0:%z}, {0:zz}, {0:zzz}", today));
            ////            // // Displays -7, -07, -07:00 on .NET Framework
            ////            // // Displays +0, +00, +00:00 on .NET Core and .NET 5+
            ////#endif

            ////            // Fix bug in .NET Framework:
            ////            // offset is always "00'00" in UTC (that’s why it’s called UTC).
            ////            string offset = Value.Kind == DateTimeKind.Utc
            ////                ? "00'00"
            ////                : Value.ToString("zzz").Replace(':', '\'');
            ////            // The trailing ‘'’ was part of the Syntax in PDF 1.x.
            ////            // Since PDF 2.0 it is not part of the syntax anymore.
            ////            // return $"D:{Value:yyyyMMddHHmmss}{offset}'";

            ////            // Page 118ff
            ////            // NOTE 1 A date string can be any valid PDF string object as described in 7.3.4, "String objects".
            ////            //        The description above relates to the text string value after appropriate processing.
            ////            // NOTE 2 PDF versions up to and including 1.7 defined a date string to include a terminating apostrophe.
            ////            //        PDF processors are recommended to accept date strings that still follow that convention.
            ////            // NOTE 3 The letter Z can optionally be followed by hour and minute offsets, which are zero in this case.

            ////            // We keep trailing ‘'’ and write ‘Z00'00’ instead of ‘Z’ for maximum compatibility.
            ////            var result = Value.Kind == DateTimeKind.Utc
            ////                     ? $"D:{Value:yyyyMMddHHmmss}Z00'00'"  // TO/DO verify: "Z" or "Z00'00"?
            ////                     : $"D:{Value:yyyyMMddHHmmss}{Value.ToString("zzz").Replace(':', '\'')}'";
            ////            return result;
            ////#endif
        }

        /// <summary>
        /// Converts a DateTimeOffset into a PDF date string.
        /// Returns the empty string if value is null.
        /// </summary>
        static string ToPdfString(DateTimeOffset? dateTimeOffset)
        {
            var result = "";
            if (dateTimeOffset != null)
            {
                result = $"D:{dateTimeOffset:yyyyMMddHHmmss}";
                if (dateTimeOffset.Value.Offset == TimeSpan.Zero)
                    result += "Z";
                else
                    // We keep trailing ‘'’ if offset is not 0 for maximum compatibility.
                    result += dateTimeOffset.Value.ToString("zzz").Replace(':', '\'') + '\'';
            }
            return result;
        }

        /// <summary>
        /// Writes the value in the PDF date format.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteDocString(ToString());
        }
    }
}
