// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

// v7.0.0 Ready

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a direct PDF name object.
    /// </summary>
    [DebuggerDisplay("({" + nameof(Value) + "})")]
    public sealed class PdfName : PdfPrimitive
    {
        // Reference 2.0: 7.3.5  Name objects / Page 27

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfName"/> class.
        /// </summary>
        public PdfName()
        {
            Name = Name.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfName"/> class.
        /// </summary>
        public PdfName(Name name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfName"/> class.
        /// Parameter value always must start with a '/'.
        /// </summary>
        public PdfName(string value)
        {
            Name = Name.FromCanonicalName(value);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this name.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is PdfName pdfName)
                return Name.Equals(pdfName.Name);
            return Value.Equals(obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Gets the name as a canonical name string.
        /// </summary>
        public string Value => Name.Value;

        /// <summary>
        /// Gets the underlying Name object.
        /// </summary>
        public Name Name { get; }

        /// <summary>
        /// Returns the name. The string always begins with a slash.
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Determines whether the specified name and string are equal.
        /// </summary>
        public static bool operator ==(PdfName? name, string? str)
        {
            if (name is null)
                return str is null;

            return name.Value == str;
        }

        /// <summary>
        /// Determines whether the specified name and string are not equal.
        /// </summary>
        public static bool operator !=(PdfName? name, string? str)
            => !(name == str);

        /// <summary>
        /// Gets an empty name.
        /// </summary>
        public static PdfName Empty => _empty ??= new();

        static PdfName? _empty;

        /// <summary>
        /// Adds the slash that is needed at the beginning of a PDFName.
        /// </summary>
        [Obsolete("Use Name.MakeName")]
        public static string AddSlash(string value)
            => Name.MakeName(value);

        /// <summary>
        /// Removes the slash that is needed at the beginning of a PDFName.
        /// </summary>
        [Obsolete("Use Name.RemoveSlash")]
        public static string RemoveSlash(string value) 
            => Name.RemoveSlash(value);

        /// <summary>
        /// Gets a PdfName from a string. The string must not start with a slash.
        /// </summary>
        public static PdfName FromString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return Empty;

            if (value[0] == '/')
                throw new ArgumentException($"String '{value}' must not start with a slash.");

            return new('/' + value);
        }

        /// <summary>
        /// Writes the name including the leading slash.
        /// </summary>
        internal override void WriteObject(PdfWriter writer) 
            => writer.Write(this);

        /// <summary>
        /// Gets the comparer for this type.
        /// </summary>
        public static PdfNameComparer Comparer => _nameComparer ??= new();

        static PdfNameComparer? _nameComparer;

        /// <summary>
        /// Implements a comparer that compares PdfName objects.
        /// </summary>
        public class PdfNameComparer : IComparer<PdfName>
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
                        return String.Compare(l.Value, r.Value, StringComparison.Ordinal);
                    return 1;
                }
                if (r != null)
                    return -1;
                return 0;
            }
        }
    }
}
