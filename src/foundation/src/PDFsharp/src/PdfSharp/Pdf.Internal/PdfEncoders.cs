﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Logging;
using PdfSharp.Pdf.Security;

namespace PdfSharp.Pdf.Internal
{
    /// <summary>
    /// Groups a set of static encoding helper functions.
    /// The class is intended for internal use only and public only
    /// for being used in unit tests.
    /// </summary>
    public static class PdfEncoders
    {
        /// <summary>
        /// Gets the raw encoding.
        /// </summary>
        public static Encoding RawEncoding => _rawEncoding ??= new RawEncoding();
        static Encoding? _rawEncoding;

        /// <summary>
        /// Gets the raw Unicode encoding.
        /// </summary>
        public static Encoding RawUnicodeEncoding => _rawUnicodeEncoding ??= new RawUnicodeEncoding();
        static Encoding? _rawUnicodeEncoding;

        /// <summary>
        /// Gets the Windows 1252 (ANSI) encoding.
        /// </summary>
        public static Encoding WinAnsiEncoding
        {
            // We consistently use our own WinAnsiEncoding implementation in PDFsharp.
            get => _winAnsiEncoding ??= new AnsiEncoding();
        }
        static Encoding? _winAnsiEncoding;

        /// <summary>
        /// Gets the PDF DocEncoding encoding.
        /// </summary>
        public static Encoding DocEncoding => _docEncoding ??= new DocEncoding();
        static Encoding? _docEncoding;

        /// <summary>
        /// Gets the UNICODE little-endian encoding.
        /// </summary>
        public static Encoding UnicodeEncoding => _unicodeEncoding ??= Encoding.Unicode;
        static Encoding? _unicodeEncoding;

        ///// <summary>
        ///// Encodes a string from a byte array. Each character gets the code of the corresponding byte.
        ///// </summary>
        //public static string RawString(byte[] bytes, int offset, int length)
        //{
        //  char[] chars = new char[length];
        //  for (int idx = offset, ch = 0; idx < offset +  length; idx++, ch++)
        //    chars[ch] = (char)bytes[idx];
        //  return new string(chars, 0, length);
        //}
        //
        //public static string RawString(byte[] bytes)
        //{
        //  return RawString(bytes, 0, bytes.Length);
        //}

#if true_
        public static string EncodeAsLiteral(string text, bool unicode)
        {
            if (text == null || text == "")
                return "<>";

            StringBuilder pdf = new StringBuilder("");
            if (!unicode)
            {
                byte[] bytes = WinAnsiEncoding.GetBytes(text);
                int count = bytes.Length;
                pdf.Append("(");
                for (int idx = 0; idx < count; idx++)
                {
                    char ch = (char)bytes[idx];
                    if (ch < 32)
                    {
                        switch (ch)
                        {
                            case '\n':
                                pdf.Append("\\n");
                                break;

                            case '\r':
                                pdf.Append("\\r");
                                break;

                            case '\t':
                                pdf.Append("\\t");
                                break;

                            case '\f':
                                pdf.Append("\\f");
                                break;

                            default:
                                pdf.Append(InvalidChar); // TO/DO
                                break;
                        }
                    }
                    else
                    {
                        switch (ch)
                        {
                            case '(':
                                pdf.Append("\\(");
                                break;

                            case ')':
                                pdf.Append("\\)");
                                break;

                            case '\\':
                                pdf.Append("\\\\");
                                break;

                            default:
                                pdf.Append(ch);
                                break;
                        }
                    }
                }
                pdf.Append(')');
            }
            else
            {
                pdf.Append("<");
                byte[] bytes = UnicodeEncoding.GetBytes(text);
                int count = bytes.Length;
                for (int idx = 0; idx < count; idx += 2)
                {
                    pdf.AppendFormat("{0:X2}{1:X2}", bytes[idx + 1], bytes[idx]);
                    if (idx != 0 && (idx % 48) == 0)
                        pdf.Append("\n");
                }
                pdf.Append(">");
            }
            return pdf.ToString();
        }
#endif

        //public static string EncodeAsLiteral(string text)
        //{
        //  return EncodeAsLiteral(text, false);
        //}

