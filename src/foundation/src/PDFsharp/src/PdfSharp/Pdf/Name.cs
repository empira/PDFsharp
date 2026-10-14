// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Internal;

// v7.0 Ready

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Implements a PDF name as a value type.
    /// </summary>
    public readonly struct Name : IComparer<Name>, IEquatable<Name>
    {
        // Reference 2.0: 7.3.5  Name objects / Page 27

        // ReSharper disable GrammarMistakeInComment

        // “Literal name” and “resulting name” are defined in PDF specs.
        // See Table 4 — Examples of literal names on page 28.
        // 
        // Canonical name is what we use during programming
        // 
        // Literal name    | Resulting name   |                    << Terms used by Adobe/ISO
        // Literal name    | (not used)       | Canonical name     << Terms used in PDFsharp
        // ----------------+------------------+------------------
        // /Name1          | Name1            | /Name1
        // /Lime#20Green   | Lime Green       | /Lime Green
        // /Mambo#20#235   | Mambo #5         | /Mambo #5

        // In one sentence:
        // In PDFsharp, the canonical name is the resulting name prefixed with a '/'.
        // This is compatible with the previous implementation of PdfName.

        // ReSharper restore GrammarMistakeInComment

        // Literal:   What’s written into or coming from a PDF file.
        // Canonical: What’s in the .NET string and used in C# code.
        // Regular:   Literal and canonical name are identical because
        //            there are characters that are escaped.
        //
        // The term resulting name used by PDF specification is identical with 
        // the canonical name, except the resulting name has no slash but the
        // canonical name has one.

        // What happens if a '\0' character appears in a canonical name?
        // PDFsharp skips this character.
        // What happens if a "#00", "#0", or "#xx" character appears in a literal name?
        // PDFsharp skips this character.

        /// <summary>
        /// Initializes a new instance of the <see cref="Name"/> struct
        /// with the empty name "/".
        /// </summary>
        public Name()
        {
            _literalName = _canonicalName = "/";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Name"/> struct
        /// with a canonical name. The string must start with a  "/".
        /// </summary>
        public Name(string canonicalName)
        {
            if (String.IsNullOrEmpty(canonicalName))
                throw new ArgumentException("A PDF name must not be null or empty.", nameof(canonicalName));

            if (canonicalName[0] != '/')
                throw new ArgumentException("A PDF name must start with a '/'.");

            _literalName = BuildLiteralName(canonicalName);
            _canonicalName = canonicalName;
        }

        internal Name(string literalName, string canonicalName)
        {
            _literalName = literalName;
            _canonicalName = canonicalName;
        }

        /// <summary>
        /// Creates a <see cref="Name"/> object from an enum value.
        /// </summary>
        public static Name FromEnum<T>(T value) where T : Enum
            => new('/' + value.ToString());

        /// <summary>
        /// Gets the literal value of the name.
        /// That is the form which is written in a PDF file.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public string LiteralValue => _literalName;

        /// <summary>
        /// Gets the canonical value of the name.
        /// That is the form used as keys in PdfDictionary.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public string Value => _canonicalName;

        public bool IsEmpty => _canonicalName.Length == 1;

        /// <summary>
        /// Ensures that the name is formally correct.
        /// It must be a non-empty string starting with a '/'.
        /// </summary>
        public static void EnsureName(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (name[0] != '/')
                throw new ArgumentException($"The PDF name '{name}' must start with a slash ('/').");
        }

        /// <summary>
        /// Converts a string into a canonical name by adding a '/'
        /// as prefix to the string.
        /// If the specified string already starts with a '/'
        /// no action is taken.
        /// </summary>
        public static string MakeName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return "/";
            if (name[0] != '/')
                return String.Concat('/', name);
            return name;
        }

        /// <summary>
        /// Removes the slash from a string, that is needed at the beginning of a PDF name.
        /// </summary>
        public static string RemoveSlash(string value)
        {
            return value.Length == 0 || value[0] != '/'
                ? value
                : value[1..];
        }
        /// <summary>
        /// Creates a new instance of Name from a literal name.
        /// </summary>
        public static Name FromLiteralName(string literalName)
        {
            if (String.IsNullOrEmpty(literalName))
                throw new ArgumentNullException(nameof(literalName));

            if (literalName[0] != '/')
                throw new ArgumentException($"Literal name '{literalName}' must start with a slash ('/').");

            var (literal, canonical) = BuildCanonicalName(literalName);
            return new(literal, canonical);
        }

        /// <summary>
        /// Creates a new instance of Name from a canonical name.
        /// </summary>
        public static Name FromCanonicalName(string canonicalName)
        {
            if (String.IsNullOrEmpty(canonicalName))
                throw new ArgumentNullException(nameof(canonicalName));

            if (canonicalName[0] != '/')
                throw new ArgumentException($"Canonical name '{canonicalName}' must start with a slash ('/').");

            return new(BuildLiteralName(canonicalName), canonicalName);
        }

        public static readonly Name Empty = new();

        static string BuildLiteralName(string canonicalName)
        {
            // Case: The canonical name is defined and the literal name is created.
            // ISSUE: What if we get a '\0'? -> ignore it


            Debug.Assert(!String.IsNullOrEmpty(canonicalName) && canonicalName[0] == '/');

            // The specification says:
            // “Regular characters that are outside the range EXCLAMATION MARK(21h) (!) to
            // TILDE (7Eh) (~) should be written using the hexadecimal notation.”
            // and
            // “
            // a) A NUMBER SIGN (23h) (#) in a name shall be written by using its 2-digit hexadecimal
            //    code (23), preceded by the NUMBER SIGN.
            // b) Any character in a name that is a regular character (other than NUMBER SIGN) shall
            //    be written as itself or by using its 2-digit hexadecimal code, preceded by the
            //    NUMBER SIGN.
            // c) Any character that is not a regular character shall be written using its 2-digit
            //    hexadecimal code, preceded by the NUMBER SIGN only.
            // ”
            // PDFsharp escapes the 10 regular characters that are used as delimiters defined in
            // “Table 2 — Delimiter characters” on page 23.

            // Step 1
            // Encode to raw string using UTF-8 encoding if any char is larger than 126.
            // 127 [DEL] is not a valid value and is also encoded.
            string name = canonicalName;
            int length = name.Length;
            for (int idx = 1; idx < length; idx++)
            {
                char ch = name[idx];
                if (ch == '\0')
                {
                    // PDFsharp skips null characters.
                    idx++;
                    continue;
                }
                if (ch >= 127)
                {
                    // Case: First non-ASCII character found - convert whole string to raw UTF-8.

                    // Interpret name as Unicode and get the bytes.
                    var bytes = Encoding.UTF8.GetBytes(name);
                    // Convert bytes into a PDFsharp raw string.
                    name = PdfEncoders.RawEncoding.GetString(bytes);
                    break;
                }
            }

            // Step 2
            // Escape characters/bytes not allowed in PDF names.
            // Avoid allocation of StringBuilder if name is regular.
            StringBuilder? sb = null;
            length = name.Length;
            for (int idx = 1; idx < length; idx++)
            {
                var ch = name[idx];
                Debug.Assert(ch < 256);
                // Must escape this character/byte?
                if (ch switch
                {
                    < '!' or > '~' => true,
                    '#' => true,  // PDF name escaping
                                  // Reference 2.0: 7.1  Table 2 — Delimiter characters / Page 23
                    '(' => true,  // PDF string delimiter
                    ')' => true,  //        "
                    '<' => true,  // PDF dictionary delimiter
                    '>' => true,  //        "
                    '[' => true,  // PDF array delimiter
                    ']' => true,  //        "
                    '{' => true,  // Type 4 PostScript calculator functions
                    '}' => true,  //        "
                    '/' => true,  // PDF names delimiter
                    '%' => true,  // PDF comments delimiter
                    _ => false
                })
                {
                    sb ??= new(name[..idx]);
                    sb.Append('#');
                    byte hi = (byte)((ch >> 4) + '0');
                    byte lo = (byte)((ch & 0xF) + '0');
                    sb.Append((char)(hi < ':' ? hi : hi + ('A' - ':')));
                    sb.Append((char)(lo < ':' ? lo : lo + ('A' - ':')));
                }
                else
                {
                    // No string builder means no escaping needed yet.
                    sb?.Append(ch);
                }
            }
            // String builder is null if name is regular.
            return sb?.ToString() ?? name;
        }

        static (string Literal, string Canonical) BuildCanonicalName(string literalName)
        {
            // ISSUE: What if we get a '#00'? Solved: ignore.

            // Case: The literal name is defined and the canonical name is created.

            Debug.Assert(true);  // Dummy statement to prevent ill formatted code because of label.
        TryAgain:
            string name = literalName;
            // Step 1
            // Unescape all characters/bytes.
            // Avoid allocation of StringBuilder if name is regular.
            StringBuilder? sb = null;
            int length = name.Length;
            bool illegalLiteralCharacterFound = false;
            for (int idx = 1; idx < length; idx++)
            {
                var ch = name[idx];
                if (ch == '#')
                {
                    sb ??= new(name[..idx]);

                    // Check the bs stuff.
                    if (idx + 2 >= length)
                    {
                        // Just add some filler and don’t care.
                        literalName += idx + 1 == length ? "00" : '0';
                        goto TryAgain;
                    }

                    char hi = name[++idx];
                    char lo = name[++idx];
                    ch = (char)((hi switch
                    {
                        >= '0' and <= '9' => hi - '0',
                        >= 'A' and <= 'F' => hi - ('A' - 10), // Without parenthesis the expressions are not
                        >= 'a' and <= 'f' => hi - ('a' - 10), // optimized as constant expressions in IL.
                        _ => LogError(hi)
                    } << 4) + lo switch
                    {
                        >= '0' and <= '9' => lo - '0',
                        >= 'A' and <= 'F' => lo - ('A' - 10),
                        >= 'a' and <= 'f' => lo - ('a' - 10),
                        _ => LogError(lo)
                    });
                    if (ch == '\0')
                    {
                        PdfSharpLogHost.Logger.LogError("Name '{name}' contains illegal '#' sequence that evaluates to '\\0' and is skipped.", name);
                        illegalLiteralCharacterFound = true;
                        continue;
                    }
                    static int LogError(int ch)
                    {
                        PdfSharpLogHost.Logger.LogError("Name contains illegal character '{char}' in escape sequence.", ch);
                        return 0;
                    }
                    sb.Append(ch);
                }
                else
                {
                    // What if a literal name contains illegal characters?
                    if (ch switch
                    {
                        < '!' or > '~' => true,
                        // Already checked: '#' => true,  // PDF name escaping
                        // Reference 2.0: 7.1  Table 2 — Delimiter characters / Page 23
                        '(' => true, // PDF string delimiter
                        ')' => true, //        "
                        '<' => true, // PDF dictionary delimiter
                        '>' => true, //        "
                        '[' => true, // PDF array delimiter
                        ']' => true, //        "
                        '{' => true, // Type 4 PostScript calculator functions
                        '}' => true, //        "
                        '/' => true, // PDF names delimiter
                        '%' => true, // PDF comments delimiter
                        _ => false
                    })
                    {
                        // The literal name contains at least one illegal character.
                        illegalLiteralCharacterFound = true;
                        sb ??= new(name[..idx]);
                        // Append the UTF-8 bytes of the illegal character as a raw string.
                        sb.Append(PdfEncoders.RawEncoding.GetString(Encoding.UTF8.GetBytes([ch])));
                    }
                    else
                    {
                        sb?.Append(ch);
                    }
                }
            }

            // Step 2
            // If a StringBuilder was created, at least one either escaped or an illegal character was
            // found. As the PDF specs recommend, we assume that the string is UTF-8 encoded.
            // If no string builder was created, name is regular and therefore always legal.
            if (sb != null)
            {
                // Convert raw name into a Unicode string using UTF-8 decoding. This is our canonical name.
                name = Encoding.UTF8.GetString(PdfEncoders.RawEncoding.GetBytes(sb.ToString()));
            }

            if (illegalLiteralCharacterFound)
            {
                // Rebuild literal name because it was not a legal literal one.
                literalName = BuildLiteralName(name);
            }
            return (literalName, name);
        }

        public bool Equals(Name other)
            => Comparer.Compare(this, other) == 0;

        public override bool Equals(object? obj)
        {
            if (obj is Name name)
                return Comparer.Compare(this, name) == 0;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_literalName.GetHashCode() * 397) ^ _canonicalName.GetHashCode();
            }
        }

        public static implicit operator Name(string name) => new(name);
        public static implicit operator String(Name name) => name.Value;

        public static bool operator ==(Name l, Name r)
            => Comparer.Compare(l, r) == 0;

        public static bool operator !=(Name l, Name r)
            => !(l == r);

        /// <summary>
        /// Compares two names.
        /// </summary>
        public int Compare(Name l, Name r)
            => Comparer.Compare(l, r);

        /// <summary>
        /// Gets the comparer for this type.
        /// </summary>
        public static NameComparer Comparer => _comparer ??= new();

        /// <summary>
        /// Implements a comparer that compares Name objects.
        /// </summary>
        public class NameComparer : IComparer<Name?>
        {
            /// <summary>
            /// Compares two names and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            public int Compare(Name? l, Name? r)
            {
                // Compare the canonical name.
                if (l != null)
                {
                    return r != null
                        ? String.Compare(l.Value.Value, r.Value.Value, StringComparison.Ordinal)
                        : 1;
                }
                return r == null ? 0 : -1;
            }
        }

        // Recall that Name is a value type. Therefore, it makes no sense to
        // lazy evaluate one of the two strings when the other was specified.

        /// <summary>
        /// The literal name with escaped characters.
        /// </summary>
        readonly string _literalName;

        /// <summary>
        /// The resulting name with solidus.
        /// </summary>
        readonly string _canonicalName;

        static NameComparer? _comparer;
    }
}
