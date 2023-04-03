// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Security.Encryption
{
    /// <summary>
    /// Implements the SASLprep profile (RFC4013) of the stringprep algorithm (RFC3454) for processing strings.
    /// SASLprep Documentation:
    /// SASLprep: https://www.rfc-editor.org/rfc/rfc4013
    /// stringprep: https://www.rfc-editor.org/rfc/rfc3454
    /// UAX15 (Unicode Normalization Forms): https://www.unicode.org/reports/tr15/tr15-22.html
    /// </summary>
    class SASLprep
    {
        /// <summary>
        /// Processes the string with the SASLprep profile (RFC4013) of the stringprep algorithm (RFC3454).
        /// As defined for preparing "stored strings", unassigned code points are prohibited.
        /// </summary>
        public static string PrepareStoredString(string str)
        {
            return Prepare(str, false);
        }

        /// <summary>
        /// Processes the string with the SASLprep profile (RFC4013) of the stringprep algorithm (RFC3454).
        /// As defined for preparing "queries", unassigned code points are allowed.
        /// </summary>
        public static string PrepareQuery(string str)
        {
            return Prepare(str, true);
        }

        static string Prepare(string str, bool allowUnassignedCodepoints)
        {
            // Step 1. Map: SASLprep "2.1. Mapping"
            var strBuilder = new StringBuilder();
            foreach (var c in str)
            {
                // stringprep "C.1.2 Non-ASCII space characters" mapped to space.
                if (IsNonAsciiSpace(c))
                    strBuilder.Append(' ');
                // stringprep "B.1 Commonly mapped to nothing" mapped to nothing.
                else if (!IsCommonlyMappedToNothing(c))
                    strBuilder.Append(c);
            }
            str = strBuilder.ToString();


            // Step 2. Normalize: SASLprep "2.2. Normalization"
            // Normalize with normalization form KC (stringprep "4. Normalization" and UAX15) 
            var normalized = str.Normalize(NormalizationForm.FormKC);


            // Initialize some variables for the next steps.
            var normalizedLength = normalized.Length;
            var containsRandALCat = false;
            var containsLCat = false;
            var firstCodepointIsRandALCat = false;
            var lastCodepointIsRandALCat = false;

            // The next steps need to handle with Unicode code points instead of chars. Iterate through the string...
            for (var i = 0; i < normalizedLength;)
            {
                // ... and get its code points.
                var c = normalized[i];
                int codepoint;
                int charCount;
                if (Char.IsHighSurrogate(c))
                {
                    // Found a codepoint defined by two chars, the high surrogate and the following low surrogate.
                    codepoint = Char.ConvertToUtf32(c, normalized[c + 1]);
                    charCount = 2;
                }
                else
                {
                    // Found a codepoint defined by one Char.
                    codepoint = c;
                    charCount = 1;
                }


                // Step 3. Prohibit: SASLprep "2.3. Prohibited Output".
                if (IsProhibited(codepoint))
                    throw TH.ArgumentException_SASLprepProhibitedCharacter(codepoint, i);

                // Check for unassigned code points, if desired.
                if (!allowUnassignedCodepoints && IsUnassignedCodePoint(codepoint))
                    throw TH.ArgumentException_SASLprepProhibitedUnassignedCodepoint(codepoint, i);


                // Step 4. Check bidi: SASLprep "2.4. Bidirectional Characters" and stringprep "6. Bidirectional Characters"
                // Check if the current code point is RandAlCat (stringprep "D.1 Characters with bidirectional property "R" or "AL")
                // or LCat (stringprep "D.2 Characters with bidirectional property "L").
                // To check bidirectional requirements determine if the string contains RandAlCat or LCat code points
                // and if the first and the last code point is RandAlCat.
                lastCodepointIsRandALCat = IsRandAlCat(codepoint);
                if (i == 0)
                    firstCodepointIsRandALCat = lastCodepointIsRandALCat;

                containsRandALCat |= lastCodepointIsRandALCat;

                containsLCat |= IsLCat(codepoint);


                // Increase i by the char count of the current code point and precede.
                i += charCount;
            }


            // Finally do check for step 4: stringprep "6. Bidirectional Characters".
            if (containsRandALCat)
            {
                // 1) The characters in section 5.8 MUST be prohibited.
                // Characters in stringprep "5.8 Change display properties or are deprecated" are already prohibited via
                // SASLPrep "2.3. Prohibited Output" -> stringprep "C.8 Change display properties or are deprecated".

                // 2) If a string contains any RandALCat character, the string MUST NOT contain any LCat character.
                if (containsLCat)
                    throw TH.ArgumentException_SASLprepRandALCatAndLCatCharacters();

                // 3) If a string contains any RandALCat character, a RandALCat character MUST be the first character of the string,
                // and a RandALCat character MUST be the last character of the string.
                if (!firstCodepointIsRandALCat || !lastCodepointIsRandALCat)
                    throw TH.ArgumentException_SASLprepRandALCatButFirstOrLastDivergent();
            }

            return normalized;
        }



        /// <summary>
        /// Checks if a char is part of stringprep "C.1.2 Non-ASCII space characters".
        /// </summary>
        static bool IsNonAsciiSpace(char c)
        {
            return c is '\u00A0' 
                or '\u1680' 
                or >= '\u2000' and <= '\u200B' 
                or '\u202F' 
                or '\u205F' 
                or '\u3000';
        }

        /// <summary>
        /// Checks if a char is part of stringprep "B.1 Commonly mapped to nothing".
        /// </summary>
        static bool IsCommonlyMappedToNothing(char c)
        {
            return c is '\u00AD' 
                or '\u034F' 
                or '\u1806' 
                or >= '\u180B' and <= '\u180D' 
                or >= '\u200B' and <= '\u200D' 
                or '\u2060' 
                or >= '\uFE00' and <= '\uFE0F' 
                or '\uFEFF';
        }

        /// <summary>
        /// Checks if a Unicode codepoint is prohibited in SASLprep.
        /// </summary>
        static bool IsProhibited(int codepoint)
        {
            return IsNonAsciiSpace((char)codepoint)
                   || IsAsciiControlCharacter((char)codepoint)
                   || IsNonAsciiControlCharacter(codepoint)
                   || IsPrivateUseCharacter(codepoint)
                   || IsNonCharacterCodePoint(codepoint)
                   || IsSurrogateCodePoint(codepoint)
                   || IsInappropriateForPlainTextCharacter(codepoint)
                   || IsInappropriateForCanonicalRepresentationCharacter(codepoint)
                   || IsChangeDisplayPropertyDeprecatedCharacter(codepoint)
                   || IsTaggingCharacter(codepoint);
        }

        /// <summary>
        /// Checks if a char is part of stringprep "C.2.1 ASCII control characters".
        /// </summary>
        static bool IsAsciiControlCharacter(char ch)
        {
            return ch is >= '\u0000' and <= '\u001F' 
                or '\u007F';
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.2.2 Non-ASCII control characters".
        /// </summary>
        static bool IsNonAsciiControlCharacter(int codepoint)
        {
            return codepoint is >= 0x0080 and <= 0x009F
                or 0x06DD
                or 0x070F
                or 0x180E
                or 0x200C
                or 0x200D
                or 0x2028
                or 0x2029
                or >= 0x2060 and <= 0x2063
                or >= 0x206A and <= 0x206F
                or 0xFEFF
                or >= 0xFFF9 and <= 0xFFFC
                or >= 0x1D173 and <= 0x1D17A;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.3 Private use".
        /// </summary>
        static bool IsPrivateUseCharacter(int codepoint)
        {
            return codepoint is >= 0xE000 and <= 0xF8FF 
                or >= 0xF0000 and <= 0xFFFFD 
                or >= 0x100000 and <= 0x10FFFD;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.4 Non-character code points".
        /// </summary>
        static bool IsNonCharacterCodePoint(int codepoint)
        {
            return codepoint is >= 0xFDD0 and <= 0xFDEF
                or >= 0xFFFE and <= 0xFFFF
                or >= 0x1FFFE and <= 0x1FFFF
                or >= 0x2FFFE and <= 0x2FFFF
                or >= 0x3FFFE and <= 0x3FFFF
                or >= 0x4FFFE and <= 0x4FFFF
                or >= 0x5FFFE and <= 0x5FFFF
                or >= 0x6FFFE and <= 0x6FFFF
                or >= 0x7FFFE and <= 0x7FFFF
                or >= 0x8FFFE and <= 0x8FFFF
                or >= 0x9FFFE and <= 0x9FFFF
                or >= 0xAFFFE and <= 0xAFFFF
                or >= 0xBFFFE and <= 0xBFFFF
                or >= 0xCFFFE and <= 0xCFFFF
                or >= 0xDFFFE and <= 0xDFFFF
                or >= 0xEFFFE and <= 0xEFFFF
                or >= 0xFFFFE and <= 0xFFFFF
                or >= 0x10FFFE and <= 0x10FFFF;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.5 Surrogate codes".
        /// </summary>
        static bool IsSurrogateCodePoint(int codepoint)
        {
            return codepoint is >= 0xD800 and <= 0xDFFF;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.6 Inappropriate for plain text".
        /// </summary>
        static bool IsInappropriateForPlainTextCharacter(int codepoint)
        {
            return codepoint is >= 0xFFF9 and <= 0xFFFD;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.7 Inappropriate for canonical representation".
        /// </summary>
        static bool IsInappropriateForCanonicalRepresentationCharacter(int codepoint)
        {
            return codepoint is >= 0x2FF0 and <= 0x2FFB;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.8 Change display properties or are deprecated".
        /// </summary>
        static bool IsChangeDisplayPropertyDeprecatedCharacter(int codepoint)
        {
            return codepoint is 0x0340
                or 0x0341
                or 0x200E
                or 0x200F
                or >= 0x202A and <= 0x202E
                or >= 0x206A and <= 0x206F;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "C.9 Tagging characters".
        /// </summary>
        static bool IsTaggingCharacter(int codepoint)
        {
            return codepoint is 0xE0001 
                or >= 0xE0020 and <= 0xE007F;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "D.1 Characters with bidirectional property "R" or "AL"".
        /// </summary>
        static bool IsRandAlCat(int codepoint)
        {
            return codepoint
                is 0x05BE
                or 0x05C0
                or 0x05C3
                or >= 0x05D0 and <= 0x05EA
                or >= 0x05F0 and <= 0x05F4
                or 0x061B
                or 0x061F
                or >= 0x0621 and <= 0x063A
                or >= 0x0640 and <= 0x064A
                or >= 0x066D and <= 0x066F
                or >= 0x0671 and <= 0x06D5
                or 0x06DD
                or >= 0x06E5 and <= 0x06E6
                or >= 0x06FA and <= 0x06FE
                or >= 0x0700 and <= 0x070D
                or 0x0710
                or >= 0x0712 and <= 0x072C
                or >= 0x0780 and <= 0x07A5
                or 0x07B1
                or 0x200F
                or 0xFB1D
                or >= 0xFB1F and <= 0xFB28
                or >= 0xFB2A and <= 0xFB36
                or >= 0xFB38 and <= 0xFB3C
                or 0xFB3E
                or >= 0xFB40 and <= 0xFB41
                or >= 0xFB43 and <= 0xFB44
                or >= 0xFB46 and <= 0xFBB1
                or >= 0xFBD3 and <= 0xFD3D
                or >= 0xFD50 and <= 0xFD8F
                or >= 0xFD92 and <= 0xFDC7
                or >= 0xFDF0 and <= 0xFDFC
                or >= 0xFE70 and <= 0xFE74
                or >= 0xFE76 and <= 0xFEFC;
        }

        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "D.2 Characters with bidirectional property "L"".
        /// </summary>
        static bool IsLCat(int codepoint)
        {
            return codepoint
                is >= 0x0041 and <= 0x005A
                or >= 0x0061 and <= 0x007A
                or 0x00AA
                or 0x00B5
                or 0x00BA
                or >= 0x00C0 and <= 0x00D6
                or >= 0x00D8 and <= 0x00F6
                or >= 0x00F8 and <= 0x0220
                or >= 0x0222 and <= 0x0233
                or >= 0x0250 and <= 0x02AD
                or >= 0x02B0 and <= 0x02B8
                or >= 0x02BB and <= 0x02C1
                or >= 0x02D0 and <= 0x02D1
                or >= 0x02E0 and <= 0x02E4
                or 0x02EE
                or 0x037A
                or 0x0386
                or >= 0x0388 and <= 0x038A
                or 0x038C
                or >= 0x038E and <= 0x03A1
                or >= 0x03A3 and <= 0x03CE
                or >= 0x03D0 and <= 0x03F5
                or >= 0x0400 and <= 0x0482
                or >= 0x048A and <= 0x04CE
                or >= 0x04D0 and <= 0x04F5
                or >= 0x04F8 and <= 0x04F9
                or >= 0x0500 and <= 0x050F
                or >= 0x0531 and <= 0x0556
                or >= 0x0559 and <= 0x055F
                or >= 0x0561 and <= 0x0587
                or 0x0589
                or 0x0903
                or >= 0x0905 and <= 0x0939
                or >= 0x093D and <= 0x0940
                or >= 0x0949 and <= 0x094C
                or 0x0950
                or >= 0x0958 and <= 0x0961
                or >= 0x0964 and <= 0x0970
                or >= 0x0982 and <= 0x0983
                or >= 0x0985 and <= 0x098C
                or >= 0x098F and <= 0x0990
                or >= 0x0993 and <= 0x09A8
                or >= 0x09AA and <= 0x09B0
                or 0x09B2
                or >= 0x09B6 and <= 0x09B9
                or >= 0x09BE and <= 0x09C0
                or >= 0x09C7 and <= 0x09C8
                or >= 0x09CB and <= 0x09CC
                or 0x09D7
                or >= 0x09DC and <= 0x09DD
                or >= 0x09DF and <= 0x09E1
                or >= 0x09E6 and <= 0x09F1
                or >= 0x09F4 and <= 0x09FA
                or >= 0x0A05 and <= 0x0A0A
                or >= 0x0A0F and <= 0x0A10
                or >= 0x0A13 and <= 0x0A28
                or >= 0x0A2A and <= 0x0A30
                or >= 0x0A32 and <= 0x0A33
                or >= 0x0A35 and <= 0x0A36
                or >= 0x0A38 and <= 0x0A39
                or >= 0x0A3E and <= 0x0A40
                or >= 0x0A59 and <= 0x0A5C
                or 0x0A5E
                or >= 0x0A66 and <= 0x0A6F
                or >= 0x0A72 and <= 0x0A74
                or 0x0A83
                or >= 0x0A85 and <= 0x0A8B
                or 0x0A8D
                or >= 0x0A8F and <= 0x0A91
                or >= 0x0A93 and <= 0x0AA8
                or >= 0x0AAA and <= 0x0AB0
                or >= 0x0AB2 and <= 0x0AB3
                or >= 0x0AB5 and <= 0x0AB9
                or >= 0x0ABD and <= 0x0AC0
                or 0x0AC9
                or >= 0x0ACB and <= 0x0ACC
                or 0x0AD0
                or 0x0AE0
                or >= 0x0AE6 and <= 0x0AEF
                or >= 0x0B02 and <= 0x0B03
                or >= 0x0B05 and <= 0x0B0C
                or >= 0x0B0F and <= 0x0B10
                or >= 0x0B13 and <= 0x0B28
                or >= 0x0B2A and <= 0x0B30
                or >= 0x0B32 and <= 0x0B33
                or >= 0x0B36 and <= 0x0B39
                or >= 0x0B3D and <= 0x0B3E
                or 0x0B40
                or >= 0x0B47 and <= 0x0B48
                or >= 0x0B4B and <= 0x0B4C
                or 0x0B57
                or >= 0x0B5C and <= 0x0B5D
                or >= 0x0B5F and <= 0x0B61
                or >= 0x0B66 and <= 0x0B70
                or 0x0B83
                or >= 0x0B85 and <= 0x0B8A
                or >= 0x0B8E and <= 0x0B90
                or >= 0x0B92 and <= 0x0B95
                or >= 0x0B99 and <= 0x0B9A
                or 0x0B9C
                or >= 0x0B9E and <= 0x0B9F
                or >= 0x0BA3 and <= 0x0BA4
                or >= 0x0BA8 and <= 0x0BAA
                or >= 0x0BAE and <= 0x0BB5
                or >= 0x0BB7 and <= 0x0BB9
                or >= 0x0BBE and <= 0x0BBF
                or >= 0x0BC1 and <= 0x0BC2
                or >= 0x0BC6 and <= 0x0BC8
                or >= 0x0BCA and <= 0x0BCC
                or 0x0BD7
                or >= 0x0BE7 and <= 0x0BF2
                or >= 0x0C01 and <= 0x0C03
                or >= 0x0C05 and <= 0x0C0C
                or >= 0x0C0E and <= 0x0C10
                or >= 0x0C12 and <= 0x0C28
                or >= 0x0C2A and <= 0x0C33
                or >= 0x0C35 and <= 0x0C39
                or >= 0x0C41 and <= 0x0C44
                or >= 0x0C60 and <= 0x0C61
                or >= 0x0C66 and <= 0x0C6F
                or >= 0x0C82 and <= 0x0C83
                or >= 0x0C85 and <= 0x0C8C
                or >= 0x0C8E and <= 0x0C90
                or >= 0x0C92 and <= 0x0CA8
                or >= 0x0CAA and <= 0x0CB3
                or >= 0x0CB5 and <= 0x0CB9
                or 0x0CBE
                or >= 0x0CC0 and <= 0x0CC4
                or >= 0x0CC7 and <= 0x0CC8
                or >= 0x0CCA and <= 0x0CCB
                or >= 0x0CD5 and <= 0x0CD6
                or 0x0CDE
                or >= 0x0CE0 and <= 0x0CE1
                or >= 0x0CE6 and <= 0x0CEF
                or >= 0x0D02 and <= 0x0D03
                or >= 0x0D05 and <= 0x0D0C
                or >= 0x0D0E and <= 0x0D10
                or >= 0x0D12 and <= 0x0D28
                or >= 0x0D2A and <= 0x0D39
                or >= 0x0D3E and <= 0x0D40
                or >= 0x0D46 and <= 0x0D48
                or >= 0x0D4A and <= 0x0D4C
                or 0x0D57
                or >= 0x0D60 and <= 0x0D61
                or >= 0x0D66 and <= 0x0D6F
                or >= 0x0D82 and <= 0x0D83
                or >= 0x0D85 and <= 0x0D96
                or >= 0x0D9A and <= 0x0DB1
                or >= 0x0DB3 and <= 0x0DBB
                or 0x0DBD
                or >= 0x0DC0 and <= 0x0DC6
                or >= 0x0DCF and <= 0x0DD1
                or >= 0x0DD8 and <= 0x0DDF
                or >= 0x0DF2 and <= 0x0DF4
                or >= 0x0E01 and <= 0x0E30
                or >= 0x0E32 and <= 0x0E33
                or >= 0x0E40 and <= 0x0E46
                or >= 0x0E4F and <= 0x0E5B
                or >= 0x0E81 and <= 0x0E82
                or 0x0E84
                or >= 0x0E87 and <= 0x0E88
                or 0x0E8A
                or 0x0E8D
                or >= 0x0E94 and <= 0x0E97
                or >= 0x0E99 and <= 0x0E9F
                or >= 0x0EA1 and <= 0x0EA3
                or 0x0EA5
                or 0x0EA7
                or >= 0x0EAA and <= 0x0EAB
                or >= 0x0EAD and <= 0x0EB0
                or >= 0x0EB2 and <= 0x0EB3
                or 0x0EBD
                or >= 0x0EC0 and <= 0x0EC4
                or 0x0EC6
                or >= 0x0ED0 and <= 0x0ED9
                or >= 0x0EDC and <= 0x0EDD
                or >= 0x0F00 and <= 0x0F17
                or >= 0x0F1A and <= 0x0F34
                or 0x0F36
                or 0x0F38
                or >= 0x0F3E and <= 0x0F47
                or >= 0x0F49 and <= 0x0F6A
                or 0x0F7F
                or 0x0F85
                or >= 0x0F88 and <= 0x0F8B
                or >= 0x0FBE and <= 0x0FC5
                or >= 0x0FC7 and <= 0x0FCC
                or 0x0FCF
                or >= 0x1000 and <= 0x1021
                or >= 0x1023 and <= 0x1027
                or >= 0x1029 and <= 0x102A
                or 0x102C
                or 0x1031
                or 0x1038
                or >= 0x1040 and <= 0x1057
                or >= 0x10A0 and <= 0x10C5
                or >= 0x10D0 and <= 0x10F8
                or 0x10FB
                or >= 0x1100 and <= 0x1159
                or >= 0x115F and <= 0x11A2
                or >= 0x11A8 and <= 0x11F9
                or >= 0x1200 and <= 0x1206
                or >= 0x1208 and <= 0x1246
                or 0x1248
                or >= 0x124A and <= 0x124D
                or >= 0x1250 and <= 0x1256
                or 0x1258
                or >= 0x125A and <= 0x125D
                or >= 0x1260 and <= 0x1286
                or 0x1288
                or >= 0x128A and <= 0x128D
                or >= 0x1290 and <= 0x12AE
                or 0x12B0
                or >= 0x12B2 and <= 0x12B5
                or >= 0x12B8 and <= 0x12BE
                or 0x12C0
                or >= 0x12C2 and <= 0x12C5
                or >= 0x12C8 and <= 0x12CE
                or >= 0x12D0 and <= 0x12D6
                or >= 0x12D8 and <= 0x12EE
                or >= 0x12F0 and <= 0x130E
                or 0x1310
                or >= 0x1312 and <= 0x1315
                or >= 0x1318 and <= 0x131E
                or >= 0x1320 and <= 0x1346
                or >= 0x1348 and <= 0x135A
                or >= 0x1361 and <= 0x137C
                or >= 0x13A0 and <= 0x13F4
                or >= 0x1401 and <= 0x1676
                or >= 0x1681 and <= 0x169A
                or >= 0x16A0 and <= 0x16F0
                or >= 0x1700 and <= 0x170C
                or >= 0x170E and <= 0x1711
                or >= 0x1720 and <= 0x1731
                or >= 0x1735 and <= 0x1736
                or >= 0x1740 and <= 0x1751
                or >= 0x1760 and <= 0x176C
                or >= 0x176E and <= 0x1770
                or >= 0x1780 and <= 0x17B6
                or >= 0x17BE and <= 0x17C5
                or >= 0x17C7 and <= 0x17C8
                or >= 0x17D4 and <= 0x17DA
                or 0x17DC
                or >= 0x17E0 and <= 0x17E9
                or >= 0x1810 and <= 0x1819
                or >= 0x1820 and <= 0x1877
                or >= 0x1880 and <= 0x18A8
                or >= 0x1E00 and <= 0x1E9B
                or >= 0x1EA0 and <= 0x1EF9
                or >= 0x1F00 and <= 0x1F15
                or >= 0x1F18 and <= 0x1F1D
                or >= 0x1F20 and <= 0x1F45
                or >= 0x1F48 and <= 0x1F4D
                or >= 0x1F50 and <= 0x1F57
                or 0x1F59
                or 0x1F5B
                or 0x1F5D
                or >= 0x1F5F and <= 0x1F7D
                or >= 0x1F80 and <= 0x1FB4
                or >= 0x1FB6 and <= 0x1FBC
                or 0x1FBE
                or >= 0x1FC2 and <= 0x1FC4
                or >= 0x1FC6 and <= 0x1FCC
                or >= 0x1FD0 and <= 0x1FD3
                or >= 0x1FD6 and <= 0x1FDB
                or >= 0x1FE0 and <= 0x1FEC
                or >= 0x1FF2 and <= 0x1FF4
                or >= 0x1FF6 and <= 0x1FFC
                or 0x200E
                or 0x2071
                or 0x207F
                or 0x2102
                or 0x2107
                or >= 0x210A and <= 0x2113
                or 0x2115
                or >= 0x2119 and <= 0x211D
                or 0x2124
                or 0x2126
                or 0x2128
                or >= 0x212A and <= 0x212D
                or >= 0x212F and <= 0x2131
                or >= 0x2133 and <= 0x2139
                or >= 0x213D and <= 0x213F
                or >= 0x2145 and <= 0x2149
                or >= 0x2160 and <= 0x2183
                or >= 0x2336 and <= 0x237A
                or 0x2395
                or >= 0x249C and <= 0x24E9
                or >= 0x3005 and <= 0x3007
                or >= 0x3021 and <= 0x3029
                or >= 0x3031 and <= 0x3035
                or >= 0x3038 and <= 0x303C
                or >= 0x3041 and <= 0x3096
                or >= 0x309D and <= 0x309F
                or >= 0x30A1 and <= 0x30FA
                or >= 0x30FC and <= 0x30FF
                or >= 0x3105 and <= 0x312C
                or >= 0x3131 and <= 0x318E
                or >= 0x3190 and <= 0x31B7
                or >= 0x31F0 and <= 0x321C
                or >= 0x3220 and <= 0x3243
                or >= 0x3260 and <= 0x327B
                or >= 0x327F and <= 0x32B0
                or >= 0x32C0 and <= 0x32CB
                or >= 0x32D0 and <= 0x32FE
                or >= 0x3300 and <= 0x3376
                or >= 0x337B and <= 0x33DD
                or >= 0x33E0 and <= 0x33FE
                or >= 0x3400 and <= 0x4DB5
                or >= 0x4E00 and <= 0x9FA5
                or >= 0xA000 and <= 0xA48C
                or >= 0xAC00 and <= 0xD7A3
                or >= 0xD800 and <= 0xFA2D
                or >= 0xFA30 and <= 0xFA6A
                or >= 0xFB00 and <= 0xFB06
                or >= 0xFB13 and <= 0xFB17
                or >= 0xFF21 and <= 0xFF3A
                or >= 0xFF41 and <= 0xFF5A
                or >= 0xFF66 and <= 0xFFBE
                or >= 0xFFC2 and <= 0xFFC7
                or >= 0xFFCA and <= 0xFFCF
                or >= 0xFFD2 and <= 0xFFD7
                or >= 0xFFDA and <= 0xFFDC
                or >= 0x10300 and <= 0x1031E
                or >= 0x10320 and <= 0x10323
                or >= 0x10330 and <= 0x1034A
                or >= 0x10400 and <= 0x10425
                or >= 0x10428 and <= 0x1044D
                or >= 0x1D000 and <= 0x1D0F5
                or >= 0x1D100 and <= 0x1D126
                or >= 0x1D12A and <= 0x1D166
                or >= 0x1D16A and <= 0x1D172
                or >= 0x1D183 and <= 0x1D184
                or >= 0x1D18C and <= 0x1D1A9
                or >= 0x1D1AE and <= 0x1D1DD
                or >= 0x1D400 and <= 0x1D454
                or >= 0x1D456 and <= 0x1D49C
                or >= 0x1D49E and <= 0x1D49F
                or 0x1D4A2
                or >= 0x1D4A5 and <= 0x1D4A6
                or >= 0x1D4A9 and <= 0x1D4AC
                or >= 0x1D4AE and <= 0x1D4B9
                or 0x1D4BB
                or >= 0x1D4BD and <= 0x1D4C0
                or >= 0x1D4C2 and <= 0x1D4C3
                or >= 0x1D4C5 and <= 0x1D505
                or >= 0x1D507 and <= 0x1D50A
                or >= 0x1D50D and <= 0x1D514
                or >= 0x1D516 and <= 0x1D51C
                or >= 0x1D51E and <= 0x1D539
                or >= 0x1D53B and <= 0x1D53E
                or >= 0x1D540 and <= 0x1D544
                or 0x1D546
                or >= 0x1D54A and <= 0x1D550
                or >= 0x1D552 and <= 0x1D6A3
                or >= 0x1D6A8 and <= 0x1D7C9
                or >= 0x20000 and <= 0x2A6D6
                or >= 0x2F800 and <= 0x2FA1D
                or >= 0xF0000 and <= 0xFFFFD
                or >= 0x100000 and <= 0x10FFFD;
        }


        /// <summary>
        /// Checks if a Unicode codepoint is part of stringprep "A.1 Unassigned code points in Unicode 3.2".
        /// </summary>
        static bool IsUnassignedCodePoint(int codepoint)
        {
            return codepoint is 0x0221
                or >= 0x0234 and <= 0x024F
                or >= 0x02AE and <= 0x02AF
                or >= 0x02EF and <= 0x02FF
                or >= 0x0350 and <= 0x035F
                or >= 0x0370 and <= 0x0373
                or >= 0x0376 and <= 0x0379
                or >= 0x037B and <= 0x037D
                or >= 0x037F and <= 0x0383
                or 0x038B
                or 0x038D
                or 0x03A2
                or 0x03CF
                or >= 0x03F7 and <= 0x03FF
                or 0x0487
                or 0x04CF
                or >= 0x04F6 and <= 0x04F7
                or >= 0x04FA and <= 0x04FF
                or >= 0x0510 and <= 0x0530
                or >= 0x0557 and <= 0x0558
                or 0x0560
                or 0x0588
                or >= 0x058B and <= 0x0590
                or 0x05A2
                or 0x05BA
                or >= 0x05C5 and <= 0x05CF
                or >= 0x05EB and <= 0x05EF
                or >= 0x05F5 and <= 0x060B
                or >= 0x060D and <= 0x061A
                or >= 0x061C and <= 0x061E
                or 0x0620
                or >= 0x063B and <= 0x063F
                or >= 0x0656 and <= 0x065F
                or >= 0x06EE and <= 0x06EF
                or 0x06FF
                or 0x070E
                or >= 0x072D and <= 0x072F
                or >= 0x074B and <= 0x077F
                or >= 0x07B2 and <= 0x0900
                or 0x0904
                or >= 0x093A and <= 0x093B
                or >= 0x094E and <= 0x094F
                or >= 0x0955 and <= 0x0957
                or >= 0x0971 and <= 0x0980
                or 0x0984
                or >= 0x098D and <= 0x098E
                or >= 0x0991 and <= 0x0992
                or 0x09A9
                or 0x09B1
                or >= 0x09B3 and <= 0x09B5
                or >= 0x09BA and <= 0x09BB
                or 0x09BD
                or >= 0x09C5 and <= 0x09C6
                or >= 0x09C9 and <= 0x09CA
                or >= 0x09CE and <= 0x09D6
                or >= 0x09D8 and <= 0x09DB
                or 0x09DE
                or >= 0x09E4 and <= 0x09E5
                or >= 0x09FB and <= 0x0A01
                or >= 0x0A03 and <= 0x0A04
                or >= 0x0A0B and <= 0x0A0E
                or >= 0x0A11 and <= 0x0A12
                or 0x0A29
                or 0x0A31
                or 0x0A34
                or 0x0A37
                or >= 0x0A3A and <= 0x0A3B
                or 0x0A3D
                or >= 0x0A43 and <= 0x0A46
                or >= 0x0A49 and <= 0x0A4A
                or >= 0x0A4E and <= 0x0A58
                or 0x0A5D
                or >= 0x0A5F and <= 0x0A65
                or >= 0x0A75 and <= 0x0A80
                or 0x0A84
                or 0x0A8C
                or 0x0A8E
                or 0x0A92
                or 0x0AA9
                or 0x0AB1
                or 0x0AB4
                or >= 0x0ABA and <= 0x0ABB
                or 0x0AC6
                or 0x0ACA
                or >= 0x0ACE and <= 0x0ACF
                or >= 0x0AD1 and <= 0x0ADF
                or >= 0x0AE1 and <= 0x0AE5
                or >= 0x0AF0 and <= 0x0B00
                or 0x0B04
                or >= 0x0B0D and <= 0x0B0E
                or >= 0x0B11 and <= 0x0B12
                or 0x0B29
                or 0x0B31
                or >= 0x0B34 and <= 0x0B35
                or >= 0x0B3A and <= 0x0B3B
                or >= 0x0B44 and <= 0x0B46
                or >= 0x0B49 and <= 0x0B4A
                or >= 0x0B4E and <= 0x0B55
                or >= 0x0B58 and <= 0x0B5B
                or 0x0B5E
                or >= 0x0B62 and <= 0x0B65
                or >= 0x0B71 and <= 0x0B81
                or 0x0B84
                or >= 0x0B8B and <= 0x0B8D
                or 0x0B91
                or >= 0x0B96 and <= 0x0B98
                or 0x0B9B
                or 0x0B9D
                or >= 0x0BA0 and <= 0x0BA2
                or >= 0x0BA5 and <= 0x0BA7
                or >= 0x0BAB and <= 0x0BAD
                or 0x0BB6
                or >= 0x0BBA and <= 0x0BBD
                or >= 0x0BC3 and <= 0x0BC5
                or 0x0BC9
                or >= 0x0BCE and <= 0x0BD6
                or >= 0x0BD8 and <= 0x0BE6
                or >= 0x0BF3 and <= 0x0C00
                or 0x0C04
                or 0x0C0D
                or 0x0C11
                or 0x0C29
                or 0x0C34
                or >= 0x0C3A and <= 0x0C3D
                or 0x0C45
                or 0x0C49
                or >= 0x0C4E and <= 0x0C54
                or >= 0x0C57 and <= 0x0C5F
                or >= 0x0C62 and <= 0x0C65
                or >= 0x0C70 and <= 0x0C81
                or 0x0C84
                or 0x0C8D
                or 0x0C91
                or 0x0CA9
                or 0x0CB4
                or >= 0x0CBA and <= 0x0CBD
                or 0x0CC5
                or 0x0CC9
                or >= 0x0CCE and <= 0x0CD4
                or >= 0x0CD7 and <= 0x0CDD
                or 0x0CDF
                or >= 0x0CE2 and <= 0x0CE5
                or >= 0x0CF0 and <= 0x0D01
                or 0x0D04
                or 0x0D0D
                or 0x0D11
                or 0x0D29
                or >= 0x0D3A and <= 0x0D3D
                or >= 0x0D44 and <= 0x0D45
                or 0x0D49
                or >= 0x0D4E and <= 0x0D56
                or >= 0x0D58 and <= 0x0D5F
                or >= 0x0D62 and <= 0x0D65
                or >= 0x0D70 and <= 0x0D81
                or 0x0D84
                or >= 0x0D97 and <= 0x0D99
                or 0x0DB2
                or 0x0DBC
                or >= 0x0DBE and <= 0x0DBF
                or >= 0x0DC7 and <= 0x0DC9
                or >= 0x0DCB and <= 0x0DCE
                or 0x0DD5
                or 0x0DD7
                or >= 0x0DE0 and <= 0x0DF1
                or >= 0x0DF5 and <= 0x0E00
                or >= 0x0E3B and <= 0x0E3E
                or >= 0x0E5C and <= 0x0E80
                or 0x0E83
                or >= 0x0E85 and <= 0x0E86
                or 0x0E89
                or >= 0x0E8B and <= 0x0E8C
                or >= 0x0E8E and <= 0x0E93
                or 0x0E98
                or 0x0EA0
                or 0x0EA4
                or 0x0EA6
                or >= 0x0EA8 and <= 0x0EA9
                or 0x0EAC
                or 0x0EBA
                or >= 0x0EBE and <= 0x0EBF
                or 0x0EC5
                or 0x0EC7
                or >= 0x0ECE and <= 0x0ECF
                or >= 0x0EDA and <= 0x0EDB
                or >= 0x0EDE and <= 0x0EFF
                or 0x0F48
                or >= 0x0F6B and <= 0x0F70
                or >= 0x0F8C and <= 0x0F8F
                or 0x0F98
                or 0x0FBD
                or >= 0x0FCD and <= 0x0FCE
                or >= 0x0FD0 and <= 0x0FFF
                or 0x1022
                or 0x1028
                or 0x102B
                or >= 0x1033 and <= 0x1035
                or >= 0x103A and <= 0x103F
                or >= 0x105A and <= 0x109F
                or >= 0x10C6 and <= 0x10CF
                or >= 0x10F9 and <= 0x10FA
                or >= 0x10FC and <= 0x10FF
                or >= 0x115A and <= 0x115E
                or >= 0x11A3 and <= 0x11A7
                or >= 0x11FA and <= 0x11FF
                or 0x1207
                or 0x1247
                or 0x1249
                or >= 0x124E and <= 0x124F
                or 0x1257
                or 0x1259
                or >= 0x125E and <= 0x125F
                or 0x1287
                or 0x1289
                or >= 0x128E and <= 0x128F
                or 0x12AF
                or 0x12B1
                or >= 0x12B6 and <= 0x12B7
                or 0x12BF
                or 0x12C1
                or >= 0x12C6 and <= 0x12C7
                or 0x12CF
                or 0x12D7
                or 0x12EF
                or 0x130F
                or 0x1311
                or >= 0x1316 and <= 0x1317
                or 0x131F
                or 0x1347
                or >= 0x135B and <= 0x1360
                or >= 0x137D and <= 0x139F
                or >= 0x13F5 and <= 0x1400
                or >= 0x1677 and <= 0x167F
                or >= 0x169D and <= 0x169F
                or >= 0x16F1 and <= 0x16FF
                or 0x170D
                or >= 0x1715 and <= 0x171F
                or >= 0x1737 and <= 0x173F
                or >= 0x1754 and <= 0x175F
                or 0x176D
                or 0x1771
                or >= 0x1774 and <= 0x177F
                or >= 0x17DD and <= 0x17DF
                or >= 0x17EA and <= 0x17FF
                or 0x180F
                or >= 0x181A and <= 0x181F
                or >= 0x1878 and <= 0x187F
                or >= 0x18AA and <= 0x1DFF
                or >= 0x1E9C and <= 0x1E9F
                or >= 0x1EFA and <= 0x1EFF
                or >= 0x1F16 and <= 0x1F17
                or >= 0x1F1E and <= 0x1F1F
                or >= 0x1F46 and <= 0x1F47
                or >= 0x1F4E and <= 0x1F4F
                or 0x1F58
                or 0x1F5A
                or 0x1F5C
                or 0x1F5E
                or >= 0x1F7E and <= 0x1F7F
                or 0x1FB5
                or 0x1FC5
                or >= 0x1FD4 and <= 0x1FD5
                or 0x1FDC
                or >= 0x1FF0 and <= 0x1FF1
                or 0x1FF5
                or 0x1FFF
                or >= 0x2053 and <= 0x2056
                or >= 0x2058 and <= 0x205E
                or >= 0x2064 and <= 0x2069
                or >= 0x2072 and <= 0x2073
                or >= 0x208F and <= 0x209F
                or >= 0x20B2 and <= 0x20CF
                or >= 0x20EB and <= 0x20FF
                or >= 0x213B and <= 0x213C
                or >= 0x214C and <= 0x2152
                or >= 0x2184 and <= 0x218F
                or >= 0x23CF and <= 0x23FF
                or >= 0x2427 and <= 0x243F
                or >= 0x244B and <= 0x245F
                or 0x24FF
                or >= 0x2614 and <= 0x2615
                or 0x2618
                or >= 0x267E and <= 0x267F
                or >= 0x268A and <= 0x2700
                or 0x2705
                or >= 0x270A and <= 0x270B
                or 0x2728
                or 0x274C
                or 0x274E
                or >= 0x2753 and <= 0x2755
                or 0x2757
                or >= 0x275F and <= 0x2760
                or >= 0x2795 and <= 0x2797
                or 0x27B0
                or >= 0x27BF and <= 0x27CF
                or >= 0x27EC and <= 0x27EF
                or >= 0x2B00 and <= 0x2E7F
                or 0x2E9A
                or >= 0x2EF4 and <= 0x2EFF
                or >= 0x2FD6 and <= 0x2FEF
                or >= 0x2FFC and <= 0x2FFF
                or 0x3040
                or >= 0x3097 and <= 0x3098
                or >= 0x3100 and <= 0x3104
                or >= 0x312D and <= 0x3130
                or 0x318F
                or >= 0x31B8 and <= 0x31EF
                or >= 0x321D and <= 0x321F
                or >= 0x3244 and <= 0x3250
                or >= 0x327C and <= 0x327E
                or >= 0x32CC and <= 0x32CF
                or 0x32FF
                or >= 0x3377 and <= 0x337A
                or >= 0x33DE and <= 0x33DF
                or 0x33FF
                or >= 0x4DB6 and <= 0x4DFF
                or >= 0x9FA6 and <= 0x9FFF
                or >= 0xA48D and <= 0xA48F
                or >= 0xA4C7 and <= 0xABFF
                or >= 0xD7A4 and <= 0xD7FF
                or >= 0xFA2E and <= 0xFA2F
                or >= 0xFA6B and <= 0xFAFF
                or >= 0xFB07 and <= 0xFB12
                or >= 0xFB18 and <= 0xFB1C
                or 0xFB37
                or 0xFB3D
                or 0xFB3F
                or 0xFB42
                or 0xFB45
                or >= 0xFBB2 and <= 0xFBD2
                or >= 0xFD40 and <= 0xFD4F
                or >= 0xFD90 and <= 0xFD91
                or >= 0xFDC8 and <= 0xFDCF
                or >= 0xFDFD and <= 0xFDFF
                or >= 0xFE10 and <= 0xFE1F
                or >= 0xFE24 and <= 0xFE2F
                or >= 0xFE47 and <= 0xFE48
                or 0xFE53
                or 0xFE67
                or >= 0xFE6C and <= 0xFE6F
                or 0xFE75
                or >= 0xFEFD and <= 0xFEFE
                or 0xFF00
                or >= 0xFFBF and <= 0xFFC1
                or >= 0xFFC8 and <= 0xFFC9
                or >= 0xFFD0 and <= 0xFFD1
                or >= 0xFFD8 and <= 0xFFD9
                or >= 0xFFDD and <= 0xFFDF
                or 0xFFE7
                or >= 0xFFEF and <= 0xFFF8
                or >= 0x10000 and <= 0x102FF
                or 0x1031F
                or >= 0x10324 and <= 0x1032F
                or >= 0x1034B and <= 0x103FF
                or >= 0x10426 and <= 0x10427
                or >= 0x1044E and <= 0x1CFFF
                or >= 0x1D0F6 and <= 0x1D0FF
                or >= 0x1D127 and <= 0x1D129
                or >= 0x1D1DE and <= 0x1D3FF
                or 0x1D455
                or 0x1D49D
                or >= 0x1D4A0 and <= 0x1D4A1
                or >= 0x1D4A3 and <= 0x1D4A4
                or >= 0x1D4A7 and <= 0x1D4A8
                or 0x1D4AD
                or 0x1D4BA
                or 0x1D4BC
                or 0x1D4C1
                or 0x1D4C4
                or 0x1D506
                or >= 0x1D50B and <= 0x1D50C
                or 0x1D515
                or 0x1D51D
                or 0x1D53A
                or 0x1D53F
                or 0x1D545
                or >= 0x1D547 and <= 0x1D549
                or 0x1D551
                or >= 0x1D6A4 and <= 0x1D6A7
                or >= 0x1D7CA and <= 0x1D7CD
                or >= 0x1D800 and <= 0x1FFFD
                or >= 0x2A6D7 and <= 0x2F7FF
                or >= 0x2FA1E and <= 0x2FFFD
                or >= 0x30000 and <= 0x3FFFD
                or >= 0x40000 and <= 0x4FFFD
                or >= 0x50000 and <= 0x5FFFD
                or >= 0x60000 and <= 0x6FFFD
                or >= 0x70000 and <= 0x7FFFD
                or >= 0x80000 and <= 0x8FFFD
                or >= 0x90000 and <= 0x9FFFD
                or >= 0xA0000 and <= 0xAFFFD
                or >= 0xB0000 and <= 0xBFFFD
                or >= 0xC0000 and <= 0xCFFFD
                or >= 0xD0000 and <= 0xDFFFD
                or 0xE0000
                or >= 0xE0002 and <= 0xE001F
                or >= 0xE0080 and <= 0xEFFFD;
        }
    }
}
