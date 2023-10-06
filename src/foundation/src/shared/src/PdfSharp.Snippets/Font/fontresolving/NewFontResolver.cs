using PdfSharp.Fonts;
using System.Runtime.InteropServices;

namespace PdfSharp.Snippets.Font
{
    public class NewFontResolver : IFontResolver
    {
        /// <summary>
        /// NewFontResolver singleton for use in unit tests.
        /// </summary>
        public static NewFontResolver Get()
        {
            try
            {
                Monitor.Enter(typeof(NewFontResolver));

                if (_singleton != null)
                    return _singleton;
                return _singleton = new NewFontResolver();
            }
            finally
            {
                Monitor.Exit(typeof(NewFontResolver));
            }
        }
        private static NewFontResolver? _singleton;

#if DEBUG
        public override String ToString()
        {
            var result = "Base: " + (base.ToString() ?? "<null>");
            if (ReferenceEquals(this, _singleton))
                result = "<Singleton>. " + result;

            return result;
        }
#endif

        public class Family 
        {
            public string FamilyName;
            public string FaceName;
            public string LinuxFaceName;
            public string[] LinuxSubstituteFamilyNames;

            public Family(
            string familyName,
            string faceName,
            string linuxFaceName = "",
            params string[] linuxSubstituteFamilyNames)
            {
                this.FamilyName = familyName;
                this.FaceName = faceName;
                this.LinuxFaceName = linuxFaceName;
                this.LinuxSubstituteFamilyNames = linuxSubstituteFamilyNames;
            }
        }

        public static readonly List<Family> Families;