        /// <summary>
        /// Converts a raw string into a raw string literal, possibly encrypted.
        /// </summary>
        public static string ToStringLiteral(string text, PdfStringEncoding encoding, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            if (String.IsNullOrEmpty(text))
                return "()";

            var bytes = encoding switch
            {
                PdfStringEncoding.RawEncoding => RawEncoding.GetBytes(text),
                PdfStringEncoding.WinAnsiEncoding => WinAnsiEncoding.GetBytes(text),
                PdfStringEncoding.PDFDocEncoding => DocEncoding.GetBytes(text),
                PdfStringEncoding.Unicode => RawUnicodeEncoding.GetBytes(text),
                _ => throw new NotImplementedException(encoding.ToString())
            };
            var temp = FormatStringLiteral(bytes, encoding == PdfStringEncoding.Unicode, true, false, effectiveSecurityHandler);
            return RawEncoding.GetString(temp, 0, temp.Length);
        }

        /// <summary>
        /// Converts a raw string into a PDF raw string literal, possibly encrypted.
        /// </summary>
        public static string ToStringLiteral(byte[]? bytes, bool unicode, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            if (bytes == null || bytes.Length == 0)
                return "()";

            byte[] temp = FormatStringLiteral(bytes, unicode, true, false, effectiveSecurityHandler);
            return RawEncoding.GetString(temp, 0, temp.Length);
        }

        /// <summary>
        /// Converts a raw string into a raw hexadecimal string literal, possibly encrypted.
        /// </summary>
        public static string ToHexStringLiteral(string text, PdfStringEncoding encoding, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            if (String.IsNullOrEmpty(text))
                return "<>";

            byte[] bytes;
            switch (encoding)
            {
                case PdfStringEncoding.RawEncoding:
                    bytes = RawEncoding.GetBytes(text);
                    break;

                case PdfStringEncoding.WinAnsiEncoding:
                    bytes = WinAnsiEncoding.GetBytes(text);
                    break;

                case PdfStringEncoding.PDFDocEncoding:
                    bytes = DocEncoding.GetBytes(text);
                    break;

                case PdfStringEncoding.Unicode:
                    //bytes = UnicodeEncoding.GetBytes(text);
                    bytes = RawUnicodeEncoding.GetBytes(text);
                    break;

                default:
                    throw new NotImplementedException(encoding.ToString());
            }

            byte[] agTemp = FormatStringLiteral(bytes, encoding == PdfStringEncoding.Unicode, true, true, effectiveSecurityHandler);
            return RawEncoding.GetString(agTemp, 0, agTemp.Length);
        }

        /// <summary>
        /// Converts a raw string into a raw hexadecimal string literal, possibly encrypted.
        /// </summary>
        public static string ToHexStringLiteral(byte[]? bytes, bool unicode, bool prefix, PdfStandardSecurityHandler? securityHandler)
        {
            if (bytes == null || bytes.Length == 0)
                return "<>";

            byte[] agTemp = FormatStringLiteral(bytes, unicode, prefix, true, securityHandler);
            return RawEncoding.GetString(agTemp, 0, agTemp.Length);
        }

