// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Text;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// The PDF font descriptor flags.
    /// </summary>
    [Flags]
    enum PdfFontDescriptorFlags
    {
        /// <summary>
        /// All glyphs have the same width (as opposed to proportional or variable-pitch
        /// fonts, which have different widths).
        /// </summary>
        FixedPitch = 1 << 0,

        /// <summary>
        /// Glyphs have serifs, which are short strokes drawn at an angle on the top and
        /// bottom of glyph stems. (Sans serif fonts do not have serifs.)
        /// </summary>
        Serif = 1 << 1,

        /// <summary>
        /// Font contains glyphs outside the Adobe standard Latin character set. This
        /// flag and the Nonsymbolic flag cannot both be set or both be clear.
        /// </summary>
        Symbolic = 1 << 2,

        /// <summary>
        /// Glyphs resemble cursive handwriting.
        /// </summary>
        Script = 1 << 3,

        /// <summary>
        /// Font uses the Adobe standard Latin character set or a subset of it.
        /// </summary>
        Nonsymbolic = 1 << 5,

        /// <summary>
        /// Glyphs have dominant vertical strokes that are slanted.
        /// </summary>
        Italic = 1 << 6,

        /// <summary>
        /// Font contains no lowercase letters; typically used for display purposes,
        /// such as for titles or headlines.
        /// </summary>
        AllCap = 1 << 16,

        /// <summary>
        /// Font contains both uppercase and lowercase letters. The uppercase letters are
        /// similar to those in the regular version of the same typeface family. The glyphs
        /// for the lowercase letters have the same shapes as the corresponding uppercase
        /// letters, but they are sized and their proportions adjusted so that they have the
        /// same size and stroke weight as lowercase glyphs in the same typeface family.
        /// </summary>
        SmallCap = 1 << 17,

        /// <summary>
        /// Determines whether bold glyphs are painted with extra pixels even at very small
        /// text sizes.
        /// </summary>
        ForceBold = 1 << 18,
    }

    /// <summary>
    /// A PDF font descriptor specifies metrics and other attributes of a simple font, 
    /// as distinct from the metrics of individual glyphs.
    /// </summary>
    public sealed class PdfFontDescriptor : PdfDictionary
    {
        internal PdfFontDescriptor(PdfDocument document, OpenTypeDescriptor otDescriptor)
            : base(document)
        {
            Descriptor = otDescriptor;
            _cmapInfo = new CMapInfo(otDescriptor);

            Elements.SetName(Keys.Type, "/FontDescriptor");

            Elements.SetInteger(Keys.Ascent, Descriptor.DesignUnitsToPdf(Descriptor.Ascender));
            Elements.SetInteger(Keys.CapHeight, Descriptor.DesignUnitsToPdf(Descriptor.CapHeight));
            Elements.SetInteger(Keys.Descent, Descriptor.DesignUnitsToPdf(Descriptor.Descender));
            Elements.SetInteger(Keys.Flags, (int)FlagsFromDescriptor(Descriptor));
            Elements.SetRectangle(Keys.FontBBox, new PdfRectangle(
              Descriptor.DesignUnitsToPdf(Descriptor.XMin),
              Descriptor.DesignUnitsToPdf(Descriptor.YMin),
              Descriptor.DesignUnitsToPdf(Descriptor.XMax),
              Descriptor.DesignUnitsToPdf(Descriptor.YMax)));
            // not here, done in PdfFont later... 
            //Elements.SetName(Keys.FontName, "abc"); //descriptor.FontName);
            Elements.SetReal(Keys.ItalicAngle, Descriptor.ItalicAngle);
            Elements.SetInteger(Keys.StemV, Descriptor.StemV);
            Elements.SetInteger(Keys.XHeight, Descriptor.DesignUnitsToPdf(Descriptor.XHeight));
        }

        //HACK OpenTypeDescriptor descriptor
        internal OpenTypeDescriptor Descriptor { get; }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string FontName
        {
            get => Elements.GetName(Keys.FontName);
            set => Elements.SetName(Keys.FontName, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is symbol font.
        /// </summary>
        public bool IsSymbolFont { get; private set; }

        // HACK FlagsFromDescriptor(OpenTypeDescriptor descriptor)
        PdfFontDescriptorFlags FlagsFromDescriptor(OpenTypeDescriptor descriptor)
        {
            PdfFontDescriptorFlags flags = 0;
            IsSymbolFont = descriptor.IsSymbolFont;
            flags |= descriptor.IsSymbolFont ? PdfFontDescriptorFlags.Symbolic : PdfFontDescriptorFlags.Nonsymbolic;
            return flags;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cmap table must be added to the
        /// font subset.
        /// </summary>
        internal bool AddCmapTable { get; set; }
        
        /// <summary>
        /// Gets the CMapInfo for PDF font descriptor.
        /// It contains all characters, ANSI and Unicode.
        /// </summary>
        internal CMapInfo CMapInfo
        {
            get => _cmapInfo ?? NRT.ThrowOnNull<CMapInfo>();
            //set => _cmapInfo2 = value;
        }
        CMapInfo _cmapInfo;

        internal override void PrepareForSave()
        {
            // Shared by ANSI and Unicode encoded PDF fonts. So we maybe get called twice.
            if (_prepared)
                return;
            _prepared = true;
            
            var pdfFontFile = new PdfFontProgram(Owner);
            pdfFontFile.CreateFontFileAndAddToDescriptor(this, _cmapInfo, !AddCmapTable);
        }
        bool _prepared;

        /// <summary>
        /// Adds a tag of exactly six uppercase letters to the font name 
        /// according to PDF Reference Section 5.5.3 'Font Subsets'.
        /// </summary>
        internal string CreateEmbeddedFontSubsetName(string name)
        {
        TryAgain:
            var s = new StringBuilder(64);
            byte[] bytes = Guid.NewGuid().ToByteArray();
            for (int idx = 0; idx < 6; idx++)
                s.Append((char)('A' + bytes[idx] % 26));
            s.Append('+');
            if (name.StartsWith("/", StringComparison.Ordinal))
                s.Append(name.Substring(1));
            else
                s.Append(name);
            var newName = s.ToString();
            // The probability is low for a single document
            // with a handful of fonts, but it is better to check.
#if NET6_0_OR_GREATER_
            if (!_fontSubsetNames.TryAdd(newName, null))
                goto TryAgain;
#else
            if (_fontSubsetNames.ContainsKey(newName))
                goto TryAgain;
            _fontSubsetNames.Add(newName, null);
#endif
            return newName;
        }
        readonly Dictionary<string, object?> _fontSubsetNames = [];

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; must be
            /// FontDescriptor for a font descriptor.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "FontDescriptor")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The PostScript name of the font. This name should be the same as the 
            /// value of BaseFont in the font or CIDFont dictionary that refers to this font descriptor.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string FontName = "/FontName";

            /// <summary>
            /// (Optional; PDF 1.5; strongly recommended for Type 3 fonts in Tagged PDF documents)
            /// A string specifying the preferred font family name. For example, for the font 
            /// Times Bold Italic, the FontFamily is Times.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string FontFamily = "/FontFamily";

            /// <summary>
            /// (Optional; PDF 1.5; strongly recommended for Type 3 fonts in Tagged PDF documents)
            /// The font stretch value. It must be one of the following names (ordered from 
            /// narrowest to widest): UltraCondensed, ExtraCondensed, Condensed, SemiCondensed, 
            /// Normal, SemiExpanded, Expanded, ExtraExpanded or UltraExpanded.
            /// Note: The specific interpretation of these values varies from font to font. 
            /// For example, Condensed in one font may appear most similar to Normal in another.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string FontStretch = "/FontStretch";

            /// <summary>
            /// (Optional; PDF 1.5; strongly recommended for Type 3 fonts in Tagged PDF documents)
            /// The weight (thickness) component of the fully-qualified font name or font specifier.
            /// The possible values are 100, 200, 300, 400, 500, 600, 700, 800, or 900, where each
            /// number indicates a weight that is at least as dark as its predecessor. A value of 
            /// 400 indicates a normal weight; 700 indicates bold.
            /// Note: The specific interpretation of these values varies from font to font. 
            /// For example, 300 in one font may appear most similar to 500 in another.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string FontWeight = "/FontWeight";

            /// <summary>
            /// (Required) A collection of flags defining various characteristics of the font.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Flags = "/Flags";

            /// <summary>
            /// (Required, except for Type 3 fonts) A rectangle (see Section 3.8.4, “Rectangles”),
            /// expressed in the glyph coordinate system, specifying the font bounding box. This 
            /// is the smallest rectangle enclosing the shape that would result if all of the 
            /// glyphs of the font were placed with their origins coincident and then filled.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Required)]
            public const string FontBBox = "/FontBBox";

            /// <summary>
            /// (Required) The angle, expressed in degrees counterclockwise from the vertical, of
            /// the dominant vertical strokes of the font. (For example, the 9-o’clock position is 90 
            /// degrees, and the 3-o’clock position is –90 degrees.) The value is negative for fonts 
            /// that slope to the right, as almost all italic fonts do.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string ItalicAngle = "/ItalicAngle";

            /// <summary>
            /// (Required, except for Type 3 fonts) The maximum height above the baseline reached 
            /// by glyphs in this font, excluding the height of glyphs for accented characters.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string Ascent = "/Ascent";

            /// <summary>
            /// (Required, except for Type 3 fonts) The maximum depth below the baseline reached 
            /// by glyphs in this font. The value is a negative number.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string Descent = "/Descent";

            /// <summary>
            /// (Optional) The spacing between baselines of consecutive lines of text.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string Leading = "/Leading";

            /// <summary>
            /// (Required for fonts that have Latin characters, except for Type 3 fonts) The vertical
            /// coordinate of the top of flat capital letters, measured from the baseline.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string CapHeight = "/CapHeight";

            /// <summary>
            /// (Optional) The font’s x height: the vertical coordinate of the top of flat nonascending
            /// lowercase letters (like the letter x), measured from the baseline, in fonts that have 
            /// Latin characters. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string XHeight = "/XHeight";

            /// <summary>
            /// (Required, except for Type 3 fonts) The thickness, measured horizontally, of the dominant 
            /// vertical stems of glyphs in the font.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Required)]
            public const string StemV = "/StemV";

            /// <summary>
            /// (Optional) The thickness, measured vertically, of the dominant horizontal stems 
            /// of glyphs in the font. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string StemH = "/StemH";

            /// <summary>
            /// (Optional) The average width of glyphs in the font. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string AvgWidth = "/AvgWidth";

            /// <summary>
            /// (Optional) The maximum width of glyphs in the font. Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string MaxWidth = "/MaxWidth";

            /// <summary>
            /// (Optional) The width to use for character codes whose widths are not specified in a 
            /// font dictionary’s Widths array. This has a predictable effect only if all such codes 
            /// map to glyphs whose actual widths are the same as the value of the MissingWidth entry.
            /// Default value: 0.
            /// </summary>
            [KeyInfo(KeyType.Real | KeyType.Optional)]
            public const string MissingWidth = "/MissingWidth";

            /// <summary>
            /// (Optional) A stream containing a Type 1 font program.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string FontFile = "/FontFile";

            /// <summary>
            /// (Optional; PDF 1.1) A stream containing a TrueType font program.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string FontFile2 = "/FontFile2";

            /// <summary>
            /// (Optional; PDF 1.2) A stream containing a font program whose format is specified 
            /// by the Subtype entry in the stream dictionary.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string FontFile3 = "/FontFile3";

            /// <summary>
            /// (Optional; meaningful only in Type 1 fonts; PDF 1.1) A string listing the character
            /// names defined in a font subset. The names in this string must be in PDF syntax—that is,
            /// each name preceded by a slash (/). The names can appear in any order. The name .notdef
            /// should be omitted; it is assumed to exist in the font subset. If this entry is absent,
            /// the only indication of a font subset is the subset tag in the FontName entry.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string CharSet = "/CharSet";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get
                {
                    if (_meta == null)
                        _meta = CreateMeta(typeof(Keys));
                    return _meta;
                }
            }

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
