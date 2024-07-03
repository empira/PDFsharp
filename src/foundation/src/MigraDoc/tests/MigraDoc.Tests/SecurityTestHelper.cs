// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
#if CORE
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;
#endif
#if WPF
using System.IO;
#endif

namespace MigraDoc.Tests
{
    public class SecurityTestHelper
    {
        public const string PasswordUserDefault = "User";
        public const string PasswordOwnerDefault = "Owner";
        public const string PasswordWrong = "Nobody";

        /// <summary>
        /// Enum for different encryption configurations.
        /// Using the enum for [InlineData], [MemberData] or [ClassData] in unit tests produces a readable output of the test configuration.
        /// </summary>
        public enum TestOptionsEnum
        {
            None,
            Default,
            V1,
            V2With40Bits,
            V2With128Bits,
            // ReSharper disable InconsistentNaming
            V4UsingRC4,
            V4UsingRC4WithoutMetadata,
            // ReSharper restore InconsistentNaming
            V4UsingAES,
            V4UsingAESWithoutMetadata,
            V5,
            V5WithoutMetadata
        }

        static readonly TestOptionsEnum[] SkippedTestOptions = {
            // Add test options to skip here.
        };

        public const string SkippedTestOptionsMessage = "Skipped through SkippedTestOptions.";

        /// <summary>
        /// Wrapper class containing classes returning the desired encryption configuration for use in [Theory] unit test with [ClassData].
        /// Note that every class needs its pendant returning the skipped tests of the desired encryption configuration range.
        /// </summary>
        public class TestData
        {
            public class AllVersionsAndDefault : TestDataBase
            {
                public AllVersionsAndDefault() : this(false)
                { }

                protected AllVersionsAndDefault(bool getSkipped) : base(x => x is not TestOptionsEnum.None, getSkipped)
                { }
            }
            public class AllVersionsAndDefaultSkipped : AllVersionsAndDefault
            {
                public AllVersionsAndDefaultSkipped() : base(true)
                { }
            }

            public class AllVersions : TestDataBase
            {
                public AllVersions() : this(false)
                { }

                protected AllVersions(bool getSkipped) : base(x => x is not TestOptionsEnum.None and not TestOptionsEnum.Default, getSkipped)
                { }
            }
            public class AllVersionsSkipped : AllVersions
            {
                public AllVersionsSkipped() : base(true)
                { }
            }

            public class V4 : TestDataBase
            {
                public V4() : this(false)
                { }

#if NET6_0_OR_GREATER
                protected V4(bool getSkipped) : base(x => Enum.GetName(x)!.StartsWith("V4", StringComparison.OrdinalIgnoreCase), getSkipped)
                { }
#else
                protected V4(bool getSkipped) : base(x => Enum.GetName(typeof(TestOptionsEnum), x)!.StartsWith("V4", StringComparison.OrdinalIgnoreCase), getSkipped)
                { }
#endif
            }
            public class V4Skipped : V4
            {
                public V4Skipped() : base(true)
                { }
            }

            public class V5 : TestDataBase
            {
                public V5() : this(false)
                { }

#if NET6_0_OR_GREATER
                protected V5(bool getSkipped) : base(x => Enum.GetName(x)!.StartsWith("V5", StringComparison.OrdinalIgnoreCase), getSkipped)
                { }
#else
                protected V5(bool getSkipped) : base(x => Enum.GetName(typeof(TestOptionsEnum), x)!.StartsWith("V5", StringComparison.OrdinalIgnoreCase), getSkipped)
                { }
#endif
            }
            public class V5Skipped : V5
            {
                public V5Skipped() : base(true)
                { }
            }

            /// <summary>
            /// Base class for the encryption configuration [ClassData] containing the logic.
            /// </summary>
            public abstract class TestDataBase : IEnumerable<object[]>
            {
                readonly List<object[]> _data;

