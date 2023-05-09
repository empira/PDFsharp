// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#define RotisSerifPro
#define xRotisSemiSansPro

#define regular
#define xbold
#define xitalic

#define ansi

using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Quality;

#pragma warning disable 1591
namespace PdfSharp.Features
{
    public class RotisWinAnsiTester : Feature
    {
        public void RotisWinAnsiTest()
        {
            RenderSnippetAsPdf(new RotisWinAnsiSnippet());
        }
    }

    public class RotisWinAnsiSnippet : Snippet
    {
        static readonly IFontResolver Fs = new RotisFontResolver();

        public override void RenderSnippet(XGraphics gfx)
        {
            GlobalFontSettings.FontResolver = Fs;

#if ansi
            var pdfOptions = XPdfFontOptions.WinAnsiDefault;
#else
            var pdfOptions = XPdfFontOptions.UnicodeDefault;
#endif

            var defaultFont = new XFont("Calibri", 10, XFontStyleEx.Underline, pdfOptions);

            var rotisSize = 30;
            XFont font;


#if RotisSerifPro
            gfx.DrawString("Rotis Serif Pro", defaultFont, XBrushes.Black, 40, 40);

#if regular
            gfx.DrawString("regular", defaultFont, XBrushes.Black, 40, 80);

            font = new XFont("Rotis Serif Pro", rotisSize, XFontStyleEx.Regular, pdfOptions);
            gfx.DrawString("Rotis Serif Pro Regular: kviec", font, XBrushes.Black, 40, 120);
#endif


#if bold
            gfx.DrawString("bold", defaultFont, XBrushes.Black, 40, 160);
#else
            gfx.DrawString("bold simulated", defaultFont, XBrushes.Black, 40, 160);
#endif

#if bold || regular
            font = new XFont("Rotis Serif Pro", rotisSize, XFontStyleEx.Bold, pdfOptions);
            gfx.DrawString("Rotis Serif Pro Bold: kviec", font, XBrushes.Black, 40, 200);
#endif


#if italic
            gfx.DrawString("italic", defaultFont, XBrushes.Black, 40, 240);
#else
            gfx.DrawString("italic simulated", defaultFont, XBrushes.Black, 40, 240);
#endif

#if italic || regular
            font = new XFont("Rotis Serif Pro", rotisSize, XFontStyleEx.Italic, pdfOptions);
            gfx.DrawString("Rotis Serif Pro Italic: kviec", font, XBrushes.Black, 40, 280);
#endif

#endif


#if RotisSemiSansPro
            gfx.DrawString("Rotis Semi Sans Pro", defaultFont, XBrushes.Black, 40, 340);

#if regular
            gfx.DrawString("regular", defaultFont, XBrushes.Black, 40, 380);

            font = new XFont("Rotis Semi Sans Pro", rotisSize, XFontStyleEx.Regular, pdfOptions);
            gfx.DrawString("Rotis Semi Sans Pro Regular: kviec", font, XBrushes.Black, 40, 420);
#endif


#if bold
            gfx.DrawString("bold", defaultFont, XBrushes.Black, 40, 460);
#else
            gfx.DrawString("bold simulated", defaultFont, XBrushes.Black, 40, 460);
#endif

#if bold || regular
            font = new XFont("Rotis Semi Sans Pro", rotisSize, XFontStyleEx.Bold, pdfOptions);
            gfx.DrawString("Rotis Semi Sans Pro Bold: kviec", font, XBrushes.Black, 40, 500);
#endif


#if italic
            gfx.DrawString("italic", defaultFont, XBrushes.Black, 40, 540);
#else
            gfx.DrawString("italic simulated", defaultFont, XBrushes.Black, 40, 540);
#endif

#if italic || regular
            font = new XFont("Rotis Semi Sans Pro", rotisSize, XFontStyleEx.Italic, pdfOptions);
            gfx.DrawString("Rotis Semi Sans Pro Italic: kviec", font, XBrushes.Black, 40, 580);
#endif

#endif
        }
    }

    public class RotisFontResolver : IFontResolver
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// The font family names that can be used in the constructor of XFont.
        /// Used in the first parameter of ResolveTypeface.
        /// Family names are given in lower case because the implementation of this FontResolver ignores case.
        /// </summary>
        static class FamilyNames
        {
#if RotisSerifPro
            public const string RotisSerifPro = "rotis serif pro";
#endif

#if RotisSemiSansPro
            public const string RotisSemiSansPro = "rotis semi sans pro";
#endif
        }

        /// <summary>
        /// The internal names that uniquely identify a font's type faces (i.e. a physical font file).
        /// Used in the first parameter of the FontResolverInfo constructor.
        /// </summary>
        static class FaceNames
        {
            /// Used in the first parameter of the FontResolverInfo constructor.

#if RotisSerifPro
            public const string RotisSerifPro = "RotisSerifPro";
            public const string RotisSerifProBold = "RotisSerifProBold";
            public const string RotisSerifProItalic = "RotisSerifProItalic";
#endif

#if RotisSemiSansPro
            public const string RotisSemiSansPro = "RotisSemiSansPro";
            public const string RotisSemiSansProBold = "RotisSemiSansProBold";
            public const string RotisSemiSansProItalic = "RotisSemiSansProItalic";
#endif
        }

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Converts specified information about a required typeface into a specific font.
        /// </summary>
        /// <param name="familyName">Name of the font family.</param>
        /// <param name="isBold">Set to <c>true</c> when a bold fontface is required.</param>
        /// <param name="isItalic">Set to <c>true</c> when an italic fontface is required.</param>
        /// <returns>
        /// Information about the physical font, or null if the request cannot be satisfied.
        /// </returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Note: PDFsharp calls ResolveTypeface only once for each unique combination
            // of familyName, isBold, and isItalic.

