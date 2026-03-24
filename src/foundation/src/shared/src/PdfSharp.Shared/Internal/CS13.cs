// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if DEBUG

namespace PdfSharp.Internal
{
    // Test how CS 13 features works in all PDFsharp builds.

    public class TimerRemaining
    {
        const string Dummy = "abc\edef";

        //public string Name { get => field; set => field = value; }

        public int[] Buffer { get; set; } = new int[10];

        void Foo()
        {
            var countdown = new TimerRemaining()
            {
                Buffer =
                {
                    [^1] = 0,
                    [^2] = 1,
                    [^3] = 2,
                    [^4] = 3,
                    [^5] = 4,
                    [^6] = 5,
                    [^7] = 6,
                    [^8] = 7,
                    [^9] = 8,
                    [^10] = 9
                }
            };
        }
    }

    public partial class C
    {
        // Declaring declaration.
        public partial string Name { get; set; }
    }

    public partial class C
    {
        // Implementation declaration.
        public partial string Name
        {
            get => _name;
            set => _name = value;
        }
        string _name = "";
    }
}
#endif