                protected TestDataBase(Func<TestOptionsEnum, bool>? condition = null, bool getSkipped = false)
                {
#if NET6_0_OR_GREATER
                    _data = Enum.GetValues<TestOptionsEnum>()
                        .Where(x =>
                            SkippedTestOptions.Contains(x) == getSkipped // Get Skipped or not skipped encryption configurations, like desired.
                            && (condition is null || condition(x))) // Get only the encryption configurations matching the desired condition, if given.
                        .Select(x => new object[] { x }).ToList();
#else
                    var enums = Enum.GetValues(typeof(TestOptionsEnum));
                    var list = new List<TestOptionsEnum>();
                    foreach (TestOptionsEnum e in enums)
                    {
                        list.Add(e);
                    }
                    _data = list
                        .Where(x =>
                            SkippedTestOptions.Contains(x) == getSkipped // Get Skipped or not skipped encryption configurations, like desired.
                            && (condition is null || condition(x))) // Get only the encryption configurations matching the desired condition, if given.
                        .Select(x => new object[] { x }).ToList();
#endif
                }

                public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }

        /// <summary>
        /// A class containing the test configuration to avoid multiple parameters in many test methods and reduce refactoring need on changes of test configurations.
        /// Use ByEnum() to create options fromTestOptionsEnum inside the test.
        /// </summary>
        public class TestOptions
        {
            public PdfDefaultEncryption Encryption { get; init; }

            /// <summary>
            /// Encrypt the Metadata dictionary (default = true). Valid for Version 4 and 5.
            /// </summary>
            public bool EncryptMetadata { get; private set; } = true;

            public string? UserPassword { get; private set; }
            public string? OwnerPassword { get; private set; }

            public void SetDefaultPasswords(bool userPassword, bool ownerPassword = false)
            {
                UserPassword = userPassword ? PasswordUserDefault : null;
                OwnerPassword = ownerPassword ? PasswordOwnerDefault : null;
            }

            public void SetPasswords(string userPassword, string? ownerPassword = null)
            {
                UserPassword = userPassword;
                OwnerPassword = ownerPassword;
            }

            public static TestOptions ByEnum(TestOptionsEnum? @enum)
            {
                return @enum switch
                {
                    TestOptionsEnum.None => new() { Encryption = PdfDefaultEncryption.None },
                    TestOptionsEnum.Default => new() { Encryption = PdfDefaultEncryption.Default },
                    TestOptionsEnum.V1 => new() { Encryption = PdfDefaultEncryption.V1 },
                    TestOptionsEnum.V2With40Bits => new() { Encryption = PdfDefaultEncryption.V2With40Bits },
                    TestOptionsEnum.V2With128Bits => new() { Encryption = PdfDefaultEncryption.V2With128Bits },
                    TestOptionsEnum.V4UsingRC4 => new() { Encryption = PdfDefaultEncryption.V4UsingRC4 },
                    TestOptionsEnum.V4UsingRC4WithoutMetadata => new() { Encryption = PdfDefaultEncryption.V4UsingRC4, EncryptMetadata = false },
                    TestOptionsEnum.V4UsingAES => new() { Encryption = PdfDefaultEncryption.V4UsingAES },
                    TestOptionsEnum.V4UsingAESWithoutMetadata => new() { Encryption = PdfDefaultEncryption.V4UsingAES, EncryptMetadata = false },
                    TestOptionsEnum.V5 => new() { Encryption = PdfDefaultEncryption.V5 },
                    TestOptionsEnum.V5WithoutMetadata => new() { Encryption = PdfDefaultEncryption.V5, EncryptMetadata = false },
                    _ => throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null)
                };
            }
        }

        public static Document CreateEmptyTestDocument()
        {
            var doc = new Document();
            return doc;
        }

        public static Document CreateStandardTestDocument()
        {
            var doc = CreateEmptyTestDocument();
            doc.AddSection().AddParagraph().AddText("Text");

            return doc;
        }

        public static PdfDocumentRenderer RenderDocument(Document document)
        {
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            return pdfRenderer;
        }

