// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Quality;
#if GDI
using System.Drawing;
using System.Drawing.Text;
using GdiFont = System.Drawing.Font;
#endif
#if WPF
using System.Windows.Markup;
using System.Windows.Media;
#endif

namespace PdfSharp.Snippets.Font
{
    public class FontAnsiEncoding : Snippet
    {
        public FontAnsiEncoding()
        {
            Title = "Font ANSI encoding";
            PathName = "snippets/drawing/text/encoding/FontAnsiEncoding";
        }

        public override void RenderSnippet(XGraphics gfx)
        {
            //var font = new XFont("Segoe UI Emoji", 10);
            var font = new XFont("Arial", 9);
            var font2 = new XFont("Arial", 7);

            var fontAnsi = new XFont("Arial", 10, XFontStyleEx.Regular, XPdfFontOptions.WinAnsiDefault);

            const int top = 90;
            const int left = 50;
            const int dx = 140;
            const int dy = 15;

            var encoder = new PdfSharp.Pdf.Internal.AnsiEncoding();

            var ansi = new byte[1];

            for (int idx = 0; idx <= 255; idx++)
            {
                if (idx == 129)
                    _ = typeof(int);

                ansi[0] = (byte)idx;

                //GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
                var sss = encoder.GetString(ansi);

                var x = left + (idx >> 6) * dx + (idx >= 192 ? 50 : 0);
                var y = top + (idx % 64) * dy;
                var item = Table[idx];
                gfx.DrawString($"{idx,-3:0}  {idx:X2}   {item.Char} {sss}", fontAnsi, XBrushes.DarkBlue, x, y);
                //gfx.DrawString($"{item.Description}", font2, XBrushes.DarkBlue, x + 60, y);
                gfx.DrawString($"U-{(int)item.UnicodeValue:X4} U-{(int)sss[0]:X4}", font2, XBrushes.DarkBlue, x + 60, y);
            }
        }

