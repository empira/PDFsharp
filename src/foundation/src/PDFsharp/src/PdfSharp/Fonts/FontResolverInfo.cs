// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
#if CORE
//using System.Drawing;
#endif
#if GDI
//using System.Drawing;
#endif
#if WPF
//using System.Windows.Media;
#endif

namespace PdfSharp.Fonts
{
    // The English terms font, font family, typeface, glyph etc. are sometimes confusingly used.
    // Here a short clarification by Wikipedia.
    //
    // Wikipedia EN -> DE
    //     Font -> Schriftschnitt
    //     Computer font -> Font (Informationstechnik)
    //     Typeface (Font family) -> Schriftart / Schriftfamilie
    //     FontFace ->  
    //     Glyph -> Glyphe 
    // 
    // It seems that typeface and font family are synonyms in English.
    // In WPF a family name is used as a term for a bunch of fonts that share the same
    // characteristics, like Univers or Times New Roman.
    // In WPF a font face describes a request of a font of a particular font family, e.g.
    // Univers medium bold italic.
    // In WPF a glyph typeface is the result of requesting a typeface, i.e. a physical font
    // plus the information whether bold and/or italic should be simulated.
    // 
    // Wikipedia DE -> EN
    //     Schriftart -> Typeface
    //     Schriftschnitt -> Font
    //     Schriftfamilie -> ~   (means Font family)
    //     Schriftsippe -> Font superfamily
    //     Font -> Computer font
    // 
    // http://en.wikipedia.org/wiki/Font
    // http://en.wikipedia.org/wiki/Computer_font
    // http://en.wikipedia.org/wiki/Typeface
    // http://en.wikipedia.org/wiki/Glyph
    // http://en.wikipedia.org/wiki/Typographic_unit
    // 
    // FaceName: A unique and only internally used name of a glyph typeface. In other words the name of the font data that represents a specific font.
    // 
    // https://graphicdesign.stackexchange.com/questions/12717/what-is-the-difference-between-a-font-and-a-typeface
    // https://graphicdesign.stackexchange.com/questions/35619/difference-between-font-face-typeface-font-in-the-context-of-typography
    //

    /// <summary>
    /// Describes the physical font that must be used to render a particular XFont.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class FontResolverInfo
    {
        const string KeySuffix = ":frik";  // Font Resolver Info Key

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResolverInfo"/> struct.
        /// </summary>
        /// <param name="faceName">The name that uniquely identifies the font face.</param>
        public FontResolverInfo(string faceName) :
            this(faceName, false, false, 0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResolverInfo"/> struct.
        /// </summary>
        /// <param name="faceName">The name that uniquely identifies the font face.</param>
        /// <param name="mustSimulateBold">Set to <c>true</c> to simulate bold when rendered. Not implemented and must be false.</param>
        /// <param name="mustSimulateItalic">Set to <c>true</c> to simulate italic when rendered.</param>
        /// <param name="collectionNumber">Index of the font in a true type font collection.
        /// Not yet implemented and must be zero.
        /// </param>
        internal FontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic, int collectionNumber = 0)
        {
            if (String.IsNullOrEmpty(faceName))
                throw new ArgumentNullException(nameof(faceName));
            if (collectionNumber != 0)
                throw new NotImplementedException("collectionNumber is not yet implemented and must be 0.");

            FaceName = faceName;
            MustSimulateBold = mustSimulateBold;
            MustSimulateItalic = mustSimulateItalic;
            CollectionNumber = collectionNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResolverInfo"/> struct.
        /// </summary>
        /// <param name="faceName">The name that uniquely identifies the font face.</param>
        /// <param name="mustSimulateBold">Set to <c>true</c> to simulate bold when rendered. Not implemented and must be false.</param>
        /// <param name="mustSimulateItalic">Set to <c>true</c> to simulate italic when rendered.</param>
        public FontResolverInfo(string faceName, bool mustSimulateBold, bool mustSimulateItalic)
            : this(faceName, mustSimulateBold, mustSimulateItalic, 0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontResolverInfo" /> struct.
        /// </summary>
        /// <param name="faceName">The name that uniquely identifies the font face.</param>
        /// <param name="styleSimulations">The style simulation flags.</param>
        public FontResolverInfo(string faceName, XStyleSimulations styleSimulations)
            : this(faceName,
                  (styleSimulations & XStyleSimulations.BoldSimulation) == XStyleSimulations.BoldSimulation,
                  (styleSimulations & XStyleSimulations.ItalicSimulation) == XStyleSimulations.ItalicSimulation, 0)
        { }

        /// <summary>
        /// Gets the font resolver info key for this object.
        /// </summary>
        internal string Key =>
            _key ??= //KeyPrefix
                FaceName.ToLowerInvariant()
                + '/'
                + (MustSimulateBold ? 'B' : 'b')
                + (MustSimulateItalic ? 'I' : 'i')
                + KeySuffix;
        string? _key;

        /// <summary>
        /// A name that uniquely identifies the font face (not the family), e.g. the file name of the font. PDFsharp does not use this
        /// name internally, but passes it to the GetFont function of the IFontResolver interface to retrieve the font data.
        /// </summary>
        public string FaceName { get; }

        /// <summary>
        /// Indicates whether bold must be simulated.
        /// </summary>
        public bool MustSimulateBold { get; }

        /// <summary>
        /// Indicates whether italic must be simulated.
        /// </summary>
        public bool MustSimulateItalic { get; }

        /// <summary>
        /// Gets the style simulation flags.
        /// </summary>
        public XStyleSimulations StyleSimulations
            => (MustSimulateBold ? XStyleSimulations.BoldSimulation : 0) | (MustSimulateItalic ? XStyleSimulations.ItalicSimulation : 0);

        /// <summary>
        /// The number of the font in a TrueType font collection file. The number of the first font is 0.
        /// NOT YET IMPLEMENTED. Must be zero.
        /// </summary>
        internal int CollectionNumber { get; }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        internal string DebuggerDisplay =>
           $"FontResolverInfo: '{FaceName}',{(MustSimulateBold ? " simulate Bold" : "")}{(MustSimulateItalic ? " simulate Italic" : "")}";
    }
}