        static NewFontResolver()
        {
            Families = new List<Family>
            {
                new("Arial", "arial", "Arial", "FreeSans"),
                new("Arial Black", "ariblk", "Arial-Black"),
                new("Arial Bold", "arialbd", "Arial-Bold", "FreeSansBold"),
                new("Arial Italic", "ariali", "Arial-Italic", "FreeSansOblique"),
                new("Arial Bold Italic", "arialbi", "Arial-BoldItalic", "FreeSansBoldOblique"),

                new("Courier New", "cour", "Courier-Bold", "DejaVu Sans Mono", "Bitstream Vera Sans Mono", "FreeMono"),
                new("Courier New Bold", "courbd", "CourierNew-Bold", "DejaVu Sans Mono Bold", "Bitstream Vera Sans Mono Bold", "FreeMonoBold"),
                new("Courier New Italic", "couri", "CourierNew-Italic", "DejaVu Sans Mono Oblique", "Bitstream Vera Sans Mono Italic", "FreeMonoOblique"),
                new("Courier New Bold Italic", "courbi", "CourierNew-BoldItalic", "DejaVu Sans Mono Bold Oblique", "Bitstream Vera Sans Mono Bold Italic", "FreeMonoBoldOblique"),

                new("Verdana", "verdana", "Verdana", "DejaVu Sans", "Bitstream Vera Sans"),
                new("Verdana Bold", "verdanab", "Verdana-Bold", "DejaVu Sans Bold", "Bitstream Vera Sans Bold"),
                new("Verdana Italic", "verdanai", "Verdana-Italic", "DejaVu Sans Oblique", "Bitstream Vera Sans Italic"),
                new("Verdana Bold Italic", "verdanaz", "Verdana-BoldItalic", "DejaVu Sans Bold Oblique", "Bitstream Vera Sans Bold Italic"),

                new("Times New Roman", "times", "TimesNewRoman", "FreeSerif"),
                new("Times New Roman Bold", "timesbd", "TimesNewRoman-Bold", "FreeSerifBold"),
                new("Times New Roman Italic", "timesi", "TimesNewRoman-Italic", "FreeSerifItalic"),
                new("Times New Roman Bold Italic", "timesbi", "TimesNewRoman-BoldItalic", "FreeSerifBoldItalic"),

                new("Lucida Console", "lucon", "LucidaConsole", "DejaVu Sans Mono"),

                new("Segoe UI Emoji", "seguiemj"), // No Linux substitute

                new("Symbol", "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly

                new("Wingdings", "wingding"), // No Linux substitute

                // Linux Substitute Fonts
                // TODO Nimbus and Liberation are only readily available as OTF.

                // Ubuntu packages: fonts-dejavu fonts-dejavu-core fonts-dejavu-extra
                new("DejaVu Sans", "DejaVuSans"),
                new("DejaVu Sans Bold", "DejaVuSans-Bold"),
                new("DejaVu Sans Oblique", "DejaVuSans-Oblique"),
                new("DejaVu Sans Bold Oblique", "DejaVuSans-BoldOblique"),
                new("DejaVu Sans Mono", "DejaVuSansMono"),
                new("DejaVu Sans Mono Bold", "DejaVuSansMono-Bold"),
                new("DejaVu Sans Mono Oblique", "DejaVuSansMono-Oblique"),
                new("DejaVu Sans Mono Bold Oblique", "DejaVuSansMono-BoldOblique"),

                // Ubuntu packages: fonts-freefont-ttf
                new("FreeSans", "FreeSans"),
                new("FreeSansBold", "FreeSansBold"),
                new("FreeSansOblique", "FreeSansOblique"),
                new("FreeSansBoldOblique", "FreeSansBoldOblique"),
                new("FreeMono", "FreeMono"),
                new("FreeMonoBold", "FreeMonoBold"),
                new("FreeMonoOblique", "FreeMonoOblique"),
                new("FreeMonoBoldOblique", "FreeMonoBoldOblique"),
                new("FreeSerif", "FreeSerif"),
                new("FreeSerifBold", "FreeSerifBold"),
                new("FreeSerifItalic", "FreeSerifItalic"),
                new("FreeSerifBoldItalic", "FreeSerifBoldItalic"),

                // Ubuntu packages: ttf-bitstream-vera
                new("Bitstream Vera Sans", "Vera"),
                new("Bitstream Vera Sans Bold", "VeraBd"),
                new("Bitstream Vera Sans Italic", "VeraIt"),
                new("Bitstream Vera Sans Bold Italic", "VeraBI"),
                new("Bitstream Vera Sans Mono", "VeraMono"),
                new("Bitstream Vera Sans Mono Bold", "VeraMoBd"),
                new("Bitstream Vera Sans Mono Italic", "VeraMoIt"),
                new("Bitstream Vera Sans Mono Bold Italic", "VeraMoBI"),

                // Ubuntu packages: fonts-noto-core
                new("Noto Sans Symbols Regular", "NotoSansSymbols-Regular"),
                new("Noto Sans Symbols Bold", "NotoSansSymbols-Bold"),
            };
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var families = Families.Where(f => f.FamilyName.StartsWith(familyName));
            var baseFamily = Families.FirstOrDefault();

            if (isBold)
                families = families.Where(f => f.FamilyName.ToLowerInvariant().Contains("bold") || f.FamilyName.ToLowerInvariant().Contains("heavy"));

            if (isItalic)
                families = families.Where(f => f.FamilyName.ToLowerInvariant().Contains("italic") || f.FamilyName.ToLowerInvariant().Contains("oblique"));

            var family = families.FirstOrDefault();
            if (family is not null)
                return new FontResolverInfo(family.FaceName);

            if (baseFamily is not null)
                return new FontResolverInfo(baseFamily.FaceName, isBold, isItalic);

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetFontWindows(faceName);
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetFontLinux(faceName);
            
            return null;
        }

        byte[]? GetFontWindows(string faceName)
        {
            var fontLocations = new List<string>
            {
                @"C:\Windows\Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\Fonts")
            };

            foreach (var fontLocation in fontLocations)
            {
                var filepath = Path.Combine(fontLocation, faceName + ".ttf");
                if (File.Exists(filepath))
                    return File.ReadAllBytes(filepath);
            }

            return null;
        }