            // In this sample we use 6 fonts from the Segoe font family which come with the
            // Windows Phone SDK. These fonts are pretty and well designed and their
            // font files are much smaller than their Windows counterparts.

            // What this implementation do:
            // * if both isBold and isItalic is false the regular font is resolved
            // * if isBold is true either a bolder font is resolved or the bold request is ignored
            // * if isItalic is true italic simulation is turned on because there is no italic font
            //
            // Currently there are two minor design flaws/bugs in PDFsharp that will be fixed later:
            // If the same font is used with and without italic simulation two subsets of it 
            // are embedded in the PDF file (instead of one subset with the glyphs of both usages).
            // If an XFont is italic and the resolved font is not an italic font, italic simulation is
            // always used (i.e. you cannot turn italic simulation off in ResolveTypeface).
            // One more thing: TrueType font collections are also not yet supported.

            string lowerFamilyName = familyName.ToLower();


            //return PlatformFontResolver.ResolveTypeface("Calibri", isBold, isItalic);

            // Looking for a Rotis font?
            if (lowerFamilyName.StartsWith("rotis", StringComparison.Ordinal))
            {
                var simulateBold = false;
                var simulateItalic = false;


                string? faceName = null;

                // In this sample family names are case sensitive. You can relax this in your own implementation
                // and make them case insensitive.
                switch (lowerFamilyName)
                {
#if RotisSerifPro
                    case FamilyNames.RotisSerifPro:
#if regular
                        faceName = FaceNames.RotisSerifPro;
#endif

                        if (isBold)
                        {
#if bold
                            faceName = FaceNames.RotisSerifProBold;
#else
                            simulateBold = true;
#endif
                            if (isItalic)
                                simulateItalic = true;
                        }
                        else if (isItalic)
                        {
#if italic
                            faceName = FaceNames.RotisSerifProItalic;
#else
                            simulateItalic = true;
#endif
                        }
                        break;
#endif


#if RotisSemiSansPro
                    case FamilyNames.RotisSemiSansPro:
#if regular
                        faceName = FaceNames.RotisSemiSansPro;
#endif

                        if (isBold)
                        {
#if bold
                            faceName = FaceNames.RotisSemiSansProBold;
#else
                            simulateBold = true;
#endif
                            if (isItalic)
                                simulateItalic = true;
                        }
                        else if (isItalic)
                        {
#if italic
                            faceName = FaceNames.RotisSemiSansProItalic;
#else
                            simulateItalic = true;
#endif
                        }
                        break;
#endif

                    default:
                        Debug.Assert(false, "Unknown Rotis font: " + familyName);
                        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
                        //goto case FamilyNames.RotisSerifPro;  // Alternatively throw an exception in this case.
                }

                if (faceName != null)
                    return new FontResolverInfo(faceName, simulateBold, simulateItalic);
            }

            // Return null means that the typeface cannot be resolved and PDFsharp stops working.
            // Alternatively forward call to PlatformFontResolver.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        /// <summary>
        /// Gets the bytes of a physical font with specified face name.
        /// </summary>
        /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
        /// <returns>
        /// The bytes of the font.
        /// </returns>
        public byte[] GetFont(string faceName)
        {
            // Note: PDFsharp never calls GetFont twice with the same face name.

            // Return the bytes of a font.
            switch (faceName)
            {
#if RotisSerifPro

#if regular
                case FaceNames.RotisSerifPro:
                    return RotisFontDataHelper.RotisSerifPro;
#endif

#if bold
                case FaceNames.RotisSerifProBold:
                    return RotisFontDataHelper.RotisSerifProBold;
#endif

#if italic
                case FaceNames.RotisSerifProItalic:
                    return RotisFontDataHelper.RotisSerifProItalic;
#endif

#endif


#if RotisSemiSansPro

#if regular
                case FaceNames.RotisSemiSansPro:
                    return RotisFontDataHelper.RotisSemiSansPro;
#endif

#if bold
                case FaceNames.RotisSemiSansProBold:
                    return RotisFontDataHelper.RotisSemiSansProBold;
#endif

#if italic
                case FaceNames.RotisSemiSansProItalic:
                    return RotisFontDataHelper.RotisSemiSansProItalic;
#endif

#endif
            }
            // PDFsharp never calls GetFont with a face name that was not returned by ResolveTypeface.
            throw new ArgumentException(String.Format("Invalid face name '{0}'", faceName));
        }
    }

    public class RotisFontDataHelper
    {
#if RotisSerifPro

#if regular
        public static byte[] RotisSerifPro
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSerifPro-Regular.otf"); }
        }
#endif

#if bold
        public static byte[] RotisSerifProBold
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSerifPro-Bold.otf"); }
        }
#endif

#if italic
        public static byte[] RotisSerifProItalic
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSerifPro-Italic.otf"); }
        }
#endif

#endif


#if RotisSemiSansPro

#if regular
        public static byte[] RotisSemiSansPro
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSemiSansPro-Regular.otf"); }
        }
#endif

#if bold
        public static byte[] RotisSemiSansProBold
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSemiSansPro-Bold.otf"); }
        }
#endif

#if italic
        public static byte[] RotisSemiSansProItalic
        {
            get { return LoadFontData("PdfSharp.Features.Fonts.Rotis.RotisSemiSansPro-Italic.otf"); }
        }
#endif

#endif

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        static byte[] LoadFontData(string name)
        {
            var assembly = typeof(RotisFontDataHelper).Assembly;

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                var count = (int)stream.Length;
                var data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }
}
