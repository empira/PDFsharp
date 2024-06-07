// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591
namespace PdfSharp.Features
{
    public class PdfSharpFeatures
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// List of names of reviewed feature classes and it functions.
        /// </summary>
        public static class Names
        {
            // Drawing/paths
            public const string Drawing_paths_Paths__PathCurves = "Drawing/paths/Paths/Paths:PathCurves";
            public const string Drawing_paths_Paths__PathMisc = "Drawing/paths/Paths/Paths:PathMisc";
            public const string Drawing_paths_Paths__PathShapes = "Drawing/paths/Paths/Paths:PathShapes";
            public const string Drawing_paths_Paths__PathText = "Drawing/paths/Paths/Paths:PathText";
            public const string Drawing_paths_Paths__PathWpf = "Drawing/paths/Paths/Paths:PathWpf";
            public const string Drawing_paths_Paths__RenderAllSnippets = "Drawing/paths/Paths/Paths:RenderAllSnippets";
            
            // Drawing/graphics
            public const string Drawing_graphics_GraphicsUnit__Downwards = "Drawing/graphics/GraphicsUnit:Downwards";
            public const string Drawing_graphics_GraphicsUnit__Upwards = "Drawing/graphics/GraphicsUnit:Upwards";

            // Drawing/text
            public const string Drawing_text_SurrogateChars__Surrogates = "Drawing/text/SurrogateChars:Surrogates";
            public const string Drawing_text_SymbolFonts__Symbols = "Drawing/text/SymbolFonts:Symbols";

            // Font/encoding
            public const string Font_encoding_Encodings_AnsiEncoding = "Font/encoding/Encodings:AnsiEncoding";
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// The features by their names.
        /// </summary>
        public static readonly Dictionary<string, FeatureAction> Features = new()
        {
            // Drawing/paths
            { Names.Drawing_paths_Paths__PathCurves,                new(Names.Drawing_paths_Paths__PathCurves, () => new Drawing.Paths().PathCurves()) },
            { Names.Drawing_paths_Paths__PathMisc,                  new(Names.Drawing_paths_Paths__PathMisc, () => new Drawing.Paths().PathMisc()) },
            { Names.Drawing_paths_Paths__PathShapes,                new(Names.Drawing_paths_Paths__PathShapes, () => new Drawing.Paths().PathShapes()) },
            { Names.Drawing_paths_Paths__PathText,                  new(Names.Drawing_paths_Paths__PathText, () => new Drawing.Paths().PathText()) },
            { Names.Drawing_paths_Paths__PathWpf,                   new(Names.Drawing_paths_Paths__PathWpf, () => new Drawing.Paths().PathWpf()) },
            //{ Names.Drawing_paths_Paths__RenderAllSnippets,         new(Names.Drawing_paths_Paths__RenderAllSnippets, () => new Drawing.Paths()..ras()) },
            
            // Drawing/graphics
            { Names.Drawing_graphics_GraphicsUnit__Downwards,       new(Names.Drawing_graphics_GraphicsUnit__Downwards, () => new Drawing.GraphicsUnit().Downwards()) },
            { Names.Drawing_graphics_GraphicsUnit__Upwards,         new(Names.Drawing_graphics_GraphicsUnit__Upwards, () => new Drawing.GraphicsUnit().Upwards()) },
            
            // Drawing/text
            { Names.Drawing_text_SurrogateChars__Surrogates,        new(Names.Drawing_text_SurrogateChars__Surrogates, () => new Drawing.SurrogateChars().Surrogates()) },
            { Names.Drawing_text_SymbolFonts__Symbols,              new(Names.Drawing_text_SymbolFonts__Symbols, () => new Drawing.SymbolFontFeature().Symbols()) },

            // Font/encoding
            { Names.Font_encoding_Encodings_AnsiEncoding,           new(Names.Font_encoding_Encodings_AnsiEncoding, () => new Font.Encodings().AnsiEncodingTable()) },

        };

        public FeatureAction this[string name] => Features[name];

        /// <summary>
        /// Delegate to a feature function.
        /// </summary>
        public readonly struct FeatureAction(string name, Action run)
        {
            public string Name { get; } = name;

            public Action Run { get; } = run;
        }
    }
}