        public static PdfDocumentRenderer RenderSecuredDocument(Document document, TestOptions options)
        {
            var pdfRenderer = new PdfDocumentRenderer { Document = document };
            pdfRenderer.RenderDocument();

            SecureDocument(pdfRenderer.PdfDocument, options);

            return pdfRenderer;
        }
        public static void SecureDocument(PdfDocument pdfDoc, TestOptions options)
        {
            if (options.Encryption != PdfDefaultEncryption.None)
            {
                if (options.UserPassword is not null)
                    pdfDoc.SecuritySettings.UserPassword = options.UserPassword;
                if (options.OwnerPassword is not null)
                    pdfDoc.SecuritySettings.OwnerPassword = options.OwnerPassword;

                var securityHandler = pdfDoc.SecurityHandler;

                // Encryptions to initialize manually with additional options.
                if (options.Encryption == PdfDefaultEncryption.V4UsingRC4)
                    securityHandler.SetEncryptionToV4UsingRC4(options.EncryptMetadata);
                else if (options.Encryption == PdfDefaultEncryption.V4UsingAES)
                    securityHandler.SetEncryptionToV4UsingAES(options.EncryptMetadata);
                else if (options.Encryption == PdfDefaultEncryption.V5)
                    securityHandler.SetEncryptionToV5(options.EncryptMetadata);
                // Encryptions to initialize through enum. Default encryption is already set, so we avoid to set it again.
                else if (options.Encryption != PdfDefaultEncryption.Default)
                    securityHandler.SetEncryption(options.Encryption);
            }
        }

        public static void WriteSecuredTestDocument(Document document, string filename, TestOptions options)
        {
            var pdfRenderer = RenderSecuredDocument(document, options);
            pdfRenderer.Save(filename);
        }
        
        public static PdfDocumentRenderer RenderSecuredStandardTestDocument(TestOptions options)
        {
            return RenderSecuredDocument(CreateStandardTestDocument(), options);
        }

        public static void WriteStandardTestDocument(string filename)
        {
            var pdfRenderer = RenderDocument(CreateStandardTestDocument());
            pdfRenderer.Save(filename);
        }

        public static void WriteSecuredStandardTestDocument(string filename, TestOptions options)
        {
            var pdfRenderer = RenderSecuredStandardTestDocument(options);
            pdfRenderer.Save(filename);
        }

        /// <summary>
        /// Adds a prefix to the filename, depending on the options Encryption and EncryptMetadata properties.
        /// Other information must be added manually to the filename parameter (this applies also to the use of user and/or owner password).
        /// </summary>
        public static string AddPrefixToFilename(string filename, TestOptions? options = null)
        {
            var prefix = GetFilenamePrefix(options);

            return $"{prefix} {filename}";
        }

        /// <summary>
        /// Adds a suffix to the filename, depending on the options Encryption and EncryptMetadata properties.
        /// Other information must be added manually to the filename parameter (this applies also to the use of user and/or owner password).
        /// </summary>
        public static string AddSuffixToFilename(string filename, TestOptions? options = null)
        {
            var suffix = GetFilenamePrefix(options);

            var extension = Path.GetExtension(filename);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            return $"{filenameWithoutExtension} {suffix}{extension}";
        }

        static string GetFilenamePrefix(TestOptions? options)
        {
            // mor information to file name scheme in SecurityTests class.
            var prefixSuffix = "S_";

            // Prefix for non-encrypted file.
            if (options is null || options.Encryption is PdfDefaultEncryption.None)
                prefixSuffix += "_No";
            // Prefix for encrypted file.
            else
            {
#if NET6_0_OR_GREATER
                prefixSuffix += $"{Enum.GetName(options.Encryption)}"
                    .Replace("Default", "Def")
                    .Replace("Using", "_")
                    .Replace("With", "_")
                    .Replace("Bits", "B");
#else
                prefixSuffix += $"{Enum.GetName(typeof(PdfDefaultEncryption), options.Encryption)}"
                    .Replace("Default", "Def")
                    .Replace("Using", "_")
                    .Replace("With", "_")
                    .Replace("Bits", "B");
#endif

                if (!options.EncryptMetadata)
                    prefixSuffix += "_XMeta";
            }

            return prefixSuffix;
        }
    }
}
