// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using PdfSharp.Quality;
#if WPF
using System.IO;
#endif

namespace PdfSharp.TestHelper
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
            V5WithoutMetadata,
            V5R5ReadOnly
        }

        public enum TestEncryptions
        {
            None = PdfDefaultEncryption.None,
            Default = PdfDefaultEncryption.Default,
            V1 = PdfDefaultEncryption.V1,
            V2With40Bits = PdfDefaultEncryption.V2With40Bits,
            V2With128Bits = PdfDefaultEncryption.V2With128Bits,
            V4UsingRC4 = PdfDefaultEncryption.V4UsingRC4,
            V4UsingAES = PdfDefaultEncryption.V4UsingAES,
            V5 = PdfDefaultEncryption.V5,
            V5R5ReadOnly // Encryption version 5 revision is proprietary and deprecate, but reading those documents is supported.
        }

        static readonly TestOptionsEnum[] SkippedTestOptions =
        [
            // Add test options to skip here.
        ];

        public const string SkippedTestOptionsMessage = "Skipped through SkippedTestOptions.";

        /// <summary>
        /// Wrapper class containing classes returning the desired encryption configuration for use in [Theory] unit test with [ClassData].
        /// Note that every class needs its pendant returning the skipped tests of the desired encryption configuration range.
        /// </summary>
        public class TestData
        {
            /// <summary>
            /// Returns all TestOptionsEnum values, except None. Also, ReadOnly values are excluded.
            /// </summary>
            public class AllWriteVersionsAndDefault : TestDataBase
            {
                public AllWriteVersionsAndDefault() : this(false)
                { }

                protected AllWriteVersionsAndDefault(bool getSkipped) : base(true, false, true, getSkipped)
                { }
            }
            /// <summary>
            /// Returns all skipped TestOptionsEnum values, except None. Also, ReadOnly values are excluded.
            /// </summary>
            public class AllWriteVersionsAndDefaultSkipped : AllWriteVersionsAndDefault
            {
                public AllWriteVersionsAndDefaultSkipped() : base(true)
                { }
            }


            /// <summary>
            /// Returns all TestOptionsEnum values, except None and Default. Also, ReadOnly values are excluded.
            /// </summary>
            public class AllWriteVersions : TestDataBase
            {
                public AllWriteVersions() : this(false)
                { }

                protected AllWriteVersions(bool getSkipped) : base(true, true, true, getSkipped)
                { }
            }
            /// <summary>
            /// Returns all skipped TestOptionsEnum values, except None and Default. Also, ReadOnly values are excluded.
            /// </summary>
            public class AllWriteVersionsSkipped : AllWriteVersions
            {
                public AllWriteVersionsSkipped() : base(true)
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

                protected TestDataBase(bool excludeNone, bool excludeDefault, bool excludeReadonly, bool getSkipped = false) 
                    : this(x =>
                    {
                        if (excludeNone && x == TestOptionsEnum.None ||
                            excludeDefault && x == TestOptionsEnum.Default ||
                            excludeReadonly && x == TestOptionsEnum.V5R5ReadOnly)
                            return false;
                        return true;
                    }, getSkipped)
                {}

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
            public TestEncryptions Encryption { get; init; }

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
                    TestOptionsEnum.None => new() { Encryption = TestEncryptions.None },
                    TestOptionsEnum.Default => new() { Encryption = TestEncryptions.Default },
                    TestOptionsEnum.V1 => new() { Encryption = TestEncryptions.V1 },
                    TestOptionsEnum.V2With40Bits => new() { Encryption = TestEncryptions.V2With40Bits },
                    TestOptionsEnum.V2With128Bits => new() { Encryption = TestEncryptions.V2With128Bits },
                    TestOptionsEnum.V4UsingRC4 => new() { Encryption = TestEncryptions.V4UsingRC4 },
                    TestOptionsEnum.V4UsingRC4WithoutMetadata => new() { Encryption = TestEncryptions.V4UsingRC4, EncryptMetadata = false },
                    TestOptionsEnum.V4UsingAES => new() { Encryption = TestEncryptions.V4UsingAES },
                    TestOptionsEnum.V4UsingAESWithoutMetadata => new() { Encryption = TestEncryptions.V4UsingAES, EncryptMetadata = false },
                    TestOptionsEnum.V5 => new() { Encryption = TestEncryptions.V5 },
                    TestOptionsEnum.V5WithoutMetadata => new() { Encryption = TestEncryptions.V5, EncryptMetadata = false },
                    TestOptionsEnum.V5R5ReadOnly => new TestOptions() { Encryption = TestEncryptions.V5R5ReadOnly },
                    _ => throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null)
                };
            }
        }

        public static void SecureDocument(PdfDocument pdfDoc, TestOptions options)
        {
            if (options.Encryption == TestEncryptions.V5R5ReadOnly)
                throw new InvalidOperationException("A document cannot be secured with a ReadOnly encryption.");

            if (options.Encryption != TestEncryptions.None)
            {
                if (options.UserPassword is not null)
                    pdfDoc.SecuritySettings.UserPassword = options.UserPassword;
                if (options.OwnerPassword is not null)
                    pdfDoc.SecuritySettings.OwnerPassword = options.OwnerPassword;

                var securityHandler = pdfDoc.SecurityHandler;

                // Encryptions to initialize manually with additional options.
                if (options.Encryption == TestEncryptions.V4UsingRC4)
                    securityHandler.SetEncryptionToV4UsingRC4(options.EncryptMetadata);
                else if (options.Encryption == TestEncryptions.V4UsingAES)
                    securityHandler.SetEncryptionToV4UsingAES(options.EncryptMetadata);
                else if (options.Encryption == TestEncryptions.V5)
                    securityHandler.SetEncryptionToV5(options.EncryptMetadata);
                // Encryptions to initialize through enum. Default encryption is already set, so we avoid to set it again.
                else if (options.Encryption != TestEncryptions.Default)
                    securityHandler.SetEncryption((PdfDefaultEncryption)options.Encryption);
            }
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
            if (options is null || options.Encryption is TestEncryptions.None)
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
                prefixSuffix += $"{Enum.GetName(typeof(TestEncryptions), options.Encryption)}"
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

        public static void GetAssetsTestFile(TestOptions options, out string filename)
        {
            filename = "";
            if (options.Encryption == TestEncryptions.V5R5ReadOnly)
            {
                IOUtility.EnsureAssetsVersion(1021);

                var hasUserPassword = !String.IsNullOrEmpty(options.UserPassword);
                var hasOwnerPassword = !String.IsNullOrEmpty(options.OwnerPassword);

                if (hasUserPassword && hasOwnerPassword)
                {
                    filename = IOUtility.GetAssetsPath("pdfsharp/encryption/S_V5R5 w UO.pdf")!;
                    return;
                }
                if (hasUserPassword)
                {
                    filename = IOUtility.GetAssetsPath("pdfsharp/encryption/S_V5R5 w U.pdf")!;
                    return;
                }
                if (hasOwnerPassword)
                {
                    filename = IOUtility.GetAssetsPath("pdfsharp/encryption/S_V5R5 w O.pdf")!;
                    return;
                }

                // How to cache a file from the web in assets.
                //filename = IOUtility.GetCachedWebFile("pdfsharp/encryption/r5-owner-password.pdf",
                //    "https://github.com/py-pdf/pypdf/blob/0c81f3cfad26ddffbfc60d0ae855118e515fad8c/resources/encryption/r5-owner-password.pdf?raw=true");
                //options.SetPasswords("", "asdfzxcv");

                throw new InvalidOperationException("There are test files to cache for version 5 revision 5 for user and owner password only.");
            }

            throw new InvalidOperationException("There are no test files to cache for this encryption configuration.");
        }
    }
}
