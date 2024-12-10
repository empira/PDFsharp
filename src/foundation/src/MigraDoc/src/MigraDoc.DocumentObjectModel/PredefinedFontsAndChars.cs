// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace MigraDoc
{
    /// <summary>
    /// Defines the font family names of the fonts used for error messages and bullets in a MigraDoc document.
    /// </summary>
    public static class PredefinedFontsAndChars
    {
        static PredefinedFontsAndChars()
        {
            Initialize();
        }

        static void Initialize()
        {
            _errorFontName = "Courier New";
            _rtfDocumentInfoFontName = "Courier New";
        }

        /// <summary>
        /// Resets all properties to its initial values.
        /// </summary>
        public static void Reset()
        {
            Initialize();
            Bullets.Reset();
        }

        /// <summary>
        /// Get or sets the font name of the font that is used for error messages in a MigraDoc document.
        /// </summary>
        public static string ErrorFontName
        {
            get
            {
                if (_errorFontName == null)
                    throw new InvalidOperationException($"{nameof(PredefinedFontsAndChars)}.{nameof(ErrorFontName)} is not specified.");
                return _errorFontName;
            }
            set => _errorFontName = value;
        }
        static string? _errorFontName;

        /// <summary>
        /// Get or sets the font name of the font that is used to print the document information in an RTF document.
        /// </summary>
        public static string RtfDocumentInfoFontName
        {
            get
            {
                if (_rtfDocumentInfoFontName == null)
                    throw new InvalidOperationException($"{nameof(PredefinedFontsAndChars)}.{nameof(RtfDocumentInfoFontName)} is not specified.");
                return _rtfDocumentInfoFontName;
            }
            set => _rtfDocumentInfoFontName = value;
        }
        static string? _rtfDocumentInfoFontName;

        /// <summary>
        /// Defines the characters and the font family names of the fonts used for bullets in a MigraDoc document.
        /// </summary>
        public static class Bullets
        {
            static Bullets()
            {
                Initialize();
            }
            
            // ReSharper disable once MemberHidesStaticFromOuterClass
            static void Initialize()
            {
                _level1FontName = "Courier New";
                Level1Character = '●';
                _level2FontName = "Courier New";
                Level2Character = '○';
                _level3FontName = "Courier New";
                Level3Character = '▪';
            }

            /// <summary>
            /// Resets all properties to its initial values.
            /// </summary>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal static void Reset()
            {
                Initialize();
            }

            /// <summary>
            /// Get or sets the font name of the font that is used for bullets of list level 1 in a MigraDoc document.
            /// </summary>
            public static string Level1FontName
            {
                get
                {
                    if (_level1FontName == null)
                        throw new InvalidOperationException($"{nameof(PredefinedFontsAndChars)}.{nameof(Bullets)}.{nameof(Level1FontName)} is not specified.");
                    return _level1FontName;
                }
                set => _level1FontName = value;
            }
            static string? _level1FontName;

            /// <summary>
            /// Get or sets the character that is used for bullets of list level 1 in a MigraDoc document.
            /// </summary>
            public static char Level1Character { get; set; }

            /// <summary>
            /// Get or sets the font name of the font that is used for bullets of list level 2 in a MigraDoc document.
            /// </summary>
            public static string Level2FontName
            {
                get
                {
                    if (_level2FontName == null)
                        throw new InvalidOperationException($"{nameof(PredefinedFontsAndChars)}.{nameof(Bullets)}.{nameof(Level2FontName)} is not specified.");
                    return _level2FontName;
                }
                set => _level2FontName = value;
            }
            static string? _level2FontName;

            /// <summary>
            /// Get or sets the character that is used for bullets of list level 2 in a MigraDoc document.
            /// </summary>
            public static char Level2Character { get; set; }

            /// <summary>
            /// Get or sets the font name of the font that is used for bullets of list level 3 in a MigraDoc document.
            /// </summary>
            public static string Level3FontName
            {
                get
                {
                    if (_level3FontName == null)
                        throw new InvalidOperationException($"{nameof(PredefinedFontsAndChars)}.{nameof(Bullets)}.{nameof(Level3FontName)} is not specified.");
                    return _level3FontName;
                }
                set => _level3FontName = value;
            }
            static string? _level3FontName;

            /// <summary>
            /// Get or sets the character that is used for bullets of list level 3 in a MigraDoc document.
            /// </summary>
            public static char Level3Character { get; set; }

        }
    }
}
