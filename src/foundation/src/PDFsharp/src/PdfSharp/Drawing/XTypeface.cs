// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Represents a combination of XFontFamily, XFontWeight, XFontStyleEx, and XFontStretch.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class XTypeface  // Note: In English, it’s spelled 'typeface', but 'font face'.
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XTypeface"/> class.
        /// </summary>
        /// <param name="typefaceName">Name of the typeface.</param>
        public XTypeface(string typefaceName) : 
            this(new XFontFamily(typefaceName), XFontStyles.Normal, XFontWeights.Normal, XFontStretches.Normal)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XTypeface"/> class.
        /// </summary>
        /// <param name="family">The font family of the typeface.</param>
        /// <param name="style">The style of the typeface.</param>
        /// <param name="weight">The relative weight of the typeface.</param>
        /// <param name="stretch">The degree to which the typeface is stretched.</param>
        public XTypeface(XFontFamily family, XFontStyle style, XFontWeight weight, XFontStretch stretch)
        {
            Family = family;
            Style = style;
            Weight = weight;
            Stretch = stretch;
        }

        /// <summary>
        /// Gets the font family from which the typeface was constructed.
        /// </summary>
        public XFontFamily Family { get; }

        /// <summary>
        /// Gets the style of the Typeface.
        /// </summary>
        public XFontStyle Style { get; }

        /// <summary>
        /// Gets the relative weight of the typeface.
        /// </summary>
        public XFontWeight Weight { get; }

        /// <summary>
        /// Gets the stretch value for the Typeface.
        /// The stretch value determines whether a typeface is expanded or condensed when it is displayed.
        /// </summary>
        public XFontStretch Stretch { get; }

        /// <summary>
        /// Tries the get GlyphTypeface that corresponds to the Typeface.
        /// </summary>
        /// <param name="glyphTypeface">The glyph typeface that corresponds to this typeface,
        /// or null if the typeface was constructed from a composite font.
        /// </param>
        public bool TryGetGlyphTypeface(out XGlyphTypeface? glyphTypeface)
        {
            Debug.Assert(false, "Should not yet come here.");
            glyphTypeface = null;
            return false;
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        string DebuggerDisplay => String.Format(CultureInfo.InvariantCulture, "XTypeface");
    }
}

/*
   Properties of WPF
 
   CapsHeight	
   Gets the distance from the baseline to the top of an English capital letter for the typeface.
   
   FaceNames	
   Gets a collection of culture-specific names for the Typeface.
   
   FontFamily	
   Gets the name of the font family from which the typeface was constructed.
   
   IsBoldSimulated	
   Determines whether to simulate a bold weight for the glyphs represented by the Typeface.
   
   IsObliqueSimulated	
   Determines whether to simulate an italic style for the glyphs represented by the Typeface.
   
   Stretch	
   Gets the stretch value for the Typeface. The stretch value determines whether a typeface is expanded or condensed when it is displayed.
   
   StrikethroughPosition	
   Gets a value that indicates the distance from the baseline to the strikethrough for the typeface.
   
   StrikethroughThickness	
   Gets a value that indicates the thickness of the strikethrough relative to the font em size.
   
   Style	
   Gets the style of the Typeface.
   
   UnderlinePosition	
   Gets a value that indicates the distance of the underline from the baseline for the typeface.
   
   UnderlineThickness	
   Gets a value that indicates the thickness of the underline relative to the font em size for the typeface.
   
   Weight	
   Gets the relative weight of the typeface.
   
   XHeight	
   Gets the distance from the baseline to the top of an English lowercase letter for a typeface. The distance excludes ascenders.

*/