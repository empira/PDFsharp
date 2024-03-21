// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;

namespace PdfSharp.Snippets.Font
{
    // PDFsharp development
    // edf\src\internal\PDFsharp\snippets\font\fontresolving\ExoticFontsFontResolver.cs
    // Copy any changes to edf\src\public\PDFsharp\samples\core\FontResolver\SegoeWpFontResolver.cs
    // and remove 'Edf.WPFonts.'

    /// <summary>
    /// Maps font requests for a SegoeWP font to a bunch of 6 specific font files. These 6 fonts are embedded as resources in the WPFonts assembly.
    /// </summary>
    public class ExoticFontsFontResolver : IFontResolver
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The font family names that can be used in the constructor of XFont.
        /// Used in the first parameter of ResolveTypeface.
        /// Family names are given in lower case because the implementation of SegoeWpFontResolver ignores case.
        /// </summary>
        static class FamilyNames
        {
            public const string Oblivious = "oblivious";
            public const string XFiles = "xfiles";
        }

        /// <summary>
        /// The internal names that uniquely identify a font's type faces (i.e. a physical font file).
        /// Used in the first parameter of the FontResolverInfo constructor.
        /// </summary>
        static class FaceNames
        {
            /// Used in the first parameter of the FontResolverInfo constructor.
            public const string Oblivious = "Oblivious";
            public const string XFiles = "XFiles";
        }

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Converts specified information about a required typeface into a specific font.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="isBold">Set to <c>true</c> when a bold font face is required.</param>
        /// <param name="isItalic">Set to <c>true</c> when an italic font face is required.</param>
        /// <returns>
        /// Information about the physical font, or null if the request cannot be satisfied.
        /// </returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Bold simulation is not yet implemented in PDFsharp.
            const bool simulateBold = false;

            // Since all typefaces do not contain any italic typeface
            // always simulate italic if it is requested.
            bool simulateItalic = isItalic;

            string? faceName = null;

            // In this sample family names are case-sensitive. You can relax this in your own implementation
            // and make them case-insensitive.
            switch (familyName.ToLower())
            {
                case FamilyNames.Oblivious:
                    faceName = FaceNames.Oblivious;
                    break;

                case FamilyNames.XFiles:
                    faceName = FaceNames.XFiles;
                    break;
            }

            if (faceName != null)
                return new FontResolverInfo(faceName, simulateBold, simulateItalic);

            // Return null means that the typeface cannot be resolved and PDFsharp stops working.
            // Alternatively forward call to PlatformFontResolver.
            return FailsafeFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
        /// <returns>
        /// The bytes of the font.
        /// </returns>
        public byte[]? GetFont(string faceName)
        {
            // Note: PDFsharp never calls GetFont twice with the same face name.

            // Return the bytes of a font.
            return faceName switch
            {
                FaceNames.Oblivious => ExoticFontsDataHelper.Oblivious,
                FaceNames.XFiles => ExoticFontsDataHelper.XFiles,
                // PDFsharp never calls GetFont with a face name that was not returned by ResolveTypeface.
                _ => FailsafeFontResolver.GetFont(faceName)
            };
        }

        static readonly FailsafeFontResolver FailsafeFontResolver = new();
    }
}