        byte[]? GetFontLinux(string faceName)
        {
            // TODO Query fontconfig.
            // Fontconfig is the de facto standard for indexing and managing fonts on linux.
            // Example command that should return a full file path to FreeSansBoldOblique.ttf:
            //     fc-match -f '%{file}\n' 'FreeSans:Bold:Oblique:fontformat=TrueType' : file
            //
            // Caveat: fc-match *always* returns a "next best" match or default font, even if it's bad.
            // Caveat: some preprocessing/refactoring needed to produce a pattern fc-match can understand.
            // Caveat: fontconfig needs additional configuration to know about WSL having Windows Fonts available at /mnt/c/Windows/Fonts.

            var fontLocations = new List<string>
            {
                "/mnt/c/Windows/Fonts", // WSL first or substitutes will be found.
                "/usr/share/fonts",
                "/usr/share/X11/fonts",
                "/usr/X11R6/lib/X11/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "/.fonts"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "/.local/share/fonts"),
            };

            var fcp = Environment.GetEnvironmentVariable("FONTCONFIG_PATH");
            if (fcp is not null && !fontLocations.Contains(fcp))
                fontLocations.Add(fcp);

            foreach (var fontLocation in fontLocations)
            {
                if (!Directory.Exists(fontLocation))
                    continue;

                var fontPath = FindFileRecursive(fontLocation, faceName);
                if (fontPath is not null && File.Exists(fontPath))
                    return File.ReadAllBytes(fontPath);
            }

            return null;
        }

        /// <summary>
        /// Finds filename candidates recursively on Linux, as organizing fonts into arbitrary subdirectories is allowed.
        /// </summary>
        string? FindFileRecursive(string basepath, string faceName)
        {
            var filenameCandidates = FaceNameToFilenameCandidates(faceName);

            foreach (var file in Directory.GetFiles(basepath).Select(Path.GetFileName))
            foreach (var filenameCandidate in filenameCandidates)
            {
                // Most programs treat fonts case-sensitive on Linux. We ignore case because we also target WSL.
                if (!String.IsNullOrEmpty(file) && file.Equals(filenameCandidate, StringComparison.OrdinalIgnoreCase))
                    return Path.Combine(basepath, filenameCandidate);
            }

            // Linux allows arbitrary subdirectories for organizing fonts.
            foreach (var directory in Directory.GetDirectories(basepath).Select(Path.GetFileName))
            {
                if (String.IsNullOrEmpty(directory))
                    continue;

                var file = FindFileRecursive(Path.Combine(basepath, directory), faceName);
                if (file is not null)
                    return file;
            }

            return null;
        }

        /// <summary>
        /// Generates filename candidates for Linux systems.
        /// </summary>
        string[] FaceNameToFilenameCandidates(string faceName)
        {
            const string fileExtension = ".ttf";
            // TODO OTF Fonts are popular on Linux too.

            var candidates = new List<string>
            {
                faceName + fileExtension // We need to look for Windows face name too in case of WSL or copied files.
            };

            var family = Families.FirstOrDefault(f => f.FaceName == faceName);
            if (family is null)
                return candidates.ToArray();

            if (!String.IsNullOrEmpty(family.LinuxFaceName))
                candidates.Add(family.LinuxFaceName + fileExtension);
            candidates.Add(family.FamilyName + fileExtension);

            // Add substitute fonts as last candidates.
            foreach (var replacement in family.LinuxSubstituteFamilyNames)
            {
                var replacementFamily = Families.FirstOrDefault(f => f.FamilyName == replacement);
                if (replacementFamily is null)
                    continue;

                candidates.Add(replacementFamily.FamilyName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.FaceName))
                    candidates.Add(replacementFamily.FaceName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.LinuxFaceName))
                    candidates.Add(replacementFamily.LinuxFaceName + fileExtension);
            }

            return candidates.ToArray();
        }
    }
}