        /// <summary>
        /// Converts the specified byte array into a byte array representing a string literal.
        /// </summary>
        /// <param name="bytes">The bytes of the string.</param>
        /// <param name="unicode">Indicates whether one or two bytes are one character.</param>
        /// <param name="prefix">Indicates whether to use Unicode prefix.</param>
        /// <param name="hex">Indicates whether to create a hexadecimal string literal.</param>
        /// <param name="effectiveSecurityHandler">Encrypts the bytes if specified.</param>
        /// <returns>The PDF bytes.</returns>
        public static byte[] FormatStringLiteral(byte[]? bytes, bool unicode, bool prefix, bool hex, PdfStandardSecurityHandler? effectiveSecurityHandler)
        {
            if (bytes == null || bytes.Length == 0)
                return hex ? [(byte)'<', (byte)'>'] : [(byte)'(', (byte)')'];

            Debug.Assert(!unicode || bytes.Length % 2 == 0, "Odd number of bytes in Unicode string.");

            // Add unicode BOM, if needed.
            if (unicode && prefix)
            {
                var bytes2 = new byte[bytes.Length + 2];
                // Add BOM.
                bytes2[0] = 0xfe;
                bytes2[1] = 0xff;
                // Copy bytes.
                Array.Copy(bytes, 0, bytes2, 2, bytes.Length);
                bytes = bytes2;
            }

            var encrypted = false;
            // Encrypt, if needed.
            if (effectiveSecurityHandler != null)
            {
                effectiveSecurityHandler.EncryptString(ref bytes);
                encrypted = true;
            }

            var count = bytes.Length;
            var pdf = new StringBuilder();
            if (!unicode)
            {
                // Case: ANSI
                if (!hex)
                {
                    pdf.Append('(');
                    for (var idx = 0; idx < count; idx++)
                    {
                        char ch = (char)bytes[idx];
                        if ((int)ch is < 32 or 127)
                        {
                            // Case: Not printable ASCII character
                            switch (ch)
                            {
                                case '\n':
                                    pdf.Append("\\n");
                                    break;

                                case '\r':
                                    pdf.Append("\\r");
                                    break;

                                case '\t':
                                    pdf.Append("\\t");
                                    break;

                                case '\b':
                                    pdf.Append("\\b");
                                    break;

                                case '\f':
                                    pdf.Append("\\f");
                                    break;

                                default:
                                    if (!encrypted)
                                    {
                                        // Escape not encrypted not printable character in octal format "\ddd" to be printable.
                                        pdf.Append(@"\");
                                        var div = Math.DivRem(ch, 64, out var remainder); // Math.DivRem is faster than running '/' and '%' operation.
                                        pdf.Append((char)(div + '0'));
                                        div = Math.DivRem(remainder, 8, out remainder);
                                        pdf.Append((char)(div + '0'));
                                        pdf.Append((char)(remainder + '0'));
                                    }
                                    else
                                    {
                                        // Don’t escape encrypted not printable characters to reduce file size (the string is unreadable anyway).
                                        pdf.Append(ch);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // Case: Printable ANSI character
                            switch (ch)
                            {
                                case '(':
                                    pdf.Append(@"\(");
                                    break;

                                case ')':
                                    pdf.Append(@"\)");
                                    break;

                                case '\\':
                                    pdf.Append(@"\\");
                                    break;

                                default:
                                    pdf.Append(ch);
                                    break;
                            }
                        }
                    }
                    pdf.Append(')');
                }
                else
                {
                    // Case: Hex
                    pdf.Append('<');
                    for (var idx = 0; idx < count; idx++)
                        pdf.Append($"{bytes[idx]:X2}");
                    pdf.Append('>');
                }
            }
            else
            {
                // Case: Unicode
                // No check for hex: All unicode literals are saved as hex values.
                pdf.Append('<');
                for (var idx = 0; idx < count; idx += 2)
                {
                    pdf.Append($"{bytes[idx]:X2}{bytes[idx + 1]:X2}");
                    if (idx != 0 && (idx % 48) == 0)
                        pdf.Append('\n');
                }
                pdf.Append('>');
            }
            return RawEncoding.GetBytes(pdf.ToString());
        }

        /// <summary>
        /// Converts WinAnsi to DocEncode characters. Incomplete, just maps € and some other characters.
        /// </summary>
        static byte[] docencode_______ =
        [
            // TODO_OLD: ??? Note: See table in DocEncoding.cs.
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
            0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
            0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
            0xA0, 0x7F, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
            0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x8A, 0x8C, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
            0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
            0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
            0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
            0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
            0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
            0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF
        ];

        //public static string DocEncode(string text, bool unicode)//, PdfStandardSecurityHandler securityHandler)
        //{
        //  if (text == null || text == "")
        //    return "()";
        //
        //  int length = text.Length;
        //  StringBuilder encoded = new StringBuilder(2 * length);
        //  if (!unicode)
        //  {
        //    byte[] bytes = WinAnsiEncoding.GetBytes(text);
        //    encoded.Append('(');
        //    for (int idx = 0; idx < length; idx++)
        //    {
        //      char ch = (char)bytes[idx];
        //      if (ch > 255)
        //      {
        //        //TO/DO Unicode?
        //        encoded.Append(InvalidChar);
        //        //encoded.Append(ch);
        //        continue;
        //      }
        //      ch = (char)docencode[(int)ch];
        //      if (ch < 32)
        //      {
        //        switch (ch)
        //        {
        //          case '\n':
        //            encoded.Append("\\n");
        //            break;
        //
        //          case '\r':
        //            encoded.Append("\\r");
        //            break;
        //
        //          case '\t':
        //            encoded.Append("\\t");
        //            break;
        //
        //          case '\f':
        //            encoded.Append("\\f");
        //            break;
        //
        //          default: 
        //            encoded.Append(InvalidChar); // TO/DO
        //            break;
        //        }
        //      }
        //      else
        //      {
        //        switch (ch)
        //        {
        //          case '(':
        //            encoded.Append("\\(");
        //            break;
        //
        //          case ')':
        //            encoded.Append("\\)");
        //            break;
        //
        //          case '\\':
        //            encoded.Append("\\\\");
        //            break;
        //
        //          default:
        //            encoded.Append(ch);
        //            break;
        //        }
        //      }
        //    }
        //    encoded.Append(')');
        //  }
        //  else
        //  {
        //    encoded.Append("<FEFF");
        //    for (int idx = 0; idx < length; idx++)
        //      encoded.AppendFormat("{0:X4}", (int)text[idx]);
        //    encoded.Append('>');
        //  }
        //  return encoded.ToString();
        //}

        //public static string DocEncode(string text)
        //{
        //  return DocEncode(text, false);
        //}

        ///// <summary>
        ///// Encodes a hexadecimal doc-encoded string literal.
        ///// </summary>
        //public static string DocEncodeHex(string text, bool unicode)
        //{
        //  if (text == null || text == "")
        //    return "<>";
        //
        //  int length = text.Length;
        //  StringBuilder encoded = new StringBuilder(3 * length);
        //  if (!unicode)
        //  {
        //    byte[] bytes = WinAnsiEncoding.GetBytes(text);
        //    encoded.Append('<');
        //    for (int idx = 0; idx < length; idx++)
        //      encoded.AppendFormat("{0:X2}", docencode[bytes[idx]]);
        //    encoded.Append('>');
        //  }
        //  else
        //  {
        //    encoded.Append("<FEFF");
        //    for (int idx = 0; idx < length; idx++)
        //    {
        //      encoded.AppendFormat("{0:X4}", (int)text[idx]);
        //    }
        //    encoded.Append('>');
        //  }
        //  return encoded.ToString();
        //}

        //public static string DocEncodeHex(string text)
        //{
        //  return DocEncodeHex(text, false);
        //}

        /// <summary>
        /// ...because I always forget CultureInfo.InvariantCulture and wonder why Acrobat
        /// cannot understand my German decimal separator...
        /// </summary>
        public static string Format(string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Converts a float into a string with up to 3 decimal digits and a decimal point.
        /// </summary>
        public static string ToString(double val)
        {
            return val.ToString(Config.SignificantDecimalPlaces3, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts an XColor into a string with up to 3 decimal digits and a decimal point.
        /// </summary>
        public static string ToString(XColor color, PdfColorMode colorMode)
        {
            const string format = Config.SignificantDecimalPlaces3;

            // If not defined let color decide.
            if (colorMode == PdfColorMode.Undefined)
                colorMode = color.ColorSpace == XColorSpace.Cmyk ? PdfColorMode.Cmyk : PdfColorMode.Rgb;

            switch (colorMode)
            {
                case PdfColorMode.Rgb:
                    {
                        s_formatRGB ??= "{0:" + format + "} {1:" + format + "} {2:" + format + "}";
                        // earlier:
                        // return String.Format(CultureInfo.InvariantCulture, "{0:" + format + "} {1:" + format + "} {2:" + format + "}",
                        //    color.R / 255.0, color.G / 255.0, color.B / 255.0);
                        return String.Format(CultureInfo.InvariantCulture, s_formatRGB, color.R / 255.0, color.G / 255.0, color.B / 255.0);
                    }

                case PdfColorMode.Cmyk:
                    {
                        s_formatCMYK ??= "{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}";
                        // earlier:
                        // return String.Format(CultureInfo.InvariantCulture,
                        //     "{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}",
                        //     color.C, color.M, color.Y, color.K);
                        return String.Format(CultureInfo.InvariantCulture, s_formatCMYK, color.C, color.M, color.Y, color.K);
                    }

                default:
                    Debug.Assert(false, "Cannot come here.");
                    PdfSharpLogHost.Logger.LogError("Render a color with invalid color mode.");
                    goto case PdfColorMode.Rgb;
            }
        }
        // ReSharper disable InconsistentNaming
        static string? s_formatRGB;
        static string? s_formatCMYK;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Converts an XMatrix into a string with up to 4 decimal digits and a decimal point.
        /// </summary>
        public static string ToString(XMatrix matrix)
        {
            const string format = Config.SignificantDecimalPlaces4;
            return String.Format(CultureInfo.InvariantCulture,
                "{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "} {4:" + format + "} {5:" + format + "}",
                matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
        }
    }
}
