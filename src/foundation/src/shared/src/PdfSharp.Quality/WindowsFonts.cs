// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Internal.OpenType;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Quality
{
    public static class WindowsFonts
    {
        public const string ArialRegular = "arial.ttf";
        public const string ArialBold = "arialbd.ttf";
        public const string ArialItalic = "ariali.ttf";
        public const string ArialBoldItalic = "arialbi.ttf";
        public const string ArialNarrowRegular = "ARIALN.TTF";
        public const string ArialNarrowBold = "ARIALNB.TTF";
        public const string ArialNarrowItalic = "ARIALNI.TTF";
        public const string ArialNarrowBoldItalic = "ARIALNBI.TTF";
        public const string AriaBlack = "ariblk.ttf";
        public const string ArialRoundedBold = "ARLRDBD.TTF";

        public const string TimesRegular = "times.ttf";
        public const string TimesBold = "timesbd.ttf";
        public const string TimesItalic = "timesi.ttf";
        public const string TimesBoldItalic = "timesbi.ttf";

        public const string CourierRegular = "cour.ttf";
        public const string CourierBold = "courbd.ttf";
        public const string CourierItalic = "couri.ttf";
        public const string CourierBoldItalic = "courbi.ttf";

        //public IEnumerator<string> ArialFonts : IEnumerator<string>, IEnumerable<string>
        //{
        //    yield return Ari
        //}

        public class ArialFonts : IEnumerable<string>
        {
            public IEnumerator<String> GetEnumerator()
            {
                yield return ArialRegular;
                yield return ArialBold;
                yield return ArialItalic;
                yield return ArialBoldItalic;
                yield return ArialNarrowRegular;
                yield return ArialNarrowBold;
                yield return ArialNarrowItalic;
                yield return ArialNarrowBoldItalic;
                yield return AriaBlack;
                yield return ArialRoundedBold;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public class TimesFonts : IEnumerable<string>
        {
            public IEnumerator<String> GetEnumerator()
            {
                yield return TimesRegular;
                yield return TimesBold;
                yield return TimesItalic;
                yield return TimesBoldItalic;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public class CourierFonts : IEnumerable<string>
        {
            public IEnumerator<String> GetEnumerator()
            {
                yield return CourierRegular;
                yield return CourierBold;
                yield return CourierItalic;
                yield return CourierBoldItalic;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static void RegisterArialTimesCourier(ref OpenTypeFontRegistry registry)
        {
            const string windowsFontsPath = "C:/Windows/Fonts/";

            // ----- Arial fonts -----

            // Keep original values for Arial
            var fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialRegular))!;
            registry.RegisterFont(fontSource);

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialBold))!;
            registry.RegisterFont(fontSource);

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialItalic))!;
            registry.RegisterFont(fontSource);

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialBoldItalic))!;
            registry.RegisterFont(fontSource);

            // Adjust family name for Arial Narrow.
            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialNarrowRegular))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Narrow");
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Narrow");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialNarrowBold))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Narrow Bold");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialNarrowItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Narrow Italic");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialNarrowBoldItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Narrow Bold Italic");

            // Adjust family name and weight for Arial Black.
            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + AriaBlack))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial", "Black", null, OpenTypeFontWeight.Black);

            // Adjust family name for Arial Rounded.
            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + ArialRoundedBold))!;
            registry.RegisterFont(fontSource.OTFontFace, "Arial Rounded");

            // ----- Times New Roman fonts -----

            // Adjust family name for Times New Roman to Times.
            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + TimesRegular))!;
            registry.RegisterFont(fontSource.OTFontFace, "Times");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + TimesBold))!;
            registry.RegisterFont(fontSource.OTFontFace, "Times");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + TimesItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Times");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + TimesBoldItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Times");

            // ----- Courier New fonts -----

            // Adjust family name for Courier New to Courier.
            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + CourierRegular))!;
            registry.RegisterFont(fontSource.OTFontFace, "Courier");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + CourierBold))!;
            registry.RegisterFont(fontSource.OTFontFace, "Courier");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + CourierItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Courier");

            fontSource = OpenTypeFontSource.GetOrCreateFrom(new(windowsFontsPath + CourierBoldItalic))!;
            registry.RegisterFont(fontSource.OTFontFace, "Courier");
        }
    }
}