        public static readonly AnsiTable[] Table =
        [
            // ASCII
            new AnsiTable(0, '\u0000', '\x00', "NUL"),
            new AnsiTable(1, '\u0001', '\x01', "SCH"),
            new AnsiTable(2, '\u0002', '\x02', "STX"),
            new AnsiTable(3, '\u0003', '\x03', "ETX"),
            new AnsiTable(4, '\u0004', '\x04', "EOT"),
            new AnsiTable(5, '\u0005', '\x05', "ENQ"),
            new AnsiTable(6, '\u0006', '\x06', "ACK"),
            new AnsiTable(7, '\u0007', '\x07', "BEL"),
            new AnsiTable(8, '\u0008', '\x08', "BS"),
            new AnsiTable(9, '\u0009', '\x09', "HT"),
            new AnsiTable(10, '\u000A', '\x0A', "LF"),
            new AnsiTable(11, '\u000B', '\x0B', "VT"),
            new AnsiTable(12, '\u000C', '\x0C', "FF"),
            new AnsiTable(13, '\u000D', '\x0D', "CR"),
            new AnsiTable(14, '\u000E', '\x0E', "SO"),
            new AnsiTable(15, '\u000F', '\x0F', "SI"),
            new AnsiTable(16, '\u0010', '\x10', "DLE"),
            new AnsiTable(17, '\u0011', '\x11', "DC1 / XON"),
            new AnsiTable(18, '\u0012', '\x12', "DC2"),
            new AnsiTable(19, '\u0013', '\x13', "DC3 / XOFF"),
            new AnsiTable(20, '\u0014', '\x14', "DC4"),
            new AnsiTable(21, '\u0015', '\x15', "NAK"),
            new AnsiTable(22, '\u0016', '\x16', "SYN"),
            new AnsiTable(23, '\u0017', '\x17', "ETB"),
            new AnsiTable(24, '\u0018', '\x18', "CAN"),
            new AnsiTable(25, '\u0019', '\x19', "EM"),
            new AnsiTable(26, '\u001A', '\x1A', "SUB"),
            new AnsiTable(27, '\u001B', '\x1B', "ESC"),
            new AnsiTable(28, '\u001C', '\x1C', "FS"),
            new AnsiTable(29, '\u001D', '\x1D', "GS"),
            new AnsiTable(30, '\u001E', '\x1E', "RS"),
            new AnsiTable(31, '\u001F', '\x1F', "US"),
            new AnsiTable(32, '\u0020', ' ', "Space"),
            new AnsiTable(33, '\u0021', '!', "Exclamation mark"),
            new AnsiTable(34, '\u0022', '\"', "Quotes"),
            new AnsiTable(35, '\u0023', '#', "Hash"),
            new AnsiTable(36, '\u0024', '$', "Dollar"),
            new AnsiTable(37, '\u0025', '%', "Percent"),
            new AnsiTable(38, '\u0026', '&', "Ampersand"),
            new AnsiTable(39, '\u0027', '\'', "Apostrophe"),
            new AnsiTable(40, '\u0028', '(', "Open bracket (Parenthesis)"),
            new AnsiTable(41, '\u0029', ')', "Close bracket (Parenthesis)"),
            new AnsiTable(42, '\u002A', '*', "Asterisk"),
            new AnsiTable(43, '\u002B', '+', "Plus"),
            new AnsiTable(44, '\u002C', ',', "Comma"),
            new AnsiTable(45, '\u002D', '-', "Dash"),
            new AnsiTable(46, '\u002E', '.', "Full stop"),
            new AnsiTable(47, '\u002F', '/', "Slash"),
            new AnsiTable(48, '\u0030', '0', "Zero"),
            new AnsiTable(49, '\u0031', '1', "One"),
            new AnsiTable(50, '\u0032', '2', "Two"),
            new AnsiTable(51, '\u0033', '3', "Three"),
            new AnsiTable(52, '\u0034', '4', "Four"),
            new AnsiTable(53, '\u0035', '5', "Five"),
            new AnsiTable(54, '\u0036', '6', "Six"),
            new AnsiTable(55, '\u0037', '7', "Seven"),
            new AnsiTable(56, '\u0038', '8', "Eight"),
            new AnsiTable(57, '\u0039', '9', "Nine"),
            new AnsiTable(58, '\u003A', ':', "Colon"),
            new AnsiTable(59, '\u003B', ';', "Semi-colon"),
            new AnsiTable(60, '\u003C', '<', "Less than"),
            new AnsiTable(61, '\u003D', '=', "Equals"),
            new AnsiTable(62, '\u003E', '>', "Greater than"),
            new AnsiTable(63, '\u003F', '?', "Question mark"),
            new AnsiTable(64, '\u0040', '@', "At"),
            new AnsiTable(65, '\u0041', 'A', "Uppercase A"),
            new AnsiTable(66, '\u0042', 'B', "Uppercase B"),
            new AnsiTable(67, '\u0043', 'C', "Uppercase C"),
            new AnsiTable(68, '\u0044', 'D', "Uppercase D"),
            new AnsiTable(69, '\u0045', 'E', "Uppercase E"),
            new AnsiTable(70, '\u0046', 'F', "Uppercase F"),
            new AnsiTable(71, '\u0047', 'G', "Uppercase G"),
            new AnsiTable(72, '\u0048', 'H', "Uppercase H"),
            new AnsiTable(73, '\u0049', 'I', "Uppercase I"),
            new AnsiTable(74, '\u004A', 'J', "Uppercase J"),
            new AnsiTable(75, '\u004B', 'K', "Uppercase K"),
            new AnsiTable(76, '\u004C', 'L', "Uppercase L"),
            new AnsiTable(77, '\u004D', 'M', "Uppercase M"),
            new AnsiTable(78, '\u004E', 'N', "Uppercase N"),
            new AnsiTable(79, '\u004F', 'O', "Uppercase O"),
            new AnsiTable(80, '\u0050', 'P', "Uppercase P"),
            new AnsiTable(81, '\u0051', 'Q', "Uppercase Q"),
            new AnsiTable(82, '\u0052', 'R', "Uppercase R"),
            new AnsiTable(83, '\u0053', 's', "Uppercase S"),
            new AnsiTable(84, '\u0054', 'T', "Uppercase T"),
            new AnsiTable(85, '\u0055', 'U', "Uppercase U"),
            new AnsiTable(86, '\u0056', 'V', "Uppercase V"),
            new AnsiTable(87, '\u0057', 'W', "Uppercase W"),
            new AnsiTable(88, '\u0058', 'X', "Uppercase X"),
            new AnsiTable(89, '\u0059', 'Y', "Uppercase Y"),
            new AnsiTable(90, '\u005A', 'Z', "Uppercase Z"),
            new AnsiTable(91, '\u005B', '[', "Open square bracket"),
            new AnsiTable(92, '\u005C', '\\', "Backslash"),
            new AnsiTable(93, '\u005D', ']', "Close square bracket"),
            new AnsiTable(94, '\u005E', '^', "Caret/hat"),
            new AnsiTable(95, '\u005F', '_', "Underscore"),
            new AnsiTable(96, '\u0060', '`', "Accent grave"),
            new AnsiTable(97, '\u0061', 'a', "Lowercase a"),
            new AnsiTable(98, '\u0062', 'b', "Lowercase b"),
            new AnsiTable(99, '\u0063', 'c', "Lowercase c"),
            new AnsiTable(00, '\u0064', 'd', "Lowercase d"),
            new AnsiTable(01, '\u0065', 'e', "Lowercase e"),
            new AnsiTable(02, '\u0066', 'f', "Lowercase f"),
            new AnsiTable(03, '\u0067', 'g', "Lowercase g"),
            new AnsiTable(04, '\u0068', 'h', "Lowercase h"),
            new AnsiTable(05, '\u0069', 'i', "Lowercase i"),
            new AnsiTable(06, '\u006A', 'j', "Lowercase j"),
            new AnsiTable(07, '\u006B', 'k', "Lowercase k"),
            new AnsiTable(08, '\u006C', 'l', "Lowercase l"),
            new AnsiTable(09, '\u006D', 'm', "Lowercase m"),
            new AnsiTable(110, '\u006E', 'n', "Lowercase n"),
            new AnsiTable(111, '\u006F', 'o', "Lowercase o"),
            new AnsiTable(112, '\u0070', 'p', "Lowercase p"),
            new AnsiTable(113, '\u0071', 'q', "Lowercase q"),
            new AnsiTable(114, '\u0072', 'r', "Lowercase r"),
            new AnsiTable(115, '\u0073', 's', "Lowercase s"),
            new AnsiTable(116, '\u0074', 't', "Lowercase t"),
            new AnsiTable(117, '\u0075', 'u', "Lowercase u"),
            new AnsiTable(118, '\u0076', 'v', "Lowercase v"),
            new AnsiTable(119, '\u0077', 'w', "Lowercase w"),
            new AnsiTable(120, '\u0078', 'x', "Lowercase x"),
            new AnsiTable(121, '\u0079', 'y', "Lowercase y"),
            new AnsiTable(122, '\u007A', 'z', "Lowercase z"),
            new AnsiTable(123, '\u007B', '{', "Open brace"),
            new AnsiTable(124, '\u007C', '|', "Pipe"),
            new AnsiTable(125, '\u007D', '}', "Close brace"),
            new AnsiTable(126, '\u007E', '~', "Tilde"),
            new AnsiTable(127, '\u007F', '⌂', "Delete"),

            // ANSI
            new AnsiTable(128, '\u20AC', '€', "Euro Sign"),
            new AnsiTable(129, '\u0081', '\0', "Undefined"), // Unicode: High Octet Preset
            new AnsiTable(130, '\u201A', '‚', "Single Low-9 Quotation Mark"),
            new AnsiTable(131, '\u0192', 'ƒ', "Latin Small Letter F With Hook"),
            new AnsiTable(132, '\u201E', '„', "Double Low-9 Quotation Mark"),
            new AnsiTable(133, '\u2026', '…', "Horizontal Ellipsis"),
            new AnsiTable(134, '\u2020', '†', "Dagger"),
            new AnsiTable(135, '\u2021', '‡', "Double Dagger"),
            new AnsiTable(136, '\u02C6', 'ˆ', "Modifier Letter Circumflex Accent"),
            new AnsiTable(137, '\u2030', '‰', "Per Mille Sign"),
            new AnsiTable(138, '\u0160', 'Š', "Latin Capital Letter S With Caron"),
            new AnsiTable(139, '\u2039', '‹', "Single Left-pointing Angle Quotation Mark"),
            new AnsiTable(140, '\u0152', 'Œ', "Latin Capital Ligature Oe"),
            new AnsiTable(141, '\u008D', '\0', "Undefined"), // Unicode: Reverse Line Feed
            new AnsiTable(142, '\u017D', 'Ž', "Latin Capital Letter Z With Caron"),
            new AnsiTable(143, '\u008F', '\0', "Undefined"), // Unicode: Single-Shift Three
            new AnsiTable(144, '\u0090', '\0', "Undefined"), // Unicode: Device Control String
            new AnsiTable(145, '\u2018', '‘', "Left Single Quotation Mark"),
            new AnsiTable(146, '\u2019', '’', "Right Single Quotation Mark"),
            new AnsiTable(147, '\u201C', '“', "Left Double Quotation Mark"),
            new AnsiTable(148, '\u201D', '”', "Right Double Quotation Mark"),
            new AnsiTable(149, '\u2022', '•', "Bullet"),
            new AnsiTable(150, '\u2013', '–', "En Dash"),
            new AnsiTable(151, '\u2014', '—', "Em Dash"),
            new AnsiTable(152, '\u02DC', '˜', "Small Tilde"),
            new AnsiTable(153, '\u2122', '™', "Trade Mark Sign"),
            new AnsiTable(154, '\u0161', 'š', "Latin Small Letter S With Caron"),
            new AnsiTable(155, '\u203A', '›', "Single Right-pointing Angle Quotation Mark"),
            new AnsiTable(156, '\u0153', 'œ', "Latin Small Ligature Oe"),
            new AnsiTable(157, '\u009D', '\0', "Undefined"), // Unicode: Operating System Command
            new AnsiTable(158, '\u017E', 'ž', "Latin Small Letter Z With Caron"),
            new AnsiTable(159, '\u0178', 'Ÿ', "Latin Capital Letter Y With Diaeresis"),
            new AnsiTable(160, '\u00A0', ' ', "No-break Space"),
            new AnsiTable(161, '\u00A1', '¡', "Inverted Exclamation Mark"),
            new AnsiTable(162, '\u00A2', '¢', "Cent Sign"),
            new AnsiTable(163, '\u00A3', '£', "Pound Sign"),
            new AnsiTable(164, '\u00A4', '¤', "Currency Sign"),
            new AnsiTable(165, '\u00A5', '¥', "Yen Sign"),
            new AnsiTable(166, '\u00A6', '¦', "Broken Bar"),
            new AnsiTable(167, '\u00A7', '§', "Section Sign"),
            new AnsiTable(168, '\u00A8', '¨', "Diaeresis"),
            new AnsiTable(169, '\u00A9', '©', "Copyright Sign"),
            new AnsiTable(170, '\u00AA', 'ª', "Feminine Ordinal Indicator"),
            new AnsiTable(171, '\u00AB', '«', "Left-pointing Double Angle Quotation Mark"),
            new AnsiTable(172, '\u00AC', '¬', "Not Sign"),
            new AnsiTable(173, '\u00AD', '­', "Undefined / Soft Hyphen"), // Alt-173
            new AnsiTable(174, '\u00AE', '®', "Registered Sign"),
            new AnsiTable(175, '\u00AF', '¯', "Macron"),
            new AnsiTable(176, '\u00B0', '°', "Degree Sign"),
            new AnsiTable(177, '\u00B1', '±', "Plus-minus Sign"),
            new AnsiTable(178, '\u00B2', '²', "Superscript Two"),
            new AnsiTable(179, '\u00B3', '³', "Superscript Three"),
            new AnsiTable(180, '\u00B4', '´', "Acute Accent"),
            new AnsiTable(181, '\u00B5', 'µ', "Micro Sign"),
            new AnsiTable(182, '\u00B6', '¶', "Pilcrow Sign"),
            new AnsiTable(183, '\u00B7', '·', "Middle Dot"),
            new AnsiTable(184, '\u00B8', '¸', "Cedilla"),
            new AnsiTable(185, '\u00B9', '¹', "Superscript One"),
            new AnsiTable(186, '\u00BA', 'º', "Masculine Ordinal Indicator"),
            new AnsiTable(187, '\u00BB', '»', "Right-pointing Double Angle Quotation Mark"),
            new AnsiTable(188, '\u00BC', '¼', "Vulgar Fraction One Quarter"),
            new AnsiTable(189, '\u00BD', '½', "Vulgar Fraction One Half"),
            new AnsiTable(190, '\u00BE', '¾', "Vulgar Fraction Three Quarters"),
            new AnsiTable(191, '\u00BF', '¿', "Inverted Question Mark"),
            new AnsiTable(192, '\u00C0', 'À', "Latin Capital Letter A With Grave"),
            new AnsiTable(193, '\u00C1', 'Á', "Latin Capital Letter A With Acute"),
            new AnsiTable(194, '\u00C2', 'Â', "Latin Capital Letter A With Circumflex"),
            new AnsiTable(195, '\u00C3', 'Ã', "Latin Capital Letter A With Tilde"),
            new AnsiTable(196, '\u00C4', 'Ä', "Latin Capital Letter A With Diaeresis"),
            new AnsiTable(197, '\u00C5', 'Å', "Latin Capital Letter A With Ring Above"),
            new AnsiTable(198, '\u00C6', 'Æ', "Latin Capital Ligature Ae"),
            new AnsiTable(199, '\u00C7', 'Ç', "Latin Capital Letter C With Cedilla"),
            new AnsiTable(200, '\u00C8', 'È', "Latin Capital Letter E With Grave"),
            new AnsiTable(201, '\u00C9', 'É', "Latin Capital Letter E With Acute"),
            new AnsiTable(202, '\u00CA', 'Ê', "Latin Capital Letter E With Circumflex"),
            new AnsiTable(203, '\u00CB', 'Ë', "Latin Capital Letter E With Diaeresis"),
            new AnsiTable(204, '\u00CC', 'Ì', "Latin Capital Letter I With Grave"),
            new AnsiTable(205, '\u00CD', 'Í', "Latin Capital Letter I With Acute"),
            new AnsiTable(206, '\u00CE', 'Î', "Latin Capital Letter I With Circumflex"),
            new AnsiTable(207, '\u00CF', 'Ï', "Latin Capital Letter I With Diaeresis"),
            new AnsiTable(208, '\u00D0', 'Ð', "Latin Capital Letter Eth"),
            new AnsiTable(209, '\u00D1', 'Ñ', "Latin Capital Letter N With Tilde"),
            new AnsiTable(210, '\u00D2', 'Ò', "Latin Capital Letter O With Grave"),
            new AnsiTable(211, '\u00D3', 'Ó', "Latin Capital Letter O With Acute"),
            new AnsiTable(212, '\u00D4', 'Ô', "Latin Capital Letter O With Circumflex"),
            new AnsiTable(213, '\u00D5', 'Õ', "Latin Capital Letter O With Tilde"),
            new AnsiTable(214, '\u00D6', 'Ö', "Latin Capital Letter O With Diaeresis"),
            new AnsiTable(215, '\u00D7', '×', "Multiplication Sign"),
            new AnsiTable(216, '\u00D8', 'Ø', "Latin Capital Letter O With Stroke"),
            new AnsiTable(217, '\u00D9', 'Ù', "Latin Capital Letter U With Grave"),
            new AnsiTable(218, '\u00DA', 'Ú', "Latin Capital Letter U With Acute"),
            new AnsiTable(219, '\u00DB', 'Û', "Latin Capital Letter U With Circumflex"),
            new AnsiTable(220, '\u00DC', 'Ü', "Latin Capital Letter U With Diaeresis"),
            new AnsiTable(221, '\u00DD', 'Ý', "Latin Capital Letter Y With Acute"),
            new AnsiTable(222, '\u00DE', 'Þ', "Latin Capital Letter Thorn"),
            new AnsiTable(223, '\u00DF', 'ß', "Latin Small Letter Sharp S"),
            new AnsiTable(224, '\u00E0', 'à', "Latin Small Letter A With Grave"),
            new AnsiTable(225, '\u00E1', 'á', "Latin Small Letter A With Acute"),
            new AnsiTable(226, '\u00E2', 'â', "Latin Small Letter A With Circumflex"),
            new AnsiTable(227, '\u00E3', 'ã', "Latin Small Letter A With Tilde"),
            new AnsiTable(228, '\u00E4', 'ä', "Latin Small Letter A With Diaeresis"),
            new AnsiTable(229, '\u00E5', 'å', "Latin Small Letter A With Ring Above"),
            new AnsiTable(230, '\u00E6', 'æ', "Latin Small Ligature Ae"),
            new AnsiTable(231, '\u00E7', 'ç', "Latin Small Letter C With Cedilla"),
            new AnsiTable(232, '\u00E8', 'è', "Latin Small Letter E With Grave"),
            new AnsiTable(233, '\u00E9', 'é', "Latin Small Letter E With Acute"),
            new AnsiTable(234, '\u00EA', 'ê', "Latin Small Letter E With Circumflex"),
            new AnsiTable(235, '\u00EB', 'ë', "Latin Small Letter E With Diaeresis"),
            new AnsiTable(236, '\u00EC', 'ì', "Latin Small Letter I With Grave"),
            new AnsiTable(237, '\u00ED', 'í', "Latin Small Letter I With Acute"),
            new AnsiTable(238, '\u00EE', 'î', "Latin Small Letter I With Circumflex"),
            new AnsiTable(239, '\u00EF', 'ï', "Latin Small Letter I With Diaeresis"),
            new AnsiTable(240, '\u00F0', 'ð', "Latin Small Letter Eth"),
            new AnsiTable(241, '\u00F1', 'ñ', "Latin Small Letter N With Tilde"),
            new AnsiTable(242, '\u00F2', 'ò', "Latin Small Letter O With Grave"),
            new AnsiTable(243, '\u00F3', 'ó', "Latin Small Letter O With Acute"),
            new AnsiTable(244, '\u00F4', 'ô', "Latin Small Letter O With Circumflex"),
            new AnsiTable(245, '\u00F5', 'õ', "Latin Small Letter O With Tilde"),
            new AnsiTable(246, '\u00F6', 'ö', "Latin Small Letter O With Diaeresis"),
            new AnsiTable(247, '\u00F7', '÷', "Division Sign"),
            new AnsiTable(248, '\u00F8', 'ø', "Latin Small Letter O With Stroke"),
            new AnsiTable(249, '\u00F9', 'ù', "Latin Small Letter U With Grave"),
            new AnsiTable(250, '\u00FA', 'ú', "Latin Small Letter U With Acute"),
            new AnsiTable(251, '\u00FB', 'û', "Latin Small Letter U With Circumflex"),
            new AnsiTable(252, '\u00FC', 'ü', "Latin Small Letter U With Diaeresis"),
            new AnsiTable(253, '\u00FD', 'ý', "Latin Small Letter Y With Acute"),
            new AnsiTable(254, '\u00FE', 'þ', "Latin Small Letter Thorn"),
            new AnsiTable(255, '\u00FF', 'ÿ', "Latin Small Letter Y With Diaeresis")
        ];
    }

    public readonly struct AnsiTable(int number, char unicode, char ch, string description)
    {
        public readonly int Number => number;

        public readonly string Hex => number.ToString("X2", CultureInfo.InvariantCulture);

        public readonly char UnicodeValue => unicode;

        public readonly string UnicodeText => $"U+{unicode:X4}";

        public readonly char Char => ch;

        public readonly string Description => description;
    }
}
