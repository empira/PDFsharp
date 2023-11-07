// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF name value.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfName : PdfItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfName"/> class.
        /// </summary>
        public PdfName()
        {
            _value = "/";  // Empty name.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfName"/> class.
        /// Parameter value always must start with a '/'.
        /// </summary>
        public PdfName(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length == 0 || value[0] != '/')
                throw new ArgumentException(PSSR.NameMustStartWithSlash);

            _value = value;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this name.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is PdfName pdfName)
                return _value.Equals(pdfName._value);
            return _value.Equals(obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() 
            => _value.GetHashCode();

        /// <summary>
        /// Gets the name as a string.
        /// </summary>
        public string Value => _value;

        readonly string _value;

        /// <summary>
        /// Returns the name. The string always begins with a slash.
        /// </summary>
        public override string ToString() => _value;

        /// <summary>
        /// Determines whether the specified name and string are equal.
        /// </summary>
        public static bool operator ==(PdfName? name, string? str)  // BUG TODO check all operator ==
        {
            if (name is null)
                return str is null;

            return name._value == str;
        }

        /// <summary>
        /// Determines whether the specified name and string are not equal.
        /// </summary>
        public static bool operator !=(PdfName? name, string? str)
            => !(name == str);

        /// <summary>
        /// Represents the empty name.
        /// </summary>
        public static readonly PdfName Empty = new PdfName("/");

        /// <summary>
        /// Adds the slash to a string, that is needed at the beginning of a PDFName string.
        /// </summary>
        public static string AddSlash(string value) // TODO PDFsharp6: Naming. StL: WithSlash?
        {
            if (value.Length == 0)
                return "/";

            return value[0] != '/' ? $"/{value}" : value;
        }

        /// <summary>
        /// Removes the slash from a string, that is needed at the beginning of a PDFName string.
        /// </summary>
        public static string RemoveSlash(string value) // TODO PDFsharp6: Naming. StL: WithoutSlash?
        {
            if (value.Length == 0 || value[0] != '/')
                return value;

            return value.TrimStart('/');
        }

        /// <summary>
        /// Writes the name including the leading slash.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            // TODO: what if Unicode character are part of the name?
            // TODO: 7.3.5 Name objects: "In such situations, the sequence of bytes making up the name object should be interpreted according to UTF-8, a variable-length byte-encoded representation of Unicode in which the printable ASCII characters have the same representations as in ASCII. This enables a name object to represent text virtually in any natural language, subject to the implementation limit on the length of a name."
            writer.Write(this);
        }

        /// <summary>
        /// Gets the comparer for this type.
        /// </summary>
        public static PdfXNameComparer Comparer => new PdfXNameComparer();

        /// <summary>
        /// Implements a comparer that compares PdfName objects.
        /// </summary>
        public class PdfXNameComparer : IComparer<PdfName>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="l">The first object to compare.</param>
            /// <param name="r">The second object to compare.</param>
            public int Compare(PdfName? l, PdfName? r)
            {
                if (l != null)
                {
                    if (r != null)
                        return String.Compare(l._value, r._value, StringComparison.Ordinal);
                    return -1;
                }
                if (r != null)
                    return 1;
                return 0;
            }
        }
    }
}
